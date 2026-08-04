import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../shared/widgets/custom_button.dart';
import '../../../../shared/widgets/custom_card.dart';
import '../../../../shared/widgets/custom_text_field.dart';
import '../../../auth/presentation/auth_provider.dart';

class AgencyApplicationScreen extends ConsumerStatefulWidget {
  const AgencyApplicationScreen({super.key});

  @override
  ConsumerState<AgencyApplicationScreen> createState() => _AgencyApplicationScreenState();
}

class _AgencyApplicationScreenState extends ConsumerState<AgencyApplicationScreen> {
  final _applicantNameController = TextEditingController();
  final _provinceCityController = TextEditingController();
  final _licenseNumberController = TextEditingController();
  final _phoneController = TextEditingController();
  final _businessTypeController = TextEditingController();
  final _detailsController = TextEditingController();
  bool _isSubmitting = false;

  @override
  void dispose() {
    _applicantNameController.dispose();
    _provinceCityController.dispose();
    _licenseNumberController.dispose();
    _phoneController.dispose();
    _businessTypeController.dispose();
    _detailsController.dispose();
    super.dispose();
  }

  Future<void> _handleSubmit() async {
    final applicantName = _applicantNameController.text.trim();
    final location = _provinceCityController.text.trim();

    if (applicantName.isEmpty || location.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please fill in required fields (Applicant Name and City/Province).')),
      );
      return;
    }

    setState(() => _isSubmitting = true);

    final apiClient = ref.read(apiClientProvider);
    final locale = ref.read(localeProvider);
    final authState = ref.read(authProvider);

    final response = await apiClient.post(
      '/vendors/register',
      {
        "shopNameEn": "$applicantName Agency ($location)",
        "shopNamePrs": "نمایندگی رسمی $applicantName ($location)",
        "shopNamePs": "د $applicantName رسمي نمایندګي ($location)",
        "description": "Official Agency Application: Business Type: ${_businessTypeController.text}, License: ${_licenseNumberController.text}, Details: ${_detailsController.text}",
        "bankAccountInfo": "Agency Pending Verification"
      },
      languageCode: locale.languageCode,
      token: authState.token,
    );

    setState(() => _isSubmitting = false);

    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('درخواست اخذ نمایندگی رسمی با موفقیت ثبت گردید. همکاران ما با شما تماس خواهند گرفت.'),
          backgroundColor: Colors.green,
          duration: Duration(seconds: 4),
        ),
      );
      await ref.read(authProvider.notifier).fetchUserProfile();
      Navigator.pop(context);
    }
  }

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;

    return Scaffold(
      appBar: AppBar(
        title: Text(langCode == 'ps' ? 'د رسمي نمایندګۍ غوښتنه' : (langCode == 'prs' || langCode == 'fa' ? 'درخواست اخذ نمایندگی رسمی' : 'Agency & Representation Application')),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24.0),
        child: Center(
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 700),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                CustomCard(
                  padding: const EdgeInsets.all(24),
                  child: Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.all(16),
                        decoration: BoxDecoration(
                          color: AppColors.royalBlue.withAlpha(40),
                          borderRadius: BorderRadius.circular(16),
                        ),
                        child: const Icon(Icons.verified_user, size: 40, color: AppColors.royalBlue),
                      ),
                      const SizedBox(width: 16),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              langCode == 'ps' ? 'د نورزي بازار رسمي نمایندګي' : (langCode == 'prs' || langCode == 'fa' ? 'نمایندگی رسمی بازار چند فروشندگی نورزی' : 'Noorzai Official Business Agency'),
                              style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              langCode == 'ps' ? 'په خپل ولایت او ښار کې د نورزي اعتباري استازی شئ' : (langCode == 'prs' || langCode == 'fa' ? 'نماینده رسمی فروش و توزیع محصولات در استان و شهر خود شوید.' : 'Become an authorized regional agency and distributor in your region.'),
                              style: const TextStyle(fontSize: 13, color: AppColors.textSecondary),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 24),
                CustomTextField(
                  controller: _applicantNameController,
                  labelText: langCode == 'ps' ? 'د غوښتونکي / شرکت نوم' : (langCode == 'prs' || langCode == 'fa' ? 'نام متقاضی یا نام شرکت / متقاضی' : 'Applicant Name / Company Name'),
                  prefixIcon: Icons.business,
                ),
                const SizedBox(height: 16),
                CustomTextField(
                  controller: _provinceCityController,
                  labelText: langCode == 'ps' ? 'ولایت او ښار' : (langCode == 'prs' || langCode == 'fa' ? 'ولایت / استان و شهر محل نمایندگی' : 'Province & City'),
                  prefixIcon: Icons.location_city,
                ),
                const SizedBox(height: 16),
                CustomTextField(
                  controller: _phoneController,
                  labelText: langCode == 'ps' ? 'د اړیکې شمیره' : (langCode == 'prs' || langCode == 'fa' ? 'شماره تماس مستقیم' : 'Contact Phone Number'),
                  prefixIcon: Icons.phone,
                ),
                const SizedBox(height: 16),
                CustomTextField(
                  controller: _licenseNumberController,
                  labelText: langCode == 'ps' ? 'د جواز شمیره (اختیاري)' : (langCode == 'prs' || langCode == 'fa' ? 'شماره جواز کسب یا ثبت تجاری (اختیاری)' : 'Business License Number (Optional)'),
                  prefixIcon: Icons.badge,
                ),
                const SizedBox(height: 16),
                CustomTextField(
                  controller: _businessTypeController,
                  labelText: langCode == 'ps' ? 'د کار ساحه (خشکه‌بار، قالین، جامې)' : (langCode == 'prs' || langCode == 'fa' ? 'زمینه فعالیت (خشکبار، قالین، پوشاک، سایر)' : 'Business Category / Sector'),
                  prefixIcon: Icons.category,
                ),
                const SizedBox(height: 16),
                CustomTextField(
                  controller: _detailsController,
                  labelText: langCode == 'ps' ? 'اضافي توضیحات او د کار سابقه' : (langCode == 'prs' || langCode == 'fa' ? 'توضیحات سوابق کاری و امکانات دفتر/فروشگاه' : 'Additional Details & Experience'),
                  prefixIcon: Icons.notes,
                ),
                const SizedBox(height: 32),
                _isSubmitting
                    ? const Center(child: CircularProgressIndicator())
                    : CustomButton(
                        text: langCode == 'ps' ? 'د نمایندګۍ غوښتنلیک لیږل' : (langCode == 'prs' || langCode == 'fa' ? 'ارسال رسمی فرم درخواست نمایندگی' : 'Submit Agency Application'),
                        icon: Icons.send,
                        onPressed: _handleSubmit,
                      ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
