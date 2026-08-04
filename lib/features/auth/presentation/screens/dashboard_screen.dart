import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../shared/widgets/custom_button.dart';
import '../../../../shared/widgets/custom_card.dart';
import '../../../agency/presentation/screens/agency_application_screen.dart';
import '../../../vendor/presentation/screens/vendor_register_screen.dart';
import '../auth_provider.dart';
import 'profile_screen.dart';

final myOrdersProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  final apiClient = ref.watch(apiClientProvider);
  final locale = ref.watch(localeProvider);
  final token = ref.watch(authProvider).token;

  if (token == null) return [];

  final response = await apiClient.get(
    '/orders/my',
    languageCode: locale.languageCode,
    token: token,
  );

  if (response != null && response['isSuccess'] == true && response['value'] != null) {
    return List<dynamic>.from(response['value']);
  }
  return [];
});

class DashboardScreen extends ConsumerWidget {
  final VoidCallback? onGoToHome;

  const DashboardScreen({super.key, this.onGoToHome});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final authState = ref.watch(authProvider);
    final myOrdersAsync = ref.watch(myOrdersProvider);
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;

    // Guard: Prevent unauthenticated users from seeing the dashboard contents
    if (!authState.isAuthenticated) {
      return Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24.0),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 500),
            child: CustomCard(
              padding: const EdgeInsets.all(32),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  const Icon(Icons.lock_outline, size: 64, color: AppColors.royalBlue),
                  const SizedBox(height: 16),
                  Text(
                    langCode == 'ps' ? 'تاسې نښلول شوي نه یاست' : (langCode == 'prs' || langCode == 'fa' ? 'شما وارد حساب کاربری نشده‌اید' : 'You are not logged in'),
                    style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    langCode == 'ps' ? 'د داشبورډ لیدلو لپاره مهرباني وکړئ لومړی خپل حساب ته ننوځئ.' : (langCode == 'prs' || langCode == 'fa' ? 'برای مشاهده داشبورد، لطفاً ابتدا وارد حساب کاربری خود شوید.' : 'Please log in to view your dashboard and order history.'),
                    textAlign: TextAlign.center,
                    style: const TextStyle(color: AppColors.textSecondary),
                  ),
                  const SizedBox(height: 24),
                  Row(
                    children: [
                      Expanded(
                        child: CustomButton(
                          text: langCode == 'ps' ? 'اصلي پاڼه' : (langCode == 'prs' || langCode == 'fa' ? 'صفحه اصلی' : 'Home Page'),
                          isSecondary: true,
                          icon: Icons.home,
                          onPressed: () {
                            if (onGoToHome != null) {
                              onGoToHome!();
                            } else {
                              context.go('/');
                            }
                          },
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: CustomButton(
                          text: langCode == 'ps' ? 'ننوتل' : (langCode == 'prs' || langCode == 'fa' ? 'ورود به حساب' : 'Log In'),
                          icon: Icons.login,
                          onPressed: () => context.go('/login'),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ),
        ),
      );
    }

    return SingleChildScrollView(
      padding: const EdgeInsets.all(24.0),
      child: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 800),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    langCode == 'ps' ? 'د کارونکي داشبورډ' : (langCode == 'prs' || langCode == 'fa' ? 'داشبورد کاربری' : 'User Dashboard'),
                    style: Theme.of(context).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.bold),
                  ),
                  ElevatedButton.icon(
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.royalBlue,
                      foregroundColor: Colors.white,
                    ),
                    icon: const Icon(Icons.home, size: 18),
                    label: Text(langCode == 'ps' ? 'اصلي پاڼه' : (langCode == 'prs' || langCode == 'fa' ? 'صفحه اصلی' : 'Home Page')),
                    onPressed: () {
                      if (onGoToHome != null) {
                        onGoToHome!();
                      } else {
                        context.go('/');
                      }
                    },
                  ),
                ],
              ),
              const SizedBox(height: 16),
              CustomCard(
                child: Column(
                  children: [
                    Row(
                      children: [
                        const CircleAvatar(
                          radius: 36,
                          backgroundColor: AppColors.royalBlue,
                          child: Icon(Icons.person, size: 36, color: Colors.white),
                        ),
                        const SizedBox(width: 16),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                authState.userName ?? "User",
                                style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
                              ),
                              const SizedBox(height: 4),
                              Text(
                                authState.email ?? '',
                                style: const TextStyle(color: AppColors.textSecondary),
                              ),
                              const SizedBox(height: 4),
                              Chip(
                                label: Text(authState.role ?? 'Customer'),
                                backgroundColor: AppColors.royalBlue.withAlpha(50),
                              ),
                            ],
                          ),
                        ),
                        Column(
                          crossAxisAlignment: CrossAxisAlignment.end,
                          children: [
                            IconButton(
                              icon: const Icon(Icons.edit_note, color: Colors.white70),
                              tooltip: 'Edit Profile',
                              onPressed: () {
                                Navigator.push(
                                  context,
                                  MaterialPageRoute(builder: (_) => const ProfileScreen()),
                                );
                              },
                            ),
                            IconButton(
                              icon: const Icon(Icons.logout, color: Colors.redAccent),
                              tooltip: 'Log Out',
                              onPressed: () async {
                                await ref.read(authProvider.notifier).logout();
                                if (context.mounted) {
                                  context.go('/login');
                                }
                              },
                            ),
                          ],
                        ),
                      ],
                    ),
                    const Divider(height: 24),
                    // Quick Action Buttons
                    Wrap(
                      spacing: 12,
                      runSpacing: 12,
                      alignment: WrapAlignment.start,
                      children: [
                        OutlinedButton.icon(
                          icon: const Icon(Icons.person_outline, size: 18),
                          label: Text(langCode == 'ps' ? 'د پروفایل تنظیمات' : (langCode == 'prs' || langCode == 'fa' ? 'ویرایش پروفایل من' : 'Edit My Profile')),
                          onPressed: () {
                            Navigator.push(
                              context,
                              MaterialPageRoute(builder: (_) => const ProfileScreen()),
                            );
                          },
                        ),
                        OutlinedButton.icon(
                          icon: const Icon(Icons.verified_user_outlined, size: 18),
                          label: Text(langCode == 'ps' ? 'د نمایندګۍ غوښتنه' : (langCode == 'prs' || langCode == 'fa' ? 'درخواست اخذ نمایندگی رسمی' : 'Apply for Official Agency')),
                          onPressed: () {
                            Navigator.push(
                              context,
                              MaterialPageRoute(builder: (_) => const AgencyApplicationScreen()),
                            );
                          },
                        ),
                        if (!authState.isVendor)
                          ElevatedButton.icon(
                            style: ElevatedButton.styleFrom(backgroundColor: Colors.green.shade700),
                            icon: const Icon(Icons.storefront, size: 18),
                            label: Text(langCode == 'ps' ? 'پلورونکی شئ' : (langCode == 'prs' || langCode == 'fa' ? 'ثبت‌نام فروشنده' : 'Become a Vendor')),
                            onPressed: () {
                              Navigator.push(
                                context,
                                MaterialPageRoute(builder: (_) => const VendorRegisterScreen()),
                              );
                            },
                          )
                        else
                          Chip(
                            avatar: const Icon(Icons.verified, size: 16, color: Colors.greenAccent),
                            label: Text(langCode == 'ps' ? 'تایید شوی پلورونکی' : (langCode == 'prs' || langCode == 'fa' ? 'فروشنده تایید شده' : 'Verified Vendor')),
                          ),
                      ],
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 24),
              Text(
                langCode == 'ps' ? 'زما پخواني پېرودونه (سفارشونه)' : (langCode == 'prs' || langCode == 'fa' ? 'سفارشات من' : 'My Order History'),
                style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 16),
              myOrdersAsync.when(
                loading: () => const Center(child: CircularProgressIndicator()),
                error: (err, stack) => Text('Error loading orders: $err'),
                data: (orders) {
                  if (orders.isEmpty) {
                    return CustomCard(
                      padding: const EdgeInsets.all(32),
                      child: Center(
                        child: Text(
                          langCode == 'ps' ? 'تاسو تر اوسه هیڅ فرمایش نه دی ثبت کړی.' : (langCode == 'prs' || langCode == 'fa' ? 'شما هنوز هیچ سفارشی ثبت نکرده‌اید.' : 'You have not placed any orders yet.'),
                          style: const TextStyle(color: Colors.white70),
                        ),
                      ),
                    );
                  }

                  return ListView.separated(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    itemCount: orders.length,
                    separatorBuilder: (_, __) => const SizedBox(height: 12),
                    itemBuilder: (context, index) {
                      final order = orders[index];
                      return CustomCard(
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Text(
                                  'Order #${order['orderNumber']}',
                                  style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                                ),
                                Container(
                                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                                  decoration: BoxDecoration(
                                    color: Colors.green.withAlpha(40),
                                    borderRadius: BorderRadius.circular(12),
                                  ),
                                  child: Text(
                                    '${order['status']}',
                                    style: const TextStyle(color: Colors.greenAccent, fontSize: 12, fontWeight: FontWeight.bold),
                                  ),
                                ),
                              ],
                            ),
                            const Divider(height: 20),
                            Text('Address: ${order['shippingAddress']}', style: const TextStyle(fontSize: 13, color: Colors.white70)),
                            const SizedBox(height: 8),
                            Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Text('Items: ${(order['items'] as List).length}', style: const TextStyle(fontSize: 13, color: Colors.white70)),
                                Text(
                                  '\$${(order['totalAmount'] as num).toStringAsFixed(2)}',
                                  style: const TextStyle(fontWeight: FontWeight.bold, color: AppColors.secondaryPurple, fontSize: 16),
                                ),
                              ],
                            ),
                          ],
                        ),
                      );
                    },
                  );
                },
              ),
            ],
          ),
        ),
      ),
    );
  }
}
