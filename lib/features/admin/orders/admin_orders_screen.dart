import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../shared/widgets/custom_card.dart';

class OrderMockItem {
  final String orderId;
  final String customerName;
  final double amount;
  String status;

  OrderMockItem({
    required this.orderId,
    required this.customerName,
    required this.amount,
    required this.status,
  });
}

class AdminOrdersScreen extends ConsumerStatefulWidget {
  const AdminOrdersScreen({super.key});

  @override
  ConsumerState<AdminOrdersScreen> createState() => _AdminOrdersScreenState();
}

class _AdminOrdersScreenState extends ConsumerState<AdminOrdersScreen> {
  final List<OrderMockItem> _orders = [
    OrderMockItem(orderId: 'ORD-9021', customerName: 'Ahmad Rahimi', amount: 145.00, status: 'Processing'),
    OrderMockItem(orderId: 'ORD-9022', customerName: 'Fatima Noori', amount: 280.00, status: 'Shipped'),
    OrderMockItem(orderId: 'ORD-9023', customerName: 'Mohammad Karimi', amount: 65.00, status: 'Delivered'),
  ];

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;

    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            langCode == 'ps' ? 'د فرمایشونو مدیریت' : (langCode == 'prs' || langCode == 'fa' ? 'مدیریت و وضعیت سفارشات' : 'Order Management'),
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 24),
          Expanded(
            child: ListView.separated(
              itemCount: _orders.length,
              separatorBuilder: (_, __) => const SizedBox(height: 12),
              itemBuilder: (context, index) {
                final order = _orders[index];
                return CustomCard(
                  child: Row(
                    children: [
                      const Icon(Icons.receipt_long, color: Color(0xFF6C5CE7), size: 28),
                      const SizedBox(width: 16),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              '${order.orderId} - ${order.customerName}',
                              style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              '\$${order.amount.toStringAsFixed(2)}',
                              style: const TextStyle(color: Color(0xFFA29BFE), fontWeight: FontWeight.bold),
                            ),
                          ],
                        ),
                      ),
                      DropdownButton<String>(
                        value: order.status,
                        dropdownColor: Theme.of(context).colorScheme.surface,
                        items: const [
                          DropdownMenuItem(value: 'Processing', child: Text('Processing')),
                          DropdownMenuItem(value: 'Shipped', child: Text('Shipped')),
                          DropdownMenuItem(value: 'Delivered', child: Text('Delivered')),
                        ],
                        onChanged: (newStatus) {
                          if (newStatus != null) {
                            setState(() {
                              order.status = newStatus;
                            });
                          }
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
