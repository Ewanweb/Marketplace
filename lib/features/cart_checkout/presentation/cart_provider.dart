import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../catalog/domain/models/product.dart';

class CartItem {
  final Product product;
  int quantity;
  String selectedSize;
  String selectedColor;

  CartItem({
    required this.product,
    this.quantity = 1,
    required this.selectedSize,
    required this.selectedColor,
  });

  double get totalPrice => product.price * quantity;
}

class CartNotifier extends StateNotifier<List<CartItem>> {
  CartNotifier() : super([]);

  void addToCart(Product product, {String size = 'Standard', String color = 'Default'}) {
    final existingIndex = state.indexWhere(
      (item) => item.product.id == product.id && item.selectedSize == size && item.selectedColor == color,
    );

    if (existingIndex >= 0) {
      final updatedList = List<CartItem>.from(state);
      updatedList[existingIndex].quantity += 1;
      state = updatedList;
    } else {
      state = [
        ...state,
        CartItem(product: product, selectedSize: size, selectedColor: color),
      ];
    }
  }

  void updateQuantity(CartItem item, int delta) {
    final updatedList = List<CartItem>.from(state);
    final index = updatedList.indexOf(item);
    if (index >= 0) {
      updatedList[index].quantity += delta;
      if (updatedList[index].quantity <= 0) {
        updatedList.removeAt(index);
      }
      state = updatedList;
    }
  }

  void removeFromCart(CartItem item) {
    state = state.where((element) => element != item).toList();
  }

  void clearCart() {
    state = [];
  }

  double get subtotal => state.fold(0, (sum, item) => sum + item.totalPrice);
  double get shippingFee => state.isEmpty ? 0 : 5.0;
  double get totalAmount => subtotal + shippingFee;
}

final cartProvider = StateNotifierProvider<CartNotifier, List<CartItem>>((ref) {
  return CartNotifier();
});
