import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../network/api_client.dart';

final backendLocalizationProvider = StateNotifierProvider<BackendLocalizationNotifier, Map<String, String>>((ref) {
  return BackendLocalizationNotifier(ref);
});

class BackendLocalizationNotifier extends StateNotifier<Map<String, String>> {
  final Ref _ref;

  BackendLocalizationNotifier(this._ref) : super(_defaultFallbackStrings) {
    fetchBackendTranslations('prs'); // Default to Dari
  }

  Future<void> fetchBackendTranslations(String langCode) async {
    try {
      final apiClient = _ref.read(apiClientProvider);
      final translations = await apiClient.getLocalizationStrings(langCode);
      if (translations != null && translations.isNotEmpty) {
        state = translations;
      }
    } catch (_) {
      // Retain fallback if offline or loading
    }
  }

  String translate(String key, [String fallback = '']) {
    return state[key] ?? (fallback.isNotEmpty ? fallback : key);
  }

  static const Map<String, String> _defaultFallbackStrings = {
    "AppName": "بازار نورزی",
    "PopularProducts": "محصولات محبوب",
    "ExploreNew": "کشف جدید",
    "ClothingAndShoes": "پوشاک و کفش",
    "GiftsAndLiving": "هدایا و لوازم منزل",
    "Inspiration": "الهام‌بخش",
    "QuickActions": "اقدامات سریع",
    "RequestForProduct": "درخواست محصول",
    "AddMember": "افزودن عضو",
    "LastOrders": "سفارشات اخیر",
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
}
