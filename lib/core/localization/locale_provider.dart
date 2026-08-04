import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'backend_localization_provider.dart';

class LocaleNotifier extends StateNotifier<Locale> {
  static const String _prefKey = 'selected_app_locale';
  final Ref _ref;

  LocaleNotifier(this._ref) : super(const Locale('fa', 'AF')) {
    _loadSavedLocale();
  }

  Future<void> _loadSavedLocale() async {
    final prefs = await SharedPreferences.getInstance();
    final langCode = prefs.getString(_prefKey) ?? 'prs';
    state = _getLocaleFromCode(langCode);
    _ref.read(backendLocalizationProvider.notifier).fetchBackendTranslations(langCode);
  }

  Future<void> setLocale(String languageCode) async {
    state = _getLocaleFromCode(languageCode);
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_prefKey, languageCode);
    _ref.read(backendLocalizationProvider.notifier).fetchBackendTranslations(languageCode);
  }

  Locale _getLocaleFromCode(String code) {
    switch (code) {
      case 'ps':
        return const Locale('ps', 'AF');
      case 'prs':
      case 'fa':
        return const Locale('fa', 'AF');
      case 'en':
      default:
        return const Locale('en', 'US');
    }
  }

  TextDirection get textDirection {
    if (state.languageCode == 'ps' || state.languageCode == 'fa' || state.languageCode == 'prs') {
      return TextDirection.rtl;
    }
    return TextDirection.ltr;
  }
}

final localeProvider = StateNotifierProvider<LocaleNotifier, Locale>((ref) {
  return LocaleNotifier(ref);
});
