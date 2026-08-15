import 'package:flutter/material.dart';

import '../models/current_user.dart';
import '../services/steps_controller.dart';
import '../theme.dart';
import '../widgets/progress_circle.dart';

String _fmt(int value) {
  final digits = value.toString();
  final buffer = StringBuffer();
  for (var i = 0; i < digits.length; i++) {
    if (i > 0 && (digits.length - i) % 3 == 0) {
      buffer.write(',');
    }
    buffer.write(digits[i]);
  }
  return buffer.toString();
}

class HomeScreen extends StatelessWidget {
  const HomeScreen({
    super.key,
    required this.user,
    required this.steps,
    required this.onComingSoon,
    required this.onOpenLuckyDraw,
  });

  final CurrentUser? user;
  final StepsController steps;
  final void Function(String title) onComingSoon;
  final VoidCallback onOpenLuckyDraw;

  static const _mockTemp = 26;
  static const _mockAqi = 45;
  static const _goalReward = 50;

  String get _greeting {
    final hour = DateTime.now().hour;
    if (hour < 12) {
      return 'Good Morning';
    }
    if (hour < 17) {
      return 'Good Afternoon';
    }
    return 'Good Evening';
  }

  @override
  Widget build(BuildContext context) {
    final name = user?.displayName?.trim().isNotEmpty == true ? user!.displayName! : 'Walker';
    final city = user?.city?.trim().isNotEmpty == true ? user!.city! : 'your city';
    final points = user?.walletBalance ?? 0;
    final goal = steps.goal;
    final milestones = [0.25, 0.5, 0.75, 1.0].map((f) => (goal * f).round()).toList();

    return DecoratedBox(
      decoration: const BoxDecoration(gradient: MovoqTheme.pageGradient),
      child: RefreshIndicator(
        onRefresh: steps.refresh,
        child: ListView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.only(bottom: 24),
          children: [
            _Header(
              greeting: _greeting,
              name: name,
              points: points,
              city: city,
              onPointsTap: () => onComingSoon('Wallet'),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 16, 20, 0),
              child: Column(
                children: [
                _StepsStatusCard(steps: steps),
                _LuckyDrawBanner(onTap: onOpenLuckyDraw),
                const SizedBox(height: 16),
                _DailyGoalCard(
                  steps: steps.todaySteps,
                  goal: goal,
                  percent: steps.progressPercent,
                  milestones: milestones,
                  syncing: steps.syncing,
                ),
                const SizedBox(height: 16),
                Row(
                  children: [
                    Expanded(
                      child: _ActionCard(
                        title: 'Spin Wheel',
                        subtitle: 'Win up to 50 pts',
                        icon: Icons.bolt_rounded,
                        colors: const [Color(0xFFFB923C), Color(0xFFEF4444)],
                        badge: 'Ad',
                        onTap: () => onComingSoon('Spin Wheel'),
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: _ActionCard(
                        title: 'Mystery Box',
                        subtitle: 'Daily surprise',
                        icon: Icons.card_giftcard_rounded,
                        colors: const [MovoqTheme.purple, MovoqTheme.pink],
                        onTap: () => onComingSoon('Mystery Box'),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 16),
                _StreakCard(
                  streak: steps.streak,
                  onWatchAd: () => onComingSoon('Streak repair'),
                ),
                const SizedBox(height: 16),
                _TeamTeaser(onTap: () => onComingSoon('Teams')),
                const SizedBox(height: 16),
                const _HealthTipCard(),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Surfaces the reasons the step count could be wrong: no motion permission, no sensor at all,
/// or a day the server refused as implausible.
class _StepsStatusCard extends StatelessWidget {
  const _StepsStatusCard({required this.steps});

  final StepsController steps;

  @override
  Widget build(BuildContext context) {
    if (steps.permission == StepsPermission.denied) {
      return _Banner(
        icon: Icons.directions_walk_rounded,
        color: const Color(0xFFF59E0B),
        title: 'Step tracking is off',
        message: 'MOVOQ needs motion access to count your steps.',
        actionLabel: 'Allow',
        onAction: steps.requestPermission,
      );
    }

    if (steps.today?.rejected == true) {
      return _Banner(
        icon: Icons.warning_amber_rounded,
        color: const Color(0xFFEF4444),
        title: 'Today could not be verified',
        message: 'This day looks implausible, so it will not earn points.',
      );
    }

    if (steps.sensorUnavailable && steps.canSimulate) {
      return _Banner(
        icon: Icons.developer_mode_rounded,
        color: const Color(0xFF6366F1),
        title: 'No pedometer on this device',
        message: 'Add steps by hand to exercise the sync while developing.',
        actionLabel: '+1,000',
        onAction: () => steps.addSimulatedSteps(1000),
      );
    }

    return const SizedBox.shrink();
  }
}

class _Banner extends StatelessWidget {
  const _Banner({
    required this.icon,
    required this.color,
    required this.title,
    required this.message,
    this.actionLabel,
    this.onAction,
  });

  final IconData icon;
  final Color color;
  final String title;
  final String message;
  final String? actionLabel;
  final VoidCallback? onAction;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.08),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: color.withValues(alpha: 0.35)),
      ),
      child: Row(
        children: [
          Icon(icon, color: color),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(title, style: const TextStyle(fontWeight: FontWeight.w800)),
                Text(message, style: TextStyle(color: Colors.grey.shade700, fontSize: 13)),
              ],
            ),
          ),
          if (actionLabel != null) ...[
            const SizedBox(width: 8),
            FilledButton(
              onPressed: onAction,
              style: FilledButton.styleFrom(backgroundColor: color),
              child: Text(actionLabel!),
            ),
          ],
        ],
      ),
    );
  }
}

class _Header extends StatelessWidget {
  const _Header({
    required this.greeting,
    required this.name,
    required this.points,
    required this.city,
    required this.onPointsTap,
  });

  final String greeting;
  final String name;
  final int points;
  final String city;
  final VoidCallback onPointsTap;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: const BoxDecoration(
        color: Color(0xCCFFFFFF),
        border: Border(bottom: BorderSide(color: Color(0xFFE5E7EB))),
      ),
      child: SafeArea(
        bottom: false,
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 12, 20, 16),
          child: Column(
            children: [
              Row(
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text('$greeting 👋', style: TextStyle(color: Colors.grey.shade600)),
                        Text(name, style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w900)),
                      ],
                    ),
                  ),
                  InkWell(
                    onTap: onPointsTap,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.end,
                      children: [
                        Text('Points', style: TextStyle(fontSize: 12, color: Colors.grey.shade500)),
                        Text('🪙 ${_fmt(points)}', style: const TextStyle(fontSize: 20, fontWeight: FontWeight.w900, color: Color(0xFF0891B2))),
                      ],
                    ),
                  ),
                  const SizedBox(width: 16),
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.end,
                    children: [
                      const Text('${HomeScreen._mockTemp}°C', style: TextStyle(fontSize: 22, fontWeight: FontWeight.w900)),
                      Text('AQI: ${HomeScreen._mockAqi}', style: TextStyle(fontSize: 12, color: Colors.grey.shade600)),
                    ],
                  ),
                  const SizedBox(width: 8),
                  Icon(Icons.cloud, color: Colors.blue.shade300, size: 32),
                ],
              ),
              const SizedBox(height: 12),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: const Color(0x1A3B82F6),
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(color: const Color(0xFFBFDBFE)),
                ),
                child: Row(
                  children: [
                    const Icon(Icons.auto_awesome, size: 16, color: Color(0xFF2563EB)),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Text(
                        HomeScreen._mockAqi > 150
                            ? 'Air quality is poor in $city today — try indoor steps'
                            : 'Great weather in $city for a 5K outdoor walk',
                        style: const TextStyle(fontSize: 13),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _LuckyDrawBanner extends StatelessWidget {
  const _LuckyDrawBanner({required this.onTap});

  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(24),
        child: Ink(
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(24),
            gradient: const LinearGradient(colors: [Color(0xFF3B82F6), Color(0xFF06B6D4), Color(0xFF14B8A6)]),
            boxShadow: const [BoxShadow(color: Color(0x403B82F6), blurRadius: 18, offset: Offset(0, 8))],
          ),
          child: ClipRRect(
            borderRadius: BorderRadius.circular(24),
            child: Stack(
              children: [
                const Positioned(top: -40, right: -20, child: _GlowCircle(size: 140)),
                const Positioned(bottom: -30, left: -20, child: _GlowCircle(size: 100)),
                Padding(
                  padding: const EdgeInsets.all(20),
                  child: Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(color: Colors.white.withValues(alpha: 0.25), borderRadius: BorderRadius.circular(16)),
                        child: const Icon(Icons.confirmation_number_rounded, color: Colors.white, size: 32),
                      ),
                      const SizedBox(width: 12),
                      const Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text('✦ GRAND PRIZE', style: TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.w700)),
                            Text('Lucky Draw', style: TextStyle(color: Colors.white, fontSize: 22, fontWeight: FontWeight.w900)),
                            Text('Win iPhone, Cash & More!', style: TextStyle(color: Colors.white70)),
                          ],
                        ),
                      ),
                      const Column(
                        children: [
                          Icon(Icons.auto_awesome, color: Colors.white, size: 28),
                          Text('Tap to Enter', style: TextStyle(color: Colors.white, fontSize: 11)),
                        ],
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _DailyGoalCard extends StatelessWidget {
  const _DailyGoalCard({
    required this.steps,
    required this.goal,
    required this.percent,
    required this.milestones,
    required this.syncing,
  });

  final int steps;
  final int goal;
  final double percent;
  final List<int> milestones;
  final bool syncing;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(28),
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [MovoqTheme.purple, MovoqTheme.pink, Color(0xFFF97316)],
        ),
      ),
      child: Column(
        children: [
          Row(
            children: [
              const Icon(Icons.flag_rounded, color: Colors.white),
              const SizedBox(width: 8),
              const Text('Daily Goal', style: TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.w700)),
              if (syncing) ...[
                const SizedBox(width: 8),
                const SizedBox(
                  width: 14,
                  height: 14,
                  child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white70),
                ),
              ],
              const Spacer(),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                decoration: BoxDecoration(color: Colors.white24, borderRadius: BorderRadius.circular(20)),
                child: Text('${_fmt(goal)} steps', style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w600)),
              ),
            ],
          ),
          const SizedBox(height: 16),
          ProgressCircle(
            percentage: percent,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(_fmt(steps), style: const TextStyle(color: Colors.white, fontSize: 36, fontWeight: FontWeight.w900)),
                const Text('steps today', style: TextStyle(color: Colors.white70)),
                Text('${percent.round()}%', style: const TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.w700)),
              ],
            ),
          ),
          const SizedBox(height: 16),
          Row(
            children: [
              for (final mark in milestones)
                Expanded(
                  child: Column(
                    children: [
                      Container(
                        height: 6,
                        margin: const EdgeInsets.symmetric(horizontal: 4),
                        decoration: BoxDecoration(
                          color: steps >= mark ? Colors.white : Colors.white30,
                          borderRadius: BorderRadius.circular(8),
                        ),
                      ),
                      const SizedBox(height: 6),
                      Text(_fmt(mark), style: TextStyle(color: steps >= mark ? Colors.white : Colors.white60, fontSize: 11)),
                    ],
                  ),
                ),
            ],
          ),
          const SizedBox(height: 16),
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(color: Colors.white24, borderRadius: BorderRadius.circular(16)),
            child: const Row(
              children: [
                CircleAvatar(backgroundColor: Color(0xFFFACC15), child: Icon(Icons.card_giftcard, color: Color(0xFF713F12))),
                SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('Complete goal to earn', style: TextStyle(color: Colors.white70, fontSize: 13)),
                      Text('${HomeScreen._goalReward} points + surprise', style: TextStyle(color: Colors.white, fontWeight: FontWeight.w700)),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _ActionCard extends StatelessWidget {
  const _ActionCard({
    required this.title,
    required this.subtitle,
    required this.icon,
    required this.colors,
    required this.onTap,
    this.badge,
  });

  final String title;
  final String subtitle;
  final IconData icon;
  final List<Color> colors;
  final VoidCallback onTap;
  final String? badge;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(24),
        child: Ink(
          height: 150,
          padding: const EdgeInsets.all(18),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(24),
            gradient: LinearGradient(begin: Alignment.topLeft, end: Alignment.bottomRight, colors: colors),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Icon(icon, color: Colors.white, size: 36),
                  const Spacer(),
                  if (badge != null)
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                      decoration: BoxDecoration(color: Colors.white24, borderRadius: BorderRadius.circular(12)),
                      child: Row(
                        children: [
                          const Icon(Icons.lock_rounded, color: Colors.white, size: 12),
                          const SizedBox(width: 4),
                          Text(badge!, style: const TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.w700)),
                        ],
                      ),
                    ),
                ],
              ),
              const Spacer(),
              Text(title, style: const TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.w800)),
              Text(subtitle, style: const TextStyle(color: Colors.white70)),
            ],
          ),
        ),
      ),
    );
  }
}

