class CurrentUser {
  const CurrentUser({
    required this.id,
    required this.email,
    required this.emailVerified,
    required this.phoneVerified,
    required this.profileComplete,
    required this.status,
    this.phoneNumber,
    this.displayName,
    this.country,
    this.state,
    this.city,
    this.gender,
    this.walletBalance = 0,
    this.dailyTargetSteps,
  });

  final String id;
  final String email;
  final bool emailVerified;
  final bool phoneVerified;
  final bool profileComplete;
  final String status;
  final String? phoneNumber;
  final String? displayName;
  final String? country;
  final String? state;
  final String? city;
  final String? gender;
  final int walletBalance;
  final int? dailyTargetSteps;

  CurrentUser copyWith({int? walletBalance, int? dailyTargetSteps}) => CurrentUser(
        id: id,
        email: email,
        emailVerified: emailVerified,
        phoneVerified: phoneVerified,
        profileComplete: profileComplete,
        status: status,
        phoneNumber: phoneNumber,
        displayName: displayName,
        country: country,
        state: state,
        city: city,
        gender: gender,
        walletBalance: walletBalance ?? this.walletBalance,
        dailyTargetSteps: dailyTargetSteps ?? this.dailyTargetSteps,
      );

  static CurrentUser localPreview() => const CurrentUser(
        id: 'local-dev-user',
        email: 'dev@localhost',
        emailVerified: true,
        phoneVerified: false,
        profileComplete: true,
        status: 'Active',
        displayName: 'Local Walker',
        country: 'PK',
        state: 'Punjab',
        city: 'Lahore',
        walletBalance: 1245,
        dailyTargetSteps: 6000,
      );

  factory CurrentUser.fromJson(Map<String, dynamic> json) {
    final profile = json['profile'] as Map<String, dynamic>?;
    return CurrentUser(
      id: json['id'] as String,
      email: json['email'] as String,
      emailVerified: json['emailVerified'] as bool? ?? false,
      phoneVerified: json['phoneVerified'] as bool? ?? false,
      profileComplete: json['profileComplete'] as bool? ?? false,
      status: json['status'] as String? ?? 'Active',
      phoneNumber: json['phoneNumber'] as String?,
      displayName: profile?['displayName'] as String?,
      country: profile?['country'] as String?,
      state: profile?['state'] as String?,
      city: profile?['city'] as String?,
      gender: profile?['gender'] as String?,
      walletBalance: (json['walletBalance'] as num?)?.toInt() ?? 0,
      dailyTargetSteps: (json['dailyTargetSteps'] as num?)?.toInt(),
    );
  }
}
