// Parked with the Lucky Draw screen. Not part of the running app.
import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:movoq/config.dart';
import 'package:movoq/services/api_client.dart';

import 'lucky_draw.dart';

class LuckyDrawApi {
  LuckyDrawApi(this.api);

  final ApiClient api;

  Future<LuckyDrawList> listLuckyDraws(String idToken) async {
    final response = await http.get(_uri('/api/v1/lucky-draws'), headers: _headers(idToken));
    _throwIfFailed(response);
    return LuckyDrawList.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<BuyTicketResult> buyLuckyDrawTicket({
    required String idToken,
    required String drawId,
    required String commandId,
    int quantity = 1,
  }) async {
    final response = await http.post(
      _uri('/api/v1/lucky-draws/$drawId/tickets'),
      headers: _headers(idToken),
      body: jsonEncode({'commandId': commandId, 'quantity': quantity}),
    );
    _throwIfFailed(response);
    return BuyTicketResult.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Uri _uri(String path) => Uri.parse('${AppConfig.apiBaseUrl}$path');

  Map<String, String> _headers(String idToken) {
    final useTestIdentity = AppConfig.isLocalDev && (idToken.isEmpty || idToken == 'test');
    return {
      'Authorization': useTestIdentity ? 'Bearer test' : 'Bearer $idToken',
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      if (useTestIdentity) ...{
        'X-Test-Firebase-Uid': api.localIdentity.uid,
        'X-Test-Email': api.localIdentity.email,
        'X-Test-Email-Verified': 'true',
        'X-Test-Name': api.localIdentity.name,
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
