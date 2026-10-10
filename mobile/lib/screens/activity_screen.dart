import 'package:flutter/material.dart';

import '../models/daily_steps.dart';
import '../services/api_client.dart';
import '../services/session_controller.dart';
import '../services/steps_controller.dart';
import '../widgets/progress_circle.dart';

class ActivityScreen extends StatefulWidget {
  const ActivityScreen({
    super.key,
    required this.session,
    required this.steps,
    required this.onBack,
  });

  final SessionController session;
  final StepsController steps;
  final VoidCallback onBack;

  @override
  State<ActivityScreen> createState() => _ActivityScreenState();
}

class _ActivityScreenState extends State<ActivityScreen> {
  static const _windowDays = 30;

  StepsHistory? _history;
  String? _error;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final history = await widget.session.api.getStepsHistory(
        await widget.session.apiToken(),
        days: _windowDays,
      );
      if (mounted) {
        setState(() {
          _history = history;
          _loading = false;
        });
      }
    } on ApiException catch (e) {
      if (mounted) {
        setState(() {
          _error = e.message;
          _loading = false;
        });
      }
    } catch (_) {
      if (mounted) {
        setState(() {
          _error = 'Could not load your activity.';
          _loading = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final steps = widget.steps;
    final history = _history;
    final days = [...?history?.days.reversed];
    final visible = days.where((day) => day.steps > 0 || day.pointsAwarded > 0 || _isToday(day.stepDate)).toList();
    final recent = days.take(7).toList().reversed.toList();

    return DecoratedBox(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFFF9FAFB), Color(0xFFF3F4F6)],
        ),
      ),
      child: RefreshIndicator(
        onRefresh: () async {
          await widget.steps.refresh();
          await _load();
        },
        child: ListView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.only(bottom: 24),
          children: [
            _Header(
              totalSteps: history?.totalSteps ?? steps.history?.totalSteps ?? 0,
              streak: history?.currentStreak ?? steps.streak,
              onBack: widget.onBack,
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(24, 24, 24, 0),
              child: _TodayCard(steps: steps),
            ),
            if (recent.isNotEmpty)
              Padding(
                padding: const EdgeInsets.fromLTRB(24, 16, 24, 0),
                child: _StreakCard(streak: history?.currentStreak ?? steps.streak, days: recent),
              ),
            Padding(
              padding: const EdgeInsets.fromLTRB(24, 24, 24, 12),
              child: const Text('Step history', style: TextStyle(fontSize: 20, fontWeight: FontWeight.w900, color: Color(0xFF1F2937))),
            ),
            if (_loading && history == null)
              const Padding(
                padding: EdgeInsets.symmetric(vertical: 32),
                child: Center(child: CircularProgressIndicator()),
              ),
            if (_error != null)
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 24),
                child: Column(
                  children: [
                    Text(_error!, style: const TextStyle(color: Color(0xFFEF4444))),
                    TextButton(onPressed: _load, child: const Text('Retry')),
                  ],
                ),
              ),
            if (!_loading && visible.isEmpty)
              const Padding(
                padding: EdgeInsets.symmetric(vertical: 32, horizontal: 24),
                child: Center(
                  child: Text('No steps recorded yet', style: TextStyle(color: Color(0xFF6B7280))),
                ),
              ),
            for (final day in visible)
              Padding(
                padding: const EdgeInsets.fromLTRB(24, 0, 24, 12),
                child: _DayCard(day: day),
              ),
          ],
        ),
      ),
    );
  }
}

class _Header extends StatelessWidget {
  const _Header({required this.totalSteps, required this.streak, required this.onBack});

  final int totalSteps;
  final int streak;
  final VoidCallback onBack;

