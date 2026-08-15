import 'dart:convert';

import 'package:shared_preferences/shared_preferences.dart';

import 'ids.dart';

class PendingStepClaim {
  const PendingStepClaim({required this.date, required this.steps, required this.commandId});

  final String date;
  final int steps;
  final String commandId;

  Map<String, Object?> toJson() => {'steps': steps, 'commandId': commandId};
}

/// Holds one unacknowledged claim per calendar day so steps taken offline are not lost.
///
/// A claim keeps its command id while its step count is unchanged, so a retry after a dropped
/// response is a true replay the API can recognise. Raising the count mints a new id.
class StepsOutbox {
  StepsOutbox(this._prefs) {
    _load();
  }

  final SharedPreferences _prefs;
  final Map<String, PendingStepClaim> _claims = {};

  static const _key = 'movoq_steps_outbox';

  /// Days older than this are refused by the API, so stop retrying them.
  static const _retentionDays = 7;

  List<PendingStepClaim> get pending {
    final claims = _claims.values.toList()..sort((a, b) => a.date.compareTo(b.date));
    return claims;
  }

  Future<PendingStepClaim> stage(String date, int steps) async {
    final existing = _claims[date];
    final claim = existing != null && existing.steps == steps
        ? existing
        : PendingStepClaim(date: date, steps: steps, commandId: newCommandId());
    _claims[date] = claim;
    await _save();
    return claim;
  }

  Future<void> settle(PendingStepClaim claim) async {
    // A newer claim may have replaced this one while the request was in flight.
    if (_claims[claim.date]?.commandId == claim.commandId) {
      _claims.remove(claim.date);
      await _save();
    }
  }

  Future<void> pruneBefore(DateTime today) async {
    final cutoff = DateTime(today.year, today.month, today.day)
        .subtract(const Duration(days: _retentionDays));
    final stale = _claims.keys.where((date) => DateTime.parse(date).isBefore(cutoff)).toList();
    if (stale.isEmpty) {
      return;
    }
    stale.forEach(_claims.remove);
    await _save();
  }

  void _load() {
    final raw = _prefs.getString(_key);
    if (raw == null) {
      return;
    }
    try {
      final decoded = jsonDecode(raw) as Map<String, dynamic>;
      decoded.forEach((date, value) {
        final row = Map<String, dynamic>.from(value as Map);
        _claims[date] = PendingStepClaim(
          date: date,
          steps: (row['steps'] as num?)?.toInt() ?? 0,
          commandId: row['commandId'] as String? ?? newCommandId(),
        );
      });
    } catch (_) {
      _claims.clear();
    }
  }

  Future<void> _save() =>
      _prefs.setString(_key, jsonEncode(_claims.map((date, claim) => MapEntry(date, claim.toJson()))));
}
