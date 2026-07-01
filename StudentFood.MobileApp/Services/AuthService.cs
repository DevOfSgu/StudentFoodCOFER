using System.Net.Http.Json;
using System.Text.Json;
using StudentFood.MobileApp.Models;

namespace StudentFood.MobileApp.Services;

public class AuthService
{
    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<(bool Success, string Message, AuthUser? User)> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync(ApiConfig.LoginUrl, request);
            var payload = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);

            if (!response.IsSuccessStatusCode || payload?.User is null)
            {
                return (false, payload?.Message ?? "Đăng nhập thất bại.", null);
            }

            Preferences.Default.Set("user_full_name", payload.User.FullName);
            Preferences.Default.Set("user_email", payload.User.Email);
            Preferences.Default.Set("user_id", payload.User.Id);
            Preferences.Default.Set("user_phone_number", payload.User.PhoneNumber ?? string.Empty);

            return (true, payload.Message, payload.User);
        }
        catch
        {
            return (false, "Không kết nối được tới máy chủ.", null);
        }
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync(ApiConfig.RegisterUrl, request);
            var payload = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                return (false, payload?.Message ?? "Đăng ký thất bại.");
            }

            Preferences.Default.Set("user_full_name", request.FullName);
            Preferences.Default.Set("user_email", request.Email);
            Preferences.Default.Set("user_phone_number", request.PhoneNumber);
            if (payload?.User != null)
            {
                Preferences.Default.Set("user_id", payload.User.Id);
            }

            return (true, payload?.Message ?? "Đăng ký thành công.");
        }
        catch
        {
            return (false, "Không kết nối được tới máy chủ.");
        }
    }

    public string GetCurrentUserName()
    {
        return Preferences.Default.Get("user_full_name", "Người dùng");
    }

    public string GetCurrentPhoneNumber()
    {
        return Preferences.Default.Get("user_phone_number", string.Empty);
    }

    public void Logout()
    {
        Preferences.Default.Remove("user_full_name");
        Preferences.Default.Remove("user_email");
        Preferences.Default.Remove("user_id");
        Preferences.Default.Remove("user_phone_number");
    }
}
