import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'config.dart';
import 'screens/complete_profile_screen.dart';
import 'screens/main_shell.dart';
import 'screens/onboarding_screen.dart';
import 'screens/sign_in_screen.dart';
import 'screens/splash_screen.dart';
import 'services/session_controller.dart';
import 'services/steps_controller.dart';
import 'theme.dart';

class MovoqApp extends StatefulWidget {
  const MovoqApp({super.key, required this.session});

  final SessionController session;

  @override
  State<MovoqApp> createState() => _MovoqAppState();
}

class _MovoqAppState extends State<MovoqApp> with WidgetsBindingObserver {
  static const _onboardingKey = 'movoq_seen_onboarding';

  late final StepsController _steps = StepsController(widget.session);

  bool _showSplash = true;
  bool _showOnboarding = false;
  bool _stepsStarted = false;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    widget.session.addListener(_refresh);
    widget.session.restore();
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    widget.session.removeListener(_refresh);
    _steps.dispose();
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed && _stepsStarted) {
      _steps.refresh();
    }
  }

  void _refresh() {
    // Counting only makes sense once there is an account behind the sync calls.
    if (!_stepsStarted && widget.session.step == AuthStep.ready) {
      _stepsStarted = true;
      _steps.start();
    }
    setState(() {});
  }

  Future<void> _finishSplash() async {
    final prefs = await SharedPreferences.getInstance();
    final seen = prefs.getBool(_onboardingKey) ?? false;
    if (!mounted) {
      return;
    }
    setState(() {
      _showSplash = false;
      _showOnboarding = !seen && widget.session.step == AuthStep.signedOut;
    });
  }

  Future<void> _finishOnboarding() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool(_onboardingKey, true);
    if (mounted) {
      setState(() => _showOnboarding = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'MOVOQ',
      theme: MovoqTheme.light,
      home: _home,
    );
  }

  Widget get _home {
    if (_showSplash) {
      return SplashScreen(onFinished: _finishSplash);
    }
    if (_showOnboarding) {
      return OnboardingScreen(onDone: _finishOnboarding);
    }
    return switch (widget.session.step) {
      AuthStep.loading => const Scaffold(body: Center(child: CircularProgressIndicator())),
      AuthStep.signedOut => SignInScreen(session: widget.session),
      AuthStep.needsProfile => AppConfig.isLocalDev
          ? MainShell(session: widget.session, steps: _steps)
          : CompleteProfileScreen(session: widget.session),
      AuthStep.ready => MainShell(session: widget.session, steps: _steps),
    };
  }
}
