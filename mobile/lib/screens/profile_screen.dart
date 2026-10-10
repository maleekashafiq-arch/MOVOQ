import 'package:flutter/material.dart';

import '../services/session_controller.dart';
import '../services/steps_controller.dart';

class ProfileScreen extends StatelessWidget {
  const ProfileScreen({
    super.key,
    required this.session,
    required this.steps,
    required this.onOpenActivity,
  });

  final SessionController session;
  final StepsController steps;
  final VoidCallback onOpenActivity;

  @override
  Widget build(BuildContext context) {
    final top = MediaQuery.paddingOf(context).top;
    return DecoratedBox(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFFF9FAFB), Color(0xFFF3F4F6)],
        ),
      ),
      child: ListView(
        padding: const EdgeInsets.only(bottom: 24),
        children: [
          Stack(
            children: [
              Container(
                height: top + 128,
                decoration: const BoxDecoration(
                  gradient: LinearGradient(
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                    colors: [Color(0xFF9333EA), Color(0xFFDB2777), Color(0xFFF97316)],
                  ),
                ),
              ),
              Padding(
                padding: EdgeInsets.fromLTRB(24, top + 16, 24, 0),
                child: _ProfileCard(
                  session: session,
                  steps: steps,
                  onEdit: () => _soon(context, 'Profile editing'),
                ),
              ),
            ],
          ),
          const SizedBox(height: 24),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 24),
            child: _StatsRow(steps: steps),
          ),
          const SizedBox(height: 24),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 24),
            child: _QuickAccess(onOpenActivity: onOpenActivity, onOpenWallet: () => _soon(context, 'Wallet')),
          ),
          const SizedBox(height: 24),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 24),
            child: _MenuCard(
              onSettings: () => _soon(context, 'Settings'),
              onNotifications: () => _soon(context, 'Notifications'),
              onHelp: () => _soon(context, 'Help & Support'),
              onLogout: session.signOut,
            ),
          ),
          const Padding(
            padding: EdgeInsets.fromLTRB(24, 24, 24, 0),
            child: Text('MOVOQ v1.0.0', textAlign: TextAlign.center, style: TextStyle(color: Color(0xFF9CA3AF), fontSize: 14)),
          ),
        ],
      ),
    );
  }

  void _soon(BuildContext context, String title) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('$title is next. We will build that page after Home.')),
    );
  }
}

class _ProfileCard extends StatelessWidget {
  const _ProfileCard({required this.session, required this.steps, required this.onEdit});

  final SessionController session;
  final StepsController steps;
  final VoidCallback onEdit;

  @override
  Widget build(BuildContext context) {
    final user = session.user;
    final name = user?.displayName?.trim().isNotEmpty == true ? user!.displayName! : 'Walker';
    final email = user?.email ?? '';
    final points = user?.walletBalance ?? 0;
    final goal = steps.goal;
    final today = steps.todaySteps;
    final level = steps.history?.days.where((day) => day.goalCompleted).length ?? 0;
    final progress = goal <= 0 ? 0.0 : (today / goal).clamp(0.0, 1.0);

    return Stack(
      clipBehavior: Clip.none,
      alignment: Alignment.topCenter,
      children: [
        Container(
          margin: const EdgeInsets.only(top: 48),
          width: double.infinity,
          padding: const EdgeInsets.fromLTRB(24, 56, 24, 24),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(24),
            boxShadow: const [BoxShadow(color: Color(0x33000000), blurRadius: 24, offset: Offset(0, 12))],
          ),
          child: Column(
            children: [
              Text(name, textAlign: TextAlign.center, style: const TextStyle(fontSize: 24, fontWeight: FontWeight.w900, color: Color(0xFF1F2937))),
              const SizedBox(height: 4),
              Text(email, textAlign: TextAlign.center, style: const TextStyle(color: Color(0xFF6B7280))),
              const SizedBox(height: 16),
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  _Pill(
                    label: 'Level',
                    value: '$level',
                    colors: const [Color(0xFFF3E8FF), Color(0xFFFCE7F3)],
                    valueColor: const Color(0xFF9333EA),
                  ),
                  const SizedBox(width: 16),
                  _Pill(
                    label: 'Points',
                    value: _fmt(points),
                    colors: const [Color(0xFFFEF9C3), Color(0xFFFFEDD5)],
                    valueColor: const Color(0xFFEA580C),
                  ),
                ],
              ),
              const SizedBox(height: 16),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text('Level $level', style: const TextStyle(fontSize: 14, color: Color(0xFF4B5563))),
                  Text('Level ${level + 1}', style: const TextStyle(fontSize: 14, color: Color(0xFF4B5563))),
                ],
              ),
              const SizedBox(height: 8),
              Container(
                height: 8,
                alignment: Alignment.centerLeft,
                decoration: BoxDecoration(color: const Color(0xFFE5E7EB), borderRadius: BorderRadius.circular(999)),
                child: FractionallySizedBox(
                  widthFactor: progress,
                  child: Container(
                    decoration: BoxDecoration(
                      borderRadius: BorderRadius.circular(999),
                      gradient: const LinearGradient(colors: [Color(0xFF9333EA), Color(0xFFDB2777)]),
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 4),
              Text(
                '${_fmt(today)} / ${_fmt(goal)} steps',
                style: const TextStyle(fontSize: 12, color: Color(0xFF6B7280)),
              ),
            ],
          ),
        ),
        Positioned(top: 0, child: _Avatar(onEdit: onEdit)),
      ],
    );
  }
}

