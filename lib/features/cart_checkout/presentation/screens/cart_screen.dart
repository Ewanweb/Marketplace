import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/network/api_client.dart';
import '../../../../shared/widgets/custom_button.dart';
import '../../../../shared/widgets/custom_card.dart';
import '../../../auth/presentation/auth_provider.dart';
import '../cart_provider.dart';

class CartScreen extends ConsumerStatefulWidget {
  const CartScreen({super.key});

  @override
  ConsumerState<CartScreen> createState() => _CartScreenState();
}

class _CartScreenState extends ConsumerState<CartScreen> {
  bool _isCheckingOut = false;

  Future<void> _handleCheckout() async {
    final cartItems = ref.read(cartProvider);
    if (cartItems.isEmpty) return;

    setState(() => _isCheckingOut = true);

    final apiClient = ref.read(apiClientProvider);
    final locale = ref.read(localeProvider);
    final token = ref.read(authProvider).token;

    final itemsPayload = cartItems.map((item) {
      return {
        "productId": item.product.id,
        "quantity": item.quantity,
      };
    }).toList();

    final response = await apiClient.post(
      '/orders',
      {
        "customerName": "Customer", // This should ideally come from User Profile
        "items": itemsPayload
      },
      languageCode: locale.languageCode,
      token: token,
    );

    setState(() => _isCheckingOut = false);

    if (response != null && response['isSuccess'] == true && mounted) {
      ref.read(cartProvider.notifier).clearCart();
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Order placed successfully! Thank you for buying.'),
          backgroundColor: Colors.green,
        ),
      );
    } else if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Failed to place order: ${response?['error']?['message'] ?? 'Unknown error'}'),
          backgroundColor: Colors.redAccent,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final cartItems = ref.watch(cartProvider);
    final cartNotifier = ref.read(cartProvider.notifier);
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;

    if (cartItems.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.shopping_cart_outlined, size: 72, color: Colors.white38),
            const SizedBox(height: 16),
            Text(
              langCode == 'ps' ? 'د پېرلو ټوکرۍ تشه ده' : (langCode == 'prs' || langCode == 'fa' ? 'سبد خرید شما خالی است' : 'Your cart is empty'),
              style: const TextStyle(fontSize: 16, color: Colors.white70),
            ),
          ],
        ),
      );
    }

    return Column(
      children: [
        Expanded(
          child: ListView.separated(
            padding: const EdgeInsets.all(20),
            itemCount: cartItems.length,
            separatorBuilder: (_, __) => const SizedBox(height: 16),
            itemBuilder: (context, index) {
              final item = cartItems[index];
              return CustomCard(
                padding: const EdgeInsets.all(12),
                child: Row(
                  children: [
                    Container(
                      width: 70,
                      height: 70,
                      decoration: BoxDecoration(
                        borderRadius: BorderRadius.circular(12),
                        image: DecorationImage(
                          image: NetworkImage(item.product.imageUrl),
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
                            item.product.getTitle(langCode),
                            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                          ),
                          const SizedBox(height: 8),
                          Text(
                            '\$${item.product.price.toStringAsFixed(2)}',
                            style: const TextStyle(color: Color(0xFFA29BFE), fontWeight: FontWeight.bold),
                          ),
                        ],
                      ),
                    ),
                    Row(
                      children: [
                        IconButton(
                          icon: const Icon(Icons.remove_circle_outline, color: Colors.white70),
                          onPressed: () {
                            if (item.quantity > 1) {
                              cartNotifier.updateQuantity(item, -1);
                            } else {
                              cartNotifier.removeFromCart(item);
                            }
                          },
                        ),
                        Text(
                          '${item.quantity}',
                          style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                        ),
                        IconButton(
                          icon: const Icon(Icons.add_circle_outline, color: Colors.white70),
                          onPressed: () {
                            cartNotifier.updateQuantity(item, 1);
                          },
                        ),
                      ],
                    ),
                  ],
                ),
              );
            },
          ),
        ),

        // Summary Order Card
        Container(
          padding: const EdgeInsets.all(24),
          decoration: BoxDecoration(
            color: Theme.of(context).colorScheme.surface,
            borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
            border: Border.all(color: Colors.white.withAlpha(20)),
          ),
          child: Column(
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    langCode == 'ps' ? 'ټوله مجموعه' : (langCode == 'prs' || langCode == 'fa' ? 'مجموع کل' : 'Total'),
                    style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                  ),
                  Text(
                    '\$${cartNotifier.totalAmount.toStringAsFixed(2)}',
                    style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: Color(0xFFA29BFE)),
                  ),
                ],
              ),
              const SizedBox(height: 16),
              _isCheckingOut
                  ? const Center(child: CircularProgressIndicator())
                  : CustomButton(
                      text: langCode == 'ps' ? 'د پېرلو منل او تادیه' : (langCode == 'prs' || langCode == 'fa' ? 'تکمیل سفارش و پرداخت' : 'Proceed to Checkout'),
                      onPressed: _handleCheckout,
                    ),
            ],
          ),
        ),
      ],
    );
  }
}
