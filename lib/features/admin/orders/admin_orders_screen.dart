import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/theme/app_colors.dart';

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

  Color _getStatusColor(String status) {
    switch (status) {
      case 'Delivered':
        return AppColors.horizonGreen;
      case 'Shipped':
        return AppColors.horizonSky;
      case 'Processing':
      default:
        return AppColors.horizonOrange;
    }
  }

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            langCode == 'ps'
                ? 'د فرمایشونو مدیریت'
                : (langCode == 'prs' || langCode == 'fa' ? 'مدیریت و تغییر وضعیت سفارشات' : 'Order Lifecycle Management'),
            style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: AppColors.horizonNavy),
          ),
          const SizedBox(height: 20),
          Expanded(
            child: ListView.separated(
              itemCount: _orders.length,
              separatorBuilder: (_, __) => const SizedBox(height: 12),
              itemBuilder: (context, index) {
                final order = _orders[index];
                final statusColor = _getStatusColor(order.status);

                return Container(
                  padding: const EdgeInsets.all(20),
                  decoration: BoxDecoration(
                    color: AppColors.horizonCard,
                    borderRadius: BorderRadius.circular(20),
                    boxShadow: const [AppColors.horizonShadow],
                  ),
                  child: Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: statusColor.withAlpha(25),
                          borderRadius: BorderRadius.circular(14),
                        ),
                        child: Icon(Icons.receipt_long_rounded, color: statusColor, size: 24),
                      ),
                      const SizedBox(width: 16),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              '${order.orderId} — ${order.customerName}',
                              style: const TextStyle(
                                fontWeight: FontWeight.bold,
                                fontSize: 15,
                                color: AppColors.horizonNavy,
                              ),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              '\$${order.amount.toStringAsFixed(2)}',
                              style: const TextStyle(
                                color: AppColors.horizonBrand,
                                fontWeight: FontWeight.w800,
                                fontSize: 14,
                              ),
                            ),
                          ],
                        ),
                      ),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                        decoration: BoxDecoration(
                          color: statusColor.withAlpha(20),
                          borderRadius: BorderRadius.circular(16),
                          border: Border.all(color: statusColor.withAlpha(100)),
                        ),
                        child: DropdownButtonHideUnderline(
                          child: DropdownButton<String>(
                            value: order.status,
                            icon: Icon(Icons.keyboard_arrow_down_rounded, color: statusColor, size: 18),
                            style: TextStyle(
                              fontSize: 12,
                              fontWeight: FontWeight.bold,
                              color: statusColor,
                            ),
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
                        ),
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
