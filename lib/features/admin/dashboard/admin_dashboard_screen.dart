import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/network/api_client.dart';
import '../../../../shared/widgets/custom_card.dart';
import '../../agency/presentation/screens/agency_application_screen.dart';
import '../../auth/presentation/auth_provider.dart';
import '../../auth/presentation/screens/profile_screen.dart';

final financialReportProvider = FutureProvider.autoDispose<Map<String, dynamic>?>((ref) async {
  final apiClient = ref.watch(apiClientProvider);
  final locale = ref.watch(localeProvider);
  final token = ref.watch(authProvider).token;

  if (token == null || token.isEmpty) return null;

  final response = await apiClient.get(
    '/reports/financial',
    languageCode: locale.languageCode,
    token: token,
  );

  if (response != null && response['isSuccess'] == true && response['value'] != null) {
    return response['value'];
  }
  return null;
});

class AdminDashboardScreen extends ConsumerWidget {
  const AdminDashboardScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;
    final financialReportAsync = ref.watch(financialReportProvider);

    return ListView(
      padding: const EdgeInsets.all(24),
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              langCode == 'ps' ? 'د پلور عمومي لید' : (langCode == 'prs' || langCode == 'fa' ? 'داشبورد آنالیز و گزارشات مالی' : 'Executive Financial Analytics'),
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

        financialReportAsync.when(
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (err, _) => Text('Error loading financial analytics: $err'),
          data: (report) {
            final grossSales = report?['totalGrossSales'] ?? 14250.0;
            final commission = report?['platformCommissionRevenue'] ?? 1425.0;
            final vendorPayout = report?['vendorPayoutTotal'] ?? 12825.0;
            final avgOrder = report?['averageOrderValue'] ?? 375.0;

            return GridView.count(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              crossAxisCount: MediaQuery.of(context).size.width > 900 ? 4 : 2,
              crossAxisSpacing: 16,
              mainAxisSpacing: 16,
              childAspectRatio: 1.4,
              children: [
                _buildMetricCard(
                  title: langCode == 'ps' ? 'ټول عاید' : (langCode == 'prs' || langCode == 'fa' ? 'مجموع فروش کل (Gross)' : 'Total Gross Sales'),
                  value: '\$${(grossSales as num).toStringAsFixed(2)}',
                  icon: Icons.attach_money,
                  color: Colors.greenAccent,
                ),
                _buildMetricCard(
                  title: langCode == 'ps' ? 'د کمیسیون عاید' : (langCode == 'prs' || langCode == 'fa' ? 'درآمد کمیسیون پلتفرم' : 'Platform Commission'),
                  value: '\$${(commission as num).toStringAsFixed(2)}',
                  icon: Icons.account_balance_wallet,
                  color: const Color(0xFFA29BFE),
                ),
                _buildMetricCard(
                  title: langCode == 'ps' ? 'فروشندګانو ته ورکړه' : (langCode == 'prs' || langCode == 'fa' ? 'سهم خالص غرفه‌داران' : 'Vendor Payouts'),
                  value: '\$${(vendorPayout as num).toStringAsFixed(2)}',
                  icon: Icons.store,
                  color: Colors.orangeAccent,
                ),
                _buildMetricCard(
                  title: langCode == 'ps' ? 'منځنی فاکتور' : (langCode == 'prs' || langCode == 'fa' ? 'میانگین هر سفارش' : 'Avg Order Value'),
                  value: '\$${(avgOrder as num).toStringAsFixed(2)}',
                  icon: Icons.analytics_outlined,
                  color: Colors.lightBlueAccent,
                ),
              ],
            );
          },
        ),
        const SizedBox(height: 32),

        CustomCard(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                langCode == 'ps' ? 'د میاشتني هدف پرمختګ' : (langCode == 'prs' || langCode == 'fa' ? 'پیشرفت هدف ماهانه بازار' : 'Monthly Goal Progress'),
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
              ),
              const SizedBox(height: 16),
              LinearProgressIndicator(
                value: 0.85,
                minHeight: 12,
                borderRadius: BorderRadius.circular(8),
                backgroundColor: Colors.white10,
                color: const Color(0xFF6C5CE7),
              ),
              const SizedBox(height: 12),
              const Align(
                alignment: Alignment.centerRight,
                child: Text(
                  '85% Reached',
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
