import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../shared/widgets/custom_card.dart';
import '../../../../shared/widgets/custom_text_field.dart';
import '../../../cart_checkout/presentation/cart_provider.dart';
import '../catalog_provider.dart';
import '../domain/models/product.dart';
import 'product_detail_screen.dart';

class ShopScreen extends ConsumerStatefulWidget {
  const ShopScreen({super.key});

  @override
  ConsumerState<ShopScreen> createState() => _ShopScreenState();
}

class _ShopScreenState extends ConsumerState<ShopScreen> {
  String _searchQuery = '';
  String? _selectedCategory;

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;
    
    final productsAsync = ref.watch(productsProvider);
    final categoriesAsync = ref.watch(categoriesProvider);

    return Padding(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          CustomTextField(
            hintText: langCode == 'ps' ? 'د توکو لټون...' : (langCode == 'prs' || langCode == 'fa' ? 'جستجوی محصولات...' : 'Search products...'),
            labelText: langCode == 'ps' ? 'لټون' : (langCode == 'prs' || langCode == 'fa' ? 'جستجو' : 'Search'),
            prefixIcon: Icons.search,
            onChanged: (val) {
              setState(() {
                _searchQuery = val;
              });
            },
          ),
          const SizedBox(height: 16),

          // Category Filter Chips
          categoriesAsync.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (err, stack) => Text('Error loading categories: $err'),
            data: (categories) => SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: Row(
                children: [
                  FilterChip(
                    label: Text(langCode == 'ps' ? 'ټول' : (langCode == 'prs' || langCode == 'fa' ? 'همه' : 'All')),
                    selected: _selectedCategory == null,
                    onSelected: (_) {
                      setState(() {
                        _selectedCategory = null;
                      });
                    },
                  ),
                  const SizedBox(width: 8),
                  ...categories.map((cat) {
                    return Padding(
                      padding: const EdgeInsets.only(right: 8),
                      child: FilterChip(
                        label: Text(cat.getName(langCode)),
                        selected: _selectedCategory == cat.id,
                        onSelected: (selected) {
                          setState(() {
                            _selectedCategory = selected ? cat.id : null;
                          });
                        },
                      ),
                    );
                  }),
                ],
              ),
            ),
          ),
          const SizedBox(height: 24),

          // Products Grid
          Expanded(
            child: productsAsync.when(
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (err, stack) => Center(child: Text('Error loading products: $err')),
              data: (allProducts) {
                final filteredProducts = allProducts.where((p) {
                  final matchesSearch = p.getTitle(langCode).toLowerCase().contains(_searchQuery.toLowerCase());
                  final matchesCategory = _selectedCategory == null || p.categoryId == _selectedCategory;
                  return matchesSearch && matchesCategory;
                }).toList();

                if (filteredProducts.isEmpty) {
                  return Center(
                    child: Text(
                      langCode == 'ps'
                          ? 'هیڅ توکي ونه موندل شول.'
                          : (langCode == 'prs' || langCode == 'fa' ? 'هیچ محصولی یافت نشد.' : 'No products found.'),
                      style: Theme.of(context).textTheme.titleLarge,
                    ),
                  );
                }

                return LayoutBuilder(
                  builder: (context, constraints) {
                    final isWide = constraints.maxWidth > 900;
                    return GridView.builder(
                      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                        crossAxisCount: isWide ? 4 : (constraints.maxWidth > 600 ? 3 : 2),
                        crossAxisSpacing: 16,
                        mainAxisSpacing: 16,
                        childAspectRatio: 0.7,
                      ),
                      itemCount: filteredProducts.length,
                      itemBuilder: (context, index) {
                        return _buildProductCard(context, ref, filteredProducts[index], langCode);
                      },
                    );
                  },
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildProductCard(BuildContext context, WidgetRef ref, Product product, String langCode) {
    return CustomCard(
      padding: EdgeInsets.zero,
      onTap: () {
        Navigator.of(context).push(
          MaterialPageRoute(
            builder: (_) => ProductDetailScreen(product: product),
          ),
        );
      },
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            child: Stack(
              children: [
                Container(
                  decoration: BoxDecoration(
                    borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
                    image: DecorationImage(
                      image: NetworkImage(product.imageUrl),
                      fit: BoxFit.cover,
                    ),
                  ),
                ),
                Positioned(
                  top: 10,
                  right: 10,
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                    decoration: BoxDecoration(
                      color: Colors.black.withAlpha(160),
                      borderRadius: BorderRadius.circular(20),
                      border: Border.all(color: Colors.white.withAlpha(40)),
                    ),
                    child: Row(
                      children: [
                        const Icon(Icons.star, color: AppColors.accentGold, size: 13),
                        const SizedBox(width: 4),
                        Text(
                          '${product.rating}',
                          style: const TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: Colors.white),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(14),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  product.getTitle(langCode),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
                ),
                const SizedBox(height: 8),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      '\$${product.price.toStringAsFixed(2)}',
                      style: const TextStyle(
                        color: AppColors.secondaryPurple,
                        fontWeight: FontWeight.bold,
                        fontSize: 15,
                      ),
                    ),
                    InkWell(
                      onTap: () {
                        ref.read(cartProvider.notifier).addToCart(product);
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(
                            content: Text('${product.getTitle(langCode)} added to cart!'),
                            duration: const Duration(seconds: 1),
                          ),
                        );
                      },
                      borderRadius: BorderRadius.circular(12),
                      child: Container(
                        padding: const EdgeInsets.all(8),
                        decoration: BoxDecoration(
                          gradient: AppColors.primaryGradient,
                          borderRadius: BorderRadius.circular(12),
                        ),
                        child: const Icon(Icons.add_shopping_cart, size: 16, color: Colors.white),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
