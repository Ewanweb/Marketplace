import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../shared/widgets/custom_button.dart';
import '../../../../shared/widgets/custom_card.dart';
import '../../../../shared/widgets/custom_text_field.dart';
import '../../catalog/data/mock_data.dart';
import '../../catalog/domain/models/product.dart';

class AdminProductsScreen extends ConsumerStatefulWidget {
  const AdminProductsScreen({super.key});

  @override
  ConsumerState<AdminProductsScreen> createState() => _AdminProductsScreenState();
}

class _AdminProductsScreenState extends ConsumerState<AdminProductsScreen> {
  late List<Product> _productList;

  @override
  void initState() {
    super.initState();
    _productList = List.from(MockData.products);
  }

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
                labelText: 'Product Title',
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
            CustomButton(
              text: 'Save Product',
              onPressed: () {
                if (titleEnController.text.isNotEmpty && priceController.text.isNotEmpty) {
                  final newProduct = Product(
                    id: 'p_${DateTime.now().millisecondsSinceEpoch}',
                    titleEn: titleEnController.text,
                    titlePrs: titleEnController.text,
                    titlePs: titleEnController.text,
                    descriptionEn: 'New added product item',
                    descriptionPrs: 'محصول جدید اضافه شده',
                    descriptionPs: 'نوی زیات شوی توکی',
                    price: double.tryParse(priceController.text) ?? 10.0,
                    rating: 5.0,
                    imageUrl: 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500',
                    categoryId: 'cat_electronics',
                    availableSizes: ['M'],
                    availableColors: ['Black'],
                  );

                  setState(() {
                    _productList.add(newProduct);
                  });
                  Navigator.pop(context);
                }
              },
            ),
          ],
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;

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

          // Product List Table
          Expanded(
            child: ListView.separated(
              itemCount: _productList.length,
              separatorBuilder: (_, __) => const SizedBox(height: 12),
              itemBuilder: (context, index) {
                final product = _productList[index];
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
                        onPressed: () {
                          setState(() {
                            _productList.removeAt(index);
                          });
                        },
                      ),
                    ],
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }
}
