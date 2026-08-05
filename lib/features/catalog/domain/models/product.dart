class Category {
  final String id;
  final String nameEn;
  final String namePrs;
  final String namePs;
  final String iconName;

  const Category({
    required this.id,
    required this.nameEn,
    required this.namePrs,
    required this.namePs,
    required this.iconName,
  });

  factory Category.fromJson(Map<String, dynamic> json) {
    return Category(
      id: json['id']?.toString() ?? '',
      nameEn: json['nameEn'] ?? json['name'] ?? '',
      namePrs: json['namePrs'] ?? json['nameEn'] ?? '',
      namePs: json['namePs'] ?? json['nameEn'] ?? '',
      iconName: json['iconName'] ?? 'category',
    );
  }

  String getName(String langCode) {
    if (langCode == 'ps') return namePs;
    if (langCode == 'prs' || langCode == 'fa') return namePrs;
    return nameEn;
  }
}

class Product {
  final String id;
  final String titleEn;
  final String titlePrs;
  final String titlePs;
  final String descriptionEn;
  final String descriptionPrs;
  final String descriptionPs;
  final double price;
  final double rating;
  final String imageUrl;
  final String categoryId;
  final String vendorId;
  final List<String> availableSizes;
  final List<String> availableColors;

  const Product({
    required this.id,
    required this.titleEn,
    required this.titlePrs,
    required this.titlePs,
    required this.descriptionEn,
    required this.descriptionPrs,
    required this.descriptionPs,
    required this.price,
    required this.rating,
    required this.imageUrl,
    required this.categoryId,
    required this.vendorId,
    required this.availableSizes,
    required this.availableColors,
  });

  factory Product.fromJson(Map<String, dynamic> json) {
    return Product(
      id: json['id']?.toString() ?? '',
      titleEn: json['titleEn'] ?? json['title'] ?? '',
      titlePrs: json['titlePrs'] ?? json['titleEn'] ?? '',
      titlePs: json['titlePs'] ?? json['titleEn'] ?? '',
      descriptionEn: json['descriptionEn'] ?? json['description'] ?? '',
      descriptionPrs: json['descriptionPrs'] ?? json['descriptionEn'] ?? '',
      descriptionPs: json['descriptionPs'] ?? json['descriptionEn'] ?? '',
      price: (json['price'] as num?)?.toDouble() ?? 0.0,
      rating: (json['rating'] as num?)?.toDouble() ?? 5.0,
      imageUrl: json['imageUrl'] ?? 'https://via.placeholder.com/150',
      categoryId: json['categoryId']?.toString() ?? '',
      vendorId: json['vendorId']?.toString() ?? '',
      availableSizes: json['availableSizes'] is List 
          ? List<String>.from(json['availableSizes'])
          : (json['availableSizes']?.toString().split(',').map((e) => e.trim()).toList() ?? ['M', 'L']),
      availableColors: json['availableColors'] is List 
          ? List<String>.from(json['availableColors'])
          : (json['availableColors']?.toString().split(',').map((e) => e.trim()).toList() ?? ['Default']),
    );
  }

  String getTitle(String langCode) {
    if (langCode == 'ps') return titlePs;
    if (langCode == 'prs' || langCode == 'fa') return titlePrs;
    return titleEn;
  }

  String getDescription(String langCode) {
    if (langCode == 'ps') return descriptionPs;
    if (langCode == 'prs' || langCode == 'fa') return descriptionPrs;
    return descriptionEn;
  }
}
