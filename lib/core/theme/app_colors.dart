import 'package:flutter/material.dart';

class AppColors {
  // Light Minimalist Premium Palette (Inspired by BuyMore Dashboard)
  static const Color background = Color(0xFFF4F7FE);
  static const Color surface = Color(0xFFFFFFFF);
  static const Color card = Color(0xFFFFFFFF);
  static const Color border = Color(0xFFE5E7EB);
  static const Color textPrimary = Color(0xFF1B2559);
  static const Color textSecondary = Color(0xFFA3AED0);

  // Horizon UI Palette
  static const Color horizonBg = Color(0xFFF4F7FE);
  static const Color horizonCard = Color(0xFFFFFFFF);
  static const Color horizonBrand = Color(0xFF4318FF);
  static const Color horizonSky = Color(0xFF6AD2FF);
  static const Color horizonNavy = Color(0xFF1B2559);
  static const Color horizonMuted = Color(0xFFA3AED0);
  static const Color horizonGreen = Color(0xFF05CD99);
  static const Color horizonOrange = Color(0xFFFFB547);
  static const Color horizonRed = Color(0xFFEE5D50);

  // Accent Colors
  static const Color royalBlue = Color(0xFF4318FF);
  static const Color primaryViolet = royalBlue;
  static const Color secondaryPurple = Color(0xFF6AD2FF);
  static const Color primaryPurple = royalBlue;
  static const Color accentViolet = secondaryPurple;

  // Pastel Card Backgrounds
  static const Color pastelMint = Color(0xFFE6F9F5);
  static const Color pastelYellow = Color(0xFFFFF7E6);
  static const Color pastelPeach = Color(0xFFFFF0EE);
  static const Color pastelGrey = Color(0xFFF4F7FE);

  // Status & Badges
  static const Color accentGold = Color(0xFFFFB547);
  static const Color accentNeonTeal = Color(0xFF05CD99);
  static const Color dangerRed = Color(0xFFEE5D50);

  // Horizon Card Shadow
  static const BoxShadow horizonShadow = BoxShadow(
    color: Color.fromRGBO(112, 144, 176, 0.12),
    blurRadius: 40,
    offset: Offset(0, 18),
  );

  // Multi-Stop Gradients
  static const LinearGradient primaryGradient = LinearGradient(
    colors: [Color(0xFF4318FF), Color(0xFF6AD2FF)],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  static const LinearGradient heroMintGradient = LinearGradient(
    colors: [Color(0xFFE6F9F5), Color(0xFFD0F5ED)],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  static const LinearGradient cardGlassGradient = LinearGradient(
    colors: [Color(0xFFFFFFFF), Color(0xFFFAFAFA)],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );
}
