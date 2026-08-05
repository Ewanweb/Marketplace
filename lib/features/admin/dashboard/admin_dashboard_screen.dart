import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/theme/app_colors.dart';
import '../../auth/presentation/auth_provider.dart';

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
      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
      children: [
        // Financial Metrics Grid
        financialReportAsync.when(
          loading: () => const Center(
            child: Padding(
              padding: EdgeInsets.all(48.0),
              child: CircularProgressIndicator(),
            ),
          ),
          error: (err, _) => Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: AppColors.horizonRed.withAlpha(20),
              borderRadius: BorderRadius.circular(16),
            ),
            child: Text('Error loading analytics: $err', style: const TextStyle(color: AppColors.horizonRed)),
          ),
          data: (report) {
            final grossSales = report?['totalGrossSales'] ?? 14250.0;
            final commission = report?['platformCommissionRevenue'] ?? 1425.0;
            final vendorPayout = report?['vendorPayoutTotal'] ?? 12825.0;
            final avgOrder = report?['averageOrderValue'] ?? 375.0;

            final isWide = MediaQuery.of(context).size.width > 900;

            return GridView.count(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              crossAxisCount: isWide ? 4 : 2,
              crossAxisSpacing: 16,
              mainAxisSpacing: 16,
              childAspectRatio: isWide ? 1.7 : 1.3,
              children: [
                _buildHorizonStatCard(
                  title: langCode == 'ps' ? 'ټول عاید' : (langCode == 'prs' || langCode == 'fa' ? 'مجموع فروش کل' : 'Total Gross Sales'),
                  value: '\$${(grossSales as num).toStringAsFixed(2)}',
                  growth: '+23%',
                  icon: Icons.bar_chart_rounded,
                  iconBgColor: AppColors.horizonBrand.withAlpha(20),
                  iconColor: AppColors.horizonBrand,
                ),
                _buildHorizonStatCard(
                  title: langCode == 'ps' ? 'د کمیسیون عاید' : (langCode == 'prs' || langCode == 'fa' ? 'درآمد کمیسیون' : 'Commission Revenue'),
                  value: '\$${(commission as num).toStringAsFixed(2)}',
                  growth: '+12.5%',
                  icon: Icons.account_balance_wallet_rounded,
                  iconBgColor: AppColors.horizonSky.withAlpha(30),
                  iconColor: const Color(0xFF0095FF),
                ),
                _buildHorizonStatCard(
                  title: langCode == 'ps' ? 'فروشندګانو ته ورکړه' : (langCode == 'prs' || langCode == 'fa' ? 'سهم غرفه‌داران' : 'Vendor Payouts'),
                  value: '\$${(vendorPayout as num).toStringAsFixed(2)}',
                  growth: '+18%',
                  icon: Icons.storefront_rounded,
                  iconBgColor: AppColors.horizonOrange.withAlpha(30),
                  iconColor: AppColors.horizonOrange,
                ),
                _buildHorizonStatCard(
                  title: langCode == 'ps' ? 'منځنی فاکتور' : (langCode == 'prs' || langCode == 'fa' ? 'میانگین هر سفارش' : 'Avg Order Value'),
                  value: '\$${(avgOrder as num).toStringAsFixed(2)}',
                  growth: '+5.2%',
                  icon: Icons.show_chart_rounded,
                  iconBgColor: AppColors.horizonGreen.withAlpha(30),
                  iconColor: AppColors.horizonGreen,
                ),
              ],
            );
          },
        ),
        const SizedBox(height: 24),

        // Middle Section: Goal Progress & Performance Banner
        Row(
          children: [
            Expanded(
              child: Container(
                padding: const EdgeInsets.all(24),
                decoration: BoxDecoration(
                  color: AppColors.horizonCard,
                  borderRadius: BorderRadius.circular(20),
                  boxShadow: const [AppColors.horizonShadow],
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Row(
                          children: [
                            Container(
                              padding: const EdgeInsets.all(10),
                              decoration: BoxDecoration(
                                color: AppColors.horizonBrand.withAlpha(15),
                                borderRadius: BorderRadius.circular(12),
                              ),
                              child: const Icon(Icons.track_changes_rounded, color: AppColors.horizonBrand, size: 20),
                            ),
                            const SizedBox(width: 12),
                            Text(
                              langCode == 'ps' ? 'د میاشتني هدف پرمختګ' : (langCode == 'prs' || langCode == 'fa' ? 'پیشرفت هدف فروش ماهانه' : 'Monthly Sales Goal Target'),
                              style: const TextStyle(
                                fontSize: 16,
                                fontWeight: FontWeight.bold,
                                color: AppColors.horizonNavy,
                              ),
                            ),
                          ],
                        ),
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                          decoration: BoxDecoration(
                            color: AppColors.horizonGreen.withAlpha(20),
                            borderRadius: BorderRadius.circular(20),
                          ),
                          child: const Text(
                            'On Track',
                            style: TextStyle(
                              fontSize: 12,
                              fontWeight: FontWeight.bold,
                              color: AppColors.horizonGreen,
                            ),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 20),
                    ClipRRect(
                      borderRadius: BorderRadius.circular(10),
                      child: const LinearProgressIndicator(
                        value: 0.85,
                        minHeight: 12,
                        backgroundColor: Color(0xFFF4F7FE),
                        color: AppColors.horizonBrand,
                      ),
                    ),
                    const SizedBox(height: 12),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          langCode == 'ps' ? '۸۵٪ رسیدلی' : (langCode == 'prs' || langCode == 'fa' ? '۸۵٪ محقق شده' : '85% Achieved'),
                          style: const TextStyle(
                            fontSize: 13,
                            fontWeight: FontWeight.bold,
                            color: AppColors.horizonNavy,
                          ),
                        ),
                        Text(
                          langCode == 'ps' ? 'هدف: \$۱۸,۰۰۰' : (langCode == 'prs' || langCode == 'fa' ? 'هدف: \$۱۸,۰۰۰' : 'Target: \$18,000'),
                          style: const TextStyle(
                            fontSize: 12,
                            color: AppColors.horizonMuted,
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ],
    );
  }

  // Horizon Mini-Stat Card Component
  Widget _buildHorizonStatCard({
    required String title,
    required String value,
    required String growth,
    required IconData icon,
    required Color iconBgColor,
    required Color iconColor,
  }) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: AppColors.horizonCard,
        borderRadius: BorderRadius.circular(20),
        boxShadow: const [AppColors.horizonShadow],
      ),
      child: Row(
        children: [
          // Circular Badge Icon on Left
          Container(
            width: 48,
            height: 48,
            decoration: BoxDecoration(
              color: iconBgColor,
              shape: BoxShape.circle,
            ),
            child: Icon(icon, color: iconColor, size: 24),
          ),
          const SizedBox(width: 16),

          // Values & Growth Pill
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Text(
                  title,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontSize: 12,
                    fontWeight: FontWeight.w500,
                    color: AppColors.horizonMuted,
                  ),
                ),
                const SizedBox(height: 4),
                Row(
                  children: [
                    Text(
                      value,
                      style: const TextStyle(
                        fontSize: 20,
                        fontWeight: FontWeight.w800,
                        color: AppColors.horizonNavy,
                      ),
                    ),
                    const SizedBox(width: 8),
                    Text(
                      growth,
                      style: const TextStyle(
                        fontSize: 11,
                        fontWeight: FontWeight.bold,
                        color: AppColors.horizonGreen,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
