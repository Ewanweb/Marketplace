import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/network/api_client.dart';
import '../../../../shared/widgets/custom_button.dart';
import '../../../../shared/widgets/custom_text_field.dart';
import '../../../auth/presentation/auth_provider.dart';

class VendorRegisterScreen extends ConsumerStatefulWidget {
  const VendorRegisterScreen({super.key});

  @override
  ConsumerState<VendorRegisterScreen> createState() => _VendorRegisterScreenState();
}

class _VendorRegisterScreenState extends ConsumerState<VendorRegisterScreen> {
  final _shopNameEnController = TextEditingController();
  final _shopNamePrsController = TextEditingController();
  final _shopNamePsController = TextEditingController();
  final _descriptionController = TextEditingController();
  final _bankAccountController = TextEditingController();
  bool _isLoading = false;

  Future<void> _handleRegister() async {
    final shopNameEn = _shopNameEnController.text.trim();
    final shopNamePrs = _shopNamePrsController.text.trim().isEmpty ? shopNameEn : _shopNamePrsController.text.trim();
    final shopNamePs = _shopNamePsController.text.trim().isEmpty ? shopNameEn : _shopNamePsController.text.trim();
    final description = _descriptionController.text.trim();
    final bankAccount = _bankAccountController.text.trim();

    if (shopNameEn.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please enter shop name in English')),
      );
      return;
    }

    setState(() => _isLoading = true);

    final apiClient = ref.read(apiClientProvider);
    final locale = ref.read(localeProvider);
    final authState = ref.read(authProvider);

    final response = await apiClient.post(
      '/vendors/register',
      {
        "shopNameEn": shopNameEn,
        "shopNamePrs": shopNamePrs,
        "shopNamePs": shopNamePs,
        "description": description,
        "bankAccountInfo": bankAccount
      },
      languageCode: locale.languageCode,
      token: authState.token,
    );

    setState(() => _isLoading = false);

    if (response != null && response['isSuccess'] == true && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Vendor registration submitted successfully!'), backgroundColor: Colors.green),
      );
      await ref.read(authProvider.notifier).fetchUserProfile();
      if (mounted) Navigator.pop(context);
    } else if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(response?['error']?['message'] ?? 'Failed to register as vendor.'),
          backgroundColor: Colors.redAccent,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;

    return Scaffold(
      appBar: AppBar(
        title: Text(langCode == 'ps' ? 'د پلورونکي په توګه نوم لیکنه' : (langCode == 'prs' || langCode == 'fa' ? 'ثبت نام به عنوان فروشنده' : 'Register as Vendor')),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              langCode == 'ps' ? 'خپل انلاین پلورنځی جوړ کړئ' : (langCode == 'prs' || langCode == 'fa' ? 'فروشگاه آنلاین خود را بسازید' : 'Create Your Online Shop'),
              style: Theme.of(context).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 8),
            Text(
              langCode == 'ps' ? 'خپل محصولات په نورزی بازار کې وپلورئ' : (langCode == 'prs' || langCode == 'fa' ? 'محصولات خود را در بازار نورزی به فروش برسانید' : 'Start selling your products on Noorzai Marketplace.'),
              style: const TextStyle(color: Colors.white70),
            ),
            const SizedBox(height: 24),
            CustomTextField(
              controller: _shopNameEnController,
              labelText: 'Shop Name (English)',
              prefixIcon: Icons.store,
            ),
            const SizedBox(height: 16),
            CustomTextField(
              controller: _shopNamePrsController,
              labelText: 'نام فروشگاه (دری)',
              prefixIcon: Icons.storefront,
            ),
            const SizedBox(height: 16),
            CustomTextField(
              controller: _shopNamePsController,
              labelText: 'د پلورنځي نوم (پښتو)',
              prefixIcon: Icons.storefront,
            ),
            const SizedBox(height: 16),
            CustomTextField(
              controller: _descriptionController,
              labelText: 'Description / توضیحات',
              prefixIcon: Icons.description,
            ),
            const SizedBox(height: 16),
            CustomTextField(
              controller: _bankAccountController,
              labelText: 'Bank Account Info / اطلاعات حساب بانکی',
              prefixIcon: Icons.account_balance,
            ),
            const SizedBox(height: 32),
            _isLoading
                ? const Center(child: CircularProgressIndicator())
                : CustomButton(
                    text: langCode == 'ps' ? 'د پلورنځي نوم لیکنه' : (langCode == 'prs' || langCode == 'fa' ? 'ثبت و ایجاد فروشگاه' : 'Submit Vendor Registration'),
                    onPressed: _handleRegister,
                  ),
          ],
        ),
      ),
    );
  }
}
