import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../shared/widgets/responsive_layout.dart';
import '../../agency/admin_agency_screen.dart';
import '../../../auth/presentation/screens/profile_screen.dart';
import '../../dashboard/admin_dashboard_screen.dart';
import '../../orders/admin_orders_screen.dart';
import '../../products/admin_products_screen.dart';

class AdminLayout extends ConsumerStatefulWidget {
  const AdminLayout({super.key});

  @override
  ConsumerState<AdminLayout> createState() => _AdminLayoutState();
}

class _AdminLayoutState extends ConsumerState<AdminLayout> {
  int _selectedIndex = 0;

  final List<Widget> _adminPages = const [
    AdminDashboardScreen(),
    AdminProductsScreen(),
    AdminOrdersScreen(),
    AdminAgencyScreen(),
  ];

  final List<String> _pageTitlesEn = const [
    'Main Dashboard',
    'Product Management',
    'Order Management',
    'Agency Applications',
  ];

  final List<String> _pageTitlesPrs = const [
    'داشبورد اصلی',
    'مدیریت و لیست محصولات',
    'مدیریت و وضعیت سفارشات',
    'درخواست‌های نمایندگی رسمی',
  ];

  final List<String> _pageTitlesPs = const [
    'اصلي ډشبورډ',
    'د توکو مدیریت',
    'د فرمایشونو مدیریت',
    'د نمایندګیو غوښتنلیکونه',
  ];

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final localeNotifier = ref.read(localeProvider.notifier);
    final isDesktop = ResponsiveLayout.isDesktop(context);
    final langCode = locale.languageCode;

    final pageTitle = langCode == 'ps'
        ? _pageTitlesPs[_selectedIndex]
        : (langCode == 'prs' || langCode == 'fa'
            ? _pageTitlesPrs[_selectedIndex]
            : _pageTitlesEn[_selectedIndex]);

