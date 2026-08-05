import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/network/api_client.dart';
import '../../../../shared/widgets/custom_button.dart';
import '../../../../shared/widgets/custom_card.dart';
import '../../../auth/presentation/auth_provider.dart';
import '../../../cart_checkout/presentation/cart_provider.dart';
import '../../domain/models/product.dart';

final productReviewsProvider = FutureProvider.family.autoDispose<List<dynamic>, String>((ref, productId) async {
  final apiClient = ref.watch(apiClientProvider);
  final locale = ref.watch(localeProvider);

  final response = await apiClient.get(
    '/reviews/product/$productId',
    languageCode: locale.languageCode,
  );

  if (response != null && response['isSuccess'] == true && response['value'] != null) {
    return List<dynamic>.from(response['value']);
  }
  return [];
});

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
    _selectedSize = widget.product.availableSizes.isNotEmpty ? widget.product.availableSizes.first : 'M';
    _selectedColor = widget.product.availableColors.isNotEmpty ? widget.product.availableColors.first : 'Default';
  }

  void _showAddReviewDialog(String langCode) {
    int selectedRating = 5;
    final commentController = TextEditingController();

    showDialog(
      context: context,
      builder: (context) => StatefulBuilder(
        builder: (context, setModalState) {
          return AlertDialog(
            title: Text(langCode == 'ps' ? 'خپل نظر ثبت کړئ' : (langCode == 'prs' || langCode == 'fa' ? 'ثبت نظر و امتیاز' : 'Write a Review')),
            content: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(langCode == 'ps' ? 'امتیاز ورکړئ:' : (langCode == 'prs' || langCode == 'fa' ? 'امتیاز شما به محصول:' : 'Select Rating:')),
                const SizedBox(height: 8),
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: List.generate(5, (index) {
                    final starNum = index + 1;
                    return IconButton(
                      icon: Icon(
                        starNum <= selectedRating ? Icons.star : Icons.star_border,
                        color: Colors.amber,
                        size: 32,
                      ),
                      onPressed: () => setModalState(() => selectedRating = starNum),
                    );
                  }),
                ),
                const SizedBox(height: 16),
                TextField(
                  controller: commentController,
                  maxLines: 3,
                  decoration: InputDecoration(
                    labelText: langCode == 'ps' ? 'ستاسو نظر' : (langCode == 'prs' || langCode == 'fa' ? 'توضیحات و نظر شما در مورد محصول' : 'Your Review Comment'),
                    border: const OutlineInputBorder(),
                  ),
                ),
              ],
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.pop(context),
                child: Text(langCode == 'ps' ? 'لغوه' : (langCode == 'prs' || langCode == 'fa' ? 'انصراف' : 'Cancel')),
              ),
              ElevatedButton(
                onPressed: () async {
                  final comment = commentController.text.trim();
                  if (comment.isEmpty) return;

                  final apiClient = ref.read(apiClientProvider);
                  final locale = ref.read(localeProvider);
                  final token = ref.read(authProvider).token;

                  final response = await apiClient.post(
                    '/reviews',
                    {
                      "productId": widget.product.id,
                      "rating": selectedRating,
                      "comment": comment
                    },
                    languageCode: locale.languageCode,
                    token: token,
                  );

                  if (context.mounted) {
                    Navigator.pop(context);
                    if (response != null && response['isSuccess'] == true) {
                      ref.invalidate(productReviewsProvider(widget.product.id));
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(content: Text('Review submitted successfully!'), backgroundColor: Colors.green),
                      );
                    } else {
                      ScaffoldMessenger.of(context).showSnackBar(
                        SnackBar(content: Text(response?['error']?['message'] ?? 'Failed to submit review.'), backgroundColor: Colors.redAccent),
                      );
                    }
                  }
                },
                child: Text(langCode == 'ps' ? 'ثبت' : (langCode == 'prs' || langCode == 'fa' ? 'ثبت نظر' : 'Submit')),
              ),
            ],
          );
        },
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;
    final reviewsAsync = ref.watch(productReviewsProvider(widget.product.id));
    final authState = ref.watch(authProvider);

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
                        style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
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
                    const Icon(Icons.star, color: Colors.amber, size: 20),
                    const SizedBox(width: 6),
                    Text(
                      '${widget.product.rating.toStringAsFixed(1)} / 5.0',
                      style: const TextStyle(fontWeight: FontWeight.bold, color: Colors.white),
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
                  const SizedBox(height: 24),
                ],

                const Divider(height: 32),

                // Customer Reviews Section
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      langCode == 'ps' ? 'د پیرودونکو نظریات' : (langCode == 'prs' || langCode == 'fa' ? 'نظرات و امتیازات خریداران' : 'Customer Reviews'),
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
                    ),
                    if (authState.isAuthenticated)
                      OutlinedButton.icon(
                        icon: const Icon(Icons.rate_review, size: 16),
                        label: Text(langCode == 'ps' ? 'نظر ورکول' : (langCode == 'prs' || langCode == 'fa' ? 'ثبت نظر جدید' : 'Write Review')),
                        onPressed: () => _showAddReviewDialog(langCode),
                      ),
                  ],
                ),
                const SizedBox(height: 16),
                reviewsAsync.when(
                  loading: () => const Center(child: CircularProgressIndicator()),
                  error: (_, __) => const SizedBox(),
                  data: (reviews) {
                    if (reviews.isEmpty) {
                      return CustomCard(
                        padding: const EdgeInsets.all(20),
                        child: Center(
                          child: Text(
                            langCode == 'ps' ? 'تر اوسه لومړنی نظر نه دی ثبت شوی.' : (langCode == 'prs' || langCode == 'fa' ? 'هنوز نظری برای این محصول ثبت نشده است.' : 'No reviews recorded for this product yet.'),
                            style: const TextStyle(color: Colors.white60),
                          ),
                        ),
                      );
                    }

                    return ListView.separated(
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      itemCount: reviews.length,
                      separatorBuilder: (_, __) => const SizedBox(height: 10),
                      itemBuilder: (context, index) {
                        final rev = reviews[index];
                        return CustomCard(
                          padding: const EdgeInsets.all(14),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Row(
                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                children: [
                                  Text(
                                    rev['userName'] ?? 'Customer',
                                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                                  ),
                                  Row(
                                    children: List.generate(5, (starIdx) {
                                      return Icon(
                                        starIdx < (rev['rating'] ?? 5) ? Icons.star : Icons.star_border,
                                        color: Colors.amber,
                                        size: 16,
                                      );
                                    }),
                                  ),
                                ],
                              ),
                              const SizedBox(height: 6),
                              Text(
                                rev['comment'] ?? '',
                                style: const TextStyle(fontSize: 13, color: Colors.white70),
                              ),
                            ],
                          ),
                        );
                      },
                    );
                  },
                ),
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
