import 'package:flutter/material.dart';

class AppColors {
  // Light Minimalist Premium Palette (Inspired by BuyMore Dashboard)
  static const Color background = Color(0xFFF4F6F8);
  static const Color surface = Color(0xFFFFFFFF);
  static const Color card = Color(0xFFFFFFFF);
  static const Color border = Color(0xFFE5E7EB);
  static const Color textPrimary = Color(0xFF111827);
  static const Color textSecondary = Color(0xFF6B7280);

  // Accent Colors
  static const Color royalBlue = Color(0xFF2563EB);
  static const Color primaryViolet = royalBlue;
  static const Color secondaryPurple = Color(0xFF3B82F6);
  static const Color primaryPurple = royalBlue;
  static const Color accentViolet = secondaryPurple;

  // Pastel Card Backgrounds
  static const Color pastelMint = Color(0xFFD4F3E4);
  static const Color pastelYellow = Color(0xFFFDE8B3);
  static const Color pastelPeach = Color(0xFFFFF3EC);
  static const Color pastelGrey = Color(0xFFF0F2F5);

  // Status & Badges
  static const Color accentGold = Color(0xFFF59E0B);
  static const Color accentNeonTeal = Color(0xFF10B981);
  static const Color dangerRed = Color(0xFFEF4444);

  // Multi-Stop Gradients
  static const LinearGradient primaryGradient = LinearGradient(
    colors: [Color(0xFF2563EB), Color(0xFF3B82F6)],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  static const LinearGradient heroMintGradient = LinearGradient(
    colors: [Color(0xFFD4F3E4), Color(0xFFE8F9F0)],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  static const LinearGradient cardGlassGradient = LinearGradient(
    colors: [Color(0xFFFFFFFF), Color(0xFFFAFAFA)],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );
}
