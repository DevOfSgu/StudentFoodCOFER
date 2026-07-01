using System.Collections.ObjectModel;
using System.Globalization;
using StudentFood.MobileApp.Models;

namespace StudentFood.MobileApp.Services;

public class CartService
{
    private static readonly CartService _instance = new();
    public static CartService Instance => _instance;

    public ObservableCollection<CartItem> Items { get; } = [];

    public decimal Subtotal => Items.Sum(i => i.TotalPrice);
    public decimal DeliveryFee => 3000m;
    public decimal Tax => Subtotal * 0.08m;
    public decimal Total => Subtotal + DeliveryFee + Tax;

    public static string FormatCurrency(decimal amount)
    {
        var culture = CultureInfo.GetCultureInfo("vi-VN");
        return $"{amount.ToString("N0", culture)}đ";
    }

    private CartService() { }

    public bool HasItemFromDifferentCanteen(int canteenId)
    {
        return Items.Any(i => i.Food.Canteen != null && i.Food.Canteen.Id != canteenId);
    }

    public void AddToCart(FoodItem food, int quantity = 1)
    {
        var existingItem = Items.FirstOrDefault(i => i.Food.Id == food.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            Items.Add(new CartItem { Food = food, Quantity = quantity });
        }
    }

    public void RemoveFromCart(CartItem item)
    {
        Items.Remove(item);
    }

    public void ClearCart()
    {
        Items.Clear();
    }
}
