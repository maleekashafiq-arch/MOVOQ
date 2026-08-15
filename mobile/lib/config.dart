import 'dart:io';

import 'package:flutter/foundation.dart';

class AppConfig {
  static const firebaseProjectId = 'movoq-cc5b8';

  /// Android emulator reaches the host machine on 10.0.2.2.
  static String get apiBaseUrl {
    const override = String.fromEnvironment('API_BASE_URL');
    if (override.isNotEmpty) {
      return override;
    }
    if (kIsWeb) {
      return 'http://localhost:5014';
    }
    if (Platform.isAndroid) {
      return 'http://10.0.2.2:5014';
    }
    return 'http://localhost:5014';
  }

  /// Debug builds talking to a machine-local API skip strict login gates.
  static bool get isLocalDev {
    if (kReleaseMode) {
      return false;
    }
    final host = Uri.parse(apiBaseUrl).host;
    return host == 'localhost' || host == '127.0.0.1' || host == '10.0.2.2';
  }
}
