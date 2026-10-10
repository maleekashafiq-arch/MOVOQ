import 'package:flutter/material.dart';

import '../services/outdoor_conditions.dart';
import '../services/session_controller.dart';
import '../services/steps_controller.dart';
import '../widgets/bottom_nav.dart';
import 'activity_screen.dart';
import 'coming_soon_screen.dart';
import 'home_screen.dart';
import 'profile_screen.dart';
import 'rewards_screen.dart';

class MainShell extends StatefulWidget {
  const MainShell({super.key, required this.session, required this.steps});

  final SessionController session;
  final StepsController steps;

  @override
  State<MainShell> createState() => _MainShellState();
}

class _MainShellState extends State<MainShell> with WidgetsBindingObserver {
  int _tab = 0;
  bool _showActivity = false;
  late final OutdoorConditions _outdoor;

  @override
  void initState() {
    super.initState();
    _outdoor = OutdoorConditions();
    widget.steps.addListener(_refresh);
    _outdoor.addListener(_refresh);
    WidgetsBinding.instance.addObserver(this);
    _outdoor.load();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) {
      _outdoor.refresh();
    }
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    widget.steps.removeListener(_refresh);
    _outdoor.removeListener(_refresh);
    _outdoor.dispose();
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

  @override
  Widget build(BuildContext context) {
    final body = _showActivity
        ? ActivityScreen(
            session: widget.session,
            steps: widget.steps,
            onBack: () => setState(() => _showActivity = false),
          )
        : switch (_tab) {
            0 => HomeScreen(
                user: widget.session.user,
                steps: widget.steps,
                outdoor: _outdoor,
                onComingSoon: _comingSoon,
              ),
            1 => RewardsScreen(session: widget.session),
            2 => const ComingSoonScreen(title: 'Team', message: 'Teams and coaches are a later module.'),
            _ => ProfileScreen(
                session: widget.session,
                steps: widget.steps,
                onOpenActivity: () => setState(() => _showActivity = true),
              ),
          };

    return Scaffold(
      backgroundColor: const Color(0xFFEFF6FF),
      body: body,
      bottomNavigationBar: BottomNav(
        index: _tab,
        onChanged: (index) => setState(() {
          _tab = index;
          _showActivity = false;
        }),
      ),
    );
  }
}
