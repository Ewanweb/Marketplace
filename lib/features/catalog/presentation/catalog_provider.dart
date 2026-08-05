import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/localization/locale_provider.dart';
import '../../../core/network/api_client.dart';
import '../../auth/presentation/auth_provider.dart';
import '../domain/models/product.dart';

final catalogSearchQueryProvider = StateProvider<String>((ref) => '');
final catalogSortByProvider = StateProvider<String>((ref) => 'newest');
final catalogCategoryFilterProvider = StateProvider<String?>((ref) => null);

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

  return [];
});

final productsProvider = FutureProvider<List<Product>>((ref) async {
  final apiClient = ref.read(apiClientProvider);
  final locale = ref.watch(localeProvider);
  final authState = ref.watch(authProvider);
  final searchQuery = ref.watch(catalogSearchQueryProvider);
  final sortBy = ref.watch(catalogSortByProvider);
  final categoryId = ref.watch(catalogCategoryFilterProvider);

  final queryParams = <String>[];
  if (searchQuery.isNotEmpty) {
    queryParams.add('search=${Uri.encodeComponent(searchQuery)}');
  }
  if (categoryId != null && categoryId.isNotEmpty) {
    queryParams.add('categoryId=$categoryId');
  }
  if (sortBy.isNotEmpty) {
    queryParams.add('sortBy=$sortBy');
  }

  final queryString = queryParams.isNotEmpty ? '?${queryParams.join('&')}' : '';

  final response = await apiClient.get(
    '/products$queryString',
    languageCode: locale.languageCode,
    token: authState.token,
  );

  if (response == null) {
    throw Exception('Network error while loading products.');
  }

  if (response['isSuccess'] == true) {
    final List<dynamic> data = response['value'] ?? [];
    return data.map((json) => Product.fromJson(json)).toList();
  } else {
    final errorCode = response['error']?['code'];
    if (errorCode == 'Auth.Unauthorized' || errorCode == 'Auth.InvalidCredentials') {
      throw Exception('Session expired. Please login again.');
    }
    throw Exception(response['error']?['message'] ?? 'Failed to load products');
  }
});
