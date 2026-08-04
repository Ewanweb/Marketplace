import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../shared/widgets/custom_button.dart';
import '../../../../shared/widgets/custom_text_field.dart';
import '../auth_provider.dart';

class ProfileScreen extends ConsumerStatefulWidget {
  const ProfileScreen({super.key});

  @override
  ConsumerState<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends ConsumerState<ProfileScreen> {
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

  Future<void> _handleSave() async {
    setState(() => _isSaving = true);
    await Future.delayed(const Duration(milliseconds: 600));
    setState(() => _isSaving = false);

    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Profile updated successfully!'),
          backgroundColor: Colors.green,
        ),
      );
      Navigator.pop(context);
    }
  }

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;

    return Scaffold(
      appBar: AppBar(
        title: Text(langCode == 'ps' ? 'د پروفایل سمول' : (langCode == 'prs' || langCode == 'fa' ? 'ویرایش پروفایل من' : 'Edit Profile')),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24.0),
        child: Center(
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 600),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Center(
                  child: Stack(
                    children: [
                      const CircleAvatar(
                        radius: 48,
                        backgroundColor: AppColors.royalBlue,
                        child: Icon(Icons.person, size: 48, color: Colors.white),
                      ),
                      Positioned(
                        bottom: 0,
                        right: 0,
                        child: Container(
                          padding: const EdgeInsets.all(6),
                          decoration: const BoxDecoration(
                            color: Colors.greenAccent,
                            shape: BoxShape.circle,
                          ),
                          child: const Icon(Icons.camera_alt, size: 16, color: Colors.black),
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 32),
                CustomTextField(
                  controller: _nameController,
                  labelText: langCode == 'ps' ? 'بشپړ نوم' : (langCode == 'prs' || langCode == 'fa' ? 'نام و نام خانوادگی' : 'Full Name'),
                  prefixIcon: Icons.person_outline,
                ),
                const SizedBox(height: 16),
                CustomTextField(
                  controller: _emailController,
                  labelText: langCode == 'ps' ? 'بریښنالیک' : (langCode == 'prs' || langCode == 'fa' ? 'ایمیل' : 'Email Address'),
                  prefixIcon: Icons.email_outlined,
                  keyboardType: TextInputType.emailAddress,
                ),
                const SizedBox(height: 16),
                CustomTextField(
                  controller: _phoneController,
                  labelText: langCode == 'ps' ? 'د تلیفون شمیره' : (langCode == 'prs' || langCode == 'fa' ? 'شماره تماس' : 'Phone Number'),
                  prefixIcon: Icons.phone_outlined,
                  keyboardType: TextInputType.phone,
                ),
                const SizedBox(height: 16),
                CustomTextField(
                  controller: _addressController,
                  labelText: langCode == 'ps' ? 'د تحویلۍ پته' : (langCode == 'prs' || langCode == 'fa' ? 'آدرس تحویل سفارشات' : 'Default Delivery Address'),
                  prefixIcon: Icons.location_on_outlined,
                ),
                const SizedBox(height: 32),
                _isSaving
                    ? const Center(child: CircularProgressIndicator())
                    : CustomButton(
                        text: langCode == 'ps' ? 'بدلونونه ذخیره کړئ' : (langCode == 'prs' || langCode == 'fa' ? 'ذخیره تغییرات پروفایل' : 'Save Profile Changes'),
                        icon: Icons.save,
                        onPressed: _handleSave,
                      ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
