import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/localization/backend_localization_provider.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../admin/presentation/screens/admin_layout.dart';
import '../../../agency/presentation/screens/agency_application_screen.dart';
import '../../../auth/presentation/auth_provider.dart';
import '../../../auth/presentation/screens/dashboard_screen.dart';
import '../../../cart_checkout/presentation/cart_provider.dart';
import '../../../cart_checkout/presentation/screens/cart_screen.dart';
import '../../../notifications/presentation/notification_provider.dart';
import 'home_screen.dart';
import 'shop_screen.dart';

class MainNavigationScreen extends ConsumerStatefulWidget {
  const MainNavigationScreen({super.key});

  @override
  ConsumerState<MainNavigationScreen> createState() => _MainNavigationScreenState();
}

class _MainNavigationScreenState extends ConsumerState<MainNavigationScreen> {
  int _selectedIndex = 1;
  int _topToggleIndex = 1;
  int _activeCategoryIndex = 0;

  late final List<Widget> _screens = [
    const HomeScreen(),
    const HomeScreen(),
    const ShopScreen(),
    const CartScreen(),
    DashboardScreen(onGoToHome: () => setState(() => _selectedIndex = 1)),
  ];

  void _handleLogout() async {
    await ref.read(authProvider.notifier).logout();
    if (mounted) {
      context.go('/login');
    }
  }

