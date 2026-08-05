import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/localization/backend_localization_provider.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../shared/widgets/custom_button.dart';
import '../../../../shared/widgets/custom_card.dart';
import '../../../../shared/widgets/custom_text_field.dart';
import '../auth_provider.dart';
import 'register_screen.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _obscurePassword = true;

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  void _handleLogin() async {
    final success = await ref.read(authProvider.notifier).login(
      _emailController.text.trim(),
      _passwordController.text.trim(),
    );

    if (success && mounted) {
      final role = ref.read(authProvider).role;
      if (role == 'SuperAdmin' || role == 'Admin') {
        context.go('/admin');
      } else {
        context.go('/');
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final localeNotifier = ref.read(localeProvider.notifier);
    final b10n = ref.watch(backendLocalizationProvider.notifier);
    final authState = ref.watch(authProvider);

    return Directionality(
      textDirection: localeNotifier.textDirection,
      child: Scaffold(
        backgroundColor: AppColors.background,
        appBar: AppBar(
          backgroundColor: Colors.transparent,
          elevation: 0,
          actions: [
            PopupMenuButton<String>(
              icon: const Icon(Icons.language, color: AppColors.textPrimary),
              onSelected: (code) => localeNotifier.setLocale(code),
              itemBuilder: (context) => const [
                PopupMenuItem(value: 'prs', child: Text('دری (Dari)')),
                PopupMenuItem(value: 'ps', child: Text('پښتو (Pashto)')),
                PopupMenuItem(value: 'en', child: Text('English')),
              ],
            ),
          ],
        ),
        body: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24.0),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 440),
              child: CustomCard(
                padding: const EdgeInsets.all(36.0),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Text(
                      b10n.translate('AppName', 'BuyMore'),
                      textAlign: TextAlign.center,
                      style: const TextStyle(
                        fontSize: 28,
                        fontWeight: FontWeight.w900,
                        color: AppColors.royalBlue,
                      ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      b10n.translate('SignIn', 'Sign In'),
                      textAlign: TextAlign.center,
                      style: const TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                        color: AppColors.textPrimary,
                      ),
                    ),
                    const SizedBox(height: 32),
                    if (authState.errorMessage != null)
                      Container(
                        padding: const EdgeInsets.all(12),
                        margin: const EdgeInsets.only(bottom: 20),
                        decoration: BoxDecoration(
                          color: AppColors.dangerRed.withAlpha(20),
                          borderRadius: BorderRadius.circular(16),
                          border: Border.all(color: AppColors.dangerRed),
                        ),
                        child: Text(
                          authState.errorMessage!,
                          style: const TextStyle(color: AppColors.dangerRed, fontSize: 13),
                          textAlign: TextAlign.center,
                        ),
                      ),
                    CustomTextField(
                      controller: _emailController,
                      keyboardType: TextInputType.emailAddress,
                      labelText: 'Email',
                      prefixIcon: Icons.email_outlined,
                    ),
                    const SizedBox(height: 20),
                    CustomTextField(
                      controller: _passwordController,
                      obscureText: _obscurePassword,
                      labelText: 'Password',
                      prefixIcon: Icons.lock_outline,
                      suffixIcon: IconButton(
                        icon: Icon(_obscurePassword ? Icons.visibility_off : Icons.visibility),
                        onPressed: () => setState(() => _obscurePassword = !_obscurePassword),
                      ),
                    ),
                    const SizedBox(height: 32),
                    CustomButton(
                      text: 'Sign In',
                      isLoading: authState.isLoading,
                      onPressed: _handleLogin,
                    ),
                    const SizedBox(height: 20),
                    TextButton(
                      onPressed: () {
                        Navigator.of(context).push(
                          MaterialPageRoute(builder: (_) => const RegisterScreen()),
                        );
                      },
                      child: const Text('Don\'t have an account? Sign Up'),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
