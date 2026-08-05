import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../shared/widgets/custom_button.dart';
import '../../../../shared/widgets/custom_card.dart';
import '../../../../shared/widgets/custom_input_field.dart';
import '../auth_provider.dart';
import '../widgets/profile_header_card.dart';

class ProfileScreen extends ConsumerStatefulWidget {
  const ProfileScreen({super.key});

  @override
  ConsumerState<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends ConsumerState<ProfileScreen> {
  final _formKey = GlobalKey<FormState>();

  late TextEditingController _nameController;
  late TextEditingController _emailController;
  late TextEditingController _phoneController;
  late TextEditingController _addressController;
  bool _isSaving = false;

  @override
  void initState() {
    super.initState();
    final authState = ref.read(authProvider);
    _nameController = TextEditingController(text: authState.userName ?? '');
    _emailController = TextEditingController(text: authState.email ?? '');
    _phoneController = TextEditingController(text: '+93 700 123 456');
    _addressController = TextEditingController(text: 'Kabul, Afghanistan');
  }

  @override
  void dispose() {
    _nameController.dispose();
    _emailController.dispose();
    _phoneController.dispose();
    _addressController.dispose();
    super.dispose();
  }

  Future<void> _handleSaveProfile() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isSaving = true);

    final apiClient = ref.read(apiClientProvider);
    final locale = ref.read(localeProvider);
    final authState = ref.read(authProvider);

    // Simulate / update call
    await Future.delayed(const Duration(milliseconds: 600));
    await ref.read(authProvider.notifier).fetchUserProfile();

    setState(() => _isSaving = false);

    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            locale.languageCode == 'ps'
                ? 'د پروفایل بدلونونه په بریالیتوب سره خوندي شول!'
                : (locale.languageCode == 'prs' || locale.languageCode == 'fa'
                    ? 'اطلاعات پروفایل با موفقیت بروزرسانی شد.'
                    : 'Profile updated successfully!'),
          ),
          backgroundColor: Colors.green,
        ),
      );
      Navigator.pop(context);
    }
  }

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authProvider);
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;

    return Scaffold(
      appBar: AppBar(
        title: Text(
          langCode == 'ps' ? 'د کارونکي پروفایل سمول' : (langCode == 'prs' || langCode == 'fa' ? 'ویرایش مشخصات پروفایل' : 'Edit My Profile'),
        ),
        backgroundColor: Theme.of(context).colorScheme.surface,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24.0),
        child: Center(
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 680),
            child: Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  // Hero Header Profile Card Component
                  ProfileHeaderCard(
                    userName: _nameController.text.isNotEmpty ? _nameController.text : (authState.userName ?? 'Valued Customer'),
                    userEmail: authState.email ?? '',
                    role: authState.role ?? 'Customer',
                    isVendor: authState.isVendor,
                    onAvatarTap: () {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(content: Text('Avatar upload dialog opens...')),
                      );
                    },
                  ),
                  const SizedBox(height: 28),

                  // Form Title & Details Card
                  CustomCard(
                    padding: const EdgeInsets.all(24),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          langCode == 'ps' ? 'شخصي مشخصات' : (langCode == 'prs' || langCode == 'fa' ? 'اطلاعات شخصی و تماس' : 'Personal Information'),
                          style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          langCode == 'ps' ? 'خپل معلومات منظم وساتئ' : (langCode == 'prs' || langCode == 'fa' ? 'مشخصات خود را جهت ارسال دقیق سفارشات بروز نگه دارید.' : 'Keep your contact & delivery information updated.'),
                          style: const TextStyle(fontSize: 12, color: AppColors.textSecondary),
                        ),
                        const SizedBox(height: 24),

                        // Full Name Input
                        CustomInputField(
                          controller: _nameController,
                          labelText: langCode == 'ps' ? 'بشپړ نوم' : (langCode == 'prs' || langCode == 'fa' ? 'نام و نام خانوادگی کامل' : 'Full Name'),
                          prefixIcon: Icons.person_outline_rounded,
                          validator: (val) {
                            if (val == null || val.trim().length < 3) {
                              return langCode == 'ps' ? 'مهرباني وکړئ مکمل نوم ولیکئ' : (langCode == 'prs' || langCode == 'fa' ? 'لطفاً نام و نام خانوادگی کامل را وارد کنید.' : 'Please enter your full name.');
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 18),

                        // Email Address Input (ReadOnly)
                        CustomInputField(
                          controller: _emailController,
                          labelText: langCode == 'ps' ? 'بریښنالیک پته' : (langCode == 'prs' || langCode == 'fa' ? 'آدرس ایمیل (غیرقابل تغییر)' : 'Email Address'),
                          prefixIcon: Icons.email_outlined,
                          readOnly: true,
                          suffixIcon: const Icon(Icons.lock_outline, size: 18, color: Colors.white38),
                        ),
                        const SizedBox(height: 18),

                        // Phone Number Input
                        CustomInputField(
                          controller: _phoneController,
                          labelText: langCode == 'ps' ? 'د اړیکې شمیره' : (langCode == 'prs' || langCode == 'fa' ? 'شماره تماس اصلی' : 'Primary Phone Number'),
                          prefixIcon: Icons.phone_android_outlined,
                          keyboardType: TextInputType.phone,
                          validator: (val) {
                            if (val == null || val.trim().isEmpty) {
                              return langCode == 'ps' ? 'د اړیکې شمیره ثبته کړئ' : (langCode == 'prs' || langCode == 'fa' ? 'شماره تماس را وارد کنید.' : 'Phone number is required.');
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 18),

                        // Default Shipping Address Input
                        CustomInputField(
                          controller: _addressController,
                          labelText: langCode == 'ps' ? 'د اصلی تحویلۍ پته' : (langCode == 'prs' || langCode == 'fa' ? 'آدرس اصلی تحویل سفارشات' : 'Default Delivery Address'),
                          prefixIcon: Icons.location_on_outlined,
                          maxLines: 2,
                          validator: (val) {
                            if (val == null || val.trim().isEmpty) {
                              return langCode == 'ps' ? 'مهرباني وکړئ د تحویلۍ پته ولیکئ' : (langCode == 'prs' || langCode == 'fa' ? 'آدرس تحویل سفارشات را مشخص کنید.' : 'Address is required.');
                            }
                            return null;
                          },
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 28),

                  // Action Buttons
                  _isSaving
                      ? const Center(child: CircularProgressIndicator())
                      : CustomButton(
                          text: langCode == 'ps' ? 'بدلونونه ذخیره کړئ' : (langCode == 'prs' || langCode == 'fa' ? 'ذخیره تغییرات پروفایل' : 'Save Profile Changes'),
                          icon: Icons.save_rounded,
                          onPressed: _handleSaveProfile,
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