class _Avatar extends StatelessWidget {
  const _Avatar({required this.onEdit});

  final VoidCallback onEdit;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: 96,
      height: 96,
      child: Stack(
        clipBehavior: Clip.none,
        children: [
          Container(
            width: 96,
            height: 96,
            alignment: Alignment.center,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              border: Border.all(color: Colors.white, width: 4),
              gradient: const LinearGradient(
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
                colors: [Color(0xFFA855F7), Color(0xFFDB2777)],
              ),
              boxShadow: const [BoxShadow(color: Color(0x33000000), blurRadius: 16, offset: Offset(0, 8))],
            ),
            child: const Text('👤', style: TextStyle(fontSize: 40)),
          ),
          Positioned(
            right: 0,
            bottom: 0,
            child: Material(
              color: Colors.white,
              shape: const CircleBorder(),
              child: InkWell(
                onTap: onEdit,
                customBorder: const CircleBorder(),
                child: Container(
                  padding: const EdgeInsets.all(6),
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    border: Border.all(color: const Color(0xFFA855F7), width: 2),
                    boxShadow: const [BoxShadow(color: Color(0x1A000000), blurRadius: 8, offset: Offset(0, 2))],
                  ),
                  child: const Icon(Icons.edit, size: 14, color: Color(0xFF9333EA)),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _Pill extends StatelessWidget {
  const _Pill({required this.label, required this.value, required this.colors, required this.valueColor});

  final String label;
  final String value;
  final List<Color> colors;
  final Color valueColor;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(999),
        gradient: LinearGradient(colors: colors),
      ),
      child: Column(
        children: [
          Text(label, style: const TextStyle(fontSize: 12, color: Color(0xFF4B5563))),
          Text(value, style: TextStyle(fontSize: 20, fontWeight: FontWeight.w900, color: valueColor)),
        ],
      ),
    );
  }
}

class _StatsRow extends StatelessWidget {
  const _StatsRow({required this.steps});

  final StepsController steps;

  @override
  Widget build(BuildContext context) {
    final streak = steps.streak;
    final streakLabel = streak == 1 ? '1 day' : '$streak days';
    return Row(
      children: [
        Expanded(child: _StatTile(icon: Icons.gps_fixed, value: _fmt(steps.history?.totalSteps ?? 0), label: 'Total Steps')),
        const SizedBox(width: 12),
        Expanded(child: _StatTile(icon: Icons.local_fire_department, value: streakLabel, label: 'Current Streak')),
        const SizedBox(width: 12),
        const Expanded(child: _StatTile(icon: Icons.emoji_events, value: '0', label: 'Achievements')),
      ],
    );
  }
}

class _StatTile extends StatelessWidget {
  const _StatTile({required this.icon, required this.value, required this.label});

  final IconData icon;
  final String value;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: const [BoxShadow(color: Color(0x14000000), blurRadius: 12, offset: Offset(0, 4))],
      ),
      child: Column(
        children: [
          Container(
            width: 48,
            height: 48,
            alignment: Alignment.center,
            decoration: const BoxDecoration(
              shape: BoxShape.circle,
              gradient: LinearGradient(
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
                colors: [Color(0xFFF3E8FF), Color(0xFFFCE7F3)],
              ),
            ),
            child: Icon(icon, color: const Color(0xFF9333EA), size: 24),
          ),
          const SizedBox(height: 8),
          Text(value, textAlign: TextAlign.center, style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w900, color: Color(0xFF1F2937))),
          const SizedBox(height: 4),
          Text(label, textAlign: TextAlign.center, style: const TextStyle(fontSize: 11, color: Color(0xFF6B7280))),
        ],
      ),
    );
  }
}

