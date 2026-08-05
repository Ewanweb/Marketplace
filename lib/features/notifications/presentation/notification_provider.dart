import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/localization/locale_provider.dart';
import '../../../core/network/api_client.dart';
import '../../auth/presentation/auth_provider.dart';

final notificationsProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  final apiClient = ref.watch(apiClientProvider);
  final locale = ref.watch(localeProvider);
  final token = ref.watch(authProvider).token;

  if (token == null || token.isEmpty) return [];

  final response = await apiClient.get(
    '/notifications',
    languageCode: locale.languageCode,
    token: token,
  );

  if (response != null && response['isSuccess'] == true && response['value'] != null) {
    return List<dynamic>.from(response['value']);
  }
  return [];
});

final unreadNotificationsCountProvider = Provider.autoDispose<int>((ref) {
  final notificationsAsync = ref.watch(notificationsProvider);
  return notificationsAsync.maybeWhen(
    data: (list) => list.where((item) => item['isRead'] == false).length,
    orElse: () => 0,
  );
});
