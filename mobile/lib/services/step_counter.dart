import 'package:shared_preferences/shared_preferences.dart';

/// Android's `STEP_COUNTER` sensor and iOS's `CMPedometer` both report a running total that
/// survives app restarts but resets to zero when the device reboots. This turns that total into
/// "steps taken today" by remembering where the current day started.
class StepCounter {
  StepCounter(this._prefs);

  final SharedPreferences _prefs;

  static const _dayKey = 'movoq_step_day';
  static const _baselineKey = 'movoq_step_baseline';
  static const _carriedKey = 'movoq_step_carried';
  static const _lastReadingKey = 'movoq_step_last_reading';
  static const _simulatedKey = 'movoq_step_simulated';

  /// Folds a raw sensor total into today's count and returns it.
  Future<int> apply(int cumulative, {DateTime? at}) async {
    final today = dayStamp(at ?? DateTime.now());
    var baseline = _prefs.getInt(_baselineKey) ?? cumulative;
    var carried = _prefs.getInt(_carriedKey) ?? 0;
    final last = _prefs.getInt(_lastReadingKey) ?? cumulative;

    if (_prefs.getString(_dayKey) != today) {
      baseline = cumulative;
      carried = 0;
      await _prefs.setString(_dayKey, today);
      await _prefs.setInt(_simulatedKey, 0);
    } else if (cumulative < last) {
      // The device rebooted, so bank what the previous boot contributed and start again from zero.
      carried += last - baseline;
      baseline = 0;
    }

    await _prefs.setInt(_baselineKey, baseline);
    await _prefs.setInt(_carriedKey, carried);
    await _prefs.setInt(_lastReadingKey, cumulative);

    final steps = carried + (cumulative - baseline);
    return steps < 0 ? 0 : steps;
  }

  /// Steps added by hand on a machine with no motion sensor. Local development only.
  int get simulated => _prefs.getInt(_simulatedKey) ?? 0;

  Future<int> addSimulated(int steps, {DateTime? at}) async {
    final today = dayStamp(at ?? DateTime.now());
    final total = _prefs.getString(_dayKey) == today ? simulated + steps : steps;
    await _prefs.setString(_dayKey, today);
    await _prefs.setInt(_simulatedKey, total);
    return total;
  }

  static String dayStamp(DateTime date) =>
      '${date.year.toString().padLeft(4, '0')}-'
      '${date.month.toString().padLeft(2, '0')}-'
      '${date.day.toString().padLeft(2, '0')}';
}
