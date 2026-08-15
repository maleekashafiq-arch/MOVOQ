import 'dart:async';
import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:pedometer/pedometer.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../config.dart';
import '../models/daily_steps.dart';
import 'api_client.dart';
import 'ids.dart';
import 'session_controller.dart';
import 'step_counter.dart';
import 'steps_outbox.dart';

enum StepsPermission { unknown, granted, denied, unavailable }

/// Owns the step pipeline: read the sensor, keep a local high-water mark for the day, queue the
/// claim, and let the server decide what it is worth. Points, streak, and wallet always come back
/// from the API — nothing here ever awards anything on its own.
class StepsController extends ChangeNotifier {
  StepsController(this._session);

  final SessionController _session;

  SharedPreferences? _prefs;
  StepCounter? _counter;
  StepsOutbox? _outbox;
  StreamSubscription<StepCount>? _sensor;
  Timer? _ticker;

  int _sensorSteps = 0;
  int _simulatedSteps = 0;
  int _lastSyncedSteps = -1;
  DateTime? _lastSyncAt;
  bool _flushing = false;

  StepsPermission permission = StepsPermission.unknown;
  DailySteps? today;
  int walletBalance = 0;
  int streak = 0;
  bool syncing = false;
  String? error;

  /// What the phone has counted so far, which may be ahead of the last server verdict.
  int get todaySteps => _sensorSteps + _simulatedSteps;

  int get goal => today?.dailyTargetSteps ?? _session.user?.dailyTargetSteps ?? 6000;

  bool get goalReached => today?.goalCompleted ?? false;

  double get progressPercent => goal <= 0 ? 0 : (100 * todaySteps / goal).clamp(0, 100).toDouble();

  bool get sensorUnavailable => permission == StepsPermission.unavailable;

  bool get canSimulate => AppConfig.isLocalDev;

  Future<void> start() async {
    _prefs ??= await SharedPreferences.getInstance();
    _counter ??= StepCounter(_prefs!);
    _outbox ??= StepsOutbox(_prefs!);
    _simulatedSteps = _counter!.simulated;

    await _pullToday();
    await _listenToSensor();
    await _flush();

    _ticker ??= Timer.periodic(const Duration(minutes: 1), (_) => _flush());
  }

  Future<void> refresh() async {
    await _pullToday();
    await _flush();
  }

  Future<void> requestPermission() async {
    final status = await _motionPermission.request();
    permission = status.isGranted ? StepsPermission.granted : StepsPermission.denied;
    notifyListeners();
    if (status.isGranted) {
      await _listenToSensor();
    }
  }

  /// Emulators and desktop have no pedometer, so local dev can push steps in by hand.
  Future<void> addSimulatedSteps(int steps) async {
    if (!canSimulate || _counter == null) {
      return;
    }
    _simulatedSteps = await _counter!.addSimulated(steps);
    notifyListeners();
    await _flush(force: true);
  }

  Future<void> _listenToSensor() async {
    if (_sensor != null || kIsWeb) {
      return;
    }

    if (!Platform.isAndroid && !Platform.isIOS) {
      permission = StepsPermission.unavailable;
      notifyListeners();
      return;
    }

    final status = await _motionPermission.status;
    if (!status.isGranted) {
      permission = StepsPermission.denied;
      notifyListeners();
      return;
    }

    permission = StepsPermission.granted;
    _sensor = Pedometer.stepCountStream.listen(
      _onSensorReading,
      onError: (_) {
        permission = StepsPermission.unavailable;
        notifyListeners();
      },
      cancelOnError: true,
    );
    notifyListeners();
  }

  Future<void> _onSensorReading(StepCount reading) async {
    final counter = _counter;
    if (counter == null) {
      return;
    }

    _sensorSteps = await counter.apply(reading.steps, at: reading.timeStamp);
    notifyListeners();

    if (_shouldSync()) {
      await _flush();
    }
  }

  /// Syncing on every step would hammer the API, so batch unless the goal is at stake.
  bool _shouldSync() {
    final crossedGoal = !goalReached && todaySteps >= goal;
    if (crossedGoal) {
      return true;
    }
    if (_lastSyncedSteps < 0) {
      return true;
    }
    if (todaySteps - _lastSyncedSteps >= 250) {
      return true;
    }
    final since = _lastSyncAt;
    return since == null || DateTime.now().difference(since) >= const Duration(minutes: 5);
  }

  Future<void> _pullToday() async {
    try {
      final token = await _session.apiToken();
      _apply(await _session.api.getTodaySteps(token));
    } on ApiException catch (e) {
      error = e.message;
      notifyListeners();
    } catch (_) {
      // Offline reads are not worth surfacing; the cached day stays on screen.
    }
  }

  Future<void> _flush({bool force = false}) async {
    final outbox = _outbox;
    if (outbox == null || _flushing) {
      return;
    }

    final now = DateTime.now();
    final todayKey = StepCounter.dayStamp(now);
    await outbox.pruneBefore(now);
    if (todaySteps > 0) {
      await outbox.stage(todayKey, todaySteps);
    }

    // Days left behind by an offline stretch should go out even when today is quiet.
    final backlog = outbox.pending.any((claim) => claim.date != todayKey);
    if (!force && !backlog && !_shouldSync()) {
      return;
    }
    if (outbox.pending.isEmpty) {
      return;
    }

    _flushing = true;
    syncing = true;
    error = null;
    notifyListeners();

    try {
      final token = await _session.apiToken();
      for (final claim in outbox.pending) {
        final snapshot = await _session.api.syncSteps(
          idToken: token,
          stepDate: claim.date,
          stepCount: claim.steps,
          commandId: claim.commandId,
          deviceIdentifier: await _deviceIdentifier(),
        );
        await outbox.settle(claim);
        if (claim.date == todayKey) {
          _apply(snapshot);
          _lastSyncedSteps = claim.steps;
        }
      }
      _lastSyncAt = DateTime.now();
    } on ApiException catch (e) {
      // A closed sync window is permanent, so drop those days instead of retrying forever.
      if (e.code == 'sync_window_closed') {
        await outbox.pruneBefore(now);
      }
      error = e.message;
    } catch (_) {
      // Still offline. The outbox keeps the claim for the next attempt.
    } finally {
      _flushing = false;
      syncing = false;
      notifyListeners();
    }
  }

  void _apply(StepsSnapshot snapshot) {
    today = snapshot.day;
    walletBalance = snapshot.walletBalance;
    streak = snapshot.currentStreak;
    _session.applyWalletBalance(snapshot.walletBalance);
    notifyListeners();
  }

  Future<String> _deviceIdentifier() async {
    final prefs = _prefs;
    if (prefs == null) {
      return 'unknown-device';
    }
    var id = prefs.getString(_deviceIdKey);
    if (id == null) {
      id = newCommandId();
      await prefs.setString(_deviceIdKey, id);
    }
    return id;
  }

  Permission get _motionPermission =>
      !kIsWeb && Platform.isIOS ? Permission.sensors : Permission.activityRecognition;

  static const _deviceIdKey = 'movoq_device_identifier';

  @override
  void dispose() {
    _ticker?.cancel();
    _sensor?.cancel();
    super.dispose();
  }
}
