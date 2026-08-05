import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../shared/widgets/custom_button.dart';
import '../../../../shared/widgets/custom_card.dart';
import '../../../../shared/widgets/custom_input_field.dart';
import '../../../auth/presentation/auth_provider.dart';
import '../widgets/agency_banner_card.dart';

class AgencyApplicationScreen extends ConsumerStatefulWidget {
  const AgencyApplicationScreen({super.key});

  @override
  ConsumerState<AgencyApplicationScreen> createState() => _AgencyApplicationScreenState();
}

class _AgencyApplicationScreenState extends ConsumerState<AgencyApplicationScreen> {
  final _formKey = GlobalKey<FormState>();

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

  Future<void> _handleSubmitApplication() async {
    if (!_formKey.currentState!.validate()) return;

    final applicantName = _applicantNameController.text.trim();
    final location = _provinceCityController.text.trim();

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
        "description": "Official Business Agency: Sector: ${_businessTypeController.text}, Phone: ${_phoneController.text}, License: ${_licenseNumberController.text}, Details: ${_detailsController.text}",
        "bankAccountInfo": "Official Regional Agency Verification"
      },
      languageCode: locale.languageCode,
      token: authState.token,
    );

    setState(() => _isSubmitting = false);

    if (mounted) {
      if (response != null && response['isSuccess'] == true) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(
              locale.languageCode == 'ps'
                  ? 'د رسمي نمایندګۍ غوښتنه په بریالیتوب سره ثبت شوه!'
                  : (locale.languageCode == 'prs' || locale.languageCode == 'fa'
                      ? 'درخواست اخذ نمایندگی رسمی با موفقیت ثبت گردید. کارشناسان ما با شما تماس خواهند گرفت.'
                      : 'Agency application submitted successfully!'),
            ),
            backgroundColor: Colors.green,
            duration: const Duration(seconds: 4),
          ),
        );
        await ref.read(authProvider.notifier).fetchUserProfile();
        Navigator.pop(context);
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(response?['error']?['message'] ?? 'Failed to submit agency application.'),
            backgroundColor: Colors.redAccent,
          ),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;

    return Scaffold(
      appBar: AppBar(
        title: Text(
          langCode == 'ps' ? 'د رسمي نمایندګۍ غوښتنه' : (langCode == 'prs' || langCode == 'fa' ? 'فرم درخواست اخذ نمایندگی رسمی' : 'Agency Application'),
        ),
        backgroundColor: Theme.of(context).colorScheme.surface,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24.0),
        child: Center(
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 750),
            child: Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  // Agency Banner Card Component
                  AgencyBannerCard(
                    title: langCode == 'ps' ? 'د نورزي بازار رسمي نمایندګي' : (langCode == 'prs' || langCode == 'fa' ? 'اخذ نمایندگی انحصاری توزیع و فروش' : 'Noorzai Official Agency Program'),
                    subtitle: langCode == 'ps'
                        ? 'په خپل ولایت او ښار کې د نورزي اعتباري استازی شئ او سوداګري پراخه کړئ.'
                        : (langCode == 'prs' || langCode == 'fa'
                            ? 'نماینده رسمی توزیع محصولات در استان خود شوید و از پلتفرم فروشگاهی بهره‌مند شوید.'
                            : 'Become an authorized regional distributor & branch partner in your province.'),
                    benefits: [
                      langCode == 'ps' ? 'د ولایتي فروش تضمین او همکاري' : (langCode == 'prs' || langCode == 'fa' ? 'اعطای پنل اختصاصی مدیریت فروشندگان استانی' : 'Dedicated provincial vendor hub access'),
                      langCode == 'ps' ? 'د پلور د محصولاتو فوري تایید' : (langCode == 'prs' || langCode == 'fa' ? 'پشتیبانی اولویت‌دار و بازاریابی منطقه‌ای' : 'Priority support and regional marketing'),
                    ],
                  ),
                  const SizedBox(height: 28),

                  // Form Input Sections
                  CustomCard(
                    padding: const EdgeInsets.all(24),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          langCode == 'ps' ? 'د متقاضي او سوداګرۍ مشخصات' : (langCode == 'prs' || langCode == 'fa' ? 'مشخصات متقاضی و کسب‌وکار' : 'Applicant & Business Details'),
                          style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          langCode == 'ps' ? 'مهرباني وکړئ لاندې مشخصات په دقت دسی ثبت کړئ' : (langCode == 'prs' || langCode == 'fa' ? 'اطلاعات دقیق خود را جهت بررسی پرونده نمایندگی وارد کنید.' : 'Provide verified business details for background review.'),
                          style: const TextStyle(fontSize: 12, color: AppColors.textSecondary),
                        ),
                        const SizedBox(height: 24),

                        // Applicant / Company Name
                        CustomInputField(
                          controller: _applicantNameController,
                          labelText: langCode == 'ps' ? 'د غوښتونکي / شرکت نوم' : (langCode == 'prs' || langCode == 'fa' ? 'نام متقاضی یا نام شرکت' : 'Applicant Name / Company'),
                          prefixIcon: Icons.business_center_rounded,
                          validator: (val) {
                            if (val == null || val.trim().isEmpty) {
                              return langCode == 'ps' ? 'د متقاضي نوم اړین دی' : (langCode == 'prs' || langCode == 'fa' ? 'نام متقاضی یا شرکت را وارد کنید.' : 'Applicant name is required.');
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 18),

                        // Province & City Location
                        CustomInputField(
                          controller: _provinceCityController,
                          labelText: langCode == 'ps' ? 'ولایت او ښار' : (langCode == 'prs' || langCode == 'fa' ? 'ولایت / استان و شهر محل نمایندگی' : 'Province & City'),
                          prefixIcon: Icons.location_city_rounded,
                          validator: (val) {
                            if (val == null || val.trim().isEmpty) {
                              return langCode == 'ps' ? 'ولایت او ښار وټاکئ' : (langCode == 'prs' || langCode == 'fa' ? 'محل نمایندگی را مشخص کنید.' : 'Location is required.');
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 18),

                        // Direct Phone Number
                        CustomInputField(
                          controller: _phoneController,
                          labelText: langCode == 'ps' ? 'د اړیکې شمیره' : (langCode == 'prs' || langCode == 'fa' ? 'شماره تماس مستقیم جهت هماهنگی' : 'Direct Phone Number'),
                          prefixIcon: Icons.phone_in_talk_rounded,
                          keyboardType: TextInputType.phone,
                          validator: (val) {
                            if (val == null || val.trim().isEmpty) {
                              return langCode == 'ps' ? 'د اړیکې شمیره نوشته کړئ' : (langCode == 'prs' || langCode == 'fa' ? 'شماره تماس را وارد کنید.' : 'Phone number is required.');
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 18),

                        // Business License Number (Optional)
                        CustomInputField(
                          controller: _licenseNumberController,
                          labelText: langCode == 'ps' ? 'د جواز شمیره (اختیاري)' : (langCode == 'prs' || langCode == 'fa' ? 'شماره جواز کسب یا ثبت تجاری (اختیاری)' : 'Business License Number (Optional)'),
                          prefixIcon: Icons.badge_rounded,
                        ),
                        const SizedBox(height: 18),

                        // Business Sector
                        CustomInputField(
                          controller: _businessTypeController,
                          labelText: langCode == 'ps' ? 'د کار ساحه (خشکه‌بار، قالین، جامې)' : (langCode == 'prs' || langCode == 'fa' ? 'زمینه اصلی فعالیت (خشکبار، قالین، پوشاک)' : 'Business Category / Sector'),
                          prefixIcon: Icons.category_rounded,
                          validator: (val) {
                            if (val == null || val.trim().isEmpty) {
                              return langCode == 'ps' ? 'د کار ساحه وټاکئ' : (langCode == 'prs' || langCode == 'fa' ? 'زمینه فعالیت را مشخص کنید.' : 'Sector is required.');
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 18),

                        // Additional Details & Facilities
                        CustomInputField(
                          controller: _detailsController,
                          labelText: langCode == 'ps' ? 'اضافي توضیحات او د کار سابقه' : (langCode == 'prs' || langCode == 'fa' ? 'توضیحات سوابق کاری و امکانات دفتر' : 'Additional Details & Facilities'),
                          prefixIcon: Icons.notes_rounded,
                          maxLines: 3,
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 28),

                  // Submit Action Button
                  _isSubmitting
                      ? const Center(child: CircularProgressIndicator())
                      : CustomButton(
                          text: langCode == 'ps' ? 'د نمایندګۍ غوښتنلیک لیږل' : (langCode == 'prs' || langCode == 'fa' ? 'ارسال رسمی فرم درخواست نمایندگی' : 'Submit Agency Application'),
                          icon: Icons.send_rounded,
                          onPressed: _handleSubmitApplication,
                        ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