  @override
  Widget build(BuildContext context) {
    final streakLabel = streak == 1 ? '1 day streak' : '$streak day streak';
    return DecoratedBox(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFF9333EA), Color(0xFFDB2777), Color(0xFFF97316)],
        ),
      ),
      child: SafeArea(
        bottom: false,
        child: Padding(
          padding: const EdgeInsets.fromLTRB(24, 16, 24, 32),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Material(
                    color: Colors.white.withValues(alpha: 0.2),
                    shape: const CircleBorder(),
                    child: InkWell(
                      customBorder: const CircleBorder(),
                      onTap: onBack,
                      child: const Padding(
                        padding: EdgeInsets.all(12),
                        child: Icon(Icons.arrow_back, color: Colors.white, size: 24),
                      ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  const Text('My Activity', style: TextStyle(color: Colors.white, fontSize: 24, fontWeight: FontWeight.w900)),
                ],
              ),
              const SizedBox(height: 24),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(24),
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.2),
                  borderRadius: BorderRadius.circular(24),
                  border: Border.all(color: Colors.white.withValues(alpha: 0.3)),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('Last 30 days', style: TextStyle(color: Colors.white.withValues(alpha: 0.8), fontSize: 14)),
                    const SizedBox(height: 8),
                    Row(
                      crossAxisAlignment: CrossAxisAlignment.end,
                      children: [
                        Text(_fmt(totalSteps), style: const TextStyle(color: Colors.white, fontSize: 48, fontWeight: FontWeight.w900, height: 1)),
                        const SizedBox(width: 8),
                        const Padding(
                          padding: EdgeInsets.only(bottom: 6),
                          child: Text('steps', style: TextStyle(color: Colors.white, fontSize: 22, fontWeight: FontWeight.w500)),
                        ),
                      ],
                    ),
                    const SizedBox(height: 16),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: 0.2),
                        borderRadius: BorderRadius.circular(999),
                      ),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          const Icon(Icons.local_fire_department, color: Colors.white, size: 16),
                          const SizedBox(width: 8),
                          Text(streakLabel, style: const TextStyle(color: Colors.white, fontSize: 14)),
                        ],
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

class _TodayCard extends StatelessWidget {
  const _TodayCard({required this.steps});

  final StepsController steps;