    return Directionality(
      textDirection: localeNotifier.textDirection,
      child: Scaffold(
        backgroundColor: AppColors.horizonBg,
        drawer: !isDesktop ? _buildRtlDrawer(context, langCode) : null,
        body: Row(
          children: [
            // Right-aligned Sidebar in RTL
            if (isDesktop) _buildHorizonSidebar(context, langCode),

            // Main Content Area
            Expanded(
              child: Column(
                children: [
                  // Floating Top Header (Horizon Style)
                  _buildHorizonTopHeader(context, pageTitle, langCode, localeNotifier),

                  // Active Page
                  Expanded(
                    child: _selectedIndex < _adminPages.length
                        ? _adminPages[_selectedIndex]
                        : _adminPages[0],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  // Horizon Top Header
  Widget _buildHorizonTopHeader(
    BuildContext context,
    String pageTitle,
    String langCode,
    LocaleNotifier localeNotifier,
  ) {
    return Container(
      margin: const EdgeInsets.fromLTRB(24, 16, 24, 0),
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
      decoration: BoxDecoration(
        color: AppColors.horizonCard.withAlpha(220),
        borderRadius: BorderRadius.circular(20),
        boxShadow: const [AppColors.horizonShadow],
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          // Right Side: Breadcrumbs & Page Title
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(
                langCode == 'ps' ? 'مدیریت / پاڼې' : (langCode == 'prs' || langCode == 'fa' ? 'صفحات / مدیریت' : 'Pages / Admin'),
                style: const TextStyle(
                  fontSize: 12,
                  color: AppColors.horizonMuted,
                  fontWeight: FontWeight.w500,
                ),
              ),
              const SizedBox(height: 2),
              Text(
                pageTitle,
                style: const TextStyle(
                  fontSize: 20,
                  fontWeight: FontWeight.w800,
                  color: AppColors.horizonNavy,
                ),
              ),
            ],
          ),

          // Left Side: Search, Quick Actions, User Avatar
          Row(
            children: [
              // Search Input
              Container(
                width: 200,
                height: 38,
                padding: const EdgeInsets.symmetric(horizontal: 12),
                decoration: BoxDecoration(
                  color: AppColors.horizonBg,
                  borderRadius: BorderRadius.circular(30),
                ),
                child: Row(
                  children: [
                    const Icon(Icons.search, size: 18, color: AppColors.horizonMuted),
                    const SizedBox(width: 8),
                    Expanded(
                      child: TextField(
                        style: const TextStyle(fontSize: 12, color: AppColors.horizonNavy),
                        decoration: InputDecoration(
                          hintText: langCode == 'ps' ? 'پلټنه...' : (langCode == 'prs' || langCode == 'fa' ? 'جستجو...' : 'Search...'),
                          hintStyle: const TextStyle(fontSize: 12, color: AppColors.horizonMuted),
                          border: InputBorder.none,
                          isDense: true,
                          contentPadding: EdgeInsets.zero,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 12),

              // Main Store Link Button
              TextButton.icon(
                style: TextButton.styleFrom(
                  backgroundColor: AppColors.horizonBrand.withAlpha(15),
                  foregroundColor: AppColors.horizonBrand,
                  padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                ),
                icon: const Icon(Icons.storefront_rounded, size: 18),
                label: Text(
                  langCode == 'ps' ? 'اصلي پاڼه' : (langCode == 'prs' || langCode == 'fa' ? 'فروشگاه' : 'Store'),
                  style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 12),
                ),
                onPressed: () => context.go('/'),
              ),
              const SizedBox(width: 8),

              // Language Dropdown
              PopupMenuButton<String>(
                icon: const Icon(Icons.language_rounded, color: AppColors.horizonMuted, size: 20),
                onSelected: (code) => localeNotifier.setLocale(code),
                itemBuilder: (context) => const [
                  PopupMenuItem(value: 'prs', child: Text('دری (Dari)')),
                  PopupMenuItem(value: 'ps', child: Text('پښتو (Pashto)')),
                  PopupMenuItem(value: 'en', child: Text('English')),
                ],
              ),

              // Profile Avatar
              GestureDetector(
                onTap: () {
                  Navigator.push(context, MaterialPageRoute(builder: (_) => const ProfileScreen()));
                },
                child: const CircleAvatar(
                  radius: 18,
                  backgroundColor: AppColors.horizonBrand,
                  child: Text(
                    'AD',
                    style: TextStyle(color: Colors.white, fontSize: 12, fontWeight: FontWeight.bold),
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  // Horizon Sidebar Component
  Widget _buildHorizonSidebar(BuildContext context, String langCode) {
    return Container(
      width: 260,
      margin: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.horizonCard,
        borderRadius: BorderRadius.circular(24),
        boxShadow: const [AppColors.horizonShadow],
      ),
      child: Column(
        children: [
          // Logo & Brand Header
          Padding(
            padding: const EdgeInsets.fromLTRB(24, 32, 24, 24),
            child: Row(
              children: [
                Container(
                  padding: const EdgeInsets.all(8),
                  decoration: BoxDecoration(
                    color: AppColors.horizonBrand,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: const Icon(Icons.bolt, color: Colors.white, size: 20),
                ),
                const SizedBox(width: 12),
                const Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'NOORZAI',
                      style: TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.w900,
                        color: AppColors.horizonNavy,
                        letterSpacing: 1.2,
                      ),
                    ),
                    Text(
                      'PRO ADMIN PANEL',
                      style: TextStyle(
                        fontSize: 10,
                        fontWeight: FontWeight.bold,
                        color: AppColors.horizonMuted,
                        letterSpacing: 0.8,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          const Divider(height: 1, color: Color(0xFFF0F3F8)),
          const SizedBox(height: 16),

          // Nav Items
          Expanded(
            child: ListView(
              padding: const EdgeInsets.symmetric(horizontal: 12),
              children: [
                _buildSidebarNavItem(
                  index: 0,
                  icon: Icons.grid_view_rounded,
                  label: langCode == 'ps' ? 'اصلي ډشبورډ' : (langCode == 'prs' || langCode == 'fa' ? 'داشبورد اصلی' : 'Main Dashboard'),
                ),
                _buildSidebarNavItem(
                  index: 1,
                  icon: Icons.inventory_2_rounded,
                  label: langCode == 'ps' ? 'توکي' : (langCode == 'prs' || langCode == 'fa' ? 'مدیریت محصولات' : 'Products'),
                ),
                _buildSidebarNavItem(
                  index: 2,
                  icon: Icons.shopping_bag_rounded,
                  label: langCode == 'ps' ? 'فرمایشونه' : (langCode == 'prs' || langCode == 'fa' ? 'مدیریت سفارشات' : 'Orders'),
                ),
                _buildSidebarNavItem(
                  index: 3,
                  icon: Icons.verified_user_rounded,
                  label: langCode == 'ps' ? 'نمایندګي' : (langCode == 'prs' || langCode == 'fa' ? 'درخواست‌های نمایندگی' : 'Agency Applications'),
                ),
              ],
            ),
          ),

          // Bottom Horizon PRO Card
          Padding(
            padding: const EdgeInsets.all(16.0),
            child: Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                gradient: const LinearGradient(
                  colors: [Color(0xFF4318FF), Color(0xFF868CFF)],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                ),
                borderRadius: BorderRadius.circular(20),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Container(
                    padding: const EdgeInsets.all(8),
                    decoration: const BoxDecoration(
                      color: Colors.white24,
                      shape: BoxShape.circle,
                    ),
                    child: const Icon(Icons.star_rounded, color: Colors.white, size: 18),
                  ),
                  const SizedBox(height: 12),
                  Text(
                    langCode == 'ps' ? 'د پرو نسخې ته لوړول' : (langCode == 'prs' || langCode == 'fa' ? 'ارتقا به نسخه Pro' : 'Upgrade to PRO'),
                    style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 14),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    langCode == 'ps' ? 'د پلور نوي راپورونه او انالیټیکس' : (langCode == 'prs' || langCode == 'fa' ? 'دسترسی به گزارشات پیشرفته' : 'Get access to advanced analytics'),
                    style: const TextStyle(color: Colors.white70, fontSize: 11),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  // Sidebar Nav Item Widget
  Widget _buildSidebarNavItem({
    required int index,
    required IconData icon,
    required String label,
  }) {
    final isSelected = _selectedIndex == index;

    return Padding(
      padding: const EdgeInsets.only(bottom: 6),
      child: InkWell(
        onTap: () => setState(() => _selectedIndex = index),
        borderRadius: BorderRadius.circular(16),
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
          decoration: BoxDecoration(
            color: isSelected ? AppColors.horizonBrand.withAlpha(12) : Colors.transparent,
            borderRadius: BorderRadius.circular(16),
          ),
          child: Row(
            children: [
              Icon(
                icon,
                size: 20,
                color: isSelected ? AppColors.horizonBrand : AppColors.horizonMuted,
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Text(
                  label,
                  style: TextStyle(
                    fontSize: 13,
                    fontWeight: isSelected ? FontWeight.bold : FontWeight.w500,
                    color: isSelected ? AppColors.horizonNavy : AppColors.horizonMuted,
                  ),
                ),
              ),

              // Horizon Active Bar Line on the inner edge
              if (isSelected)
                Container(
                  width: 4,
                  height: 20,
                  decoration: BoxDecoration(
                    color: AppColors.horizonBrand,
                    borderRadius: BorderRadius.circular(4),
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }

  // Drawer for Mobile View
  Widget _buildRtlDrawer(BuildContext context, String langCode) {
    return Drawer(
      backgroundColor: AppColors.horizonCard,
      child: Column(
        children: [
          DrawerHeader(
            decoration: const BoxDecoration(color: AppColors.horizonBrand),
            child: Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.bolt, color: Colors.white, size: 36),
                  const SizedBox(height: 8),
                  Text(
                    langCode == 'ps' ? 'د نورزي مدیریت' : 'Noorzai Horizon Admin',
                    style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 16),
                  ),
                ],
              ),
            ),
          ),
          ListTile(
            leading: const Icon(Icons.grid_view_rounded, color: AppColors.horizonBrand),
            title: Text(langCode == 'ps' ? 'اصلي ډشبورډ' : 'Dashboard'),
            onTap: () {
              setState(() => _selectedIndex = 0);
              Navigator.pop(context);
            },
          ),
          ListTile(
            leading: const Icon(Icons.inventory_2_rounded, color: AppColors.horizonBrand),
            title: Text(langCode == 'ps' ? 'توکي' : 'Products'),
            onTap: () {
              setState(() => _selectedIndex = 1);
              Navigator.pop(context);
            },
          ),
          ListTile(
            leading: const Icon(Icons.shopping_bag_rounded, color: AppColors.horizonBrand),
            title: Text(langCode == 'ps' ? 'فرمایشونه' : 'Orders'),
            onTap: () {
              setState(() => _selectedIndex = 2);
              Navigator.pop(context);
            },
          ),
          ListTile(
            leading: const Icon(Icons.verified_user_rounded, color: AppColors.horizonBrand),
            title: Text(langCode == 'ps' ? 'نمایندګي' : 'Agency'),
            onTap: () {
              setState(() => _selectedIndex = 3);
              Navigator.pop(context);
            },
          ),
        ],
      ),
    );
  }
}
