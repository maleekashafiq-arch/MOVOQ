import 'dart:convert';
import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:geolocator/geolocator.dart';
import 'package:http/http.dart' as http;

enum OutdoorStatus { loading, ready, locationOff, unavailable }

/// Live temperature and air quality for the device's current pin.
///
/// Nothing is invented locally. When location is off, the home screen stays blank until the
/// user turns it on. Readings come from Open-Meteo for that latitude and longitude.
class OutdoorConditions extends ChangeNotifier {
  OutdoorConditions({http.Client? httpClient}) : _http = httpClient ?? http.Client();

  final http.Client _http;

  OutdoorStatus status = OutdoorStatus.loading;
  double? temperatureC;
  int? aqi;
  int? weatherCode;
  String? message;
  bool _busy = false;
  bool _again = false;

  Future<void> load() => refresh();

  /// Asks for location, then loads weather. Opens settings if the user already said no forever.
  Future<void> enableLocation() async {
    var permission = await Geolocator.checkPermission();
    if (permission == LocationPermission.deniedForever) {
      await Geolocator.openAppSettings();
      return;
    }
    if (permission == LocationPermission.denied) {
      permission = await Geolocator.requestPermission();
    }
    if (!_allowed(permission)) {
      status = OutdoorStatus.locationOff;
      message = 'Turn on location to see weather and air quality for your walk';
      notifyListeners();
      return;
    }
    if (!await Geolocator.isLocationServiceEnabled()) {
      status = OutdoorStatus.locationOff;
      message = 'Turn on device location to see weather and air quality';
      notifyListeners();
      await Geolocator.openLocationSettings();
      return;
    }
    await refresh();
  }

  Future<void> refresh() async {
    if (_busy) {
      _again = true;
      return;
    }
    _busy = true;
    if (temperatureC == null) {
      status = OutdoorStatus.loading;
      notifyListeners();
    }

    try {
      final permission = await Geolocator.checkPermission();
      if (!_allowed(permission)) {
        status = OutdoorStatus.locationOff;
        message = 'Turn on location to see weather and air quality for your walk';
        notifyListeners();
        return;
      }
      if (!await Geolocator.isLocationServiceEnabled()) {
        status = OutdoorStatus.locationOff;
        message = 'Turn on device location to see weather and air quality';
        notifyListeners();
        return;
      }

      final position = await _position();
      if (position == null) {
        _fail('Could not read your location. Pull to try again.');
        return;
      }

      final reading = await _lookup(position.latitude, position.longitude);
      if (reading.temperatureC == null && reading.aqi == null) {
        _fail('Weather is unavailable right now. Pull to try again.');
        return;
      }

      temperatureC = reading.temperatureC ?? temperatureC;
      aqi = reading.aqi ?? aqi;
      weatherCode = reading.weatherCode ?? weatherCode;
      status = OutdoorStatus.ready;
      message = null;
      notifyListeners();
    } catch (_) {
      _fail('Weather is unavailable right now. Pull to try again.');
    } finally {
      _busy = false;
      if (_again) {
        _again = false;
        await refresh();
      }
    }
  }

  void _fail(String text) {
    if (temperatureC == null && aqi == null) {
      status = OutdoorStatus.unavailable;
      message = text;
    }
    notifyListeners();
  }

  bool _allowed(LocationPermission permission) {
    return permission == LocationPermission.always || permission == LocationPermission.whileInUse;
  }

  Future<Position?> _position() async {
    // Weather only needs a city-sized pin. The last fix is enough and returns
    // immediately; a fresh GPS watch is the fallback when the device has none yet.
    final last = await Geolocator.getLastKnownPosition();
    if (last != null) {
      return last;
    }

    final LocationSettings settings = Platform.isAndroid
        ? AndroidSettings(
            accuracy: LocationAccuracy.high,
            forceLocationManager: true,
          )
        : const LocationSettings(accuracy: LocationAccuracy.high);
    try {
      return await Geolocator.getCurrentPosition(locationSettings: settings).timeout(const Duration(seconds: 12));
    } catch (error) {
      debugPrint('Current position failed: $error');
      return Geolocator.getLastKnownPosition();
    }
  }

  Future<OutdoorReading> _lookup(double latitude, double longitude) async {
    final query = {
      'latitude': latitude.toString(),
      'longitude': longitude.toString(),
    };
    final weatherFuture = _http.get(
      Uri.https('api.open-meteo.com', '/v1/forecast', {
        ...query,
        'current': 'temperature_2m,weather_code',
      }),
    );
    final airFuture = _http.get(
      Uri.https('air-quality-api.open-meteo.com', '/v1/air-quality', {
        ...query,
        'current': 'us_aqi',
      }),
    );
    final responses = await Future.wait([weatherFuture, airFuture]).timeout(const Duration(seconds: 10));
    return OutdoorReading(
      temperatureC: _temperature(responses[0]),
      weatherCode: _weatherCode(responses[0]),
      aqi: _aqi(responses[1]),
    );
  }

  @override
  void dispose() {
    _http.close();
    super.dispose();
  }
}

class OutdoorReading {
  const OutdoorReading({this.temperatureC, this.weatherCode, this.aqi});

  final double? temperatureC;
  final int? weatherCode;
  final int? aqi;
}

double? _temperature(http.Response response) {
  final current = _current(response);
  final value = current?['temperature_2m'];
  return value is num ? value.toDouble() : null;
}

int? _weatherCode(http.Response response) {
  final current = _current(response);
  final value = current?['weather_code'];
  return value is num ? value.toInt() : null;
}

int? _aqi(http.Response response) {
  final current = _current(response);
  final value = current?['us_aqi'];
  return value is num ? value.round() : null;
}

Map<String, dynamic>? _current(http.Response response) {
  if (response.statusCode < 200 || response.statusCode >= 300) {
    return null;
  }
  try {
    final json = jsonDecode(response.body);
    final current = json is Map<String, dynamic> ? json['current'] : null;
    return current is Map<String, dynamic> ? current : null;
  } catch (_) {
    return null;
  }
}
