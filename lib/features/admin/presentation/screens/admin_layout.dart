import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../shared/widgets/responsive_layout.dart';
import '../../../agency/presentation/screens/agency_application_screen.dart';
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
  ];

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final localeNotifier = ref.read(localeProvider.notifier);
    final isDesktop = ResponsiveLayout.isDesktop(context);
    final langCode = locale.languageCode;

    return Directionality(
      textDirection: localeNotifier.textDirection,
      child: Scaffold(
        appBar: AppBar(
          title: Text(
            langCode == 'ps'
                ? 'د مدیریت پینل'
                : (langCode == 'prs' || langCode == 'fa' ? 'پنل مدیریت بازار' : 'Marketplace Admin Panel'),
          ),
          backgroundColor: Theme.of(context).colorScheme.surface,
          actions: [
            TextButton.icon(
              style: TextButton.styleFrom(foregroundColor: Colors.white),
              icon: const Icon(Icons.store, color: Colors.greenAccent),
              label: Text(langCode == 'ps' ? 'اصلي پاڼه' : (langCode == 'prs' || langCode == 'fa' ? 'صفحه اصلی فروشگاه' : 'Main Store')),
              onPressed: () => context.go('/'),
            ),
            const SizedBox(width: 8),
            IconButton(
              icon: const Icon(Icons.person_outline),
              tooltip: 'Profile Settings',
              onPressed: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(builder: (_) => const ProfileScreen()),
                );
              },
            ),
            IconButton(
              icon: const Icon(Icons.verified_user_outlined),
              tooltip: 'Agency Application',
              onPressed: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(builder: (_) => const AgencyApplicationScreen()),
                );
              },
            ),
            PopupMenuButton<String>(
              icon: const Icon(Icons.language, color: Colors.white70),
              onSelected: (code) => localeNotifier.setLocale(code),
              itemBuilder: (context) => const [
                PopupMenuItem(value: 'prs', child: Text('دری (Dari)')),
                PopupMenuItem(value: 'ps', child: Text('پښتو (Pashto)')),
                PopupMenuItem(value: 'en', child: Text('English')),
              ],
            ),
          ],
        ),
        drawer: !isDesktop ? _buildDrawer(context, langCode) : null,
        body: Row(
          children: [
            if (isDesktop)
              NavigationRail(
                selectedIndex: _selectedIndex < 3 ? _selectedIndex : 0,
                onDestinationSelected: (index) {
                  if (index == 3) {
                    Navigator.push(context, MaterialPageRoute(builder: (_) => const ProfileScreen()));
                  } else if (index == 4) {
                    Navigator.push(context, MaterialPageRoute(builder: (_) => const AgencyApplicationScreen()));
                  } else if (index == 5) {
                    context.go('/');
                  } else {
                    setState(() => _selectedIndex = index);
                  }
                },
                labelType: NavigationRailLabelType.all,
                destinations: [
                  NavigationRailDestination(
                    icon: const Icon(Icons.dashboard_outlined),
                    selectedIcon: const Icon(Icons.dashboard),
                    label: Text(langCode == 'ps' ? 'ډشبورډ' : 'Dashboard'),
                  ),
                  NavigationRailDestination(
                    icon: const Icon(Icons.inventory_2_outlined),
                    selectedIcon: const Icon(Icons.inventory_2),
                    label: Text(langCode == 'ps' ? 'توکي' : 'Products'),
                  ),
                  NavigationRailDestination(
                    icon: const Icon(Icons.shopping_bag_outlined),
                    selectedIcon: const Icon(Icons.shopping_bag),
                    label: Text(langCode == 'ps' ? 'فرمایشونه' : 'Orders'),
                  ),
                  NavigationRailDestination(
                    icon: const Icon(Icons.person_outline),
                    selectedIcon: const Icon(Icons.person),
                    label: Text(langCode == 'ps' ? 'پروفایل' : (langCode == 'prs' || langCode == 'fa' ? 'پروفایل' : 'Profile')),
                  ),
                  NavigationRailDestination(
                    icon: const Icon(Icons.verified_user_outlined),
                    selectedIcon: const Icon(Icons.verified_user),
                    label: Text(langCode == 'ps' ? 'نمایندګي' : (langCode == 'prs' || langCode == 'fa' ? 'نمایندگی' : 'Agency')),
                  ),
                  NavigationRailDestination(
                    icon: const Icon(Icons.store_outlined),
                    selectedIcon: const Icon(Icons.store),
                    label: Text(langCode == 'ps' ? 'اصلي پاڼه' : (langCode == 'prs' || langCode == 'fa' ? 'صفحه اصلی' : 'Main Store')),
                  ),
                ],
              ),
            Expanded(child: _selectedIndex < 3 ? _adminPages[_selectedIndex] : _adminPages[0]),
          ],
        ),
      ),
    );
  }

  Widget _buildDrawer(BuildContext context, String langCode) {
    return Drawer(
      child: ListView(
        padding: EdgeInsets.zero,
        children: [
          DrawerHeader(
            decoration: BoxDecoration(
              color: Theme.of(context).colorScheme.surface,
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                const Icon(Icons.admin_panel_settings, size: 40, color: Color(0xFF6C5CE7)),
                const SizedBox(height: 12),
                Text(
                  langCode == 'ps' ? 'د مدیریت پینل' : (langCode == 'prs' || langCode == 'fa' ? 'پنل مدیریت' : 'Admin Management'),
                  style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18),
                ),
              ],
            ),
          ),
          ListTile(
            leading: const Icon(Icons.dashboard),
            title: Text(langCode == 'ps' ? 'ډشبورډ' : 'Dashboard'),
            selected: _selectedIndex == 0,
            onTap: () {
              setState(() => _selectedIndex = 0);
              Navigator.pop(context);
            },
          ),
          ListTile(
            leading: const Icon(Icons.inventory_2),
            title: Text(langCode == 'ps' ? 'توکي' : 'Products'),
            selected: _selectedIndex == 1,
            onTap: () {
              setState(() => _selectedIndex = 1);
              Navigator.pop(context);
            },
          ),
          ListTile(
            leading: const Icon(Icons.shopping_bag),
            title: Text(langCode == 'ps' ? 'فرمایشونه' : 'Orders'),
            selected: _selectedIndex == 2,
            onTap: () {
              setState(() => _selectedIndex = 2);
              Navigator.pop(context);
            },
          ),
          const Divider(),
          ListTile(
            leading: const Icon(Icons.person_outline),
            title: Text(langCode == 'ps' ? 'ویرایش پروفایل' : (langCode == 'prs' || langCode == 'fa' ? 'ویرایش پروفایل من' : 'Edit Profile')),
            onTap: () {
              Navigator.pop(context);
              Navigator.push(context, MaterialPageRoute(builder: (_) => const ProfileScreen()));
            },
          ),
          ListTile(
            leading: const Icon(Icons.verified_user_outlined),
            title: Text(langCode == 'ps' ? 'د نمایندګۍ غوښتنه' : (langCode == 'prs' || langCode == 'fa' ? 'درخواست اخذ نمایندگی' : 'Agency Application')),
            onTap: () {
              Navigator.pop(context);
              Navigator.push(context, MaterialPageRoute(builder: (_) => const AgencyApplicationScreen()));
            },
          ),
          ListTile(
            leading: const Icon(Icons.store, color: Colors.greenAccent),
            title: Text(langCode == 'ps' ? 'صفحه اصلی فروشگاه' : (langCode == 'prs' || langCode == 'fa' ? 'بازگشت به صفحه اصلی' : 'Back to Main Store')),
            onTap: () {
              Navigator.pop(context);
              context.go('/');
            },
          ),
        ],
      ),
    );
  }
}
