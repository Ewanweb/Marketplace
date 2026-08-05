import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../../core/localization/locale_provider.dart';
import '../../../core/network/api_client.dart';

class AuthState {
  final bool isAuthenticated;
  final String? token;
  final String? userName;
  final String? email;
  final String? role;
  final String? vendorId;
  final bool isLoading;
  final String? errorMessage;

  AuthState({
    this.isAuthenticated = false,
    this.token,
    this.userName,
    this.email,
    this.role,
    this.vendorId,
    this.isLoading = false,
    this.errorMessage,
  });

  bool get isAdmin => role?.toLowerCase() == 'superadmin' || role?.toLowerCase() == 'admin';
  bool get isVendor => vendorId != null && vendorId!.isNotEmpty;

  AuthState copyWith({
    bool? isAuthenticated,
    String? token,
    String? userName,
    String? email,
    String? role,
    String? vendorId,
    bool? isLoading,
    String? errorMessage,
  }) {
    return AuthState(
      isAuthenticated: isAuthenticated ?? this.isAuthenticated,
      token: token ?? this.token,
      userName: userName ?? this.userName,
      email: email ?? this.email,
      role: role ?? this.role,
      vendorId: vendorId ?? this.vendorId,
      isLoading: isLoading ?? this.isLoading,
      errorMessage: errorMessage,
    );
  }
}

class AuthNotifier extends StateNotifier<AuthState> {
  static const String _tokenKey = 'jwt_auth_token';
  static const String _userKey = 'auth_user_name';
  static const String _roleKey = 'auth_user_role';
  static const String _vendorKey = 'auth_vendor_id';
  final Ref _ref;

  AuthNotifier(this._ref) : super(AuthState()) {
    _checkInitialAuth();
  }

  Future<void> _checkInitialAuth() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString(_tokenKey);
    final userName = prefs.getString(_userKey);
    final role = prefs.getString(_roleKey);
    final vendorId = prefs.getString(_vendorKey);

    if (token != null && token.isNotEmpty) {
      state = state.copyWith(
        isAuthenticated: true,
        token: token,
        userName: userName ?? 'User',
        role: role,
        vendorId: vendorId,
      );
      // Fetch fresh profile details from API
      await fetchUserProfile();
    }
  }

  Future<void> fetchUserProfile() async {
    if (state.token == null) return;
    final apiClient = _ref.read(apiClientProvider);
    final locale = _ref.read(localeProvider);

    final response = await apiClient.get(
      '/users/me',
      languageCode: locale.languageCode,
      token: state.token,
    );

    if (response != null && response['isSuccess'] == true && response['value'] != null) {
      final val = response['value'];
      final fullName = val['fullName'] ?? state.userName;
      final rolesList = (val['roles'] as List<dynamic>?)?.map((e) => e.toString()).toList() ?? [];
      final role = rolesList.isNotEmpty ? rolesList.first : 'Customer';
      final vendorId = val['vendorId']?.toString();

      final prefs = await SharedPreferences.getInstance();
      await prefs.setString(_userKey, fullName);
      await prefs.setString(_roleKey, role);
      if (vendorId != null) {
        await prefs.setString(_vendorKey, vendorId);
      }

      state = state.copyWith(
        userName: fullName,
        email: val['email'],
        role: role,
        vendorId: vendorId,
      );
    }
  }

  Future<bool> login(String email, String password) async {
    state = state.copyWith(isLoading: true, errorMessage: null);
    try {
      final apiClient = _ref.read(apiClientProvider);
      final locale = _ref.read(localeProvider);

      final response = await apiClient.post(
        '/auth/login',
        {'email': email, 'password': password},
        languageCode: locale.languageCode,
      );

      if (response != null && response['isSuccess'] == true && response['value'] != null) {
        final val = response['value'];
        final token = val['accessToken'] ?? val['token'] ?? '';
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

        await fetchUserProfile();
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

  Future<bool> register(String fullName, String email, String phoneNumber, String password) async {
    state = state.copyWith(isLoading: true, errorMessage: null);
    try {
      final apiClient = _ref.read(apiClientProvider);
      final locale = _ref.read(localeProvider);

      final response = await apiClient.post(
        '/auth/register',
        {
          'fullName': fullName,
          'email': email,
          'phoneNumber': phoneNumber,
          'password': password,
        },
        languageCode: locale.languageCode,
      );

      if (response != null && response['isSuccess'] == true) {
        state = state.copyWith(isLoading: false);
        return await login(email, password);
      } else {
        final errorMsg = response?['error']?['message'] ?? 'Registration failed. Please check your inputs.';
        state = state.copyWith(isLoading: false, errorMessage: errorMsg);
        return false;
      }
    } catch (e) {
      state = state.copyWith(
        isLoading: false,
        errorMessage: 'Connection error during registration.',
      );
      return false;
    }
  }

  Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_tokenKey);
    await prefs.remove(_userKey);
    await prefs.remove(_roleKey);
    await prefs.remove(_vendorKey);
    state = AuthState();
  }
}

final authProvider = StateNotifierProvider<AuthNotifier, AuthState>((ref) {
  return AuthNotifier(ref);
});
