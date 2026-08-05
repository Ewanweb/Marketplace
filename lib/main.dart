import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'core/localization/locale_provider.dart';
import 'core/network/api_client.dart';
import 'core/router/app_router.dart';
import 'core/theme/app_theme.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();

  final apiClient = ApiClient();

  // Centralized Global Flutter Exception Logger to Seq Log Server
  FlutterError.onError = (FlutterErrorDetails details) {
    FlutterError.presentError(details);
    apiClient.sendClientLog(
      message: details.exceptionAsString(),
      stackTrace: details.stack?.toString(),
      level: 'Error',
      route: 'FlutterUI',
    );
  };

  PlatformDispatcher.instance.onError = (Object error, StackTrace stack) {
    apiClient.sendClientLog(
      message: error.toString(),
      stackTrace: stack.toString(),
      level: 'Error',
      route: 'PlatformDispatcher',
    );
    return true;
  };

  runApp(
    const ProviderScope(
      child: MarketplaceApp(),
    ),
  );
}

class MarketplaceApp extends ConsumerWidget {
  const MarketplaceApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final router = ref.watch(routerProvider);
    final locale = ref.watch(localeProvider);

    return MaterialApp.router(
      title: 'Noorzai Marketplace',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.lightTheme,
      darkTheme: AppTheme.darkTheme,
      themeMode: ThemeMode.light,
      locale: locale,
      supportedLocales: const [
        Locale('en', 'US'),
        Locale('fa', 'AF'),
        Locale('ps', 'AF'),
      ],
      localizationsDelegates: const [
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      routerConfig: router,
    );
  }
}
