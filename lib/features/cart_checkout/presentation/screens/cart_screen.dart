import 'dart:math' show max;
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/theme/app_colors.dart';
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
  final _couponController = TextEditingController(text: 'NOORZAI20');
  double _appliedDiscountAmount = 0.0;
  String? _appliedCouponCode;
  bool _isApplyingCoupon = false;

  @override
  void dispose() {
    _couponController.dispose();
    super.dispose();
  }

  Future<void> _handleApplyCoupon(double rawTotal) async {
    final code = _couponController.text.trim();
    if (code.isEmpty) return;

    setState(() => _isApplyingCoupon = true);

    final apiClient = ref.read(apiClientProvider);
    final locale = ref.read(localeProvider);

    final response = await apiClient.post(
      '/coupons/apply',
      {
        "code": code,
        "orderAmount": rawTotal,
      },
      languageCode: locale.languageCode,
    );

    setState(() => _isApplyingCoupon = false);

    if (mounted) {
      if (response != null && response['isSuccess'] == true && response['value'] != null) {
        final val = response['value'];
        setState(() {
          _appliedDiscountAmount = (val['discountAmount'] as num).toDouble();
          _appliedCouponCode = val['code'];
        });
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Promo Code ${_appliedCouponCode} applied! Saved \$${_appliedDiscountAmount.toStringAsFixed(2)}'),
            backgroundColor: Colors.green,
          ),
        );
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(response?['error']?['message'] ?? 'Invalid promo code.'),
            backgroundColor: Colors.redAccent,
          ),
        );
      }
    }
  }

  void _showPaymentModal(String langCode) {
    final cartItems = ref.read(cartProvider);
    if (cartItems.isEmpty) return;

    final authState = ref.read(authProvider);
    String selectedPaymentMethod = 'CreditCard';
    final addressController = TextEditingController(text: 'Kabul, Afghanistan');
    final customerNameController = TextEditingController(text: authState.userName ?? 'Valued Customer');

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => StatefulBuilder(
        builder: (context, setModalState) {
          return Container(
            padding: EdgeInsets.only(
              top: 24,
              left: 24,
              right: 24,
              bottom: MediaQuery.of(context).viewInsets.bottom + 24,
            ),
            decoration: const BoxDecoration(
              color: Color(0xFF1E1E2E),
              borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
            ),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      langCode == 'ps' ? 'د تادیې طریقه غوره کړئ' : (langCode == 'prs' || langCode == 'fa' ? 'درگاه و روش پرداخت آنلاین' : 'Online Payment Gateway'),
                      style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold, color: Colors.white),
                    ),
                    IconButton(
                      icon: const Icon(Icons.close, color: Colors.white70),
                      onPressed: () => Navigator.pop(context),
                    ),
                  ],
                ),
                const SizedBox(height: 16),
                TextField(
                  controller: customerNameController,
                  decoration: InputDecoration(
                    labelText: langCode == 'ps' ? 'د پیرودونکي نوم' : (langCode == 'prs' || langCode == 'fa' ? 'نام گیرنده سفارش' : 'Customer Name'),
                    prefixIcon: const Icon(Icons.person),
                    border: const OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: addressController,
                  decoration: InputDecoration(
                    labelText: langCode == 'ps' ? 'د تحویلۍ پته' : (langCode == 'prs' || langCode == 'fa' ? 'آدرس دقیق تحویل سفارش' : 'Shipping Address'),
                    prefixIcon: const Icon(Icons.location_on),
                    border: const OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 20),
                Text(
                  langCode == 'ps' ? 'د تادیې بڼه:' : (langCode == 'prs' || langCode == 'fa' ? 'انتخاب روش پرداخت:' : 'Payment Method:'),
                  style: const TextStyle(fontWeight: FontWeight.bold, color: Colors.white70),
                ),
                const SizedBox(height: 10),
                RadioListTile<String>(
                  title: const Text('Credit Card / Master / Visa'),
                  secondary: const Icon(Icons.credit_card, color: Colors.blueAccent),
                  value: 'CreditCard',
                  groupValue: selectedPaymentMethod,
                  onChanged: (val) => setModalState(() => selectedPaymentMethod = val!),
                ),
                RadioListTile<String>(
                  title: const Text('Digital Wallet (PayPal / EasyPaisa)'),
                  secondary: const Icon(Icons.account_balance_wallet, color: Colors.purpleAccent),
                  value: 'DigitalWallet',
                  groupValue: selectedPaymentMethod,
                  onChanged: (val) => setModalState(() => selectedPaymentMethod = val!),
                ),
                RadioListTile<String>(
                  title: Text(langCode == 'ps' ? 'په لاس په لاس تادیه (COD)' : (langCode == 'prs' || langCode == 'fa' ? 'پرداخت در محل هنگام تحویل' : 'Cash on Delivery (COD)')),
                  secondary: const Icon(Icons.local_shipping, color: Colors.greenAccent),
                  value: 'CashOnDelivery',
                  groupValue: selectedPaymentMethod,
                  onChanged: (val) => setModalState(() => selectedPaymentMethod = val!),
                ),
                const SizedBox(height: 24),
                CustomButton(
                  text: langCode == 'ps' ? 'تادیه او نهایي کول' : (langCode == 'prs' || langCode == 'fa' ? 'پرداخت آنلاین و ثبت نهایی سفارش' : 'Pay & Confirm Order'),
                  icon: Icons.lock,
                  onPressed: () async {
                    Navigator.pop(context);
                    await _executeCheckoutAndPayment(
                      customerNameController.text.trim(),
                      addressController.text.trim(),
                      selectedPaymentMethod,
                    );
                  },
                ),
              ],
            ),
          );
        },
      ),
    );
  }

  Future<void> _executeCheckoutAndPayment(String customerName, String shippingAddress, String paymentMethod) async {
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

    // 1. Create Order
    final orderResponse = await apiClient.post(
      '/orders',
      {
        "customerName": customerName.isNotEmpty ? customerName : "Valued Customer",
        "shippingAddress": shippingAddress.isNotEmpty ? shippingAddress : "Kabul, Afghanistan",
        "items": itemsPayload
      },
      languageCode: locale.languageCode,
      token: token,
    );

    if (orderResponse != null && orderResponse['isSuccess'] == true && orderResponse['value'] != null) {
      final orderId = orderResponse['value'];

      // 2. Process Online Payment Gateway
      final paymentResponse = await apiClient.post(
        '/payments/process',
        {
          "orderId": orderId,
          "paymentMethod": paymentMethod
        },
        languageCode: locale.languageCode,
        token: token,
      );

      setState(() => _isCheckingOut = false);

      if (paymentResponse != null && paymentResponse['isSuccess'] == true && mounted) {
        ref.read(cartProvider.notifier).clearCart();
        setState(() {
          _appliedDiscountAmount = 0.0;
          _appliedCouponCode = null;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Payment successful! Order processed and status set to Paid.'),
            backgroundColor: Colors.green,
            duration: Duration(seconds: 4),
          ),
        );
      } else if (mounted) {
        ref.read(cartProvider.notifier).clearCart();
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Order created successfully! Pending payment.'),
            backgroundColor: Colors.green,
          ),
        );
      }
    } else if (mounted) {
      setState(() => _isCheckingOut = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Failed to place order: ${orderResponse?['error']?['message'] ?? 'Unknown error'}'),
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

    final rawTotal = cartNotifier.totalAmount;
    final finalTotal = max(0.0, rawTotal - _appliedDiscountAmount);

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
                            if (_appliedDiscountAmount > 0) {
                              setState(() {
                                _appliedDiscountAmount = 0.0;
                                _appliedCouponCode = null;
                              });
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
                            if (_appliedDiscountAmount > 0) {
                              setState(() {
                                _appliedDiscountAmount = 0.0;
                                _appliedCouponCode = null;
                              });
                            }
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

        // Summary Order Card with Promo Code Input
        Container(
          padding: const EdgeInsets.all(24),
          decoration: BoxDecoration(
            color: Theme.of(context).colorScheme.surface,
            borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
            border: Border.all(color: Colors.white.withAlpha(20)),
          ),
          child: Column(
            children: [
              // Promo Code Section
              Row(
                children: [
                  Expanded(
                    child: TextField(
                      controller: _couponController,
                      style: const TextStyle(fontSize: 14),
                      decoration: InputDecoration(
                        hintText: 'Enter Promo Code (e.g. NOORZAI20)',
                        prefixIcon: const Icon(Icons.confirmation_number_outlined, size: 18, color: AppColors.royalBlue),
                        isDense: true,
                        contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
                        border: OutlineInputBorder(borderRadius: BorderRadius.circular(14)),
                      ),
                    ),
                  ),
                  const SizedBox(width: 10),
                  _isApplyingCoupon
                      ? const CircularProgressIndicator()
                      : ElevatedButton(
                          style: ElevatedButton.styleFrom(
                            backgroundColor: AppColors.royalBlue,
                            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                          ),
                          onPressed: () => _handleApplyCoupon(rawTotal),
                          child: Text(langCode == 'ps' ? 'اعمال' : (langCode == 'prs' || langCode == 'fa' ? 'اعمال تخفیف' : 'Apply')),
                        ),
                ],
              ),
              const SizedBox(height: 16),

              if (_appliedDiscountAmount > 0) ...[
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text('Subtotal:', style: const TextStyle(color: Colors.white70)),
                    Text('\$${rawTotal.toStringAsFixed(2)}', style: const TextStyle(color: Colors.white70)),
                  ],
                ),
                const SizedBox(height: 6),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Row(
                      children: [
                        const Icon(Icons.check_circle, color: Colors.greenAccent, size: 16),
                        const SizedBox(width: 4),
                        Text('Discount (${_appliedCouponCode}):', style: const TextStyle(color: Colors.greenAccent, fontWeight: FontWeight.bold)),
                      ],
                    ),
                    Text('-\$${_appliedDiscountAmount.toStringAsFixed(2)}', style: const TextStyle(color: Colors.greenAccent, fontWeight: FontWeight.bold)),
                  ],
                ),
                const Divider(height: 16),
              ],

              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    langCode == 'ps' ? 'ټوله مجموعه' : (langCode == 'prs' || langCode == 'fa' ? 'مجموع قابل پرداخت' : 'Final Total'),
                    style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                  ),
                  Text(
                    '\$${finalTotal.toStringAsFixed(2)}',
                    style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold, color: Color(0xFFA29BFE)),
                  ),
                ],
              ),
              const SizedBox(height: 16),
              _isCheckingOut
                  ? const Center(child: CircularProgressIndicator())
                  : CustomButton(
                      text: langCode == 'ps' ? 'د پېرلو منل او تادیه' : (langCode == 'prs' || langCode == 'fa' ? 'تکمیل سفارش و پرداخت آنلاین' : 'Proceed to Checkout & Pay'),
                      onPressed: () => _showPaymentModal(langCode),
                    ),
            ],
          ),
        ),
      ],
    );
  }
}

