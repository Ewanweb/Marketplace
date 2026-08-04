using Marketplace.Shared.Localization;

namespace Marketplace.Application.Authentication.Common;

public static class AuthMessages
{
    public static string EmailExists => LocalizedMessage.Get(
        "A user with this email address already exists.",
        "کاربری با این نشانی ایمیل قبلاً ثبت‌نام کرده است.",
        "له دې ایمیل آدرس سره دمخه یو کارونکی راجستر شوی دی.");

    public static string InvalidCredentials => LocalizedMessage.Get(
        "Invalid email or password.",
        "ایمیل یا رمز عبور اشتباه است.",
        "ایمیل یا پټنوم غلط دی.");

    public static string AccountLocked => LocalizedMessage.Get(
        "Account is temporarily locked due to multiple failed login attempts.",
        "حساب کاربری به دلیل تلاش‌های ناموفق متعدد موقتاً مسدود شده است.",
        "حساب د متعددو ناکامو هڅو له امله په لنډمهاله توګه تړل شوی دی.");

    public static string InvalidToken => LocalizedMessage.Get(
        "Refresh token is invalid or expired.",
        "توکن بازنشانی نامعتبر یا منقضی شده است.",
        "د ریفریش ټوکن باطل یا پای ته رسیدلی دی.");

    public static string UserNotFound => LocalizedMessage.Get(
        "User was not found.",
        "کاربر یافت نشد.",
        "کارونکی ونه موندل شو.");

    public static string Invalid2FACode => LocalizedMessage.Get(
        "Invalid 2FA verification code.",
        "کد تأیید دو مرحله‌ای نامعتبر است.",
        "د دوه مرحلې تایید کوډ باطل دی.");

    public static string EmailRequired => LocalizedMessage.Get(
        "Email is required.",
        "ایمیل الزامی است.",
        "ایمیل اړین دی.");

    public static string InvalidEmailFormat => LocalizedMessage.Get(
        "Invalid email format.",
        "فرمت ایمیل نامعتبر است.",
        "د ایمیل بڼه محدوده ده.");

    public static string PasswordRequired => LocalizedMessage.Get(
        "Password is required.",
        "رمز عبور الزامی است.",
        "پټنوم اړین دی.");

    public static string PasswordMinLength => LocalizedMessage.Get(
        "Password must be at least 8 characters long.",
        "رمز عبور باید حداقل ۸ کاراکتر باشد.",
        "پټنوم باید لږترلږه ۸ حروف وي.");

    public static string PasswordUppercase => LocalizedMessage.Get(
        "Password must contain at least one uppercase letter.",
        "رمز عبور باید شامل حداقل یک حرف بزرگ باشد.",
        "پټنوم باید لږترلږه یو لوی توری ولري.");

    public static string PasswordLowercase => LocalizedMessage.Get(
        "Password must contain at least one lowercase letter.",
        "رمز عبور باید شامل حداقل یک حرف کوچک باشد.",
        "پټنوم باید لږترلږه یو واړه توری ولري.");

    public static string PasswordDigit => LocalizedMessage.Get(
        "Password must contain at least one number.",
        "رمز عبور باید شامل حداقل یک عدد باشد.",
        "پټنوم باید لږترلږه یو شمیره ولري.");

    public static string PasswordSpecialChar => LocalizedMessage.Get(
        "Password must contain at least one special character.",
        "رمز عبور باید شامل حداقل یک کاراکتر ویژه باشد.",
        "پټنوم باید لږترلږه یو ځانګړی توری ولري.");
}
