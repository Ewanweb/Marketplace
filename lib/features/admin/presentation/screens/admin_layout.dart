import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../shared/widgets/responsive_layout.dart';
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

    return Directionality(
      textDirection: localeNotifier.textDirection,
      child: Scaffold(
        appBar: AppBar(
          title: Text(
            locale.languageCode == 'ps'
                ? 'د مدیریت پینل'
                : (locale.languageCode == 'prs' || locale.languageCode == 'fa' ? 'پنل مدیریت بازار' : 'Marketplace Admin Panel'),
          ),
          backgroundColor: Theme.of(context).colorScheme.surface,
          actions: [
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
        drawer: !isDesktop ? _buildDrawer(context, locale.languageCode) : null,
        body: Row(
          children: [
            if (isDesktop)
              NavigationRail(
                selectedIndex: _selectedIndex,
                onDestinationSelected: (index) {
                  setState(() => _selectedIndex = index);
                },
                labelType: NavigationRailLabelType.all,
                destinations: [
                  NavigationRailDestination(
                    icon: const Icon(Icons.dashboard_outlined),
                    selectedIcon: const Icon(Icons.dashboard),
                    label: Text(locale.languageCode == 'ps' ? 'ډشبورډ' : 'Dashboard'),
                  ),
                  NavigationRailDestination(
                    icon: const Icon(Icons.inventory_2_outlined),
                    selectedIcon: const Icon(Icons.inventory_2),
                    label: Text(locale.languageCode == 'ps' ? 'توکي' : 'Products'),
                  ),
                  NavigationRailDestination(
                    icon: const Icon(Icons.shopping_bag_outlined),
                    selectedIcon: const Icon(Icons.shopping_bag),
                    label: Text(locale.languageCode == 'ps' ? 'فرمایشونه' : 'Orders'),
                  ),
                ],
              ),
            Expanded(child: _adminPages[_selectedIndex]),
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
        ],
      ),
    );
  }
}
