import 'dart:math' as math;

import 'package:flutter/material.dart';

class ProgressCircle extends StatelessWidget {
  const ProgressCircle({
    super.key,
    required this.percentage,
    required this.child,
    this.size = 200,
    this.strokeWidth = 12,
    this.trackColor = const Color(0x40FFFFFF),
  });

  final double percentage;
  final Widget child;
  final double size;
  final double strokeWidth;
  final Color trackColor;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: size,
      height: size,
      child: Stack(
        alignment: Alignment.center,
        children: [
          CustomPaint(
            size: Size.square(size),
            painter: _RingPainter(
              percentage: percentage.clamp(0, 100),
              strokeWidth: strokeWidth,
              trackColor: trackColor,
            ),
          ),
          child,
        ],
      ),
    );
  }
}

class _RingPainter extends CustomPainter {
  _RingPainter({
    required this.percentage,
    required this.strokeWidth,
    required this.trackColor,
  });

  final double percentage;
  final double strokeWidth;
  final Color trackColor;

  @override
  void paint(Canvas canvas, Size size) {
    final center = Offset(size.width / 2, size.height / 2);
    final radius = (size.width - strokeWidth) / 2;
    final rect = Rect.fromCircle(center: center, radius: radius);

    canvas.drawCircle(
      center,
      radius,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = strokeWidth
        ..color = trackColor,
    );

    final sweep = 2 * math.pi * (percentage / 100);
    final paint = Paint()
      ..style = PaintingStyle.stroke
      ..strokeWidth = strokeWidth
      ..strokeCap = StrokeCap.round
      ..shader = const SweepGradient(
        startAngle: -math.pi / 2,
        endAngle: 3 * math.pi / 2,
        colors: [Color(0xFF60A5FA), Color(0xFFFFFFFF), Color(0xFFF9A8D4)],
      ).createShader(rect);

    canvas.drawArc(rect, -math.pi / 2, sweep, false, paint);
  }

  @override
  bool shouldRepaint(covariant _RingPainter oldDelegate) =>
      oldDelegate.percentage != percentage;
}
