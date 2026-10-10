class MarketplaceReward {
  const MarketplaceReward({
    required this.id,
    required this.name,
    required this.sponsorName,
    required this.pointsCost,
    this.description,
    this.stockRemaining,
    this.validUntil,
    this.termsAndConditions,
  });

  final String id;
  final String name;
  final String? description;
  final String sponsorName;
  final int pointsCost;
  final int? stockRemaining;
  final DateTime? validUntil;
  final String? termsAndConditions;

  factory MarketplaceReward.fromJson(Map<String, dynamic> json) => MarketplaceReward(
        id: json['id'] as String,
        name: json['name'] as String,
        description: json['description'] as String?,
        sponsorName: json['sponsorName'] as String? ?? 'Sponsor',
        pointsCost: (json['pointsCost'] as num?)?.toInt() ?? 0,
        stockRemaining: (json['stockRemaining'] as num?)?.toInt(),
        validUntil: json['validUntil'] == null ? null : DateTime.tryParse(json['validUntil'] as String),
        termsAndConditions: json['termsAndConditions'] as String?,
      );
}

class MyRedemption {
  const MyRedemption({
    required this.id,
    required this.rewardName,
    required this.sponsorName,
    required this.pointsCost,
    required this.status,
    required this.requestedAt,
    this.assignedCode,
  });

  final String id;
  final String rewardName;
  final String sponsorName;
  final int pointsCost;
  final String status;
  final String? assignedCode;
  final DateTime requestedAt;

  factory MyRedemption.fromJson(Map<String, dynamic> json) => MyRedemption(
        id: json['id'] as String,
        rewardName: json['rewardName'] as String,
        sponsorName: json['sponsorName'] as String? ?? 'Sponsor',
        pointsCost: (json['pointsCost'] as num?)?.toInt() ?? 0,
        status: json['status'] as String? ?? 'Confirmed',
        assignedCode: json['assignedCode'] as String?,
        requestedAt: DateTime.parse(json['requestedAt'] as String),
      );
}

class MarketplaceList {
  const MarketplaceList({
    required this.rewards,
    required this.myRedemptions,
    required this.walletBalance,
  });

  final List<MarketplaceReward> rewards;
  final List<MyRedemption> myRedemptions;
  final int walletBalance;

  factory MarketplaceList.fromJson(Map<String, dynamic> json) => MarketplaceList(
        rewards: (json['rewards'] as List? ?? const [])
            .whereType<Map>()
            .map((row) => MarketplaceReward.fromJson(Map<String, dynamic>.from(row)))
            .toList(),
        myRedemptions: (json['myRedemptions'] as List? ?? const [])
            .whereType<Map>()
            .map((row) => MyRedemption.fromJson(Map<String, dynamic>.from(row)))
            .toList(),
        walletBalance: (json['walletBalance'] as num?)?.toInt() ?? 0,
      );
}

class RedeemResult {
  const RedeemResult({
    required this.redemptionId,
    required this.rewardName,
    required this.pointsSpent,
    required this.walletBalance,
    required this.status,
    required this.duplicate,
    this.assignedCode,
  });

  final String redemptionId;
  final String rewardName;
  final String? assignedCode;
  final int pointsSpent;
  final int walletBalance;
  final String status;
  final bool duplicate;

  factory RedeemResult.fromJson(Map<String, dynamic> json) => RedeemResult(
        redemptionId: json['redemptionId'] as String,
        rewardName: json['rewardName'] as String,
        assignedCode: json['assignedCode'] as String?,
        pointsSpent: (json['pointsSpent'] as num?)?.toInt() ?? 0,
        walletBalance: (json['walletBalance'] as num?)?.toInt() ?? 0,
        status: json['status'] as String? ?? 'Confirmed',
        duplicate: json['duplicate'] as bool? ?? false,
      );
}
