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

  Future<void> sendClientLog({
    required String message,
    String? stackTrace,
    String level = 'Error',
    String? route,
    String? token,
  }) async {
    try {
      final url = Uri.parse('$baseApiUrl/logs/client');
      await _client.post(
        url,
        headers: _headers('en', token: token),
        body: jsonEncode({
          'message': message,
          'stackTrace': stackTrace,
          'level': level,
          'route': route ?? 'FlutterClient',
        }),
      );
    } catch (_) {
      // Ignore logging failures
    }
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
    } catch (e) {
      sendClientLog(message: 'getLocalizationStrings error: $e', level: 'Warning');
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
      if (response.statusCode >= 400) {
        sendClientLog(message: 'GET $endpoint failed with Status ${response.statusCode}', level: 'Warning', token: token);
      }
      return _processResponse(response);
    } catch (e) {
      sendClientLog(message: 'GET $endpoint connection error: $e', level: 'Error', token: token);
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
      if (response.statusCode >= 400 && !fullEndpoint.contains('/logs/client')) {
        sendClientLog(message: 'POST $fullEndpoint failed with Status ${response.statusCode}', level: 'Warning', token: token);
      }
      return _processResponse(response);
    } catch (e) {
      if (!endpoint.contains('/logs/client')) {
        sendClientLog(message: 'POST $endpoint connection error: $e', level: 'Error', token: token);
      }
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
      if (response.statusCode >= 400) {
        sendClientLog(message: 'PUT $endpoint failed with Status ${response.statusCode}', level: 'Warning', token: token);
      }
      return _processResponse(response);
    } catch (e) {
      sendClientLog(message: 'PUT $endpoint connection error: $e', level: 'Error', token: token);
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
      if (response.statusCode >= 400) {
        sendClientLog(message: 'DELETE $endpoint failed with Status ${response.statusCode}', level: 'Warning', token: token);
      }
      return _processResponse(response);
    } catch (e) {
      sendClientLog(message: 'DELETE $endpoint connection error: $e', level: 'Error', token: token);
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
