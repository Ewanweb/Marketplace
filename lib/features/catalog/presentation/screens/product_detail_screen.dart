import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../shared/widgets/custom_button.dart';
import '../../../cart_checkout/presentation/cart_provider.dart';
import '../../domain/models/product.dart';

class ProductDetailScreen extends ConsumerStatefulWidget {
  final Product product;

  const ProductDetailScreen({super.key, required this.product});

  @override
  ConsumerState<ProductDetailScreen> createState() => _ProductDetailScreenState();
}

class _ProductDetailScreenState extends ConsumerState<ProductDetailScreen> {
  late String _selectedSize;
  late String _selectedColor;

  @override
  void initState() {
    super.initState();
    _selectedSize = widget.product.availableSizes.first;
    _selectedColor = widget.product.availableColors.first;
  }

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;

    return Scaffold(
      appBar: AppBar(
        title: Text(widget.product.getTitle(langCode)),
        backgroundColor: Theme.of(context).colorScheme.surface,
      ),
      body: Column(
        children: [
          Expanded(
            child: ListView(
              padding: const EdgeInsets.all(20),
              children: [
                Hero(
                  tag: widget.product.id,
                  child: Container(
                    height: 260,
                    decoration: BoxDecoration(
                      borderRadius: BorderRadius.circular(24),
                      image: DecorationImage(
                        image: NetworkImage(widget.product.imageUrl),
                        fit: BoxFit.cover,
                      ),
                    ),
                  ),
                ),
                const SizedBox(height: 24),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Expanded(
                      child: Text(
                        widget.product.getTitle(langCode),
                        style: Theme.of(context).textTheme.titleLarge,
                      ),
                    ),
                    Text(
                      '\$${widget.product.price.toStringAsFixed(2)}',
                      style: const TextStyle(
                        fontSize: 22,
                        fontWeight: FontWeight.bold,
                        color: Color(0xFFA29BFE),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                Row(
                  children: [
                    const Icon(Icons.star, color: Colors.amber, size: 18),
                    const SizedBox(width: 6),
                    Text(
                      '${widget.product.rating} (124 reviews)',
                      style: const TextStyle(color: Colors.white70),
                    ),
                  ],
                ),
                const SizedBox(height: 20),
                Text(
                  widget.product.getDescription(langCode),
                  style: const TextStyle(fontSize: 14, height: 1.6, color: Colors.white60),
                ),
                const SizedBox(height: 24),

                // Size Selector
                if (widget.product.availableSizes.isNotEmpty) ...[
                  const Text('Size', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                  const SizedBox(height: 8),
                  Wrap(
                    spacing: 10,
                    children: widget.product.availableSizes.map((size) {
                      final isSelected = size == _selectedSize;
                      return ChoiceChip(
                        label: Text(size),
                        selected: isSelected,
                        onSelected: (selected) {
                          if (selected) setState(() => _selectedSize = size);
                        },
                      );
                    }).toList(),
                  ),
                  const SizedBox(height: 20),
                ],

                // Color Selector
                if (widget.product.availableColors.isNotEmpty) ...[
                  const Text('Color', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                  const SizedBox(height: 8),
                  Wrap(
                    spacing: 10,
                    children: widget.product.availableColors.map((color) {
                      final isSelected = color == _selectedColor;
                      return ChoiceChip(
                        label: Text(color),
                        selected: isSelected,
                        onSelected: (selected) {
                          if (selected) setState(() => _selectedColor = color);
                        },
                      );
                    }).toList(),
                  ),
                ],
              ],
            ),
          ),

          // Bottom Bar
          Container(
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              color: Theme.of(context).colorScheme.surface,
              borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
              border: Border.all(color: Colors.white.withAlpha(20)),
            ),
            child: CustomButton(
              text: langCode == 'ps' ? 'ټوکرۍ ته ورزیاتول' : (langCode == 'prs' || langCode == 'fa' ? 'افزودن به سبد خرید' : 'Add to Cart'),
              icon: Icons.add_shopping_cart,
              onPressed: () {
                ref.read(cartProvider.notifier).addToCart(
                      widget.product,
                      size: _selectedSize,
                      color: _selectedColor,
                    );
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(
                    content: Text('${widget.product.getTitle(langCode)} added to cart!'),
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
