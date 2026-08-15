import 'package:flutter/material.dart';

class MovoqTheme {
  static const indigo = Color(0xFF4F46E5);
  static const purple = Color(0xFF7C3AED);
  static const pink = Color(0xFFDB2777);

  static ThemeData get light {
    final scheme = ColorScheme.fromSeed(seedColor: indigo);
    return ThemeData(
      colorScheme: scheme,
      useMaterial3: true,
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: Colors.white,
        border: OutlineInputBorder(borderRadius: BorderRadius.circular(16)),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(16),
          borderSide: const BorderSide(color: Color(0xFFE5E7EB)),
        ),
      ),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          minimumSize: const Size.fromHeight(52),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        ),
      ),
    );
  }

  static const pageGradient = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [Color(0xFFEFF6FF), Color(0xFFF5F3FF), Color(0xFFFDF2F8)],
  );
}
