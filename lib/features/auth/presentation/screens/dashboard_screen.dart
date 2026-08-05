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

class DashboardScreen extends ConsumerStatefulWidget {
  final VoidCallback? onGoToHome;

  const DashboardScreen({super.key, this.onGoToHome});

  @override
  ConsumerState<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends ConsumerState<DashboardScreen> {
  String _selectedOrderFilter = 'all';

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authProvider);
    final myOrdersAsync = ref.watch(myOrdersProvider);
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;

    // Guard: Unauthenticated state
    if (!authState.isAuthenticated) {
      return Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24.0),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 500),
            child: Container(
              padding: const EdgeInsets.all(36),
              decoration: BoxDecoration(
                gradient: const LinearGradient(
                  colors: [Color(0xFF1E1E2E), Color(0xFF2D2B55)],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                ),
                borderRadius: BorderRadius.circular(28),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withAlpha(80),
                    blurRadius: 20,
                    offset: const Offset(0, 10),
                  ),
                ],
                border: Border.all(color: Colors.white.withAlpha(30)),
              ),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Container(
                    padding: const EdgeInsets.all(20),
                    decoration: BoxDecoration(
                      color: AppColors.royalBlue.withAlpha(40),
                      shape: BoxShape.circle,
                      border: Border.all(color: AppColors.royalBlue.withAlpha(100)),
                    ),
                    child: const Icon(Icons.lock_outline_rounded, size: 54, color: Colors.indigoAccent),
                  ),
                  const SizedBox(height: 24),
                  Text(
                    langCode == 'ps' ? 'تاسې نښلول شوي نه یاست' : (langCode == 'prs' || langCode == 'fa' ? 'ورود به حساب کاربری لازم است' : 'Authentication Required'),
                    style: Theme.of(context).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.bold, color: Colors.white),
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 12),
                  Text(
                    langCode == 'ps' ? 'د خپل شخصي داشبورډ او د فرمایشونو سوابقو لیدلو لپاره مهرباني وکړئ خپل حساب ته ننوځئ.' : (langCode == 'prs' || langCode == 'fa' ? 'برای دسترسی به پنل کاربری، پیگیری سفارشات و خدمات اختصاصی ابتدا وارد حساب شوید.' : 'Log in to access your personal dashboard, track live orders, and manage services.'),
                    textAlign: TextAlign.center,
                    style: const TextStyle(color: Colors.white70, height: 1.5),
                  ),
                  const SizedBox(height: 32),
                  Row(
                    children: [
                      Expanded(
                        child: CustomButton(
                          text: langCode == 'ps' ? 'اصلي پاڼه' : (langCode == 'prs' || langCode == 'fa' ? 'صفحه اصلی' : 'Home'),
                          isSecondary: true,
                          icon: Icons.home_outlined,
                          onPressed: () {
                            if (widget.onGoToHome != null) {
                              widget.onGoToHome!();
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
                          icon: Icons.login_rounded,
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
          constraints: const BoxConstraints(maxWidth: 950),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // Top Action Header
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        langCode == 'ps' ? 'د کارونکي داشبورډ' : (langCode == 'prs' || langCode == 'fa' ? 'پنل اختصاصی کاربری' : 'Executive Dashboard'),
                        style: Theme.of(context).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.w900),
                      ),
                      const SizedBox(height: 4),
                      Row(
                        children: [
                          Container(
                            width: 8,
                            height: 8,
                            decoration: const BoxDecoration(color: Colors.greenAccent, shape: BoxShape.circle),
                          ),
                          const SizedBox(width: 6),
                          Text(
                            langCode == 'ps' ? 'فعال پېرودونکی' : (langCode == 'prs' || langCode == 'fa' ? 'حساب فعال • بازار نورزی' : 'Active Account • Noorzai System'),
                            style: const TextStyle(fontSize: 12, color: AppColors.textSecondary),
                          ),
                        ],
                      ),
                    ],
                  ),
                  ElevatedButton.icon(
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.royalBlue,
                      foregroundColor: Colors.white,
                      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                    ),
                    icon: const Icon(Icons.storefront_rounded, size: 18),
                    label: Text(langCode == 'ps' ? 'اصلي پاڼه' : (langCode == 'prs' || langCode == 'fa' ? 'صفحه اصلی فروشگاه' : 'Main Store')),
                    onPressed: () {
                      if (widget.onGoToHome != null) {
                        widget.onGoToHome!();
                      } else {
                        context.go('/');
                      }
                    },
                  ),
                ],
              ),
              const SizedBox(height: 24),

              // Hero Glassmorphism Profile Banner
              Container(
                decoration: BoxDecoration(
                  gradient: const LinearGradient(
                    colors: [Color(0xFF1F1C2C), Color(0xFF928DAB)],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
                  borderRadius: BorderRadius.circular(28),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withAlpha(70),
                      blurRadius: 20,
                      offset: const Offset(0, 10),
                    ),
                  ],
                ),
                padding: const EdgeInsets.all(28),
                child: Column(
                  children: [
                    Row(
                      children: [
                        Stack(
                          children: [
                            CircleAvatar(
                              radius: 42,
                              backgroundColor: Colors.white.withAlpha(40),
                              child: const Icon(Icons.person_rounded, size: 48, color: Colors.white),
                            ),
                            Positioned(
                              bottom: 0,
                              right: 0,
                              child: Container(
                                padding: const EdgeInsets.all(4),
                                decoration: const BoxDecoration(color: Colors.greenAccent, shape: BoxShape.circle),
                                child: const Icon(Icons.check, size: 14, color: Colors.black),
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(width: 20),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Row(
                                children: [
                                  Text(
                                    authState.userName ?? "Valued Customer",
                                    style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold, color: Colors.white),
                                  ),
                                  const SizedBox(width: 10),
                                  Container(
                                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                                    decoration: BoxDecoration(
                                      color: Colors.white.withAlpha(40),
                                      borderRadius: BorderRadius.circular(20),
                                      border: Border.all(color: Colors.white.withAlpha(60)),
                                    ),
                                    child: Text(
                                      authState.role ?? 'Customer',
                                      style: const TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: Colors.white),
                                    ),
                                  ),
                                ],
                              ),
                              const SizedBox(height: 6),
                              Text(
                                authState.email ?? '',
                                style: const TextStyle(color: Colors.white70, fontSize: 13),
                              ),
                            ],
                          ),
                        ),
                        Row(
                          children: [
                            IconButton(
                              icon: const Icon(Icons.edit, color: Colors.white70),
                              tooltip: 'Edit Profile',
                              onPressed: () {
                                Navigator.push(
                                  context,
                                  MaterialPageRoute(builder: (_) => const ProfileScreen()),
                                );
                              },
                            ),
                            IconButton(
                              icon: const Icon(Icons.power_settings_new, color: Colors.redAccent),
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
                    const SizedBox(height: 24),
                    const Divider(color: Colors.white24, height: 1),
                    const SizedBox(height: 20),

                    // Embedded Metrics Banner
                    myOrdersAsync.when(
                      loading: () => const SizedBox(),
                      error: (_, __) => const SizedBox(),
                      data: (orders) {
                        final totalSpent = orders.fold<double>(
                            0, (sum, item) => sum + ((item['totalAmount'] as num?)?.toDouble() ?? 0.0));
                        return Row(
                          mainAxisAlignment: MainAxisAlignment.spaceAround,
                          children: [
                            _buildHeroStat(
                              title: langCode == 'ps' ? 'ټول فرمایشونه' : (langCode == 'prs' || langCode == 'fa' ? 'سفارشات ثبت‌شده' : 'Total Orders'),
                              value: '${orders.length}',
                              icon: Icons.local_mall_outlined,
                            ),
                            _buildHeroStat(
                              title: langCode == 'ps' ? 'ټوله پیرودنه' : (langCode == 'prs' || langCode == 'fa' ? 'مجموع خریدهای شما' : 'Total Spent'),
                              value: '\$${totalSpent.toStringAsFixed(2)}',
                              icon: Icons.account_balance_wallet_outlined,
                            ),
                            _buildHeroStat(
                              title: langCode == 'ps' ? 'د حساب کچه' : (langCode == 'prs' || langCode == 'fa' ? 'اعتبار حساب' : 'Account Status'),
                              value: authState.isVendor ? 'Verified Seller' : 'Gold Member',
                              icon: Icons.verified_outlined,
                            ),
                          ],
                        );
                      },
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 28),

              // Interactive Quick Action Hub
              Text(
                langCode == 'ps' ? 'چټکې کړنې او خدمات' : (langCode == 'prs' || langCode == 'fa' ? 'میز خدمات و دسترسی سریع' : 'Service Hub & Quick Actions'),
                style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 16),
              GridView.count(
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                crossAxisCount: MediaQuery.of(context).size.width > 750 ? 3 : 1,
                crossAxisSpacing: 16,
                mainAxisSpacing: 16,
                childAspectRatio: 2.3,
                children: [
                  _buildActionCard(
                    context: context,
                    title: langCode == 'ps' ? 'د پروفایل سمول' : (langCode == 'prs' || langCode == 'fa' ? 'ویرایش مشخصات پروفایل' : 'Edit Profile Settings'),
                    subtitle: langCode == 'ps' ? 'د نوم، ټلیفون او پتې تغییر' : (langCode == 'prs' || langCode == 'fa' ? 'تغییر آدرس تحویل، شماره و ایمیل' : 'Update address, phone & info'),
                    icon: Icons.person_outline_rounded,
                    gradient: const [Color(0xFF4568DC), Color(0xFFB06AB3)],
                    onTap: () {
                      Navigator.push(context, MaterialPageRoute(builder: (_) => const ProfileScreen()));
                    },
                  ),
                  _buildActionCard(
                    context: context,
                    title: langCode == 'ps' ? 'د نمایندګۍ غوښتنه' : (langCode == 'prs' || langCode == 'fa' ? 'درخواست نمایندگی رسمی' : 'Apply for Official Agency'),
                    subtitle: langCode == 'ps' ? 'په ولایتونو کې د څانګې جوړول' : (langCode == 'prs' || langCode == 'fa' ? 'اخذ نمایندگی فروش در استان‌ها' : 'Regional distribution agency'),
                    icon: Icons.badge_outlined,
                    gradient: const [Color(0xFF00B4DB), Color(0xFF0083B0)],
                    onTap: () {
                      Navigator.push(context, MaterialPageRoute(builder: (_) => const AgencyApplicationScreen()));
                    },
                  ),
                  _buildActionCard(
                    context: context,
                    title: authState.isVendor
                        ? (langCode == 'ps' ? 'د پلورونکي پینل' : (langCode == 'prs' || langCode == 'fa' ? 'پنل غرفه فروشندگی' : 'Vendor Merchant Hub'))
                        : (langCode == 'ps' ? 'پلورونکی شئ' : (langCode == 'prs' || langCode == 'fa' ? 'ثبت‌نام غرفه فروشنده' : 'Become a Merchant')),
                    subtitle: authState.isVendor
                        ? (langCode == 'ps' ? 'فعال پلورونکی' : (langCode == 'prs' || langCode == 'fa' ? 'فروشنده تایید شده نورزی' : 'Verified Merchant Partner'))
                        : (langCode == 'ps' ? 'خپل محصولات وپلورئ' : (langCode == 'prs' || langCode == 'fa' ? 'فروش محصولات در بازار نورزی' : 'Start selling products online')),
                    icon: Icons.storefront_rounded,
                    gradient: const [Color(0xFF11998E), Color(0xFF38EF7D)],
                    onTap: () {
                      Navigator.push(context, MaterialPageRoute(builder: (_) => const VendorRegisterScreen()));
                    },
                  ),
                ],
              ),
              const SizedBox(height: 32),

              // Order History Section with Filter Tabs
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    langCode == 'ps' ? 'زما پخواني پېرودونه' : (langCode == 'prs' || langCode == 'fa' ? 'تاریخچه سفارشات من' : 'My Live Orders'),
                    style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
                  ),
                  Row(
                    children: [
                      ChoiceChip(
                        label: Text(langCode == 'ps' ? 'ټول' : (langCode == 'prs' || langCode == 'fa' ? 'همه' : 'All')),
                        selected: _selectedOrderFilter == 'all',
                        onSelected: (_) => setState(() => _selectedOrderFilter = 'all'),
                      ),
                      const SizedBox(width: 8),
                      ChoiceChip(
                        label: Text(langCode == 'ps' ? 'په جریان کې' : (langCode == 'prs' || langCode == 'fa' ? 'در حال ارسال' : 'Pending')),
                        selected: _selectedOrderFilter == 'pending',
                        onSelected: (_) => setState(() => _selectedOrderFilter = 'pending'),
                      ),
                    ],
                  ),
                ],
              ),
              const SizedBox(height: 16),
              myOrdersAsync.when(
                loading: () => const Center(child: Padding(padding: EdgeInsets.all(32), child: CircularProgressIndicator())),
                error: (err, stack) => Text('Error loading orders: $err'),
                data: (orders) {
                  if (orders.isEmpty) {
                    return CustomCard(
                      padding: const EdgeInsets.all(40),
                      child: Column(
                        children: [
                          const Icon(Icons.shopping_bag_outlined, size: 54, color: Colors.white30),
                          const SizedBox(height: 12),
                          Text(
                            langCode == 'ps' ? 'تاسو تر اوسه هیڅ فرمایش نه دی ثبت کړی.' : (langCode == 'prs' || langCode == 'fa' ? 'هنوز هیچ سفارشی در حساب شما ثبت نشده است.' : 'No orders recorded in your history.'),
                            style: const TextStyle(color: Colors.white70),
                          ),
                        ],
                      ),
                    );
                  }

                  return ListView.separated(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    itemCount: orders.length,
                    separatorBuilder: (_, __) => const SizedBox(height: 14),
                    itemBuilder: (context, index) {
                      final order = orders[index];
                      final items = (order['items'] as List<dynamic>?) ?? [];
                      return CustomCard(
                        padding: const EdgeInsets.all(20),
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
                                        color: AppColors.royalBlue.withAlpha(30),
                                        borderRadius: BorderRadius.circular(12),
                                      ),
                                      child: const Icon(Icons.inventory_2_outlined, color: AppColors.royalBlue, size: 20),
                                    ),
                                    const SizedBox(width: 12),
                                    Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        Text(
                                          'Order #${order['orderNumber']}',
                                          style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                                        ),
                                        Text(
                                          'Placed on ${order['createdAt']?.toString().split('T').first ?? 'Recent'}',
                                          style: const TextStyle(fontSize: 12, color: AppColors.textSecondary),
                                        ),
                                      ],
                                    ),
                                  ],
                                ),
                                Container(
                                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                                  decoration: BoxDecoration(
                                    color: Colors.green.withAlpha(40),
                                    borderRadius: BorderRadius.circular(20),
                                    border: Border.all(color: Colors.greenAccent.withAlpha(80)),
                                  ),
                                  child: Text(
                                    '${order['status']}',
                                    style: const TextStyle(color: Colors.greenAccent, fontSize: 12, fontWeight: FontWeight.bold),
                                  ),
                                ),
                              ],
                            ),
                            const Divider(height: 24),
                            Row(
                              children: [
                                const Icon(Icons.location_on_outlined, size: 16, color: Colors.white60),
                                const SizedBox(width: 6),
                                Expanded(
                                  child: Text(
                                    'Address: ${order['shippingAddress']}',
                                    style: const TextStyle(fontSize: 13, color: Colors.white70),
                                    maxLines: 1,
                                    overflow: TextOverflow.ellipsis,
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 12),
                            Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Text(
                                  'Purchased Items (${items.length})',
                                  style: const TextStyle(fontSize: 13, color: Colors.white60),
                                ),
                                Text(
                                  '\$${(order['totalAmount'] as num?)?.toStringAsFixed(2) ?? '0.00'}',
                                  style: const TextStyle(fontWeight: FontWeight.bold, color: AppColors.secondaryPurple, fontSize: 18),
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

  Widget _buildHeroStat({required String title, required String value, required IconData icon}) {
    return Column(
      children: [
        Icon(icon, color: Colors.white70, size: 22),
        const SizedBox(height: 6),
        Text(value, style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.white)),
        const SizedBox(height: 2),
        Text(title, style: const TextStyle(fontSize: 11, color: Colors.white70)),
      ],
    );
  }

  Widget _buildActionCard({
    required BuildContext context,
    required String title,
    required String subtitle,
    required IconData icon,
    required List<Color> gradient,
    required VoidCallback onTap,
  }) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(20),
      child: Container(
        padding: const EdgeInsets.all(18),
        decoration: BoxDecoration(
          gradient: LinearGradient(colors: gradient, begin: Alignment.topLeft, end: Alignment.bottomRight),
          borderRadius: BorderRadius.circular(20),
          boxShadow: [
            BoxShadow(
              color: gradient.first.withAlpha(80),
              blurRadius: 10,
              offset: const Offset(0, 4),
            ),
          ],
        ),
        child: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(10),
              decoration: BoxDecoration(
                color: Colors.white.withAlpha(50),
                shape: BoxShape.circle,
              ),
              child: Icon(icon, size: 24, color: Colors.white),
            ),
            const SizedBox(width: 14),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(
                    title,
                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14, color: Colors.white),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    subtitle,
                    style: const TextStyle(fontSize: 11, color: Colors.white70),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                ],
              ),
            ),
            const Icon(Icons.arrow_forward_ios_rounded, size: 14, color: Colors.white70),
          ],
        ),
      ),
    );
  }
}
