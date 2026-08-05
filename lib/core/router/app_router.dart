import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../features/admin/presentation/screens/admin_layout.dart';
import '../../features/auth/presentation/auth_provider.dart';
import '../../features/auth/presentation/screens/login_screen.dart';
import '../../features/auth/presentation/screens/register_screen.dart';
import '../../features/catalog/presentation/screens/main_navigation_screen.dart';

final routerProvider = Provider<GoRouter>((ref) {
  return GoRouter(
    initialLocation: '/',
    routes: [
      GoRoute(
        path: '/',
        builder: (context, state) => const MainNavigationScreen(),
      ),
      GoRoute(
        path: '/login',
        builder: (context, state) => const LoginScreen(),
        redirect: (context, state) {
          final authState = ref.read(authProvider);
          if (authState.isAuthenticated) {
            if (authState.role == 'SuperAdmin' || authState.role == 'Admin') {
              return '/admin';
            }
            return '/';
          }
          return null;
        },
      ),
      GoRoute(
        path: '/register',
        builder: (context, state) => const RegisterScreen(),
        redirect: (context, state) {
          final authState = ref.read(authProvider);
          if (authState.isAuthenticated) {
            if (authState.role == 'SuperAdmin' || authState.role == 'Admin') {
              return '/admin';
            }
            return '/';
          }
          return null;
        },
      ),
      GoRoute(
        path: '/admin',
        builder: (context, state) => const AdminLayout(),
        redirect: (context, state) {
          final authState = ref.read(authProvider);
          if (!authState.isAuthenticated) {
            return '/login';
          }
          // Note: In a full implementation, you should also check if the user has the 'Admin' role.
          return null;
        },
      ),
    ],
  );
});