class _StreakCard extends StatelessWidget {
  const _StreakCard({required this.streak, required this.onWatchAd});

  final int streak;
  final VoidCallback onWatchAd;

  static const _days = ['M', 'T', 'W', 'T', 'F', 'S', 'S'];

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(28),
        gradient: const LinearGradient(colors: [Color(0xFFF97316), Color(0xFFEF4444)]),
      ),
      child: Column(
        children: [
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(10),
                decoration: const BoxDecoration(color: Colors.white24, shape: BoxShape.circle),
                child: const Icon(Icons.local_fire_department, color: Colors.white, size: 32),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('$streak Day Streak', style: const TextStyle(color: Colors.white, fontSize: 26, fontWeight: FontWeight.w900)),
                    const Text('Keep it going!', style: TextStyle(color: Colors.white70)),
                  ],
                ),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                decoration: BoxDecoration(color: Colors.white24, borderRadius: BorderRadius.circular(12)),
                child: const Text('+5 pts/day', style: TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.w700)),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Row(
            children: [
              for (var i = 0; i < _days.length; i++)
                Expanded(
                  child: Column(
                    children: [
                      Container(
                        height: 36,
                        margin: const EdgeInsets.symmetric(horizontal: 3),
                        decoration: BoxDecoration(
                          color: i < streak ? Colors.white : (i == streak ? Colors.white30 : Colors.white12),
                          borderRadius: BorderRadius.circular(12),
                          border: i == streak ? Border.all(color: Colors.white60, width: 2) : null,
                        ),
                        child: Center(
                          child: i < streak
                              ? const Icon(Icons.local_fire_department, color: Color(0xFFF97316), size: 18)
                              : i == streak
                                  ? const Text('💔')
                                  : const Text('·', style: TextStyle(color: Colors.white38)),
                        ),
                      ),
                      const SizedBox(height: 4),
                      Text(_days[i], style: TextStyle(color: i <= streak ? Colors.white : Colors.white38, fontSize: 11, fontWeight: FontWeight.w700)),
                    ],
                  ),
                ),
            ],
          ),
          const SizedBox(height: 16),
          SizedBox(
            width: double.infinity,
            child: FilledButton(
              onPressed: onWatchAd,
              style: FilledButton.styleFrom(backgroundColor: Colors.white, foregroundColor: const Color(0xFFF97316)),
              child: const Text('Watch Ad to Keep Streak (+5 pts)'),
            ),
          ),
        ],
      ),
    );
  }
}

