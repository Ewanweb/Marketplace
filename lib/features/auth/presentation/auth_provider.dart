import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../../core/localization/locale_provider.dart';
import '../../../core/network/api_client.dart';

class AuthState {
  final bool isAuthenticated;
  final String? token;
  final String? userName;
  final String? email;
  final bool isLoading;
  final String? errorMessage;

  AuthState({
    this.isAuthenticated = false,
    this.token,
    this.userName,
    this.email,
    this.isLoading = false,
    this.errorMessage,
  });

  AuthState copyWith({
    bool? isAuthenticated,
    String? token,
    String? userName,
    String? email,
    bool? isLoading,
    String? errorMessage,
  }) {
    return AuthState(
      isAuthenticated: isAuthenticated ?? this.isAuthenticated,
      token: token ?? this.token,
      userName: userName ?? this.userName,
      email: email ?? this.email,
      isLoading: isLoading ?? this.isLoading,
      errorMessage: errorMessage,
    );
  }
}

class AuthNotifier extends StateNotifier<AuthState> {
  static const String _tokenKey = 'jwt_auth_token';
  static const String _userKey = 'auth_user_name';
  final Ref _ref;

  AuthNotifier(this._ref) : super(AuthState()) {
    _checkInitialAuth();
  }

  Future<void> _checkInitialAuth() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString(_tokenKey);
    final userName = prefs.getString(_userKey);

    if (token != null && token.isNotEmpty) {
      state = state.copyWith(
        isAuthenticated: true,
        token: token,
        userName: userName ?? 'User',
      );
    }
  }

  Future<bool> login(String email, String password) async {
    state = state.copyWith(isLoading: true, errorMessage: null);
    try {
      final apiClient = _ref.read(apiClientProvider);
      final locale = _ref.read(localeProvider);

      final response = await apiClient.post(
        '/login',
        {'email': email, 'password': password},
        languageCode: locale.languageCode,
      );

      if (response != null && response['isSuccess'] == true && response['value'] != null) {
        final val = response['value'];
        final token = val['accessToken'] ?? val['token'] ?? 'mock_jwt_token';
        final user = val['fullName'] ?? val['email'] ?? email;

        final prefs = await SharedPreferences.getInstance();
        await prefs.setString(_tokenKey, token);
        await prefs.setString(_userKey, user);

        state = state.copyWith(
          isAuthenticated: true,
          token: token,
          userName: user,
          email: email,
          isLoading: false,
        );
        return true;
      } else {
        final errorMsg = response?['error']?['message'] ?? 'Login failed. Please check your credentials.';
        state = state.copyWith(isLoading: false, errorMessage: errorMsg);
        return false;
      }
    } catch (e) {
      state = state.copyWith(
        isLoading: false,
        errorMessage: 'Connection error. Please try again.',
      );
      return false;
    }
  }

  Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_tokenKey);
    await prefs.remove(_userKey);
    state = AuthState();
  }
}

final authProvider = StateNotifierProvider<AuthNotifier, AuthState>((ref) {
  return AuthNotifier(ref);
});
