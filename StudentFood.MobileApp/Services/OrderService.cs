using System.Net.Http.Json;
using System.Text.Json;
using StudentFood.MobileApp.Models;

namespace StudentFood.MobileApp.Services;

public class OrderService
{
    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<(bool Success, string Message, PlaceOrderResponse? Payload)> PlaceOrderAsync(PlaceOrderRequest request)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync(ApiConfig.OrdersUrl, request);
            var payload = await response.Content.ReadFromJsonAsync<PlaceOrderResponse>(JsonOptions);

            if (!response.IsSuccessStatusCode || payload?.Order is null)
            {
                return (false, payload?.Message ?? "Không thể đặt hàng.", null);
            }

            return (true, payload.Message, payload);
        }
        catch
        {
            return (false, "Không kết nối được tới máy chủ.", null);
        }
    }

    public async Task<(bool Success, string Message, IReadOnlyList<OrderHistoryItem> Orders)> GetOrdersAsync(int studentId)
    {
        try
        {
            var response = await HttpClient.GetAsync($"{ApiConfig.OrdersUrl}/User/{studentId}");
            var payload = await response.Content.ReadFromJsonAsync<List<OrderHistoryItem>>(JsonOptions);

            if (!response.IsSuccessStatusCode || payload is null)
            {
                return (false, "Không tải được danh sách đơn hàng.", []);
            }

            return (true, string.Empty, payload);
        }
        catch
        {
            return (false, "Không kết nối được tới máy chủ.", []);
        }
    }

    public async Task<(bool Success, string Message, OrderHistoryItem? Order)> GetOrderAsync(int orderId)
    {
        try
        {
            var response = await HttpClient.GetAsync($"{ApiConfig.OrdersUrl}/{orderId}");
            var payload = await response.Content.ReadFromJsonAsync<OrderHistoryItem>(JsonOptions);

            if (!response.IsSuccessStatusCode || payload is null)
            {
                return (false, "Không tải được đơn hàng.", null);
            }

            return (true, string.Empty, payload);
        }
        catch
        {
            return (false, "Không kết nối được tới máy chủ.", null);
        }
    }

    public async Task<(bool Success, string Message)> UpdateItemNoteAsync(int orderId, int orderItemId, string note)
    {
        try
        {
            var response = await HttpClient.PatchAsJsonAsync($"{ApiConfig.OrdersUrl}/{orderId}/items/{orderItemId}/note", new { note });
            var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(JsonOptions);
            var message = payload is not null && payload.TryGetValue("message", out var responseMessage)
                ? responseMessage
                : "Không thể cập nhật ghi chú.";

            if (!response.IsSuccessStatusCode)
            {
                return (false, message);
            }

            return (true, message);
        }
        catch
        {
            return (false, "Không kết nối được tới máy chủ.");
        }
    }

    public async Task<(bool Success, string Message)> ConfirmDeliveryAsync(int orderId)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync($"{ApiConfig.OrdersUrl}/{orderId}/ConfirmDelivery", new { });
            var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(JsonOptions);
            var message = payload is not null && payload.TryGetValue("message", out var responseMessage)
                ? responseMessage
                : "Không thể xác nhận.";

            if (!response.IsSuccessStatusCode)
            {
                return (false, message);
            }

            return (true, message);
        }
        catch
        {
            return (false, "Không kết nối được tới máy chủ.");
        }
    }

    public async Task<(bool Success, string Message)> CancelOrderAsync(int orderId, string reason)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync($"{ApiConfig.OrdersUrl}/{orderId}/Cancel", new { reason });
            var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(JsonOptions);
            var message = payload is not null && payload.TryGetValue("message", out var responseMessage)
                ? responseMessage
                : "Không thể hủy đơn hàng.";

            if (!response.IsSuccessStatusCode)
            {
                return (false, message);
            }

            return (true, message);
        }
        catch
        {
            return (false, "Không kết nối được tới máy chủ.");
        }
    }

    public async Task<(bool Success, string Message)> SubmitReviewAsync(int orderId, ReviewRequest request)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync($"{ApiConfig.OrdersUrl}/{orderId}/Review", request);
            var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(JsonOptions);
            var message = payload is not null && payload.TryGetValue("message", out var responseMessage)
                ? responseMessage
                : "Không thể gửi đánh giá.";

            if (!response.IsSuccessStatusCode)
            {
                return (false, message);
            }

            return (true, message);
        }
        catch
        {
            return (false, "Không kết nối được tới máy chủ.");
        }
    }
}