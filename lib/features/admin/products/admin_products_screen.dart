import 'dart:convert';
import 'dart:html' as html;
import 'dart:typed_data';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:http/http.dart' as http;
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../shared/widgets/custom_text_field.dart';
import '../../auth/presentation/auth_provider.dart';
import '../../catalog/presentation/catalog_provider.dart';

class AdminProductsScreen extends ConsumerStatefulWidget {
  const AdminProductsScreen({super.key});

  @override
  ConsumerState<AdminProductsScreen> createState() => _AdminProductsScreenState();
}

class _AdminProductsScreenState extends ConsumerState<AdminProductsScreen> {
  bool _isLoading = false;

  void _showAddProductDialog() {
    final titleEnController = TextEditingController();
    final titlePrsController = TextEditingController();
    final titlePsController = TextEditingController();
    final priceController = TextEditingController();
    final stockController = TextEditingController(text: '10');
    final imageUrlController = TextEditingController(text: 'https://images.unsplash.com/photo-1596040033229-a9821ebd058d?w=600');
    final descEnController = TextEditingController();
    final descPrsController = TextEditingController();
    final descPsController = TextEditingController();
    final sizesController = TextEditingController(text: 'Standard, M, L');
    final colorsController = TextEditingController(text: 'Gold, Red, Natural');

    String selectedCategoryId = '11111111-1111-1111-1111-111111111111';
    String selectedVendorId = '66666666-6666-6666-6666-666666666666';
    bool isUploadingImage = false;

    showDialog(
      context: context,
      builder: (dialogContext) {
        final locale = ref.read(localeProvider);
        final langCode = locale.languageCode;

        return StatefulBuilder(
          builder: (context, setDialogState) {
            Future<void> pickAndUploadImage() async {
              try {
                final uploadInput = html.FileUploadInputElement();
                uploadInput.accept = 'image/*';
                uploadInput.click();

                uploadInput.onChange.listen((e) async {
                  final files = uploadInput.files;
                  if (files != null && files.isNotEmpty) {
                    final file = files[0];
                    setDialogState(() => isUploadingImage = true);

                    final reader = html.FileReader();
                    reader.readAsArrayBuffer(file);
                    reader.onLoadEnd.listen((e) async {
                      final bytes = reader.result as Uint8List;

                      final token = ref.read(authProvider).token;
                      final request = http.MultipartRequest(
                        'POST',
                        Uri.parse('${ApiClient.baseApiUrl}/files/upload'),
                      );
                      if (token != null && token.isNotEmpty) {
                        request.headers['Authorization'] = 'Bearer $token';
                      }

                      request.files.add(
                        http.MultipartFile.fromBytes(
                          'file',
                          bytes,
                          filename: file.name,
                        ),
                      );

                      final streamedResponse = await request.send();
                      final response = await http.Response.fromStream(streamedResponse);

                      setDialogState(() => isUploadingImage = false);

                      if (response.statusCode == 200) {
                        final json = jsonDecode(response.body);
                        if (json['isSuccess'] == true && json['value'] != null) {
                          final uploadedUrl = json['value']['url'];
                          setDialogState(() {
                            imageUrlController.text = uploadedUrl;
                          });
                          if (dialogContext.mounted) {
                            ScaffoldMessenger.of(dialogContext).showSnackBar(
                              const SnackBar(content: Text('Product image uploaded successfully!')),
                            );
                          }
                        }
                      } else {
                        if (dialogContext.mounted) {
                          ScaffoldMessenger.of(dialogContext).showSnackBar(
                            SnackBar(content: Text('Failed to upload image: ${response.statusCode}')),
                          );
                        }
                      }
                    });
                  }
                });
              } catch (err) {
                setDialogState(() => isUploadingImage = false);
                if (dialogContext.mounted) {
                  ScaffoldMessenger.of(dialogContext).showSnackBar(
                    SnackBar(content: Text('Error selecting file: $err')),
                  );
                }
              }
            }

            return AlertDialog(
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(24)),
              backgroundColor: AppColors.horizonCard,
              title: Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(8),
                    decoration: BoxDecoration(
                      color: AppColors.horizonBrand.withAlpha(20),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: const Icon(Icons.add_shopping_cart_rounded, color: AppColors.horizonBrand, size: 22),
                  ),
                  const SizedBox(width: 12),
                  Text(
                    langCode == 'ps'
                        ? 'نوی کامل محصول زیاتول'
                        : (langCode == 'prs' || langCode == 'fa' ? 'افزودن محصول جدید (اطلاعات کامل)' : 'Create Product (Comprehensive Form)'),
                    style: const TextStyle(color: AppColors.horizonNavy, fontWeight: FontWeight.bold, fontSize: 18),
                  ),
                ],
              ),
              content: SizedBox(
                width: 650,
                child: SingleChildScrollView(
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // SECTION 1: Trilingual Titles
                      _buildFormSectionHeader(langCode == 'ps' ? '۱. نښه او سرلیکونه (دری، پښتو، انګلیسي)' : (langCode == 'prs' || langCode == 'fa' ? '۱. عناوین محصول (دری، پښتو، انگلیسی)' : '1. Trilingual Titles')),
                      const SizedBox(height: 12),
                      CustomTextField(
                        controller: titlePrsController,
                        labelText: langCode == 'ps' ? 'دري سرلیک' : (langCode == 'prs' || langCode == 'fa' ? 'عنوان محصول (دری)' : 'Title (Dari)'),
                        hintText: 'مثال: زعفران ممتاز صادراتی هرات',
                      ),
                      const SizedBox(height: 12),
                      CustomTextField(
                        controller: titlePsController,
                        labelText: langCode == 'ps' ? 'پښتو سرلیک' : (langCode == 'prs' || langCode == 'fa' ? 'عنوان محصول (پښتو)' : 'Title (Pashto)'),
                        hintText: 'مثال: د هرات اعلا صادراتي زعفران',
                      ),
                      const SizedBox(height: 12),
                      CustomTextField(
                        controller: titleEnController,
                        labelText: langCode == 'ps' ? 'انګلیسي سرلیک' : (langCode == 'prs' || langCode == 'fa' ? 'عنوان محصول (انگلیسی)' : 'Title (English)'),
                        hintText: 'e.g. Herat Premium Saffron 10g',
                      ),
                      const SizedBox(height: 24),

                      // SECTION 2: Pricing & Stock Inventory
                      _buildFormSectionHeader(langCode == 'ps' ? '۲. قیمت او موجودي' : (langCode == 'prs' || langCode == 'fa' ? '۲. قیمت‌گذاری و موجودی انبار' : '2. Pricing & Stock Inventory')),
                      const SizedBox(height: 12),
                      Row(
                        children: [
                          Expanded(
                            child: CustomTextField(
                              controller: priceController,
                              labelText: langCode == 'ps' ? 'قیمت (\$)' : (langCode == 'prs' || langCode == 'fa' ? 'قیمت محصول (\$ دلار)' : 'Price (\$ USD)'),
                              keyboardType: TextInputType.number,
                              hintText: '45.00',
                            ),
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            child: CustomTextField(
                              controller: stockController,
                              labelText: langCode == 'ps' ? 'موجودي' : (langCode == 'prs' || langCode == 'fa' ? 'موجودی انبار' : 'Stock Quantity'),
                              keyboardType: TextInputType.number,
                              hintText: '10',
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 24),

                      // SECTION 3: Descriptions
                      _buildFormSectionHeader(langCode == 'ps' ? '۳. تفصیلات او توضیحات' : (langCode == 'prs' || langCode == 'fa' ? '۳. توضیحات و شرح محصول' : '3. Descriptions')),
                      const SizedBox(height: 12),
                      CustomTextField(
                        controller: descPrsController,
                        labelText: langCode == 'ps' ? 'دري توضیحات' : (langCode == 'prs' || langCode == 'fa' ? 'توضیحات کامل (دری)' : 'Description (Dari)'),
                        hintText: 'شرح کامل کیفیت، عیار و بسته‌بندی...',
                      ),
                      const SizedBox(height: 12),
                      CustomTextField(
                        controller: descEnController,
                        labelText: langCode == 'ps' ? 'انګلیسي توضیحات' : (langCode == 'prs' || langCode == 'fa' ? 'توضیحات کامل (انگلیسی)' : 'Description (English)'),
                        hintText: 'Full specification and details...',
                      ),
                      const SizedBox(height: 24),

                      // SECTION 4: Image File Upload & Variants
                      _buildFormSectionHeader(langCode == 'ps' ? '۴. انځور اپلوډ او متغیرونه' : (langCode == 'prs' || langCode == 'fa' ? '۴. آپلود تصویر محصول و ویژگی‌ها' : '4. Image File Upload & Specifications')),
                      const SizedBox(height: 12),

                      // Image Upload Drag / Pick Box
                      Container(
                        padding: const EdgeInsets.all(16),
                        decoration: BoxDecoration(
                          color: AppColors.horizonBg,
                          borderRadius: BorderRadius.circular(16),
                          border: Border.all(color: AppColors.horizonBrand.withAlpha(50), style: BorderStyle.solid),
                        ),
                        child: Row(
                          children: [
                            ClipRRect(
                              borderRadius: BorderRadius.circular(12),
                              child: Image.network(
                                imageUrlController.text,
                                width: 80,
                                height: 80,
                                fit: BoxFit.cover,
                                errorBuilder: (_, __, ___) => Container(
                                  width: 80,
                                  height: 80,
                                  color: Colors.grey[200],
                                  child: const Icon(Icons.image, color: Colors.grey),
                                ),
                              ),
                            ),
                            const SizedBox(width: 16),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  ElevatedButton.icon(
                                    style: ElevatedButton.styleFrom(
                                      backgroundColor: AppColors.horizonBrand,
                                      foregroundColor: Colors.white,
                                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                                      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                                    ),
                                    icon: isUploadingImage
                                        ? const SizedBox(
                                            width: 16,
                                            height: 16,
                                            child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2),
                                          )
                                        : const Icon(Icons.cloud_upload_rounded, size: 18),
                                    label: Text(
                                      isUploadingImage
                                          ? (langCode == 'ps' ? 'په اپلوډ کیدو کې...' : 'در حال آپلود...')
                                          : (langCode == 'ps' ? 'د انځور انتخاب او اپلوډ' : (langCode == 'prs' || langCode == 'fa' ? 'انتخاب و آپلود تصویر محصول' : 'Upload Image File')),
                                    ),
                                    onPressed: isUploadingImage ? null : pickAndUploadImage,
                                  ),
                                  const SizedBox(height: 6),
                                  Text(
                                    langCode == 'ps'
                                        ? 'د فرمتونو ملاتړ: PNG, JPG, WEBP (حداکثر ۱۰MB)'
                                        : (langCode == 'prs' || langCode == 'fa' ? 'فرمت‌های مجاز: PNG, JPG, WEBP (حداکثر ۱۰ مگابایت)' : 'Supported formats: PNG, JPG, WEBP (Max 10MB)'),
                                    style: const TextStyle(fontSize: 11, color: AppColors.horizonMuted),
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 12),
                      CustomTextField(
                        controller: imageUrlController,
                        labelText: langCode == 'ps' ? 'د انځور مستقیم لینک (URL)' : (langCode == 'prs' || langCode == 'fa' ? 'آدرس فایل تصویر (URL)' : 'Image File URL'),
                      ),
                      const SizedBox(height: 12),
                      Row(
                        children: [
                          Expanded(
                            child: CustomTextField(
                              controller: sizesController,
                              labelText: langCode == 'ps' ? 'اندازې' : (langCode == 'prs' || langCode == 'fa' ? 'سایزها یا ابعاد' : 'Available Sizes'),
                              hintText: 'e.g. 1.5x2m, 2x3m or S, M, L',
                            ),
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            child: CustomTextField(
                              controller: colorsController,
                              labelText: langCode == 'ps' ? 'رنګونه' : (langCode == 'prs' || langCode == 'fa' ? 'رنگ‌های موجود' : 'Available Colors'),
                              hintText: 'e.g. Gold, Red, Navy',
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ),
              actions: [
                TextButton(
                  onPressed: () => Navigator.pop(dialogContext),
                  child: const Text('Cancel', style: TextStyle(color: AppColors.horizonMuted)),
                ),
                ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.horizonBrand,
                    padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                  ),
                  onPressed: () async {
                    final price = double.tryParse(priceController.text) ?? 0.0;
                    final stock = int.tryParse(stockController.text) ?? 10;
                    final titleEn = titleEnController.text.trim();
                    final titlePrs = titlePrsController.text.trim().isNotEmpty ? titlePrsController.text.trim() : titleEn;
                    final titlePs = titlePsController.text.trim().isNotEmpty ? titlePsController.text.trim() : titlePrs;

                    final descEn = descEnController.text.trim().isNotEmpty ? descEnController.text.trim() : 'Premium Quality Product';
                    final descPrs = descPrsController.text.trim().isNotEmpty ? descPrsController.text.trim() : 'محصول با کیفیت ممتاز';
                    final descPs = descPsController.text.trim().isNotEmpty ? descPsController.text.trim() : descPrs;

                    if ((titleEn.isNotEmpty || titlePrs.isNotEmpty) && price > 0) {
                      setState(() => _isLoading = true);
                      Navigator.pop(dialogContext);

                      final apiClient = ref.read(apiClientProvider);
                      final token = ref.read(authProvider).token;

                      final body = {
                        "titleEn": titleEn.isNotEmpty ? titleEn : titlePrs,
                        "titlePrs": titlePrs,
                        "titlePs": titlePs,
                        "descriptionEn": descEn,
                        "descriptionPrs": descPrs,
                        "descriptionPs": descPs,
                        "price": price,
                        "stockQuantity": stock,
                        "imageUrl": imageUrlController.text.trim(),
                        "categoryId": selectedCategoryId,
                        "vendorId": selectedVendorId,
                        "availableSizes": sizesController.text.trim(),
                        "availableColors": colorsController.text.trim(),
                      };

                      final response = await apiClient.post(
                        '/products',
                        body,
                        languageCode: locale.languageCode,
                        token: token,
                      );

                      setState(() => _isLoading = false);

                      if (response != null && response['isSuccess'] == true && context.mounted) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(content: Text('Product created successfully with uploaded image!')),
                        );
                        ref.invalidate(productsProvider);
                      } else if (context.mounted) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(content: Text('Failed to create product: ${response?['error']?['message'] ?? 'Unknown'}')),
                        );
                      }
                    }
                  },
                  child: const Text('Save & Create Product', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
                ),
              ],
            );
          },
        );
      },
    );
  }

  Widget _buildFormSectionHeader(String title) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: AppColors.horizonBg,
        borderRadius: BorderRadius.circular(10),
      ),
      child: Text(
        title,
        style: const TextStyle(
          fontSize: 13,
          fontWeight: FontWeight.bold,
          color: AppColors.horizonNavy,
        ),
      ),
    );
  }

  Future<void> _deleteProduct(String id) async {
    setState(() => _isLoading = true);

    final apiClient = ref.read(apiClientProvider);
    final locale = ref.read(localeProvider);
    final token = ref.read(authProvider).token;

    final response = await apiClient.delete(
      '/products/$id',
      languageCode: locale.languageCode,
      token: token,
    );

    setState(() => _isLoading = false);

    if (response != null && response['isSuccess'] == true && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Product deleted successfully')),
      );
      ref.invalidate(productsProvider);
    } else if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to delete product: ${response?['error']?['message'] ?? 'Unknown'}')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;
    final productsAsync = ref.watch(productsProvider);

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header Bar
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                langCode == 'ps'
                    ? 'د توکو مدیریت'
                    : (langCode == 'prs' || langCode == 'fa' ? 'مدیریت کاتالوگ محصولات' : 'Product Inventory Management'),
                style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: AppColors.horizonNavy),
              ),
              ElevatedButton.icon(
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.horizonBrand,
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                  elevation: 0,
                ),
                icon: const Icon(Icons.add_rounded, size: 18),
                label: Text(
                  langCode == 'ps' ? 'نوی کامل توکی' : (langCode == 'prs' || langCode == 'fa' ? 'افزودن محصول جدید' : 'Add Comprehensive Product'),
                  style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
                ),
                onPressed: _showAddProductDialog,
              ),
            ],
          ),
          const SizedBox(height: 20),

          if (_isLoading) const LinearProgressIndicator(color: AppColors.horizonBrand),
          if (_isLoading) const SizedBox(height: 12),

          // Horizon Products List
          Expanded(
            child: productsAsync.when(
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (err, stack) => Center(child: Text('Error: $err', style: const TextStyle(color: AppColors.horizonRed))),
              data: (products) => ListView.separated(
                itemCount: products.length,
                separatorBuilder: (_, __) => const SizedBox(height: 12),
                itemBuilder: (context, index) {
                  final product = products[index];
                  return Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: AppColors.horizonCard,
                      borderRadius: BorderRadius.circular(20),
                      boxShadow: const [AppColors.horizonShadow],
                    ),
                    child: Row(
                      children: [
                        ClipRRect(
                          borderRadius: BorderRadius.circular(16),
                          child: Image.network(
                            product.imageUrl,
                            width: 60,
                            height: 60,
                            fit: BoxFit.cover,
                            errorBuilder: (_, __, ___) => Container(
                              width: 60,
                              height: 60,
                              color: AppColors.horizonBg,
                              child: const Icon(Icons.inventory_2, color: AppColors.horizonMuted),
                            ),
                          ),
                        ),
                        const SizedBox(width: 16),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                product.getTitle(langCode),
                                style: const TextStyle(
                                  fontWeight: FontWeight.bold,
                                  fontSize: 15,
                                  color: AppColors.horizonNavy,
                                ),
                              ),
                              const SizedBox(height: 4),
                              Row(
                                children: [
                                  Text(
                                    '\$${product.price.toStringAsFixed(2)}',
                                    style: const TextStyle(
                                      color: AppColors.horizonBrand,
                                      fontWeight: FontWeight.w800,
                                      fontSize: 14,
                                    ),
                                  ),
                                  const SizedBox(width: 12),
                                  Container(
                                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                                    decoration: BoxDecoration(
                                      color: AppColors.horizonGreen.withAlpha(20),
                                      borderRadius: BorderRadius.circular(10),
                                    ),
                                    child: Text(
                                      product.availableSizes.isNotEmpty ? 'Sizes: ${product.availableSizes.join(', ')}' : 'In Stock',
                                      style: const TextStyle(
                                        fontSize: 11,
                                        fontWeight: FontWeight.bold,
                                        color: AppColors.horizonGreen,
                                      ),
                                    ),
                                  ),
                                ],
                              ),
                            ],
                          ),
                        ),
                        IconButton(
                          icon: const Icon(Icons.delete_outline_rounded, color: AppColors.horizonRed, size: 20),
                          onPressed: () => _deleteProduct(product.id),
                        ),
                      ],
                    ),
                  );
                },
              ),
            ),
          ),
        ],
      ),
    );
  }
}
