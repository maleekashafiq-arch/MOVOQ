import 'package:flutter/material.dart';

import '../theme.dart';

class ComingSoonScreen extends StatelessWidget {
  const ComingSoonScreen({super.key, required this.title, required this.message});

  final String title;
  final String message;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: const BoxDecoration(gradient: MovoqTheme.pageGradient),
      child: SafeArea(
        child: Center(
          child: Padding(
            padding: const EdgeInsets.all(32),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Icon(Icons.construction_rounded, size: 48, color: MovoqTheme.purple),
                const SizedBox(height: 16),
                Text(title, style: Theme.of(context).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.w900)),
                const SizedBox(height: 8),
                Text(message, textAlign: TextAlign.center, style: TextStyle(color: Colors.grey.shade600)),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
