class LuckyDraw {
  const LuckyDraw({
    required this.id,
    required this.name,
    required this.prizeDescription,
    required this.ticketCostPoints,
    required this.startAt,
    required this.endAt,
    required this.participants,
    required this.totalTickets,
    required this.myTickets,
    required this.featured,
    this.description,
    this.emoji,
    this.imageUrl,
    this.maxTickets,
  });

  final String id;
  final String name;
  final String? description;
  final String prizeDescription;
  final String? emoji;
  final String? imageUrl;
  final int ticketCostPoints;
  final DateTime startAt;
  final DateTime endAt;
  final int participants;
  final int totalTickets;
  final int? maxTickets;
  final int myTickets;
  final bool featured;

  String get endsIn {
    final remaining = endAt.toUtc().difference(DateTime.now().toUtc());
    if (remaining.isNegative) {
      return 'ended';
    }
    if (remaining.inDays >= 1) {
      return '${remaining.inDays}d ${remaining.inHours % 24}h';
    }
    if (remaining.inHours >= 1) {
      return '${remaining.inHours}h ${remaining.inMinutes % 60}m';
    }
    return '${remaining.inMinutes}m';
  }

  factory LuckyDraw.fromJson(Map<String, dynamic> json) => LuckyDraw(
        id: json['id'] as String,
        name: json['name'] as String,
        description: json['description'] as String?,
        prizeDescription: json['prizeDescription'] as String? ?? json['name'] as String,
        emoji: json['emoji'] as String?,
        imageUrl: json['imageUrl'] as String?,
        ticketCostPoints: (json['ticketCostPoints'] as num?)?.toInt() ?? 0,
        startAt: DateTime.parse(json['startAt'] as String),
        endAt: DateTime.parse(json['endAt'] as String),
        participants: (json['participants'] as num?)?.toInt() ?? 0,
        totalTickets: (json['totalTickets'] as num?)?.toInt() ?? 0,
        maxTickets: (json['maxTickets'] as num?)?.toInt(),
        myTickets: (json['myTickets'] as num?)?.toInt() ?? 0,
        featured: json['featured'] as bool? ?? false,
      );

  LuckyDraw copyWith({int? myTickets, int? totalTickets, int? participants}) => LuckyDraw(
        id: id,
        name: name,
        description: description,
        prizeDescription: prizeDescription,
        emoji: emoji,
        imageUrl: imageUrl,
        ticketCostPoints: ticketCostPoints,
        startAt: startAt,
        endAt: endAt,
        participants: participants ?? this.participants,
        totalTickets: totalTickets ?? this.totalTickets,
        maxTickets: maxTickets,
        myTickets: myTickets ?? this.myTickets,
        featured: featured,
      );
}

class LuckyDrawList {
  const LuckyDrawList({
    required this.draws,
    required this.walletBalance,
    required this.ticketsBoughtToday,
    required this.maxTicketsPerDay,
  });

  final List<LuckyDraw> draws;
  final int walletBalance;
  final int ticketsBoughtToday;
  final int maxTicketsPerDay;

  factory LuckyDrawList.fromJson(Map<String, dynamic> json) => LuckyDrawList(
        draws: (json['draws'] as List? ?? const [])
            .whereType<Map>()
            .map((row) => LuckyDraw.fromJson(Map<String, dynamic>.from(row)))
            .toList(),
        walletBalance: (json['walletBalance'] as num?)?.toInt() ?? 0,
        ticketsBoughtToday: (json['ticketsBoughtToday'] as num?)?.toInt() ?? 0,
        maxTicketsPerDay: (json['maxTicketsPerDay'] as num?)?.toInt() ?? 0,
      );
}

class BuyTicketResult {
  const BuyTicketResult({
    required this.drawId,
    required this.ticketNumbers,
    required this.myTickets,
    required this.totalTickets,
    required this.pointsSpent,
    required this.walletBalance,
    required this.duplicate,
  });

  final String drawId;
  final List<String> ticketNumbers;
  final int myTickets;
  final int totalTickets;
  final int pointsSpent;
  final int walletBalance;
  final bool duplicate;

  factory BuyTicketResult.fromJson(Map<String, dynamic> json) => BuyTicketResult(
        drawId: json['drawId'] as String,
        ticketNumbers: (json['ticketNumbers'] as List? ?? const []).map((e) => e.toString()).toList(),
        myTickets: (json['myTickets'] as num?)?.toInt() ?? 0,
        totalTickets: (json['totalTickets'] as num?)?.toInt() ?? 0,
        pointsSpent: (json['pointsSpent'] as num?)?.toInt() ?? 0,
        walletBalance: (json['walletBalance'] as num?)?.toInt() ?? 0,
        duplicate: json['duplicate'] as bool? ?? false,
      );
}
