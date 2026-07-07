using StudentFood.MobileApp.Services;

namespace StudentFood.MobileApp.Models;

public class FoodItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public CategoryInfo? Category { get; set; }
    public CanteenInfo? Canteen { get; set; }
    public double AverageRating { get; set; }

    public bool HasNoImage => string.IsNullOrWhiteSpace(FullImageUrl);
    public string CanteenName => Canteen?.Name ?? "Căn tin";
    public string CanteenStatus => Canteen?.Status ?? "open";
    public bool IsCanteenOpen => string.Equals(CanteenStatus, "open", StringComparison.OrdinalIgnoreCase);
    public string CategoryName => Category?.Name ?? string.Empty;
    public string DisplayPrice => CartService.FormatCurrency(Price);
    public string FullImageUrl =>
        string.IsNullOrWhiteSpace(ImageUrl)
            ? string.Empty
            : ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? ImageUrl
                : $"{ApiConfig.ImageBaseUrl}/{ImageUrl.TrimStart('/')}";
}

public class CategoryInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CanteenInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "open";
}
