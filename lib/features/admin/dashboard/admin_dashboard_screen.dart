import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../shared/widgets/custom_card.dart';
import '../../agency/presentation/screens/agency_application_screen.dart';
import '../../auth/presentation/screens/profile_screen.dart';

class AdminDashboardScreen extends ConsumerWidget {
  const AdminDashboardScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;

    return ListView(
      padding: const EdgeInsets.all(24),
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              langCode == 'ps' ? 'د پلور عمومي لید' : (langCode == 'prs' || langCode == 'fa' ? 'نمای کلی فروش و عملکرد' : 'Sales Overview & Analytics'),
              style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
            ),
            Wrap(
              spacing: 8,
              children: [
                OutlinedButton.icon(
                  icon: const Icon(Icons.person_outline, size: 16),
                  label: Text(langCode == 'ps' ? 'پروفایل' : (langCode == 'prs' || langCode == 'fa' ? 'ویرایش پروفایل' : 'Profile')),
                  onPressed: () {
                    Navigator.push(context, MaterialPageRoute(builder: (_) => const ProfileScreen()));
                  },
                ),
                OutlinedButton.icon(
                  icon: const Icon(Icons.verified_user_outlined, size: 16),
                  label: Text(langCode == 'ps' ? 'نمایندګي' : (langCode == 'prs' || langCode == 'fa' ? 'درخواست نمایندگی' : 'Agency')),
                  onPressed: () {
                    Navigator.push(context, MaterialPageRoute(builder: (_) => const AgencyApplicationScreen()));
                  },
                ),
                ElevatedButton.icon(
                  icon: const Icon(Icons.store, size: 16),
                  label: Text(langCode == 'ps' ? 'اصلي پاڼه' : (langCode == 'prs' || langCode == 'fa' ? 'صفحه اصلی فروشگاه' : 'Home Page')),
                  onPressed: () => context.go('/'),
                ),
              ],
            ),
          ],
        ),
        const SizedBox(height: 20),

        // Metrics Grid
        GridView.count(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          crossAxisCount: MediaQuery.of(context).size.width > 900 ? 4 : 2,
          crossAxisSpacing: 16,
          mainAxisSpacing: 16,
          childAspectRatio: 1.4,
          children: [
            _buildMetricCard(
              title: langCode == 'ps' ? 'ټول عاید' : (langCode == 'prs' || langCode == 'fa' ? 'درآمد کل' : 'Total Revenue'),
              value: '\$14,250.00',
              icon: Icons.attach_money,
              color: Colors.greenAccent,
            ),
            _buildMetricCard(
              title: langCode == 'ps' ? 'فعال فرمایشونه' : (langCode == 'prs' || langCode == 'fa' ? 'سفارشات فعال' : 'Active Orders'),
              value: '38 Orders',
              icon: Icons.shopping_bag_outlined,
              color: const Color(0xFFA29BFE),
            ),
            _buildMetricCard(
              title: langCode == 'ps' ? 'ګودام خبرداری' : (langCode == 'prs' || langCode == 'fa' ? 'هشدار موجودی' : 'Low Stock'),
              value: '3 Items',
              icon: Icons.warning_amber_rounded,
              color: Colors.orangeAccent,
            ),
            _buildMetricCard(
              title: langCode == 'ps' ? 'ټول کاروونکي' : (langCode == 'prs' || langCode == 'fa' ? 'مشتریان کل' : 'Total Customers'),
              value: '1,420 Users',
              icon: Icons.people_outline,
              color: Colors.lightBlueAccent,
            ),
          ],
        ),
        const SizedBox(height: 32),

        // Analytics Progress Section
        CustomCard(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                langCode == 'ps' ? 'د میاشتني هدف پرمختګ' : (langCode == 'prs' || langCode == 'fa' ? 'پیشرفت هدف ماهانه' : 'Monthly Goal Progress'),
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
              ),
              const SizedBox(height: 16),
              LinearProgressIndicator(
                value: 0.78,
                minHeight: 12,
                borderRadius: BorderRadius.circular(8),
                backgroundColor: Colors.white10,
                color: const Color(0xFF6C5CE7),
              ),
              const SizedBox(height: 12),
              const Align(
                alignment: Alignment.centerRight,
                child: Text(
                  '78% Reached',
                  style: TextStyle(color: Color(0xFFA29BFE), fontWeight: FontWeight.bold, fontSize: 12),
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildMetricCard({
    required String title,
    required String value,
    required IconData icon,
    required Color color,
  }) {
    return CustomCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Icon(icon, color: color, size: 28),
            ],
          ),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                value,
                style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 4),
              Text(
                title,
                style: const TextStyle(fontSize: 12, color: Colors.white70),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
