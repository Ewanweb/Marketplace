import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/network/api_client.dart';
import '../../../../shared/widgets/custom_button.dart';
import '../../../../shared/widgets/custom_card.dart';
import '../../../../shared/widgets/custom_text_field.dart';
import '../../auth/presentation/auth_provider.dart';
import '../../catalog/presentation/catalog_provider.dart';

class AdminProductsScreen extends ConsumerStatefulWidget {
  const AdminProductsScreen({super.key});

  @override
  ConsumerState<AdminProductsScreen> createState() => _AdminProductsScreenState();
}

class _AdminProductsScreenState extends ConsumerState<AdminProductsScreen> {
  bool _isLoading = false;

  void _showAddProductDialog() {
    final titleEnController = TextEditingController();
    final priceController = TextEditingController();

    showDialog(
      context: context,
      builder: (context) {
        return AlertDialog(
          title: const Text('Add New Product'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              CustomTextField(
                controller: titleEnController,
                labelText: 'Product Title (EN)',
              ),
              const SizedBox(height: 12),
              CustomTextField(
                controller: priceController,
                labelText: 'Price (\$)',
                keyboardType: TextInputType.number,
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('Cancel'),
            ),
            ElevatedButton(
              onPressed: () async {
                final price = double.tryParse(priceController.text) ?? 0.0;
                final titleEn = titleEnController.text.trim();
                
                if (titleEn.isNotEmpty && price > 0) {
                  setState(() => _isLoading = true);
                  Navigator.pop(context);

                  final apiClient = ref.read(apiClientProvider);
                  final locale = ref.read(localeProvider);
                  final token = ref.read(authProvider).token;

                  final body = {
                    "titleEn": titleEn,
                    "titlePrs": titleEn, // Defaulting for now
                    "titlePs": titleEn,
                    "descriptionEn": "Default description",
                    "descriptionPrs": "توضیحات پیش فرض",
                    "descriptionPs": "توضیحات پیش فرض",
                    "price": price,
                    "stockQuantity": 10,
                    "imageUrl": "https://via.placeholder.com/150",
                    "categoryId": "11111111-1111-1111-1111-111111111111", // Default Category
                    "vendorId": "66666666-6666-6666-6666-666666666666", // Default Vendor
                    "availableSizes": "M,L",
                    "availableColors": "Default"
                  };

                  final response = await apiClient.post(
                    '/products',
                    body,
                    languageCode: locale.languageCode,
                    token: token,
                  );

                  setState(() => _isLoading = false);

                  if (response != null && response['isSuccess'] == true && context.mounted) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(content: Text('Product created successfully')),
                    );
                    ref.invalidate(productsProvider);
                  } else if (context.mounted) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(content: Text('Failed to create product: ${response?['error']?['message'] ?? 'Unknown'}')),
                    );
                  }
                }
              },
              child: const Text('Add'),
            ),
          ],
        );
      },
    );
  }

  Future<void> _deleteProduct(String id) async {
    setState(() => _isLoading = true);
    
    final apiClient = ref.read(apiClientProvider);
    final locale = ref.read(localeProvider);
    final token = ref.read(authProvider).token;

    final response = await apiClient.delete(
      '/products/$id',
      languageCode: locale.languageCode,
      token: token,
    );

    setState(() => _isLoading = false);

    if (response != null && response['isSuccess'] == true && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Product deleted successfully')),
      );
      ref.invalidate(productsProvider);
    } else if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to delete product: ${response?['error']?['message'] ?? 'Unknown'}')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;
    final productsAsync = ref.watch(productsProvider);

    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                langCode == 'ps' ? 'د توکو مدیریت (CRUD)' : (langCode == 'prs' || langCode == 'fa' ? 'مدیریت و ویرایش محصولات' : 'Product Management'),
                style: Theme.of(context).textTheme.titleLarge,
              ),
              CustomButton(
                text: langCode == 'ps' ? 'نوی توکی زیاتول' : (langCode == 'prs' || langCode == 'fa' ? 'افزودن محصول جدید' : 'Add Product'),
                icon: Icons.add,
                onPressed: _showAddProductDialog,
              ),
            ],
          ),
          const SizedBox(height: 24),
          
          if (_isLoading) const LinearProgressIndicator(),
          if (_isLoading) const SizedBox(height: 12),

          // Product List Table
          Expanded(
            child: productsAsync.when(
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (err, stack) => Center(child: Text('Error: $err')),
              data: (products) => ListView.separated(
                itemCount: products.length,
                separatorBuilder: (_, __) => const SizedBox(height: 12),
                itemBuilder: (context, index) {
                  final product = products[index];
                  return CustomCard(
                    padding: const EdgeInsets.all(16),
                    child: Row(
                      children: [
                        Container(
                          width: 50,
                          height: 50,
                          decoration: BoxDecoration(
                            borderRadius: BorderRadius.circular(12),
                            image: DecorationImage(
                              image: NetworkImage(product.imageUrl),
                              fit: BoxFit.cover,
                            ),
                          ),
                        ),
                        const SizedBox(width: 16),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                product.getTitle(langCode),
                                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                              ),
                              const SizedBox(height: 4),
                              Text(
                                '\$${product.price.toStringAsFixed(2)}',
                                style: const TextStyle(color: Color(0xFFA29BFE), fontWeight: FontWeight.bold),
                              ),
                            ],
                          ),
                        ),
                        IconButton(
                          icon: const Icon(Icons.edit, color: Colors.white70),
                          onPressed: () {},
                        ),
                        IconButton(
                          icon: const Icon(Icons.delete_outline, color: Colors.redAccent),
                          onPressed: () => _deleteProduct(product.id),
                        ),
                      ],
                    ),
                  );
                },
              ),
            ),
          ),
        ],
      ),
    );
  }
}
