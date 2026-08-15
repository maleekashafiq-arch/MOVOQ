import 'package:firebase_core/firebase_core.dart';
import 'package:flutter/material.dart';

import 'app.dart';
import 'config.dart';
import 'services/session_controller.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await _initFirebase();
  runApp(MovoqApp(session: SessionController()));
}

Future<void> _initFirebase() async {
  try {
    const apiKey = String.fromEnvironment('FIREBASE_API_KEY');
    const appId = String.fromEnvironment('FIREBASE_APP_ID');
    const senderId = String.fromEnvironment('FIREBASE_SENDER_ID');
    if (apiKey.isNotEmpty && appId.isNotEmpty && senderId.isNotEmpty) {
      await Firebase.initializeApp(
        options: const FirebaseOptions(
          apiKey: apiKey,
          appId: appId,
          messagingSenderId: senderId,
          projectId: AppConfig.firebaseProjectId,
          authDomain: '${AppConfig.firebaseProjectId}.firebaseapp.com',
          storageBucket: '${AppConfig.firebaseProjectId}.firebasestorage.app',
        ),
      );
      return;
    }
    await Firebase.initializeApp();
  } catch (_) {
    // Frontend can still show splash, onboarding, and login screens.
  }
}
