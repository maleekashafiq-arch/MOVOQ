import 'package:flutter/material.dart';

import '../models/current_user.dart';
import '../models/daily_steps.dart';
import '../services/outdoor_conditions.dart';
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

IconData _weatherIcon(OutdoorConditions outdoor) {
  if (outdoor.status == OutdoorStatus.locationOff) {
    return Icons.location_off_outlined;
  }
  final code = outdoor.weatherCode;
  if (code == null) {
    return Icons.cloud_outlined;
  }
  if (code == 0) {
    return Icons.wb_sunny_rounded;
  }
  if (code <= 3) {
    return Icons.wb_cloudy_rounded;
  }
  if (code == 45 || code == 48) {
    return Icons.blur_on;
  }
  if (code <= 67) {
    return Icons.water_drop_rounded;
  }
  if (code <= 77) {
    return Icons.ac_unit_rounded;
  }
  if (code <= 82) {
    return Icons.grain;
  }
  if (code >= 95) {
    return Icons.thunderstorm_rounded;
  }
  return Icons.cloud_rounded;
}

class HomeScreen extends StatelessWidget {
  const HomeScreen({
    super.key,
    required this.user,
    required this.steps,
    required this.outdoor,
    required this.onComingSoon,
  });

  final CurrentUser? user;
  final StepsController steps;
  final OutdoorConditions outdoor;
  final void Function(String title) onComingSoon;

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
        onRefresh: () => Future.wait([steps.refresh(), outdoor.refresh()]),
        child: ListView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.only(bottom: 24),
          children: [
            _Header(
              greeting: _greeting,
              name: name,
              points: points,
              city: city,
              outdoor: outdoor,
              onPointsTap: () => onComingSoon('Wallet'),
              onEnableLocation: outdoor.enableLocation,
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 16, 20, 0),
              child: Column(
                children: [
                _StepsStatusCard(steps: steps),
                const SizedBox(height: 16),
                _DailyGoalCard(
                  steps: steps.todaySteps,
                  goal: goal,
                  percent: steps.progressPercent,
                  milestones: milestones,
                  syncing: steps.syncing,
                ),
                if (steps.history != null && steps.history!.days.isNotEmpty) ...[
                  const SizedBox(height: 16),
                  _WeekHistoryCard(history: steps.history!),
                ],
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
              // The app theme gives FilledButton an infinite min width; inside a Row that
              // must be overridden with a finite size or layout throws.
              style: FilledButton.styleFrom(
                backgroundColor: color,
                minimumSize: const Size(0, 44),
                padding: const EdgeInsets.symmetric(horizontal: 14),
              ),
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
    required this.outdoor,
    required this.onPointsTap,
    required this.onEnableLocation,
  });

  final String greeting;
  final String name;
  final int points;
  final String city;
  final OutdoorConditions outdoor;
  final VoidCallback onPointsTap;
  final VoidCallback onEnableLocation;

  String get _conditionsLine {
    if (outdoor.status == OutdoorStatus.locationOff) {
      return outdoor.message ?? 'Turn on location to see weather and air quality for your walk';
    }
    if (outdoor.status == OutdoorStatus.loading && outdoor.temperatureC == null) {
      return 'Checking weather for your location…';
    }
    if (outdoor.temperatureC == null) {
      return outdoor.message ?? 'Weather is unavailable for this location';
    }
    final aqi = outdoor.aqi;
    if (aqi != null && aqi > 150) {
      return 'Air quality is poor in $city today — try indoor steps';
    }
    if (aqi != null && aqi > 100) {
      return 'Air quality is moderate in $city — a shorter outdoor walk is kinder today';
    }
    return 'Great weather in $city for a 5K outdoor walk';
  }

  String get _temperatureLabel {
    final temperature = outdoor.temperatureC;
    if (temperature == null) {
      return outdoor.status == OutdoorStatus.loading ? '…' : '—';
    }
    return '${temperature.round()}°C';
  }

  String get _aqiLabel {
    if (outdoor.status == OutdoorStatus.locationOff) {
      return 'Location off';
    }
    final aqi = outdoor.aqi;
    if (aqi == null) {
      return outdoor.status == OutdoorStatus.loading ? 'AQI' : 'AQI —';
    }
    return 'AQI: $aqi';
  }

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
                  InkWell(
                    onTap: outdoor.status == OutdoorStatus.ready ? null : onEnableLocation,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.end,
                      children: [
                        Text(_temperatureLabel, style: const TextStyle(fontSize: 22, fontWeight: FontWeight.w900)),
                        Text(_aqiLabel, style: TextStyle(fontSize: 12, color: Colors.grey.shade600)),
                      ],
                    ),
                  ),
                  const SizedBox(width: 8),
                  Icon(_weatherIcon(outdoor), color: Colors.blue.shade300, size: 32),
                ],
              ),
              const SizedBox(height: 12),
              InkWell(
                onTap: outdoor.status == OutdoorStatus.locationOff ? onEnableLocation : null,
                borderRadius: BorderRadius.circular(16),
                child: Container(
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
                      Expanded(child: Text(_conditionsLine, style: const TextStyle(fontSize: 13))),
                    ],
                  ),
                ),
              ),
            ],
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

class _WeekHistoryCard extends StatelessWidget {
  const _WeekHistoryCard({required this.history});

  final StepsHistory history;

  @override
  Widget build(BuildContext context) {
    final days = history.days;
    final maxSteps = days.fold<int>(0, (best, day) => day.steps > best ? day.steps : best);

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(24),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Last ${days.length} days', style: const TextStyle(fontWeight: FontWeight.w800)),
          const SizedBox(height: 4),
          Text(
            '${_fmt(history.totalSteps)} steps · ${history.totalPointsAwarded} pts',
            style: TextStyle(color: Colors.grey.shade600, fontSize: 13),
          ),
          const SizedBox(height: 12),
          Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              for (final day in days)
                Expanded(
                  child: Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 3),
                    child: Column(
                      children: [
                        Container(
                          height: maxSteps == 0 ? 8 : 8 + (48 * day.steps / maxSteps),
                          decoration: BoxDecoration(
                            color: day.goalCompleted ? MovoqTheme.indigo : const Color(0xFFDBEAFE),
                            borderRadius: BorderRadius.circular(8),
                          ),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          '${day.stepDate.day}',
                          style: TextStyle(fontSize: 10, color: Colors.grey.shade600),
                        ),
                      ],
                    ),
                  ),
                ),
            ],
          ),
        ],
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
