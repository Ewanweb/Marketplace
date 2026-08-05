import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/localization/locale_provider.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/theme/app_colors.dart';

class AdminAgencyScreen extends ConsumerStatefulWidget {
  const AdminAgencyScreen({super.key});

  @override
  ConsumerState<AdminAgencyScreen> createState() => _AdminAgencyScreenState();
}

class _AdminAgencyScreenState extends ConsumerState<AdminAgencyScreen> {
  bool _isLoading = true;
  String? _errorMessage;
  List<dynamic> _agencies = [];
  String _searchQuery = '';

  @override
  void initState() {
    super.initState();
    _fetchAgencies();
  }

  Future<void> _fetchAgencies() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final apiClient = ref.read(apiClientProvider);
      final locale = ref.read(localeProvider);
      final response = await apiClient.get('/vendors', languageCode: locale.languageCode);

      if (response != null && response['isSuccess'] == true && response['value'] != null) {
        setState(() {
          _agencies = response['value'] as List<dynamic>;
          _isLoading = false;
        });
      } else {
        setState(() {
          _errorMessage = response?['error']?['message'] ?? 'Failed to load agency applications.';
          _isLoading = false;
        });
      }
    } catch (e) {
      setState(() {
        _errorMessage = 'Network error while fetching agency applications.';
        _isLoading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final locale = ref.watch(localeProvider);
    final langCode = locale.languageCode;

    final filteredAgencies = _agencies.where((agency) {
      final shopNameEn = (agency['shopNameEn'] ?? '').toString().toLowerCase();
      final shopNamePrs = (agency['shopNamePrs'] ?? '').toString().toLowerCase();
      final shopNamePs = (agency['shopNamePs'] ?? '').toString().toLowerCase();
      final description = (agency['description'] ?? '').toString().toLowerCase();
      final query = _searchQuery.toLowerCase();

      return shopNameEn.contains(query) ||
          shopNamePrs.contains(query) ||
          shopNamePs.contains(query) ||
          description.contains(query);
    }).toList();

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                langCode == 'ps'
                    ? 'د نمایندګیو غوښتنلیکونه'
                    : (langCode == 'prs' || langCode == 'fa'
                        ? 'بررسی و اخذ درخواست‌های نمایندگی'
                        : 'Agency & Vendor Applications'),
                style: const TextStyle(
                  fontSize: 20,
                  fontWeight: FontWeight.bold,
                  color: AppColors.horizonNavy,
                ),
              ),
              IconButton(
                onPressed: _fetchAgencies,
                icon: const Icon(Icons.refresh_rounded, color: AppColors.horizonBrand),
                tooltip: 'Refresh',
              ),
            ],
          ),
          const SizedBox(height: 16),
          TextField(
            onChanged: (val) => setState(() => _searchQuery = val),
            style: const TextStyle(fontSize: 13, color: AppColors.horizonNavy),
            decoration: InputDecoration(
              hintText: langCode == 'ps'
                  ? 'جستجو نمایندګي...'
                  : (langCode == 'prs' || langCode == 'fa' ? 'جستجوی نمایندگی رسمی...' : 'Search agencies...'),
              prefixIcon: const Icon(Icons.search_rounded, color: AppColors.horizonMuted),
              filled: true,
              fillColor: AppColors.horizonCard,
              contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(16),
                borderSide: BorderSide.none,
              ),
              enabledBorder: OutlineInputBorder(
                borderRadius: BorderRadius.circular(16),
                borderSide: BorderSide.none,
              ),
            ),
          ),
          const SizedBox(height: 16),
          if (_isLoading)
            const Center(
              child: Padding(
                padding: EdgeInsets.all(48.0),
                child: CircularProgressIndicator(color: AppColors.horizonBrand),
              ),
            )
          else if (_errorMessage != null)
            Container(
              padding: const EdgeInsets.all(24),
              decoration: BoxDecoration(
                color: AppColors.horizonCard,
                borderRadius: BorderRadius.circular(20),
                boxShadow: const [AppColors.horizonShadow],
              ),
              child: Center(
                child: Column(
                  children: [
                    const Icon(Icons.error_outline_rounded, size: 48, color: AppColors.horizonRed),
                    const SizedBox(height: 12),
                    Text(_errorMessage!, style: const TextStyle(color: AppColors.horizonRed)),
                    const SizedBox(height: 16),
                    ElevatedButton(
                      onPressed: _fetchAgencies,
                      child: const Text('Try Again'),
                    ),
                  ],
                ),
              ),
            )
          else if (filteredAgencies.isEmpty)
            Container(
              padding: const EdgeInsets.all(48),
              decoration: BoxDecoration(
                color: AppColors.horizonCard,
                borderRadius: BorderRadius.circular(20),
                boxShadow: const [AppColors.horizonShadow],
              ),
              child: Center(
                child: Column(
                  children: [
                    const Icon(Icons.verified_user_outlined, size: 56, color: AppColors.horizonMuted),
                    const SizedBox(height: 16),
                    Text(
                      langCode == 'ps'
                          ? 'هیڅ نمایندګي ونه موندل شوه'
                          : (langCode == 'prs' || langCode == 'fa'
                              ? 'هیچ درخواست نمایندگی ثبت نشده است.'
                              : 'No agency applications found.'),
                      style: const TextStyle(color: AppColors.horizonMuted, fontSize: 15),
                    ),
                  ],
                ),
              ),
            )
          else
            Expanded(
              child: ListView.separated(
                itemCount: filteredAgencies.length,
                separatorBuilder: (_, __) => const SizedBox(height: 12),
                itemBuilder: (context, index) {
                  final agency = filteredAgencies[index];
                  final isVerified = agency['isVerified'] == true;
                  final shopName = langCode == 'ps'
                      ? (agency['shopNamePs'] ?? agency['shopNameEn'])
                      : (langCode == 'prs' || langCode == 'fa'
                          ? (agency['shopNamePrs'] ?? agency['shopNameEn'])
                          : agency['shopNameEn']);

                  return Container(
                    padding: const EdgeInsets.all(20),
                    decoration: BoxDecoration(
                      color: AppColors.horizonCard,
                      borderRadius: BorderRadius.circular(20),
                      boxShadow: const [AppColors.horizonShadow],
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Container(
                              padding: const EdgeInsets.all(12),
                              decoration: BoxDecoration(
                                color: AppColors.horizonBrand.withAlpha(20),
                                borderRadius: BorderRadius.circular(14),
                              ),
                              child: const Icon(Icons.storefront_rounded, color: AppColors.horizonBrand, size: 26),
                            ),
                            const SizedBox(width: 16),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    shopName ?? 'Agency Application',
                                    style: const TextStyle(
                                      fontSize: 16,
                                      fontWeight: FontWeight.bold,
                                      color: AppColors.horizonNavy,
                                    ),
                                  ),
                                  const SizedBox(height: 4),
                                  Text(
                                    'ID: ${agency['id']}',
                                    style: const TextStyle(fontSize: 11, color: AppColors.horizonMuted),
                                  ),
                                ],
                              ),
                            ),
                            Container(
                              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                              decoration: BoxDecoration(
                                color: isVerified
                                    ? AppColors.horizonGreen.withAlpha(20)
                                    : AppColors.horizonOrange.withAlpha(20),
                                borderRadius: BorderRadius.circular(20),
                                border: Border.all(
                                  color: isVerified ? AppColors.horizonGreen : AppColors.horizonOrange,
                                ),
                              ),
                              child: Row(
                                mainAxisSize: MainAxisSize.min,
                                children: [
                                  Icon(
                                    isVerified ? Icons.check_circle_rounded : Icons.pending_rounded,
                                    size: 14,
                                    color: isVerified ? AppColors.horizonGreen : AppColors.horizonOrange,
                                  ),
                                  const SizedBox(width: 6),
                                  Text(
                                    isVerified
                                        ? (langCode == 'ps' ? 'تأیید شوی' : 'تأیید شده')
                                        : (langCode == 'ps' ? 'در انتظار کتنه' : 'در انتظار بررسی'),
                                    style: TextStyle(
                                      fontSize: 11,
                                      fontWeight: FontWeight.bold,
                                      color: isVerified ? AppColors.horizonGreen : AppColors.horizonOrange,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ),
                        if (agency['description'] != null && agency['description'].toString().isNotEmpty) ...[
                          const Divider(height: 24, color: Color(0xFFF0F3F8)),
                          Text(
                            langCode == 'ps' ? 'توضیحات او اسناد:' : (langCode == 'prs' || langCode == 'fa' ? 'شرح و اطلاعات پرونده:' : 'Application Details:'),
                            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 12, color: AppColors.horizonMuted),
                          ),
                          const SizedBox(height: 4),
                          Text(
                            agency['description'].toString(),
                            style: const TextStyle(fontSize: 13, color: AppColors.horizonNavy),
                          ),
                        ],
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
