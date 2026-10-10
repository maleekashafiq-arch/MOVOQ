// Home-screen entry that used to open Lucky Draw. Parked with the feature.
import 'package:flutter/material.dart';

class LuckyDrawBanner extends StatelessWidget {
  const LuckyDrawBanner({super.key, required this.onTap});

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
