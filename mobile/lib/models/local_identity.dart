class LocalIdentity {
  const LocalIdentity({
    required this.uid,
    required this.email,
    required this.name,
  });

  final String uid;
  final String email;
  final String name;

  static const defaults = LocalIdentity(
    uid: 'local-dev-user',
    email: 'dev@localhost',
    name: 'Local Walker',
  );

  Map<String, String> toJson() => {
        'uid': uid,
        'email': email,
        'name': name,
      };

  factory LocalIdentity.fromJson(Map<String, dynamic> json) => LocalIdentity(
        uid: json['uid'] as String? ?? defaults.uid,
        email: json['email'] as String? ?? defaults.email,
        name: json['name'] as String? ?? defaults.name,
      );
}

class DevUser {
  const DevUser({
    required this.id,
    required this.email,
    required this.providerUserId,
    required this.walletBalance,
    required this.profileComplete,
    required this.status,
    this.displayName,
    this.city,
    this.state,
    this.country,
    this.gender,
    this.dailyTargetSteps,
  });

  final String id;
  final String email;
  final String providerUserId;
  final int walletBalance;
  final bool profileComplete;
  final String status;
  final String? displayName;
  final String? city;
  final String? state;
  final String? country;
  final String? gender;
  final int? dailyTargetSteps;

  LocalIdentity get identity => LocalIdentity(
        uid: providerUserId,
        email: email,
        name: displayName ?? email,
      );

  factory DevUser.fromJson(Map<String, dynamic> json) => DevUser(
        id: json['id'].toString(),
        email: json['email'] as String,
        providerUserId: json['providerUserId'] as String? ?? json['id'].toString(),
        walletBalance: (json['walletBalance'] as num?)?.toInt() ?? 0,
        profileComplete: json['profileComplete'] as bool? ?? false,
        status: json['status'] as String? ?? 'Active',
        displayName: json['displayName'] as String?,
        city: json['city'] as String?,
        state: json['state'] as String?,
        country: json['country'] as String?,
        gender: json['gender'] as String?,
        dailyTargetSteps: (json['dailyTargetSteps'] as num?)?.toInt(),
      );
}