class _GlowCircle extends StatelessWidget {
  const _GlowCircle({required this.size});

  final double size;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: size,
      height: size,
      decoration: BoxDecoration(color: Colors.white.withValues(alpha: 0.1), shape: BoxShape.circle),
    );
  }
}

class _TeamTeaser extends StatelessWidget {
  const _TeamTeaser({required this.onTap});

  final VoidCallback onTap;

  static const _members = [
    ('FK', Color(0xFFF472B6)),
    ('HM', Color(0xFF60A5FA)),
    ('SR', Color(0xFFFBBF24)),
    ('BA', Color(0xFF34D399)),
  ];

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(24),
        child: Ink(
          padding: const EdgeInsets.all(20),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(24),
            gradient: const LinearGradient(
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
              colors: [Color(0xFF14B8A6), Color(0xFF10B981), Color(0xFF16A34A)],
            ),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(8),
                    decoration: BoxDecoration(color: Colors.white24, borderRadius: BorderRadius.circular(12)),
                    child: const Icon(Icons.groups_rounded, color: Colors.white),
                  ),
                  const SizedBox(width: 10),
                  const Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text('Your Team', style: TextStyle(color: Colors.white70, fontSize: 12)),
                        Text('Lahore Walkers', style: TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.w900)),
                      ],
                    ),
                  ),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                    decoration: BoxDecoration(color: Colors.white24, borderRadius: BorderRadius.circular(20)),
                    child: const Row(
                      children: [
                        Text('View Team', style: TextStyle(color: Colors.white, fontWeight: FontWeight.w700, fontSize: 12)),
                        Icon(Icons.chevron_right, color: Colors.white, size: 16),
                      ],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 12),
              const Row(
                children: [
                  Expanded(child: Text('Coach: Usman Bhai', style: TextStyle(color: Colors.white))),
                  Text('Rank #3', style: TextStyle(color: Color(0xFFFDE68A), fontWeight: FontWeight.w700)),
                ],
              ),
              const SizedBox(height: 12),
              const Text('Team is 65% towards today\'s goal', style: TextStyle(color: Colors.white70, fontSize: 13)),
              const SizedBox(height: 8),
              ClipRRect(
                borderRadius: BorderRadius.circular(8),
                child: const LinearProgressIndicator(
                  value: 0.65,
                  minHeight: 8,
                  backgroundColor: Colors.white24,
                  color: Colors.white,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                '${_fmt(142800)} steps combined  ·  Goal: ${_fmt(220000)}',
                style: const TextStyle(color: Colors.white70, fontSize: 12),
              ),
              const SizedBox(height: 14),
              Row(
                children: [
                  SizedBox(
                    width: 118,
                    height: 28,
                    child: Stack(
                      children: [
                        for (var i = 0; i < _members.length; i++)
                          Positioned(
                            left: i * 18,
                            child: CircleAvatar(
                              radius: 14,
                              backgroundColor: _members[i].$2,
                              child: Text(_members[i].$1, style: const TextStyle(color: Colors.white, fontSize: 9, fontWeight: FontWeight.w800)),
                            ),
                          ),
                        const Positioned(
                          left: 72,
                          child: CircleAvatar(
                            radius: 14,
                            backgroundColor: Colors.white24,
                            child: Text('+42', style: TextStyle(color: Colors.white, fontSize: 9, fontWeight: FontWeight.w800)),
                          ),
                        ),
                      ],
                    ),
                  ),
                  const Spacer(),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                    decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(20)),
                    child: const Text('Join Challenge', style: TextStyle(color: Color(0xFF0F766E), fontWeight: FontWeight.w800, fontSize: 12)),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _HealthTipCard extends StatelessWidget {
  const _HealthTipCard();

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(24),
        border: Border.all(color: const Color(0xFFF3F4F6)),
      ),
      child: const Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          CircleAvatar(backgroundColor: Color(0xFFDBEAFE), child: Icon(Icons.air, color: Color(0xFF2563EB))),
          SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Health Tip', style: TextStyle(fontWeight: FontWeight.w800)),
                SizedBox(height: 4),
                Text(
                  'Walking after meals helps regulate blood sugar levels and improves digestion. Try a 10-minute walk!',
                  style: TextStyle(color: Colors.black54, height: 1.4),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
