/// The server's verdict for one day. Only these numbers may drive points or streaks in the UI.
class DailySteps {
  const DailySteps({
    required this.stepDate,
    required this.steps,
    required this.dailyTargetSteps,
    required this.goalCompleted,
    required this.pointsAwarded,
    required this.validationStatus,
  });

  final DateTime stepDate;
  final int steps;
  final int dailyTargetSteps;
  final bool goalCompleted;
  final int pointsAwarded;
  final String validationStatus;

  bool get rejected => validationStatus == 'Rejected';

  factory DailySteps.empty(DateTime date, int goal) => DailySteps(
        stepDate: date,
        steps: 0,
        dailyTargetSteps: goal,
        goalCompleted: false,
        pointsAwarded: 0,
        validationStatus: 'Pending',
      );

  factory DailySteps.fromJson(Map<String, dynamic> json) => DailySteps(
        stepDate: DateTime.parse(json['stepDate'] as String),
        steps: (json['steps'] as num?)?.toInt() ?? 0,
        dailyTargetSteps: (json['dailyTargetSteps'] as num?)?.toInt() ?? 0,
        goalCompleted: json['goalCompleted'] as bool? ?? false,
        pointsAwarded: (json['pointsAwarded'] as num?)?.toInt() ?? 0,
        validationStatus: json['validationStatus']?.toString() ?? 'Pending',
      );
}

/// Everything Home needs to repaint after a sync.
class StepsSnapshot {
  const StepsSnapshot({
    required this.day,
    required this.pointsCredited,
    required this.walletBalance,
    required this.currentStreak,
    required this.duplicate,
  });

  final DailySteps day;
  final int pointsCredited;
  final int walletBalance;
  final int currentStreak;
  final bool duplicate;

  factory StepsSnapshot.fromJson(Map<String, dynamic> json) => StepsSnapshot(
        day: DailySteps.fromJson(json['day'] as Map<String, dynamic>),
        pointsCredited: (json['pointsCredited'] as num?)?.toInt() ?? 0,
        walletBalance: (json['walletBalance'] as num?)?.toInt() ?? 0,
        currentStreak: (json['currentStreak'] as num?)?.toInt() ?? 0,
        duplicate: json['duplicate'] as bool? ?? false,
      );
}

class StepsHistory {
  const StepsHistory({
    required this.days,
    required this.totalSteps,
    required this.totalPointsAwarded,
    required this.currentStreak,
  });

  final List<DailySteps> days;
  final int totalSteps;
  final int totalPointsAwarded;
  final int currentStreak;

  factory StepsHistory.fromJson(Map<String, dynamic> json) => StepsHistory(
        days: (json['days'] as List? ?? const [])
            .whereType<Map>()
            .map((row) => DailySteps.fromJson(Map<String, dynamic>.from(row)))
            .toList(),
        totalSteps: (json['totalSteps'] as num?)?.toInt() ?? 0,
        totalPointsAwarded: (json['totalPointsAwarded'] as num?)?.toInt() ?? 0,
        currentStreak: (json['currentStreak'] as num?)?.toInt() ?? 0,
      );
}
