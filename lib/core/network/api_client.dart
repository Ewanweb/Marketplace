import 'dart:convert';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:http/http.dart' as http;

final apiClientProvider = Provider<ApiClient>((ref) => ApiClient());

class ApiClient {
  static const String baseApiUrl = 'http://localhost:8085/api/v1';

  final http.Client _client = http.Client();

  Map<String, String> _headers(String languageCode, {String? token}) {
    final headers = {
      'Content-Type': 'application/json',
      'Accept-Language': languageCode,
    };

    if (token != null && token.isNotEmpty) {
      headers['Authorization'] = 'Bearer $token';
    }

    return headers;
  }

  Future<Map<String, String>?> getLocalizationStrings(String languageCode) async {
    try {
      final url = Uri.parse('$baseApiUrl/localization/strings');
      final response = await _client.get(
        url,
        headers: _headers(languageCode),
      );

      if (response.statusCode == 200) {
        final json = jsonDecode(response.body);
        if (json['isSuccess'] == true && json['value'] != null) {
          final Map<String, dynamic> val = json['value'];
          return val.map((k, v) => MapEntry(k, v.toString()));
        }
      }
    } catch (_) {
      // Fallback map handled in provider
    }
    return null;
  }

  Future<dynamic> get(
    String endpoint, {
    required String languageCode,
    String? token,
  }) async {
    try {
      final url = Uri.parse('$baseApiUrl$endpoint');
      final response = await _client.get(
        url,
        headers: _headers(languageCode, token: token),
      );
      return _processResponse(response);
    } catch (e) {
      return null;
    }
  }

  Future<dynamic> post(
    String endpoint,
    Map<String, dynamic> body, {
    required String languageCode,
    String? token,
  }) async {
    try {
      final fullEndpoint = endpoint.startsWith('/auth') ? endpoint : (endpoint.startsWith('/') ? endpoint : '/$endpoint');
      final url = Uri.parse('$baseApiUrl$fullEndpoint');
      final response = await _client.post(
        url,
        headers: _headers(languageCode, token: token),
        body: jsonEncode(body),
      );
      return _processResponse(response);
    } catch (e) {
      return null;
    }
  }

  Future<dynamic> put(
    String endpoint,
    Map<String, dynamic> body, {
    required String languageCode,
    String? token,
  }) async {
    try {
      final url = Uri.parse('$baseApiUrl$endpoint');
      final response = await _client.put(
        url,
        headers: _headers(languageCode, token: token),
        body: jsonEncode(body),
      );
      return _processResponse(response);
    } catch (e) {
      return null;
    }
  }

  Future<dynamic> delete(
    String endpoint, {
    required String languageCode,
    String? token,
  }) async {
    try {
      final url = Uri.parse('$baseApiUrl$endpoint');
      final response = await _client.delete(
        url,
        headers: _headers(languageCode, token: token),
      );
      return _processResponse(response);
    } catch (e) {
      return null;
    }
  }

  dynamic _processResponse(http.Response response) {
    try {
      final body = jsonDecode(response.body);
      return body;
    } catch (_) {
      return {'isSuccess': false, 'error': {'message': 'Invalid response format.'}};
    }
  }
}
