using System.Net.Http.Json;
using System.Text.Json;
using StudentFood.MobileApp.Models;

namespace StudentFood.MobileApp.Services;

public class FoodService
{
    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<FoodItem>> GetAllFoodsAsync()
    {
        return await GetFoodsAsync(ApiConfig.FoodsUrl);
    }

    public async Task<List<FoodItem>> GetTrendingFoodsAsync()
    {
        return await GetFoodsAsync(ApiConfig.TrendingFoodsUrl);
    }

    public async Task<List<FoodItem>> GetRecommendedFoodsAsync()
    {
        return await GetFoodsAsync(ApiConfig.RecommendedFoodsUrl);
    }

    private static async Task<List<FoodItem>> GetFoodsAsync(string url)
    {
        try
        {
            var result = await HttpClient.GetFromJsonAsync<List<FoodItem>>(url, JsonOptions);
            return result ?? [];
        }
        catch
        {
            return [];
        }
    }
}
