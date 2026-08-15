import 'dart:convert';

import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter/foundation.dart';
import 'package:google_sign_in/google_sign_in.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../config.dart';
import '../models/current_user.dart';
import '../models/local_identity.dart';
import 'api_client.dart';

enum AuthStep { loading, signedOut, needsProfile, ready }

class SessionController extends ChangeNotifier {
  SessionController({ApiClient? api}) : _api = api ?? ApiClient();

  final ApiClient _api;
  final FirebaseAuth _auth = FirebaseAuth.instance;

  static const _localIdentityKey = 'movoq_local_identity';

  AuthStep step = AuthStep.loading;
  CurrentUser? user;
  String? error;
  List<DevUser> fakeUsers = const [];

  Future<void> restore() async {
    step = AuthStep.loading;
    error = null;
    notifyListeners();

    late final User? firebaseUser;
    try {
      firebaseUser = _auth.currentUser;
    } catch (_) {
      if (AppConfig.isLocalDev) {
        await enterLocalPreview();
        return;
      }
      step = AuthStep.signedOut;
      notifyListeners();
      return;
    }
    if (firebaseUser == null) {
      if (AppConfig.isLocalDev) {
        await enterLocalPreview();
        return;
      }
      step = AuthStep.signedOut;
      notifyListeners();
      return;
    }

    try {
      await _syncWithApi();
    } catch (e) {
      if (AppConfig.isLocalDev) {
        await enterLocalPreview();
        return;
      }
      error = e.toString();
      step = AuthStep.signedOut;
      notifyListeners();
    }
  }

  Future<void> enterLocalPreview() async {
    error = null;
    await _restoreLocalIdentity();
    try {
      user = await _api.exchangeSession('test');
    } catch (_) {
      user = CurrentUser.localPreview();
    }
    step = AuthStep.ready;
    await refreshFakeUsers();
    notifyListeners();
  }

  Future<void> refreshFakeUsers() async {
    if (!AppConfig.isLocalDev) {
      return;
    }
    try {
      fakeUsers = await _api.listDevUsers();
    } catch (_) {
      fakeUsers = const [];
    }
    notifyListeners();
  }

  Future<void> switchLocalUser(LocalIdentity identity) async {
    _api.localIdentity = identity;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_localIdentityKey, jsonEncode(identity.toJson()));
    await enterLocalPreview();
  }

  Future<void> addFakeUser({String? fullName, String? city}) async {
    error = null;
    notifyListeners();
    try {
      final created = await _api.createDevUser(fullName: fullName, city: city);
      await switchLocalUser(created.identity);
    } on ApiException catch (e) {
      error = e.message;
      notifyListeners();
    }
  }

  Future<void> seedFakeUsers() async {
    try {
      await _api.seedDevUsers();
      await refreshFakeUsers();
    } on ApiException catch (e) {
      error = e.message;
      notifyListeners();
    }
  }

  Future<void> _restoreLocalIdentity() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final raw = prefs.getString(_localIdentityKey);
      if (raw != null) {
        _api.localIdentity = LocalIdentity.fromJson(jsonDecode(raw) as Map<String, dynamic>);
      }
    } catch (_) {
      _api.localIdentity = LocalIdentity.defaults;
    }
  }

  Future<void> signInWithEmail(String email, String password) async {
    await _run(() async {
      await _auth.signInWithEmailAndPassword(email: email.trim(), password: password);
      await _syncWithApi();
    }, fallbackLocal: true);
  }

  Future<void> signUpWithEmail(String email, String password) async {
    await _run(() async {
      await _auth.createUserWithEmailAndPassword(email: email.trim(), password: password);
      await _syncWithApi();
    }, fallbackLocal: true);
  }

  Future<void> signInWithGoogle() async {
    await _run(() async {
      final google = GoogleSignIn.instance;
      await google.initialize();
      final account = await google.authenticate();
      final tokens = account.authentication;
      final credential = GoogleAuthProvider.credential(idToken: tokens.idToken);
      await _auth.signInWithCredential(credential);
      await _syncWithApi();
    }, fallbackLocal: true);
  }

  Future<void> completeProfile({
    required String fullName,
    required String country,
    required String state,
    required String city,
    required int dailyTargetSteps,
    String? phoneNumber,
    String? gender,
  }) async {
    await _run(() async {
      final token = await _requireToken();
      user = await _api.completeProfile(
        idToken: token,
        fullName: fullName,
        country: country,
        state: state,
        city: city,
        dailyTargetSteps: dailyTargetSteps,
        phoneNumber: phoneNumber,
        gender: gender,
      );
      step = _stepFor(user!);
    }, fallbackLocal: true);
  }

  Future<void> signOut() async {
    await _auth.signOut();
    try {
      await GoogleSignIn.instance.signOut();
    } catch (_) {}
    user = null;
    step = AuthStep.signedOut;
    notifyListeners();
  }

  Future<void> _syncWithApi() async {
    final token = await _requireToken();
    user = await _api.exchangeSession(token);
    step = _stepFor(user!);
  }

  AuthStep _stepFor(CurrentUser current) {
    if (AppConfig.isLocalDev || current.profileComplete) {
      return AuthStep.ready;
    }
    return AuthStep.needsProfile;
  }

  ApiClient get api => _api;

  /// Token for feature calls. Local dev falls back to the API's test identity so the app still
  /// works without a Firebase sign-in.
  Future<String> apiToken() async {
    try {
      final token = await _auth.currentUser?.getIdToken();
      if (token != null && token.isNotEmpty) {
        return token;
      }
    } catch (_) {}
    if (AppConfig.isLocalDev) {
      return 'test';
    }
    throw ApiException('You are not signed in.');
  }

  /// Keeps the header points in step with a balance the server just recalculated.
  void applyWalletBalance(int balance) {
    final current = user;
    if (current == null || current.walletBalance == balance) {
      return;
    }
    user = current.copyWith(walletBalance: balance);
    notifyListeners();
  }

  Future<String> _requireToken() async {
    final token = await _auth.currentUser?.getIdToken();
    if (token == null || token.isEmpty) {
      throw ApiException('You are not signed in.');
    }
    return token;
  }

  Future<void> _run(Future<void> Function() action, {bool fallbackLocal = false}) async {
    error = null;
    notifyListeners();
    try {
      await action();
    } catch (e) {
      if (fallbackLocal && AppConfig.isLocalDev) {
        await enterLocalPreview();
        return;
      }
      if (e is FirebaseAuthException) {
        error = e.message ?? e.code;
      } else if (e is ApiException) {
        error = e.message;
      } else {
        error = e.toString();
      }
    }
    notifyListeners();
  }
}
