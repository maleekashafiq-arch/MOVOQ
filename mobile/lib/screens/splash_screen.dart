import 'package:flutter/material.dart';

import '../theme.dart';

class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key, required this.onFinished});

  final VoidCallback onFinished;

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen> {
  @override
  void initState() {
    super.initState();
    Future<void>.delayed(const Duration(milliseconds: 2200), () {
      if (mounted) {
        widget.onFinished();
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: DecoratedBox(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
            colors: [Color(0xFF2563EB), MovoqTheme.purple, MovoqTheme.pink],
          ),
        ),
        child: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Container(
                padding: const EdgeInsets.all(28),
                decoration: const BoxDecoration(color: Colors.white, shape: BoxShape.circle),
                child: const Icon(Icons.directions_walk, size: 72, color: MovoqTheme.purple),
              ),
              const SizedBox(height: 28),
              const Text(
                'MOVOQ',
                style: TextStyle(color: Colors.white, fontSize: 48, fontWeight: FontWeight.w900, letterSpacing: -1),
              ),
              const SizedBox(height: 8),
              Text(
                'Walk  •  Earn  •  Win',
                style: TextStyle(color: Colors.white.withValues(alpha: 0.9), fontSize: 18, fontWeight: FontWeight.w500),
              ),
              const SizedBox(height: 40),
              const SizedBox(width: 22, height: 22, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2)),
            ],
          ),
        ),
      ),
    );
  }
}
