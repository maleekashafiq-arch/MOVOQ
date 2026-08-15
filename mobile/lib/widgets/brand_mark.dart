import 'package:flutter/material.dart';

import '../theme.dart';

class BrandMark extends StatelessWidget {
  const BrandMark({super.key, this.size = 36, this.padded = true});

  final double size;
  final bool padded;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.all(padded ? 16 : 8),
      decoration: const BoxDecoration(
        shape: BoxShape.circle,
        gradient: LinearGradient(colors: [MovoqTheme.indigo, MovoqTheme.purple, MovoqTheme.pink]),
      ),
      child: Icon(Icons.directions_walk, color: Colors.white, size: size),
    );
  }
}
