import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/backend_localization_provider.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../shared/widgets/custom_card.dart';
import '../../../cart_checkout/presentation/cart_provider.dart';
import '../catalog_provider.dart';
import '../../domain/models/product.dart';
import 'product_detail_screen.dart';

class HomeScreen extends ConsumerWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;
    final b10n = ref.watch(backendLocalizationProvider.notifier);
    final productsAsync = ref.watch(productsProvider);

    return SingleChildScrollView(
      padding: const EdgeInsets.all(24),
      child: productsAsync.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, stack) => Center(child: Text('Error loading products: $error')),
        data: (products) => Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Section 1: Banners & Vertical Product Cards Stack
            LayoutBuilder(
            builder: (context, constraints) {
              final isWide = constraints.maxWidth > 900;
              if (isWide) {
                return Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Expanded(
                      flex: 6,
                      child: Column(
                        children: [
                          _buildMintDiscountBanner(context, b10n),
                          const SizedBox(height: 20),
                          _buildYellowWinterBanner(context, b10n),
                        ],
                      ),
                    ),
                    const SizedBox(width: 20),
                    Expanded(
                      flex: 4,
                      child: Row(
                        children: [
                          if (products.isNotEmpty) Expanded(child: _buildProductVerticalCard(context, ref, products[0], langCode, b10n.translate('OurPicks', 'Our Picks'), '\$${products[0].price}')),
                          const SizedBox(width: 16),
                          if (products.length > 1) Expanded(child: _buildProductVerticalCard(context, ref, products[1], langCode, b10n.translate('YourChoice', 'Your Choice'), '\$${products[1].price}')),
                        ],
                      ),
                    ),
                  ],
                );
              } else {
                return Column(
                  children: [
                    _buildMintDiscountBanner(context, b10n),
                    const SizedBox(height: 20),
                    _buildYellowWinterBanner(context, b10n),
                    const SizedBox(height: 20),
                    Row(
                      children: [
                        if (products.isNotEmpty) Expanded(child: _buildProductVerticalCard(context, ref, products[0], langCode, b10n.translate('OurPicks', 'Our Picks'), '\$${products[0].price}')),
                        const SizedBox(width: 16),
                        if (products.length > 1) Expanded(child: _buildProductVerticalCard(context, ref, products[1], langCode, b10n.translate('YourChoice', 'Your Choice'), '\$${products[1].price}')),
                      ],
                    ),
                  ],
                );
              }
            },
          ),
          const SizedBox(height: 24),

          // Section 2: Bottom Grid Cards
          LayoutBuilder(
            builder: (context, constraints) {
              final isWide = constraints.maxWidth > 900;
              if (isWide) {
                return Row(
                  children: [
                    Expanded(child: _buildAvailOffersCard(context, b10n)),
                    const SizedBox(width: 20),
                    Expanded(child: _buildFavouritesMiniCard(context, b10n)),
                    const SizedBox(width: 20),
                    Expanded(flex: 2, child: _buildBringBoldFashionCard(context, b10n)),
                  ],
                );
              } else {
                return Column(
                  children: [
                    _buildAvailOffersCard(context, b10n),
                    const SizedBox(height: 16),
                    _buildFavouritesMiniCard(context, b10n),
                    const SizedBox(height: 16),
                    _buildBringBoldFashionCard(context, b10n),
                  ],
                );
              }
            },
          ),
        ],
      ),
      ),
    );
  }

  Widget _buildMintDiscountBanner(BuildContext context, dynamic b10n) {
    return Container(
      height: 180,
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        color: AppColors.pastelMint,
        borderRadius: BorderRadius.circular(28),
      ),
      child: Stack(
        children: [
          Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Text(
                      b10n.translate('GetUpTo50Off', 'GET UP TO 50% OFF'),
                      style: const TextStyle(
                        fontSize: 22,
                        fontWeight: FontWeight.w900,
                        color: AppColors.textPrimary,
                        letterSpacing: -0.5,
                      ),
                    ),
                    const SizedBox(height: 16),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 10),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(20),
                        boxShadow: [
                          BoxShadow(color: Colors.black.withAlpha(10), blurRadius: 8),
                        ],
                      ),
                      child: Text(
                        b10n.translate('GetDiscount', 'Get Discount'),
                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13, color: AppColors.textPrimary),
                      ),
                    ),
                  ],
                ),
              ),
              ClipRRect(
                borderRadius: BorderRadius.circular(20),
                child: Image.network(
                  'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=300',
                  width: 140,
                  height: 140,
                  fit: BoxFit.cover,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildYellowWinterBanner(BuildContext context, dynamic b10n) {
    return Container(
      height: 160,
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        color: AppColors.pastelYellow,
        borderRadius: BorderRadius.circular(28),
      ),
      child: Stack(
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(
                    b10n.translate('WinterWeekend', "Winter's weekend"),
                    style: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold, color: AppColors.textPrimary),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    b10n.translate('KeepItCasual', 'keep it casual'),
                    style: const TextStyle(fontSize: 14, color: AppColors.textSecondary),
                  ),
                ],
              ),
              Container(
                padding: const EdgeInsets.all(10),
                decoration: const BoxDecoration(
                  color: Colors.white,
                  shape: BoxShape.circle,
                ),
                child: const Icon(Icons.north_east, size: 20, color: AppColors.textPrimary),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildProductVerticalCard(
    BuildContext context,
    WidgetRef ref,
    Product product,
    String langCode,
    String badgeText,
    String priceText,
  ) {
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
          Padding(
            padding: const EdgeInsets.all(14),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Row(
                  children: [
                    Container(width: 12, height: 12, decoration: const BoxDecoration(color: Color(0xFFF472B6), shape: BoxShape.circle)),
                    const SizedBox(width: 4),
                    Container(width: 12, height: 12, decoration: const BoxDecoration(color: Color(0xFFFBBF24), shape: BoxShape.circle)),
                  ],
                ),
                Container(
                  padding: const EdgeInsets.all(6),
                  decoration: BoxDecoration(color: AppColors.background, shape: BoxShape.circle),
                  child: const Icon(Icons.favorite_border, size: 16, color: AppColors.textSecondary),
                ),
              ],
            ),
          ),
          Expanded(
            child: Container(
              margin: const EdgeInsets.symmetric(horizontal: 14),
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(20),
                image: DecorationImage(
                  image: NetworkImage(product.imageUrl),
                  fit: BoxFit.cover,
                ),
              ),
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(14),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(badgeText, style: const TextStyle(fontSize: 11, color: AppColors.textSecondary)),
                      const SizedBox(height: 2),
                      Text(
                        product.getTitle(langCode),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
                      ),
                    ],
                  ),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                  decoration: const BoxDecoration(color: AppColors.royalBlue, shape: BoxShape.circle),
                  child: Text(
                    priceText,
                    style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 12),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildAvailOffersCard(BuildContext context, dynamic b10n) {
    return Container(
      height: 180,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(28),
        image: const DecorationImage(
          image: NetworkImage('https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=500'),
          fit: BoxFit.cover,
        ),
      ),
      child: Stack(
        children: [
          Positioned(
            bottom: 14,
            left: 14,
            right: 14,
            child: Container(
              padding: const EdgeInsets.symmetric(vertical: 12),
              alignment: Alignment.center,
              decoration: BoxDecoration(
                color: Colors.white.withAlpha(200),
                borderRadius: BorderRadius.circular(20),
              ),
              child: Text(
                b10n.translate('AvailOffers', 'Avail Offers'),
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13, color: AppColors.textPrimary),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFavouritesMiniCard(BuildContext context, dynamic b10n) {
    return Container(
      height: 180,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.pastelPeach,
        borderRadius: BorderRadius.circular(28),
      ),
      child: Column(
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                b10n.translate('Favourites', 'Favourites'),
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
              ),
              const Row(
                children: [
                  Icon(Icons.chevron_left, size: 20),
                  Icon(Icons.chevron_right, size: 20),
                ],
              ),
            ],
          ),
          const SizedBox(height: 12),
          Expanded(
            child: Row(
              children: [
                Expanded(
                  child: ClipRRect(
                    borderRadius: BorderRadius.circular(16),
                    child: Image.network('https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=300', fit: BoxFit.cover),
                  ),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: ClipRRect(
                    borderRadius: BorderRadius.circular(16),
                    child: Image.network('https://images.unsplash.com/photo-1529139574466-a303027c1d8b?w=300', fit: BoxFit.cover),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildBringBoldFashionCard(BuildContext context, dynamic b10n) {
    return Container(
      height: 180,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: AppColors.pastelGrey,
        borderRadius: BorderRadius.circular(28),
      ),
      child: Stack(
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(
                    b10n.translate('BringBoldFashion', 'Bring Bold Fashion'),
                    style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: AppColors.textPrimary),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    b10n.translate('LayersOnLayers', 'Layers on Layers'),
                    style: const TextStyle(fontSize: 13, color: AppColors.textSecondary),
                  ),
                ],
              ),
              ClipRRect(
                borderRadius: BorderRadius.circular(20),
                child: Image.network(
                  'https://images.unsplash.com/photo-1509631179647-0177331693ae?w=300',
                  width: 110,
                  height: 140,
                  fit: BoxFit.cover,
                ),
              ),
            ],
          ),
          Positioned(
            top: 0,
            right: 0,
            child: Container(
              padding: const EdgeInsets.all(8),
              decoration: const BoxDecoration(color: Colors.white, shape: BoxShape.circle),
              child: const Icon(Icons.north_east, size: 18, color: AppColors.textPrimary),
            ),
          ),
        ],
      ),
    );
  }
}
