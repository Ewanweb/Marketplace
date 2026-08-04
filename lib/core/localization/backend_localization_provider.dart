import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../network/api_client.dart';

final backendLocalizationProvider = StateNotifierProvider<BackendLocalizationNotifier, Map<String, String>>((ref) {
  return BackendLocalizationNotifier(ref);
});

class BackendLocalizationNotifier extends StateNotifier<Map<String, String>> {
  final Ref _ref;
  String _currentLangCode = 'prs';

  BackendLocalizationNotifier(this._ref) : super(Map.from(_dariFallbackStrings)) {
    _initLocaleAndFetch();
  }

  Future<void> _initLocaleAndFetch() async {
    final prefs = await SharedPreferences.getInstance();
    _currentLangCode = prefs.getString('selected_app_locale') ?? 'prs';
    state = Map.from(_getFallbackMap(_currentLangCode));
    fetchBackendTranslations(_currentLangCode);
  }

  Future<void> fetchBackendTranslations(String langCode) async {
    _currentLangCode = langCode;
    state = Map.from(_getFallbackMap(langCode));

    try {
      final apiClient = _ref.read(apiClientProvider);
      final translations = await apiClient.getLocalizationStrings(langCode);
      if (translations != null && translations.isNotEmpty) {
        state = Map.from(translations);
      }
    } catch (_) {
      // Retain fallback map
    }
  }

  String translate(String key, [String fallback = '']) {
    if (state.containsKey(key)) {
      return state[key]!;
    }
    return fallback.isNotEmpty ? fallback : key;
  }

  static Map<String, String> _getFallbackMap(String langCode) {
    final code = langCode.toLowerCase();
    if (code == 'ps' || code.startsWith('ps')) {
      return _pashtoFallbackStrings;
    }
    if (code == 'en' || code.startsWith('en')) {
      return _englishFallbackStrings;
    }
    return _dariFallbackStrings;
  }

  static const Map<String, String> _pashtoFallbackStrings = {
    "AppName": "د نورزي بازار",
    "PopularProducts": "مشهور توکي",
    "ExploreNew": "نوې موندنې",
    "ClothingAndShoes": "جامې او بوټان",
    "GiftsAndLiving": "ډالۍ او د کور سامان",
    "Inspiration": "الهام",
    "QuickActions": "چټک اقدامات",
    "RequestForProduct": "د توکي غوښتنه",
    "AddMember": "عضو زیاتول",
    "LastOrders": "وروستي فرمایشونه 37",
    "LogIn": "حساب ته ننوتل",
    "SignUp": "نوم لیکنه",
    "LogOut": "وتل",
    "GetUpTo50Off": "تر ۵۰٪ پورې تخفیف ترلاسه کړئ",
    "GetDiscount": "تخفیف ترلاسه کول",
    "WinterWeekend": "د ژمي رخصتۍ",
    "KeepItCasual": "کژوال او هوسا سټایل",
    "OurPicks": "زموږ انتخاب",
    "YourChoice": "ستاسو انتخاب",
    "AvailOffers": "وړاندیزونه وګورئ",
    "Favourites": "غوره شوي",
    "SeeAll": "ټول لیدل",
    "BringBoldFashion": "په زړه پورې فیشن",
    "LayersOnLayers": "لایه په لایه سټایلونه",
    "Orders": "فرمایشونه",
    "Last7Days": "وروستۍ ۷ ورځې",
    "Dashboard": "ډشبورډ",
    "Website": "ویب پاڼه",
    "Cart": "ټوکرۍ",
    "Explore": "کاوش",
    "All": "ټول",
    "Men": "سړي",
    "Women": "ښځې",
    "Filters": "فیلټرونه",
    "Search": "لټون"
  };

  static const Map<String, String> _dariFallbackStrings = {
    "AppName": "بازار نورزی",
    "PopularProducts": "محصولات محبوب",
    "ExploreNew": "کشف جدید",
    "ClothingAndShoes": "پوشاک و کفش",
    "GiftsAndLiving": "هدایا و لوازم منزل",
    "Inspiration": "الهام‌بخش",
    "QuickActions": "اقدامات سریع",
    "RequestForProduct": "درخواست محصول",
    "AddMember": "افزودن عضو",
    "LastOrders": "سفارشات اخیر 37",
    "LogIn": "ورود به حساب",
    "SignUp": "ثبت‌نام",
    "LogOut": "خروج",
    "GetUpTo50Off": "تا ۵۰٪ تخفیف طلایی دریافت کنید",
    "GetDiscount": "دریافت تخفیف",
    "WinterWeekend": "تعطیلات زمستانی",
    "KeepItCasual": "استایل راحت و خاص",
    "OurPicks": "انتخاب ما",
    "YourChoice": "انتخاب شما",
    "AvailOffers": "مشاهده پیشنهادات",
    "Favourites": "محبوب‌ترین‌ها",
    "SeeAll": "مشاهده همه",
    "BringBoldFashion": "مد و فیشن جسورانه",
    "LayersOnLayers": "لایه در لایه و خاص",
    "Orders": "سفارشات",
    "Last7Days": "۷ روز گذشته",
    "Dashboard": "داشبورد",
    "Website": "وب‌سایت",
    "Cart": "سبد خرید",
    "Explore": "کاوش",
    "All": "همه",
    "Men": "مردانه",
    "Women": "زنانه",
    "Filters": "فیلترها",
    "Search": "جستجو"
  };

  static const Map<String, String> _englishFallbackStrings = {
    "AppName": "BuyMore",
    "PopularProducts": "Popular Products",
    "ExploreNew": "Explore New",
    "ClothingAndShoes": "Clothing and Shoes",
    "GiftsAndLiving": "Gifts and Living",
    "Inspiration": "Inspiration",
    "QuickActions": "Quick actions",
    "RequestForProduct": "Request for product",
    "AddMember": "Add member",
    "LastOrders": "Last orders 37",
    "LogIn": "Log in",
    "SignUp": "Sign up",
    "LogOut": "Log out",
    "GetUpTo50Off": "GET UP TO 50% OFF",
    "GetDiscount": "Get Discount",
    "WinterWeekend": "Winter's weekend",
    "KeepItCasual": "keep it casual",
    "OurPicks": "Our Picks",
    "YourChoice": "Your Choice",
    "AvailOffers": "Avail Offers",
    "Favourites": "Favourites",
    "SeeAll": "See All",
    "BringBoldFashion": "Bring Bold Fashion",
    "LayersOnLayers": "Layers on Layers",
    "Orders": "Orders",
    "Last7Days": "Last 7 days",
    "Dashboard": "Dashboard",
    "Website": "Website",
    "Cart": "Cart",
    "Explore": "Explore",
    "All": "All",
    "Men": "Men",
    "Women": "Women",
    "Filters": "Filters",
    "Search": "Search"
  };
}
