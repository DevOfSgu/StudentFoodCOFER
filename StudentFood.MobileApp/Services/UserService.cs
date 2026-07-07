using System.Net.Http.Json;
using System.Text.Json;
using StudentFood.MobileApp.Models;

namespace StudentFood.MobileApp.Services;

public class UserService
{
    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<(bool Success, string Message, ProfileUserInfo? User)> GetProfileAsync(int userId)
    {
        try
        {
            var response = await HttpClient.GetAsync($"{ApiConfig.UsersUrl}/{userId}");
            var payload = await response.Content.ReadFromJsonAsync<ProfileUserInfo>(JsonOptions);

            if (!response.IsSuccessStatusCode || payload is null)
            {
                return (false, "Không tải được thông tin tài khoản.", null);
            }

            return (true, string.Empty, payload);
        }
        catch
        {
            return (false, "Không kết nối được tới máy chủ.", null);
        }
    }

    public async Task<(bool Success, string Message, ProfileUserInfo? User)> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        try
        {
            var response = await HttpClient.PutAsJsonAsync($"{ApiConfig.UsersUrl}/{userId}", request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Thử parse lỗi từ API
                try
                {
                    var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(content, JsonOptions);
                    if (errorObj != null && errorObj.TryGetValue("message", out var errorMsg))
                    {
                        return (false, errorMsg, null);
                    }
                }
                catch { }
                return (false, "Không thể lưu hồ sơ.", null);
            }

            // API trả về UserProfileDto trực tiếp (không wrapper)
            var payload = JsonSerializer.Deserialize<ProfileUserInfo>(content, JsonOptions);
            if (payload is null)
            {
                return (false, "Không thể lưu hồ sơ.", null);
            }

            return (true, "Đã lưu hồ sơ.", payload);
        }
        catch
        {
            return (false, "Không kết nối được tới máy chủ.", null);
        }
    }
}