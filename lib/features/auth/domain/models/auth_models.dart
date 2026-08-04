class ErrorModel {
  final String code;
  final String description;
  final int type;

  ErrorModel({
    required this.code,
    required this.description,
    required this.type,
  });

  factory ErrorModel.fromJson(Map<String, dynamic> json) {
    return ErrorModel(
      code: json['code'] ?? '',
      description: json['description'] ?? '',
      type: json['type'] ?? 0,
    );
  }
}

class ResultResponse<T> {
  final bool isSuccess;
  final bool isFailure;
  final T? value;
  final ErrorModel? error;

  ResultResponse({
    required this.isSuccess,
    required this.isFailure,
    this.value,
    this.error,
  });

  factory ResultResponse.fromJson(
    Map<String, dynamic> json,
    T Function(dynamic) fromJsonT,
  ) {
    return ResultResponse<T>(
      isSuccess: json['isSuccess'] ?? false,
      isFailure: json['isFailure'] ?? true,
      value: json['value'] != null ? fromJsonT(json['value']) : null,
      error: json['error'] != null ? ErrorModel.fromJson(json['error']) : null,
    );
  }
}

class LoginData {
  final String? accessToken;
  final String? refreshToken;
  final bool requiresTwoFactor;
  final String? userId;

  LoginData({
    this.accessToken,
    this.refreshToken,
    required this.requiresTwoFactor,
    this.userId,
  });

  factory LoginData.fromJson(Map<String, dynamic> json) {
    return LoginData(
      accessToken: json['accessToken'],
      refreshToken: json['refreshToken'],
      requiresTwoFactor: json['requiresTwoFactor'] ?? false,
      userId: json['userId'],
    );
  }
}
