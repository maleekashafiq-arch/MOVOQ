import 'package:flutter/material.dart';

import '../theme.dart';

class OnboardingScreen extends StatefulWidget {
  const OnboardingScreen({super.key, required this.onDone});

  final VoidCallback onDone;

  @override
  State<OnboardingScreen> createState() => _OnboardingScreenState();
}

class _OnboardingScreenState extends State<OnboardingScreen> {
  static const _slides = [
    _Slide(
      icon: Icons.directions_walk,
      title: 'Walk & Earn Points',
      description: 'Every step counts! Track your daily walks and earn points automatically.',
      colors: [Color(0xFF3B82F6), Color(0xFF06B6D4)],
    ),
    _Slide(
      icon: Icons.play_circle_outline,
      title: 'Watch Ads to Boost Rewards',
      description: 'Watch short ads to multiply your earnings and unlock special bonuses.',
      colors: [MovoqTheme.purple, MovoqTheme.pink],
    ),
    _Slide(
      icon: Icons.card_giftcard,
      title: 'Redeem Points & Win Prizes',
      description: 'Use your points for marketplace rewards and exciting prizes!',
      colors: [Color(0xFF22C55E), Color(0xFF10B981)],
    ),
  ];

  int _index = 0;

  void _next() {
    if (_index < _slides.length - 1) {
      setState(() => _index += 1);
    } else {
      widget.onDone();
    }
  }

  @override
  Widget build(BuildContext context) {
    final slide = _slides[_index];
    return Scaffold(
      body: DecoratedBox(
        decoration: const BoxDecoration(gradient: MovoqTheme.pageGradient),
        child: SafeArea(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(24, 8, 24, 24),
            child: Column(
              children: [
                Align(
                  alignment: Alignment.centerRight,
                  child: TextButton(onPressed: widget.onDone, child: const Text('Skip')),
                ),
                Expanded(
                  child: AnimatedSwitcher(
                    duration: const Duration(milliseconds: 280),
                    child: Column(
                      key: ValueKey(_index),
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Container(
                          padding: const EdgeInsets.all(36),
                          decoration: BoxDecoration(
                            shape: BoxShape.circle,
                            gradient: LinearGradient(colors: slide.colors),
                            boxShadow: [
                              BoxShadow(color: slide.colors.last.withValues(alpha: 0.35), blurRadius: 24, offset: const Offset(0, 10)),
                            ],
                          ),
                          child: Icon(slide.icon, size: 72, color: Colors.white),
                        ),
                        const SizedBox(height: 36),
                        Text(
                          slide.title,
                          textAlign: TextAlign.center,
                          style: Theme.of(context).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.w900),
                        ),
                        const SizedBox(height: 12),
                        Text(
                          slide.description,
                          textAlign: TextAlign.center,
                          style: TextStyle(fontSize: 16, height: 1.45, color: Colors.grey.shade600),
                        ),
                      ],
                    ),
                  ),
                ),
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    for (var i = 0; i < _slides.length; i++)
                      AnimatedContainer(
                        duration: const Duration(milliseconds: 200),
                        margin: const EdgeInsets.symmetric(horizontal: 4),
                        height: 8,
                        width: i == _index ? 28 : 8,
                        decoration: BoxDecoration(
                          color: i == _index ? MovoqTheme.purple : Colors.grey.shade300,
                          borderRadius: BorderRadius.circular(8),
                        ),
                      ),
                  ],
                ),
                const SizedBox(height: 24),
                FilledButton(
                  onPressed: _next,
                  child: Text(_index == _slides.length - 1 ? 'Get Started' : 'Next'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _Slide {
  const _Slide({
    required this.icon,
    required this.title,
    required this.description,
    required this.colors,
  });

  final IconData icon;
  final String title;
  final String description;
  final List<Color> colors;
}
