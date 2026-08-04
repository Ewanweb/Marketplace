import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

class AppLocalization extends ChangeNotifier {
  static const String _languageKey = 'user_selected_language';

  // Supported language codes: 'en' (English), 'prs' (Dari), 'ps' (Pashto)
  String _currentLanguage = 'prs'; // Default to Dari

  String get currentLanguage => _currentLanguage;

  TextDirection get textDirection {
    if (_currentLanguage == 'prs' || _currentLanguage == 'ps') {
      return TextDirection.rtl;
    }
    return TextDirection.ltr;
  }

  Locale get locale {
    if (_currentLanguage == 'prs') return const Locale('fa', 'AF');
    if (_currentLanguage == 'ps') return const Locale('ps', 'AF');
    return const Locale('en', 'US');
  }

  AppLocalization() {
    _loadSavedLanguage();
  }

  Future<void> _loadSavedLanguage() async {
    final prefs = await SharedPreferences.getInstance();
    _currentLanguage = prefs.getString(_languageKey) ?? 'prs';
    notifyListeners();
  }

  Future<void> setLanguage(String languageCode) async {
    if (_currentLanguage == languageCode) return;

    _currentLanguage = languageCode;
    notifyListeners();

    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_languageKey, languageCode);
  }

  String translate(String key) {
    return _localizedStrings[_currentLanguage]?[key] ??
        _localizedStrings['en']?[key] ??
        key;
  }

  static final Map<String, Map<String, String>> _localizedStrings = {
    'en': {
      'app_title': 'Marketplace Identity',
      'login': 'Login',
      'register': 'Register',
      'email': 'Email Address',
      'password': 'Password',
      'confirm_password': 'Confirm Password',
      'dont_have_account': "Don't have an account? Register",
      'already_have_account': 'Already have an account? Login',
      'submitting': 'Processing...',
      'logout': 'Logout',
      'logout_all': 'Logout All Devices',
      'welcome': 'Welcome to Marketplace',
      'language': 'Language',
      'english': 'English',
      'dari': 'دری (Dari)',
      'pashto': 'پښتو (Pashto)',
      'access_token': 'Access Token',
      'refresh_token': 'Refresh Token',
      'enable_2fa': 'Enable 2FA (TOTP)',
      'verify_2fa': 'Verify 2FA Code',
      'enter_2fa_code': 'Enter 6-digit Authenticator Code',
      'security_dashboard': 'Security & Auth Dashboard',
      'requires_2fa': 'Two-Factor Authentication Required',
      'active_session': 'Active User Session',
    },
    'prs': {
      'app_title': 'سیستم هویت مارکت‌پلیس',
      'login': 'ورود به سیستم',
      'register': 'ثبت‌نام حساب جدید',
      'email': 'نشانی ایمیل',
      'password': 'رمز عبور',
      'confirm_password': 'تأیید رمز عبور',
      'dont_have_account': 'حساب کاربری ندارید؟ ثبت‌نام کنید',
      'already_have_account': 'قبلاً ثبت‌نام کرده‌اید؟ وارد شوید',
      'submitting': 'در حال پردازش...',
      'logout': 'خروج از حساب',
      'logout_all': 'خروج از تمامی دستگاه‌ها',
      'welcome': 'به مارکت‌پلیس خوش آمدید',
      'language': 'زبان',
      'english': 'English',
      'dari': 'دری',
      'pashto': 'پښتو',
      'access_token': 'توکن دسترسی (JWT)',
      'refresh_token': 'توکن بازنشانی (Refresh Token)',
      'enable_2fa': 'فعالسازی ورود دو مرحله‌ای (2FA)',
      'verify_2fa': 'تأیید کد دو مرحله‌ای',
      'enter_2fa_code': 'کد ۶ رقمی نرم‌افزار تأییدکننده را وارد کنید',
      'security_dashboard': 'داشبورد مدیریت هویت و امنیت',
      'requires_2fa': 'احراز هویت دو مرحله‌ای الزامی است',
      'active_session': 'نشست فعال کاربر',
    },
    'ps': {
      'app_title': 'د مارکیټ پلیس پیژندنې سیسټم',
      'login': 'سیسټم ته ننوتل',
      'register': 'د نوي حساب راجستر کول',
      'email': 'ایمیل پته',
      'password': 'پټنوم',
      'confirm_password': 'د پټنوم تایید',
      'dont_have_account': 'حساب نلرئ؟ راجستر شئ',
      'already_have_account': 'دمخه حساب لرئ؟ ننوځئ',
      'submitting': 'د پروسس په حال کې...',
      'logout': 'له حسابه وتل',
      'logout_all': 'له ټولو وسایلو وتل',
      'welcome': 'مارکیټ پلیس ته ښه راغلاست',
      'language': 'ژبه',
      'english': 'English',
      'dari': 'دری',
      'pashto': 'پښتو',
      'access_token': 'د لاسرسي ټوکن (JWT)',
      'refresh_token': 'د بیا رغونې ټوکن',
      'enable_2fa': 'د دوه مرحلې ننوتلو فعالول (2FA)',
      'verify_2fa': 'د دوه مرحلې کوډ تایید',
      'enter_2fa_code': 'د ۶ رقمي تایید کوډ داخل کړئ',
      'security_dashboard': 'د امنیت او پیژندنې ډشبورډ',
      'requires_2fa': 'دوه مرحلې تصدیق اړین دی',
      'active_session': 'د کارونکي فعاله ناسته',
    },
  };
}