  void _showNotificationsDialog(BuildContext context, String langCode) {
    showDialog(
      context: context,
      builder: (context) {
        return Consumer(
          builder: (context, ref, _) {
            final notificationsAsync = ref.watch(notificationsProvider);
            return AlertDialog(
              title: Row(
                children: [
                  const Icon(Icons.notifications_active, color: AppColors.royalBlue),
                  const SizedBox(width: 8),
                  Text(langCode == 'ps' ? 'اعلانونه او خبرتیاوې' : (langCode == 'prs' || langCode == 'fa' ? 'اعلا‌ن‌های زنده سیستم' : 'Live Notifications')),
                ],
              ),
              content: SizedBox(
                width: 400,
                height: 350,
                child: notificationsAsync.when(
                  loading: () => const Center(child: CircularProgressIndicator()),
                  error: (err, _) => Text('Error loading notifications: $err'),
                  data: (notifications) {
                    if (notifications.isEmpty) {
                      return Center(
                        child: Text(
                          langCode == 'ps' ? 'هیڅ نوی اعلان نشته.' : (langCode == 'prs' || langCode == 'fa' ? 'هیچ اعلان جدیدی وجود ندارد.' : 'No notifications received.'),
                          style: const TextStyle(color: Colors.white60),
                        ),
                      );
                    }
                    return ListView.separated(
                      itemCount: notifications.length,
                      separatorBuilder: (_, __) => const Divider(),
                      itemBuilder: (context, index) {
                        final notif = notifications[index];
                        return ListTile(
                          leading: Container(
                            padding: const EdgeInsets.all(8),
                            decoration: BoxDecoration(
                              color: AppColors.royalBlue.withAlpha(30),
                              shape: BoxShape.circle,
                            ),
                            child: const Icon(Icons.notifications, color: AppColors.royalBlue, size: 20),
                          ),
                          title: Text(
                            notif['title'] ?? 'Notification',
                            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                          ),
                          subtitle: Text(
                            notif['message'] ?? '',
                            style: const TextStyle(fontSize: 12, color: Colors.white70),
                          ),
                        );
                      },
                    );
                  },
                ),
              ),
              actions: [
                TextButton(
                  onPressed: () => Navigator.pop(context),
                  child: Text(langCode == 'ps' ? 'تړل' : (langCode == 'prs' || langCode == 'fa' ? 'بستن' : 'Close')),
                ),
              ],
            );
          },
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final localeNotifier = ref.read(localeProvider.notifier);
    final b10n = ref.watch(backendLocalizationProvider.notifier);
    final authState = ref.watch(authProvider);
    final cartItems = ref.watch(cartProvider);
    final unreadNotifsCount = ref.watch(unreadNotificationsCountProvider);
    final isDesktop = MediaQuery.of(context).size.width > 900;

    return Directionality(
      textDirection: localeNotifier.textDirection,
      child: Scaffold(
        backgroundColor: AppColors.background,
        drawer: !isDesktop ? Drawer(child: _buildSidebarContent(context, b10n, authState)) : null,
        body: _topToggleIndex == 0
            ? const AdminLayout()
            : Row(
                children: [
                  if (isDesktop)
                    Container(
                      width: 250,
                      color: Colors.white,
                      child: _buildSidebarContent(context, b10n, authState),
                    ),
                  Expanded(
                    child: Column(
                      children: [
                        _buildTopHeaderBar(context, b10n, localeNotifier, authState, cartItems.length, unreadNotifsCount),
                        _buildExploreFilterHeader(context, b10n),
                        Expanded(
                          child: _screens[_selectedIndex],
                        ),
                      ],
                    ),
                  ),
                ],
              ),
      ),
    );
  }

  Widget _buildSidebarContent(BuildContext context, dynamic b10n, AuthState authState) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Text(
                b10n.translate('AppName', 'BuyMore'),
                style: const TextStyle(fontSize: 22, fontWeight: FontWeight.w900, color: AppColors.textPrimary),
              ),
              const SizedBox(width: 4),
              Container(width: 14, height: 4, decoration: BoxDecoration(color: AppColors.royalBlue, borderRadius: BorderRadius.circular(2))),
            ],
          ),
          const SizedBox(height: 32),
          _buildSidebarNavItem(0, Icons.bolt_outlined, b10n.translate('PopularProducts', 'Popular Products')),
          _buildSidebarNavItem(1, Icons.explore_outlined, b10n.translate('ExploreNew', 'Explore New')),
          _buildSidebarNavItem(2, Icons.shopping_bag_outlined, b10n.translate('ClothingAndShoes', 'Clothing and Shoes')),
          _buildSidebarNavItem(3, Icons.card_giftcard_outlined, b10n.translate('GiftsAndLiving', 'Gifts and Living')),
          _buildSidebarNavItem(4, Icons.lightbulb_outline, b10n.translate('Inspiration', 'Inspiration')),
          const Padding(
            padding: EdgeInsets.symmetric(vertical: 16),
            child: Divider(color: AppColors.border),
          ),
          Text(b10n.translate('QuickActions', 'Quick actions'), style: const TextStyle(fontSize: 11, color: AppColors.textSecondary, fontWeight: FontWeight.bold)),
          const SizedBox(height: 12),
          _buildQuickActionButton(Icons.verified_user_outlined, b10n.translate('ApplyAgency', 'درخواست اخذ نمایندگی'), () {
            Navigator.push(
              context,
              MaterialPageRoute(builder: (_) => const AgencyApplicationScreen()),
            );
          }),
          const Spacer(),

          // Conditional Auth Button
          if (authState.isAuthenticated)
            InkWell(
              onTap: _handleLogout,
              borderRadius: BorderRadius.circular(12),
              child: const Padding(
                padding: EdgeInsets.symmetric(vertical: 8.0),
                child: Row(
                  children: [
                    Icon(Icons.logout, size: 18, color: AppColors.dangerRed),
                    SizedBox(width: 8),
                    Text('Log out', style: TextStyle(fontSize: 13, fontWeight: FontWeight.bold, color: AppColors.dangerRed)),
                  ],
                ),
              ),
            )
          else
            InkWell(
              onTap: () => context.go('/login'),
              borderRadius: BorderRadius.circular(12),
              child: const Padding(
                padding: EdgeInsets.symmetric(vertical: 8.0),
                child: Row(
                  children: [
                    Icon(Icons.login, size: 18, color: AppColors.royalBlue),
                    SizedBox(width: 8),
                    Text('Log in', style: TextStyle(fontSize: 13, fontWeight: FontWeight.bold, color: AppColors.royalBlue)),
                  ],
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildSidebarNavItem(int index, IconData icon, String title) {
    final isSelected = _selectedIndex == index;
    return Container(
      margin: const EdgeInsets.only(bottom: 8),
      decoration: BoxDecoration(
        color: isSelected ? AppColors.royalBlue : Colors.transparent,
        borderRadius: BorderRadius.circular(18),
      ),
      child: ListTile(
        dense: true,
        contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 2),
        leading: Icon(icon, size: 18, color: isSelected ? Colors.white : AppColors.textPrimary),
        title: Text(
          title,
          style: TextStyle(
            fontSize: 13,
            fontWeight: isSelected ? FontWeight.bold : FontWeight.w500,
            color: isSelected ? Colors.white : AppColors.textPrimary,
          ),
        ),
        onTap: () {
          setState(() {
            _selectedIndex = index;
          });
        },
      ),
    );
  }

  Widget _buildQuickActionButton(IconData icon, String label, VoidCallback onTap) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(8),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(4),
            decoration: BoxDecoration(color: AppColors.pastelGrey, borderRadius: BorderRadius.circular(8)),
            child: Icon(icon, size: 12, color: AppColors.textPrimary),
          ),
          const SizedBox(width: 8),
          Text(label, style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w600, color: AppColors.textPrimary)),
        ],
      ),
    );
  }

  Widget _buildTopHeaderBar(
    BuildContext context,
    dynamic b10n,
    dynamic localeNotifier,
    AuthState authState,
    int cartCount,
    int unreadNotifsCount,
  ) {
    return Container(
      height: 70,
      padding: const EdgeInsets.symmetric(horizontal: 24),
      color: Colors.white,
      child: Row(
        children: [
          Row(
            children: [
              const Text('37', style: TextStyle(fontSize: 26, fontWeight: FontWeight.bold, color: AppColors.textPrimary)),
              const SizedBox(width: 8),
              Column(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(b10n.translate('Orders', 'Orders'), style: const TextStyle(fontSize: 11, fontWeight: FontWeight.bold)),
                  Text(b10n.translate('Last7Days', 'Last 7 days'), style: const TextStyle(fontSize: 10, color: AppColors.textSecondary)),
                ],
              ),
            ],
          ),
          const Spacer(),
          Container(
            padding: const EdgeInsets.all(4),
            decoration: BoxDecoration(
              color: AppColors.pastelGrey,
              borderRadius: BorderRadius.circular(24),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                GestureDetector(
                  onTap: () => setState(() => _topToggleIndex = 0),
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                    decoration: BoxDecoration(
                      color: _topToggleIndex == 0 ? Colors.white : Colors.transparent,
                      borderRadius: BorderRadius.circular(20),
                      boxShadow: _topToggleIndex == 0 ? [BoxShadow(color: Colors.black.withAlpha(10), blurRadius: 4)] : null,
                    ),
                    child: Text(b10n.translate('Dashboard', 'Dashboard'), style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
                  ),
                ),
                GestureDetector(
                  onTap: () => setState(() => _topToggleIndex = 1),
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                    decoration: BoxDecoration(
                      color: _topToggleIndex == 1 ? Colors.white : Colors.transparent,
                      borderRadius: BorderRadius.circular(20),
                      boxShadow: _topToggleIndex == 1 ? [BoxShadow(color: Colors.black.withAlpha(10), blurRadius: 4)] : null,
                    ),
                    child: Text(b10n.translate('Website', 'Website'), style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
                  ),
                ),
              ],
            ),
          ),
          const Spacer(),
          Row(
            children: [
              // Notification Bell Icon with Badge Counter
              IconButton(
                icon: Stack(
                  children: [
                    const Icon(Icons.notifications_outlined, color: AppColors.textPrimary, size: 22),
                    if (unreadNotifsCount > 0)
                      Positioned(
                        right: 0,
                        top: 0,
                        child: Container(
                          padding: const EdgeInsets.all(4),
                          decoration: const BoxDecoration(color: Colors.redAccent, shape: BoxShape.circle),
                          child: Text(
                            '$unreadNotifsCount',
                            style: const TextStyle(color: Colors.white, fontSize: 9, fontWeight: FontWeight.bold),
                          ),
                        ),
                      ),
                  ],
                ),
                onPressed: () => _showNotificationsDialog(context, localeNotifier.locale.languageCode),
              ),
              const SizedBox(width: 8),

              InkWell(
                onTap: () {
                  setState(() => _selectedIndex = 3);
                },
                borderRadius: BorderRadius.circular(20),
                child: Container(
                  padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
                  decoration: BoxDecoration(
                    color: AppColors.pastelGrey,
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Row(
                    children: [
                      const Icon(Icons.shopping_bag_outlined, size: 16),
                      const SizedBox(width: 6),
                      Text('${b10n.translate("Cart", "Cart")} ${cartCount > 0 ? "($cartCount)" : ""}', style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
                    ],
                  ),
                ),
              ),
              const SizedBox(width: 12),

              if (authState.isAuthenticated) ...[
                const CircleAvatar(radius: 14, backgroundImage: NetworkImage('https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=100')),
                const SizedBox(width: 6),
                Text(authState.userName ?? 'Customer', style: const TextStyle(fontSize: 13, fontWeight: FontWeight.bold)),
              ] else ...[
                ElevatedButton.icon(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.royalBlue,
                    foregroundColor: Colors.white,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                  ),
                  onPressed: () => context.go('/login'),
                  icon: const Icon(Icons.login, size: 16),
                  label: Text(b10n.translate('LogIn', 'Log in'), style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
                ),
              ],

              const SizedBox(width: 12),
              PopupMenuButton<String>(
                icon: const Icon(Icons.language, color: AppColors.textPrimary, size: 20),
                onSelected: (code) => localeNotifier.setLocale(code),
                itemBuilder: (context) => const [
                  PopupMenuItem(value: 'prs', child: Text('دری (Dari)')),
                  PopupMenuItem(value: 'ps', child: Text('پښتو (Pashto)')),
                  PopupMenuItem(value: 'en', child: Text('English')),
                ],
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildExploreFilterHeader(BuildContext context, dynamic b10n) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
      color: AppColors.background,
      child: Row(
        children: [
          Text(
            b10n.translate('Explore', 'Explore'),
            style: const TextStyle(fontSize: 26, fontWeight: FontWeight.bold, color: AppColors.textPrimary),
          ),
          const SizedBox(width: 32),
          Container(
            padding: const EdgeInsets.all(4),
            decoration: BoxDecoration(
              color: AppColors.pastelGrey,
              borderRadius: BorderRadius.circular(24),
            ),
            child: Row(
              children: [
                _buildCategoryPill(0, Icons.check_box_outline_blank, b10n.translate('All', 'All')),
                _buildCategoryPill(1, Icons.man, b10n.translate('Men', 'Men')),
                _buildCategoryPill(2, Icons.woman, b10n.translate('Women', 'Women')),
              ],
            ),
          ),
          const Spacer(),
          InkWell(
            onTap: () {
              setState(() => _selectedIndex = 2);
            },
            child: Container(
              padding: const EdgeInsets.all(10),
              decoration: const BoxDecoration(color: Colors.white, shape: BoxShape.circle),
              child: const Icon(Icons.search, size: 18, color: AppColors.textPrimary),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildCategoryPill(int index, IconData icon, String label) {
    final isSelected = _activeCategoryIndex == index;
    return GestureDetector(
      onTap: () => setState(() => _activeCategoryIndex = index),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        decoration: BoxDecoration(
          color: isSelected ? Colors.white : Colors.transparent,
          borderRadius: BorderRadius.circular(20),
          boxShadow: isSelected ? [BoxShadow(color: Colors.black.withAlpha(10), blurRadius: 4)] : null,
        ),
        child: Row(
          children: [
            Icon(icon, size: 14, color: isSelected ? AppColors.textPrimary : AppColors.textSecondary),
            const SizedBox(width: 6),
            Text(
              label,
              style: TextStyle(
                fontSize: 12,
                fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
                color: isSelected ? AppColors.textPrimary : AppColors.textSecondary,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
