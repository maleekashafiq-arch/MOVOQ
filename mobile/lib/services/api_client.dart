import 'dart:convert';

import 'package:http/http.dart' as http;

import '../config.dart';
import '../models/current_user.dart';
import '../models/daily_steps.dart';
import '../models/local_identity.dart';
import '../models/reward.dart';

class ApiException implements Exception {
  ApiException(this.message, {this.statusCode, this.code});

  final String message;
  final int? statusCode;
  final String? code;

  @override
  String toString() => message;
}

class ApiClient {
  ApiClient({http.Client? httpClient}) : _http = httpClient ?? http.Client();

  final http.Client _http;
  LocalIdentity localIdentity = LocalIdentity.defaults;

  Uri _uri(String path) => Uri.parse('${AppConfig.apiBaseUrl}$path');

  Future<CurrentUser> exchangeSession(String idToken) =>
      _sendUser('POST', '/api/v1/auth/session', idToken);

  Future<CurrentUser> getMe(String idToken) =>
      _sendUser('GET', '/api/v1/me', idToken);

  Future<CurrentUser> completeProfile({
    required String idToken,
    required String fullName,
    required String country,
    required String state,
    required String city,
    required int dailyTargetSteps,
    String? phoneNumber,
    String? gender,
  }) {
    return _sendUser(
      'PUT',
      '/api/v1/me/profile',
      idToken,
      body: {
        'fullName': fullName,
        'country': country,
        'state': state,
        'city': city,
        'dailyTargetSteps': dailyTargetSteps,
        'phoneNumber': phoneNumber,
        'gender': gender,
      },
    );
  }

  Future<List<DevUser>> listDevUsers() async {
    final response = await _http.get(_uri('/api/v1/dev/users'), headers: _headers('test'));
    _throwIfFailed(response);
    final json = jsonDecode(response.body);
    if (json is! List) {
      return const [];
    }
    return json
        .whereType<Map>()
        .map((row) => DevUser.fromJson(Map<String, dynamic>.from(row)))
        .toList();
  }

  Future<DevUser> createDevUser({
    String? fullName,
    String? email,
    String? city,
    String? state,
    String? country,
    int? dailyTargetSteps,
    int? walletBalance,
  }) async {
    final response = await _http.post(
      _uri('/api/v1/dev/users'),
      headers: _headers('test'),
      body: jsonEncode({
        'fullName': fullName,
        'email': email,
        'city': city,
        'state': state,
        'country': country,
        'dailyTargetSteps': dailyTargetSteps,
        'walletBalance': walletBalance,
      }),
    );
    _throwIfFailed(response);
    return DevUser.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<int> seedDevUsers() async {
    final response = await _http.post(_uri('/api/v1/dev/users/seed'), headers: _headers('test'));
    _throwIfFailed(response);
    final json = jsonDecode(response.body);
    if (json is Map<String, dynamic>) {
      return (json['added'] as num?)?.toInt() ?? 0;
    }
    return 0;
  }

  Future<StepsSnapshot> syncSteps({
    required String idToken,
    required String stepDate,
    required int stepCount,
    required String commandId,
    String? deviceIdentifier,
  }) async {
    final response = await _http.post(
      _uri('/api/v1/steps/sync'),
      headers: _headers(idToken),
      body: jsonEncode({
        'commandId': commandId,
        'stepDate': stepDate,
        'stepCount': stepCount,
        'deviceIdentifier': deviceIdentifier,
        'deviceRecordedAt': DateTime.now().toUtc().toIso8601String(),
      }),
    );
    _throwIfFailed(response);
    return StepsSnapshot.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<StepsSnapshot> getTodaySteps(String idToken) async {
    final response = await _http.get(_uri('/api/v1/steps/today'), headers: _headers(idToken));
    _throwIfFailed(response);
    return StepsSnapshot.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<StepsHistory> getStepsHistory(String idToken, {int days = 7}) async {
    final response = await _http.get(_uri('/api/v1/steps/history?days=$days'), headers: _headers(idToken));
    _throwIfFailed(response);
    return StepsHistory.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<void> registerDevice({
    required String idToken,
    required String deviceIdentifier,
    required String platform,
    required String commandId,
  }) async {
    final response = await _http.post(
      _uri('/api/v1/me/devices'),
      headers: _headers(idToken),
      body: jsonEncode({
        'deviceIdentifier': deviceIdentifier,
        'platform': platform,
        'commandId': commandId,
      }),
    );
    _throwIfFailed(response);
  }

  Future<MarketplaceList> listRewards(String idToken) async {
    final response = await _http.get(_uri('/api/v1/rewards'), headers: _headers(idToken));
    _throwIfFailed(response);
    return MarketplaceList.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<RedeemResult> redeemReward({
    required String idToken,
    required String rewardId,
    required String commandId,
  }) async {
    final response = await _http.post(
      _uri('/api/v1/rewards/$rewardId/redeem'),
      headers: _headers(idToken),
      body: jsonEncode({'commandId': commandId}),
    );
    _throwIfFailed(response);
    return RedeemResult.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<CurrentUser> _sendUser(
    String method,
    String path,
    String idToken, {
    Map<String, Object?>? body,
  }) async {
    final request = http.Request(method, _uri(path))
      ..headers.addAll(_headers(idToken));
    if (body != null) {
      request.body = jsonEncode(body);
    }

    final streamed = await _http.send(request);
    final response = await http.Response.fromStream(streamed);
    _throwIfFailed(response);
    return CurrentUser.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Map<String, String> _headers(String idToken) {
    final useTestIdentity = AppConfig.isLocalDev && (idToken.isEmpty || idToken == 'test');
    return {
      'Authorization': useTestIdentity ? 'Bearer test' : 'Bearer $idToken',
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      if (useTestIdentity) ...{
        'X-Test-Firebase-Uid': localIdentity.uid,
        'X-Test-Email': localIdentity.email,
        'X-Test-Email-Verified': 'true',
        'X-Test-Name': localIdentity.name,
      },
    };
  }

  void _throwIfFailed(http.Response response) {
    if (response.statusCode >= 200 && response.statusCode < 300) {
      return;
    }

    String message = 'Request failed (${response.statusCode})';
    String? code;
    try {
      final json = jsonDecode(response.body);
      if (json is Map<String, dynamic>) {
        message = (json['title'] ?? json['detail'] ?? message).toString();
        code = json['code'] as String?;
      }
    } catch (_) {}

    throw ApiException(message, statusCode: response.statusCode, code: code);
  }
}
