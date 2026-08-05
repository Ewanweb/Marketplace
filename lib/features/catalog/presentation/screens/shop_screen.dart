import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../shared/widgets/custom_card.dart';
import '../../../../shared/widgets/custom_text_field.dart';
import '../../../cart_checkout/presentation/cart_provider.dart';
import '../catalog_provider.dart';
import '../../domain/models/product.dart';
import 'product_detail_screen.dart';

class ShopScreen extends ConsumerStatefulWidget {
  const ShopScreen({super.key});

  @override
  ConsumerState<ShopScreen> createState() => _ShopScreenState();
}

class _ShopScreenState extends ConsumerState<ShopScreen> {
  final _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;

    final productsAsync = ref.watch(productsProvider);
    final categoriesAsync = ref.watch(categoriesProvider);
    final selectedCategory = ref.watch(catalogCategoryFilterProvider);
    final currentSort = ref.watch(catalogSortByProvider);

    return Padding(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Search Bar and Sort Dropdown Row
          Row(
            children: [
              Expanded(
                child: CustomTextField(
                  controller: _searchController,
                  hintText: langCode == 'ps' ? 'د توکو او محصولاتو لټون...' : (langCode == 'prs' || langCode == 'fa' ? 'جستجوی پیشرفته محصولات...' : 'Search products...'),
                  labelText: langCode == 'ps' ? 'لټون' : (langCode == 'prs' || langCode == 'fa' ? 'جستجو' : 'Search'),
                  prefixIcon: Icons.search,
                  onChanged: (val) {
                    ref.read(catalogSearchQueryProvider.notifier).state = val.trim();
                  },
                ),
              ),
              const SizedBox(width: 12),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                decoration: BoxDecoration(
                  color: Theme.of(context).colorScheme.surface,
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(color: Colors.white.withAlpha(30)),
                ),
                child: DropdownButtonHideUnderline(
                  child: DropdownButton<String>(
                    value: currentSort,
                    icon: const Icon(Icons.sort_rounded, color: AppColors.royalBlue),
                    dropdownColor: const Color(0xFF1E1E2E),
                    items: [
                      DropdownMenuItem(
                        value: 'newest',
                        child: Text(langCode == 'ps' ? 'نوې توکي' : (langCode == 'prs' || langCode == 'fa' ? 'جدیدترین‌ها' : 'Newest')),
                      ),
                      DropdownMenuItem(
                        value: 'rating_desc',
                        child: Text(langCode == 'ps' ? 'تر ټولو ډیر امتیاز' : (langCode == 'prs' || langCode == 'fa' ? 'بالاترین امتیاز' : 'Highest Rated')),
                      ),
                      DropdownMenuItem(
                        value: 'price_asc',
                        child: Text(langCode == 'ps' ? 'ارزانه ته ګران' : (langCode == 'prs' || langCode == 'fa' ? 'ارزان‌ترین به گران‌ترین' : 'Price: Low to High')),
                      ),
                      DropdownMenuItem(
                        value: 'price_desc',
                        child: Text(langCode == 'ps' ? 'ګران ته ارزانه' : (langCode == 'prs' || langCode == 'fa' ? 'گران‌ترین به ارزان‌ترین' : 'Price: High to Low')),
                      ),
                    ],
                    onChanged: (val) {
                      if (val != null) {
                        ref.read(catalogSortByProvider.notifier).state = val;
                      }
                    },
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),

          // Category Filter Chips
          categoriesAsync.when(
            loading: () => const SizedBox(height: 40),
            error: (err, stack) => Text('Error loading categories: $err'),
            data: (categories) => SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: Row(
                children: [
                  FilterChip(
                    label: Text(langCode == 'ps' ? 'ټول' : (langCode == 'prs' || langCode == 'fa' ? 'همه دسته‌ها' : 'All Categories')),
                    selected: selectedCategory == null,
                    onSelected: (_) {
                      ref.read(catalogCategoryFilterProvider.notifier).state = null;
                    },
                  ),
                  const SizedBox(width: 8),
                  ...categories.map((cat) {
                    return Padding(
                      padding: const EdgeInsets.only(right: 8),
                      child: FilterChip(
                        label: Text(cat.getName(langCode)),
                        selected: selectedCategory == cat.id,
                        onSelected: (selected) {
                          ref.read(catalogCategoryFilterProvider.notifier).state = selected ? cat.id : null;
                        },
                      ),
                    );
                  }),
                ],
              ),
            ),
          ),
          const SizedBox(height: 20),

          // Products Grid
          Expanded(
            child: productsAsync.when(
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (err, stack) => Center(child: Text('Error loading products: $err')),
              data: (products) {
                if (products.isEmpty) {
                  return Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const Icon(Icons.search_off_rounded, size: 64, color: Colors.white30),
                        const SizedBox(height: 16),
                        Text(
                          langCode == 'ps'
                              ? 'هیڅ توکي ونه موندل شول.'
                              : (langCode == 'prs' || langCode == 'fa' ? 'هیچ محصولی با این مشخصات یافت نشد.' : 'No products found.'),
                          style: Theme.of(context).textTheme.titleLarge?.copyWith(color: Colors.white70),
                        ),
                      ],
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
                      itemCount: products.length,
                      itemBuilder: (context, index) {
                        return _buildProductCard(context, ref, products[index], langCode);
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
                          '${product.rating.toStringAsFixed(1)}',
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
