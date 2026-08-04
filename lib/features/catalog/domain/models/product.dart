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
    required this.availableSizes,
    required this.availableColors,
  });

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
