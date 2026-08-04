import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/localization/locale_provider.dart';
import '../../../core/network/api_client.dart';
import '../../auth/presentation/auth_provider.dart';
import '../domain/models/product.dart';

final categoriesProvider = FutureProvider<List<Category>>((ref) async {
  final apiClient = ref.read(apiClientProvider);
  final locale = ref.watch(localeProvider);
  final authState = ref.watch(authProvider);

  final response = await apiClient.get(
    '/categories',
    languageCode: locale.languageCode,
    token: authState.token,
  );

  if (response != null && response['isSuccess'] == true) {
    final List<dynamic> data = response['value'] ?? [];
    return data.map((json) => Category.fromJson(json)).toList();
  }
  
  return []; // Fallback to empty list or throw error
});

final productsProvider = FutureProvider<List<Product>>((ref) async {
  final apiClient = ref.read(apiClientProvider);
  final locale = ref.watch(localeProvider);
  final authState = ref.watch(authProvider);

  // You can potentially watch a search/filter state here and append to the URL
  final response = await apiClient.get(
    '/products',
    languageCode: locale.languageCode,
    token: authState.token,
  );

  if (response != null && response['isSuccess'] == true) {
    final List<dynamic> data = response['value'] ?? [];
    return data.map((json) => Product.fromJson(json)).toList();
  }

  return [];
});
