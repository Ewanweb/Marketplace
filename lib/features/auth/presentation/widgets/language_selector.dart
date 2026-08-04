import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../../core/localization/app_localizations.dart';

class LanguageSelectorWidget extends StatelessWidget {
  const LanguageSelectorWidget({super.key});

  @override
  Widget build(BuildContext context) {
    final localization = Provider.of<AppLocalization>(context);

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.surface,
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: Colors.white.withOpacity(0.1)),
      ),
      child: DropdownButtonHideUnderline(
        child: DropdownButton<String>(
          value: localization.currentLanguage,
          icon: const Icon(Icons.language, color: Colors.white70),
          dropdownColor: Theme.of(context).colorScheme.surface,
          items: [
            DropdownMenuItem(
              value: 'prs',
              child: Text(localization.translate('dari'), style: const TextStyle(fontSize: 14)),
            ),
            DropdownMenuItem(
              value: 'ps',
              child: Text(localization.translate('pashto'), style: const TextStyle(fontSize: 14)),
            ),
            DropdownMenuItem(
              value: 'en',
              child: Text(localization.translate('english'), style: const TextStyle(fontSize: 14)),
            ),
          ],
          onChanged: (code) {
            if (code != null) {
              localization.setLanguage(code);
            }
          },
        ),
      ),
    );
  }
}
