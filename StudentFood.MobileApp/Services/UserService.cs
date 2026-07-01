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
            var payload = await response.Content.ReadFromJsonAsync<UpdateProfileResponse>(JsonOptions);

            if (!response.IsSuccessStatusCode || payload?.User is null)
            {
                return (false, payload?.Message ?? "Không thể lưu hồ sơ.", null);
            }

            return (true, payload.Message ?? "Đã lưu hồ sơ.", payload.User);
        }
        catch
        {
            return (false, "Không kết nối được tới máy chủ.", null);
        }
    }
}