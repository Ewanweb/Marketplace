import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../shared/widgets/custom_card.dart';

class AdminDashboardScreen extends ConsumerWidget {
  const AdminDashboardScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;

    return ListView(
      padding: const EdgeInsets.all(24),
      children: [
        Text(
          langCode == 'ps' ? 'د پلور عمومي لید' : (langCode == 'prs' || langCode == 'fa' ? 'نمای کلی فروش و عملکرد' : 'Sales Overview & Analytics'),
          style: Theme.of(context).textTheme.titleLarge,
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
                langCode == 'ps' ? 'د میاشتني هدف سلنه' : (langCode == 'prs' || langCode == 'fa' ? 'درصد پیشرفت هدف ماهانه' : 'Monthly Goal Progress'),
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
              ),
              const SizedBox(height: 16),
              const LinearProgressIndicator(
                value: 0.78,
                minHeight: 10,
                borderRadius: BorderRadius.all(Radius.circular(10)),
                backgroundColor: Colors.white10,
                color: Color(0xFF6C5CE7),
              ),
              const SizedBox(height: 12),
              const Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text('Target: \$18,000.00', style: TextStyle(color: Colors.white60)),
                  Text('78% Reached', style: TextStyle(color: Color(0xFFA29BFE), fontWeight: FontWeight.bold)),
                ],
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
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(icon, color: color, size: 28),
          const SizedBox(height: 12),
          Text(title, style: const TextStyle(color: Colors.white60, fontSize: 13)),
          const SizedBox(height: 4),
          Text(value, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
        ],
      ),
    );
  }
}
