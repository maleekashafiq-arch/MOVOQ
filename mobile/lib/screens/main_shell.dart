import 'package:flutter/material.dart';

import '../config.dart';
import '../services/session_controller.dart';
import '../services/steps_controller.dart';
import '../theme.dart';
import '../widgets/bottom_nav.dart';
import 'coming_soon_screen.dart';
import 'home_screen.dart';
import 'lucky_draw_screen.dart';

class MainShell extends StatefulWidget {
  const MainShell({super.key, required this.session, required this.steps});

  final SessionController session;
  final StepsController steps;

  @override
  State<MainShell> createState() => _MainShellState();
}

class _MainShellState extends State<MainShell> {
  int _tab = 0;

  @override
  void initState() {
    super.initState();
    widget.steps.addListener(_refresh);
  }

  @override
  void dispose() {
    widget.steps.removeListener(_refresh);
    super.dispose();
  }

  void _refresh() {
    if (mounted) {
      setState(() {});
    }
  }

  void _comingSoon(String title) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('$title is next. We will build that page after Home.')),
    );
  }

  void _openLuckyDraw() {
    Navigator.of(context).push(
      MaterialPageRoute<void>(
        builder: (_) => LuckyDrawScreen(points: widget.session.user?.walletBalance ?? 0),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final body = switch (_tab) {
      0 => HomeScreen(
          user: widget.session.user,
          steps: widget.steps,
          onComingSoon: _comingSoon,
          onOpenLuckyDraw: _openLuckyDraw,
        ),
      1 => const ComingSoonScreen(title: 'Rewards', message: 'Marketplace and redemptions come after Home.'),
      2 => const ComingSoonScreen(title: 'Team', message: 'Teams and coaches are a later module.'),
      _ => _ProfileTab(session: widget.session),
    };

    return Scaffold(
      backgroundColor: const Color(0xFFEFF6FF),
      body: body,
      bottomNavigationBar: BottomNav(index: _tab, onChanged: (index) => setState(() => _tab = index)),
    );
  }
}

class _ProfileTab extends StatelessWidget {
  const _ProfileTab({required this.session});

  final SessionController session;

  @override
  Widget build(BuildContext context) {
    final user = session.user;
    final name = user?.displayName?.trim().isNotEmpty == true ? user!.displayName! : 'Walker';
    final location = [user?.city, user?.country].where((part) => part != null && part.trim().isNotEmpty).join(', ');
    return DecoratedBox(
      decoration: const BoxDecoration(gradient: MovoqTheme.pageGradient),
      child: SafeArea(
        child: ListView(
          padding: const EdgeInsets.fromLTRB(24, 24, 24, 24),
          children: [
            Text('Profile', style: Theme.of(context).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.w900)),
            const SizedBox(height: 16),
            Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(24),
                border: Border.all(color: const Color(0xFFF3F4F6)),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(name, style: const TextStyle(fontSize: 20, fontWeight: FontWeight.w900)),
                  const SizedBox(height: 4),
                  Text(user?.email ?? '', style: TextStyle(color: Colors.grey.shade600)),
                  if (location.isNotEmpty) ...[
                    const SizedBox(height: 8),
                    Text(location),
                  ],
                  const SizedBox(height: 8),
                  Text('Daily goal: ${user?.dailyTargetSteps ?? '-'} steps'),
                  Text('Wallet: ${user?.walletBalance ?? 0} pts'),
                ],
              ),
            ),
            if (AppConfig.isLocalDev) ...[
              const SizedBox(height: 24),
              Text('Fake users', style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w800)),
              const SizedBox(height: 4),
              Text(
                'These accounts are stored in the API. Open ${AppConfig.apiBaseUrl}/dev to see the backend dashboard.',
                style: TextStyle(color: Colors.grey.shade600, fontSize: 13),
              ),
              const SizedBox(height: 12),
              for (final fake in session.fakeUsers)
                ListTile(
                  contentPadding: EdgeInsets.zero,
                  title: Text(fake.displayName ?? fake.email),
                  subtitle: Text('${fake.email} · ${fake.walletBalance} pts'),
                  trailing: user?.email == fake.email
                      ? const Text('Active', style: TextStyle(fontWeight: FontWeight.w700, color: MovoqTheme.purple))
                      : const Icon(Icons.swap_horiz),
                  onTap: () => session.switchLocalUser(fake.identity),
                ),
              if (session.fakeUsers.isEmpty)
                Text('No users in the API yet.', style: TextStyle(color: Colors.grey.shade600)),
              const SizedBox(height: 8),
              FilledButton(
                onPressed: () => _addFakeUser(context, session),
                child: const Text('Add fake user'),
              ),
              const SizedBox(height: 8),
              OutlinedButton(
                onPressed: session.seedFakeUsers,
                child: const Text('Add sample users'),
              ),
            ],
            const SizedBox(height: 24),
            OutlinedButton(onPressed: session.signOut, child: const Text('Sign out')),
          ],
        ),
      ),
    );
  }

  Future<void> _addFakeUser(BuildContext context, SessionController session) async {
    final name = TextEditingController();
    final city = TextEditingController();
    final created = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Add fake user'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(controller: name, decoration: const InputDecoration(labelText: 'Name (optional)')),
            const SizedBox(height: 12),
            TextField(controller: city, decoration: const InputDecoration(labelText: 'City (optional)')),
          ],
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Cancel')),
          FilledButton(onPressed: () => Navigator.pop(context, true), child: const Text('Add')),
        ],
      ),
    );
    final fullName = name.text.trim();
    final cityName = city.text.trim();
    name.dispose();
    city.dispose();
    if (created == true) {
      await session.addFakeUser(
        fullName: fullName.isEmpty ? null : fullName,
        city: cityName.isEmpty ? null : cityName,
      );
    }
  }
}
