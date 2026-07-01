using StudentFood.MobileApp.Models;
using StudentFood.MobileApp.Services;

namespace StudentFood.MobileApp.Pages;

[QueryProperty(nameof(Food), "Food")]
public partial class FoodDetailPage : ContentPage
{
    private FoodItem _food;
    private int _quantity = 1;

    public FoodItem Food
    {
        get => _food;
        set
        {
            _food = value;
            OnPropertyChanged();
        }
    }

    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = Math.Max(1, value);
            OnPropertyChanged();
        }
    }

    public FoodDetailPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnAddToCartClicked(object sender, EventArgs e)
    {
        if (Food == null) return;

        var cart = CartService.Instance;
        var canteenId = Food.Canteen?.Id ?? 0;

        // If cart has items from a different canteen, ask user to clear
        if (canteenId > 0 && cart.Items.Count > 0 && cart.HasItemFromDifferentCanteen(canteenId))
        {
            var currentCanteenName = cart.Items.FirstOrDefault()?.Food.CanteenName ?? "quán khác";
            bool confirmed = await ThemedConfirmPopupPage.ShowAsync(
                "Giỏ hàng có món từ quán khác",
                $"Giỏ hàng của bạn đang có món từ \"{currentCanteenName}\". Bạn có muốn xóa giỏ hàng cũ và thêm món từ \"{Food.CanteenName}\" không?",
                "Hủy",
                "Xóa & Thêm mới");

            if (!confirmed) return;

            cart.ClearCart();
        }

        cart.AddToCart(Food, Quantity);

        await ThemedMessagePopupPage.ShowAsync(
            "Đã thêm vào giỏ hàng",
            $"Đã thêm {Quantity}x \"{Food.Name}\" vào giỏ hàng.");

        await Shell.Current.GoToAsync("..");
    }

    private void OnIncreaseQuantityClicked(object? sender, EventArgs e)
    {
        Quantity++;
    }

    private void OnDecreaseQuantityClicked(object? sender, EventArgs e)
    {
        if (Quantity > 1)
        {
            Quantity--;
        }
    }
}