  @override
  Widget build(BuildContext context) {
    final goal = steps.goal;
    final today = steps.todaySteps;
    final percent = steps.progressPercent;
    final milestones = [0.25, 0.5, 0.75, 1.0].map((part) => (goal * part).round()).toList();

    return Container(
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(24),
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFF9333EA), Color(0xFFDB2777), Color(0xFFF97316)],
        ),
        boxShadow: const [BoxShadow(color: Color(0x339333EA), blurRadius: 24, offset: Offset(0, 12))],
      ),
      child: Column(
        children: [
          Row(
            children: [
              const Icon(Icons.track_changes, color: Colors.white, size: 24),
              const SizedBox(width: 8),
              const Text('Daily Goal', style: TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.w700)),
              const Spacer(),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                decoration: BoxDecoration(color: Colors.white.withValues(alpha: 0.2), borderRadius: BorderRadius.circular(999)),
                child: Text('${_fmt(goal)} steps', style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w600, fontSize: 14)),
              ),
            ],
          ),
          const SizedBox(height: 20),
          ProgressCircle(
            percentage: percent,
            size: 180,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(_fmt(today), style: const TextStyle(color: Colors.white, fontSize: 32, fontWeight: FontWeight.w900)),
                const Text('steps today', style: TextStyle(color: Colors.white70, fontSize: 13)),
                Text('${percent.round()}%', style: const TextStyle(color: Colors.white, fontSize: 16, fontWeight: FontWeight.w700)),
              ],
            ),
          ),
          const SizedBox(height: 20),
          Row(
            children: [
              for (final mark in milestones)
                Expanded(
                  child: Column(
                    children: [
                      Container(
                        height: 8,
                        margin: const EdgeInsets.symmetric(horizontal: 4),
                        decoration: BoxDecoration(
                          color: today >= mark ? Colors.white : Colors.white.withValues(alpha: 0.3),
                          borderRadius: BorderRadius.circular(999),
                        ),
                      ),
                      const SizedBox(height: 6),
                      Text(
                        _fmt(mark),
                        style: TextStyle(
                          color: today >= mark ? Colors.white : Colors.white.withValues(alpha: 0.6),
                          fontSize: 11,
                          fontWeight: today >= mark ? FontWeight.w600 : FontWeight.w400,
                        ),
                      ),
                    ],
                  ),
                ),
            ],
          ),
          const SizedBox(height: 20),
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(color: Colors.white.withValues(alpha: 0.2), borderRadius: BorderRadius.circular(16)),
            child: const Row(
              children: [
                CircleAvatar(
                  backgroundColor: Color(0xFFFACC15),
                  child: Icon(Icons.card_giftcard, color: Color(0xFF713F12), size: 20),
                ),
                SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('Complete goal to earn', style: TextStyle(color: Colors.white70, fontSize: 13)),
                      Text('50 points', style: TextStyle(color: Colors.white, fontWeight: FontWeight.w700)),
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

class _StreakCard extends StatelessWidget {
  const _StreakCard({required this.streak, required this.days});

  final int streak;
  final List<DailySteps> days;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(24),
        gradient: const LinearGradient(colors: [Color(0xFFF97316), Color(0xFFEF4444)]),
        boxShadow: const [BoxShadow(color: Color(0x33F97316), blurRadius: 16, offset: Offset(0, 8))],
      ),
      child: Column(
        children: [
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(color: Colors.white.withValues(alpha: 0.2), shape: BoxShape.circle),
                child: const Icon(Icons.local_fire_department, color: Colors.white, size: 28),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('$streak Day Streak', style: const TextStyle(color: Colors.white, fontSize: 24, fontWeight: FontWeight.w900)),
                    const Text('Keep it going!', style: TextStyle(color: Colors.white70)),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Row(
            children: [
              for (final day in days)
                Expanded(
                  child: Column(
                    children: [
                      Container(
                        height: 36,
                        margin: const EdgeInsets.symmetric(horizontal: 3),
                        decoration: BoxDecoration(
                          color: day.goalCompleted ? Colors.white : Colors.white.withValues(alpha: 0.15),
                          borderRadius: BorderRadius.circular(12),
                          border: _isToday(day.stepDate) && !day.goalCompleted
                              ? Border.all(color: Colors.white.withValues(alpha: 0.6), width: 2)
                              : null,
                        ),
                        child: Center(
                          child: day.goalCompleted
                              ? const Icon(Icons.local_fire_department, color: Color(0xFFF97316), size: 18)
                              : Text('·', style: TextStyle(color: Colors.white.withValues(alpha: 0.3))),
                        ),
                      ),
                      const SizedBox(height: 6),
                      Text(
                        _weekday(day.stepDate),
                        style: TextStyle(
                          color: day.goalCompleted ? Colors.white : Colors.white.withValues(alpha: 0.4),
                          fontSize: 11,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ],
                  ),
                ),
            ],
          ),
        ],
      ),
    );
  }
}

class _DayCard extends StatelessWidget {
  const _DayCard({required this.day});

  final DailySteps day;

  @override
  Widget build(BuildContext context) {
    final earned = day.pointsAwarded > 0 && !day.rejected;
    final fraction = day.dailyTargetSteps <= 0 ? 0.0 : (day.steps / day.dailyTargetSteps).clamp(0.0, 1.0);
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: const [BoxShadow(color: Color(0x14000000), blurRadius: 12, offset: Offset(0, 4))],
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: earned ? const Color(0xFFDCFCE7) : const Color(0xFFF3F4F6),
              shape: BoxShape.circle,
            ),
            child: Icon(
              earned ? Icons.south_west : Icons.directions_walk,
              size: 20,
              color: earned ? const Color(0xFF16A34A) : const Color(0xFF4B5563),
            ),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(_dayTitle(day.stepDate), style: const TextStyle(fontWeight: FontWeight.w600, color: Color(0xFF1F2937))),
                const SizedBox(height: 2),
                Text(
                  day.rejected ? 'Not counted' : '${_fmt(day.steps)} / ${_fmt(day.dailyTargetSteps)} steps',
                  style: const TextStyle(fontSize: 14, color: Color(0xFF6B7280)),
                ),
                const SizedBox(height: 8),
                ClipRRect(
                  borderRadius: BorderRadius.circular(999),
                  child: SizedBox(
                    height: 6,
                    child: LinearProgressIndicator(
                      value: fraction,
                      backgroundColor: const Color(0xFFE5E7EB),
                      color: day.goalCompleted ? const Color(0xFF16A34A) : const Color(0xFF9333EA),
                    ),
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(width: 12),
          Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Text(
                earned ? '+${day.pointsAwarded}' : '0',
                style: TextStyle(fontSize: 18, fontWeight: FontWeight.w900, color: earned ? const Color(0xFF16A34A) : const Color(0xFF9CA3AF)),
              ),
              Text('points', style: TextStyle(fontSize: 12, color: earned ? const Color(0xFF16A34A) : const Color(0xFF9CA3AF))),
            ],
          ),
        ],
      ),
    );
  }
}

bool _isToday(DateTime date) {
  final local = date.toLocal();
  final now = DateTime.now();
  return local.year == now.year && local.month == now.month && local.day == now.day;
}

String _weekday(DateTime date) {
  const labels = ['M', 'T', 'W', 'T', 'F', 'S', 'S'];
  return labels[date.toLocal().weekday - 1];
}

String _dayTitle(DateTime date) {
  final local = date.toLocal();
  final now = DateTime.now();
  final today = DateTime(now.year, now.month, now.day);
  final day = DateTime(local.year, local.month, local.day);
  final diff = today.difference(day).inDays;
  if (diff == 0) {
    return 'Today';
  }
  if (diff == 1) {
    return 'Yesterday';
  }
  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  const weekdays = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
  return '${weekdays[local.weekday - 1]}, ${months[local.month - 1]} ${local.day}';
}

String _fmt(int value) {
  final text = value.abs().toString();
  final buffer = StringBuffer();
  for (var i = 0; i < text.length; i++) {
    if (i > 0 && (text.length - i) % 3 == 0) {
      buffer.write(',');
    }
    buffer.write(text[i]);
  }
  return value < 0 ? '-$buffer' : buffer.toString();
}