class _QuickAccess extends StatelessWidget {
  const _QuickAccess({required this.onOpenActivity, required this.onOpenWallet});

  final VoidCallback onOpenActivity;
  final VoidCallback onOpenWallet;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: _QuickTile(
            colors: const [Color(0xFF3B82F6), Color(0xFF06B6D4)],
            icon: Icons.monitor_heart_outlined,
            title: 'My Activity',
            subtitle: 'Steps & history',
            onTap: onOpenActivity,
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: _QuickTile(
            colors: const [Color(0xFFA855F7), Color(0xFFEC4899)],
            icon: Icons.account_balance_wallet_outlined,
            title: 'Wallet',
            subtitle: 'Points & tickets',
            onTap: onOpenWallet,
          ),
        ),
      ],
    );
  }
}

class _QuickTile extends StatelessWidget {
  const _QuickTile({
    required this.colors,
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.onTap,
  });

  final List<Color> colors;
  final IconData icon;
  final String title;
  final String subtitle;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(16),
        child: Ink(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(16),
            gradient: LinearGradient(begin: Alignment.topLeft, end: Alignment.bottomRight, colors: colors),
            boxShadow: const [BoxShadow(color: Color(0x33000000), blurRadius: 12, offset: Offset(0, 6))],
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 40,
                height: 40,
                alignment: Alignment.center,
                decoration: BoxDecoration(color: Colors.white.withValues(alpha: 0.2), shape: BoxShape.circle),
                child: Icon(icon, color: Colors.white, size: 20),
              ),
              const SizedBox(height: 12),
              Text(title, style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w900)),
              const SizedBox(height: 2),
              Text(subtitle, style: TextStyle(color: Colors.white.withValues(alpha: 0.8), fontSize: 12)),
            ],
          ),
        ),
      ),
    );
  }
}

class _MenuCard extends StatelessWidget {
  const _MenuCard({
    required this.onSettings,
    required this.onNotifications,
    required this.onHelp,
    required this.onLogout,
  });

  final VoidCallback onSettings;
  final VoidCallback onNotifications;
  final VoidCallback onHelp;
  final VoidCallback onLogout;

  @override
  Widget build(BuildContext context) {
    final items = [
      (Icons.settings, 'Settings', false, onSettings),
      (Icons.notifications_none, 'Notifications', false, onNotifications),
      (Icons.help_outline, 'Help & Support', false, onHelp),
      (Icons.logout, 'Logout', true, onLogout),
    ];
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(24),
        boxShadow: const [BoxShadow(color: Color(0x1A000000), blurRadius: 16, offset: Offset(0, 8))],
      ),
      child: Column(
        children: [
          for (var i = 0; i < items.length; i++)
            _MenuRow(
              icon: items[i].$1,
              label: items[i].$2,
              danger: items[i].$3,
              showDivider: i != items.length - 1,
              onTap: items[i].$4,
            ),
        ],
      ),
    );
  }
}

class _MenuRow extends StatelessWidget {
  const _MenuRow({
    required this.icon,
    required this.label,
    required this.danger,
    required this.showDivider,
    required this.onTap,
  });

  final IconData icon;
  final String label;
  final bool danger;
  final bool showDivider;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final color = danger ? const Color(0xFFDC2626) : const Color(0xFF1F2937);
    return InkWell(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          border: showDivider ? const Border(bottom: BorderSide(color: Color(0xFFF3F4F6))) : null,
        ),
        child: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: danger ? const Color(0xFFFEE2E2) : const Color(0xFFF3F4F6),
                shape: BoxShape.circle,
              ),
              child: Icon(icon, size: 20, color: danger ? const Color(0xFFDC2626) : const Color(0xFF4B5563)),
            ),
            const SizedBox(width: 16),
            Expanded(child: Text(label, style: TextStyle(fontWeight: FontWeight.w600, color: color))),
            Icon(Icons.chevron_right, color: danger ? const Color(0xFFF87171) : const Color(0xFF9CA3AF)),
          ],
        ),
      ),
    );
  }
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
