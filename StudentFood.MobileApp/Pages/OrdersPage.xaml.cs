using System.Collections.ObjectModel;
using StudentFood.MobileApp.Models;
using StudentFood.MobileApp.Services;

namespace StudentFood.MobileApp.Pages;

public partial class OrdersPage : ContentPage
{
    public ObservableCollection<CartItem> CartItems => CartService.Instance.Items;

    public bool HasCartItems => CartItems.Count > 0;
    public bool IsCartEmpty => !HasCartItems;
    public string ItemsCountText => $"{CartItems.Count} món";
    public string SubtotalDisplay => CartService.FormatCurrency(CartService.Instance.Subtotal);

    public OrdersPage()
    {
        InitializeComponent();
        BindingContext = this;
        CartService.Instance.Items.CollectionChanged += (_, __) => RefreshState();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshState();
    }

    private void RefreshState()
    {
        OnPropertyChanged(nameof(HasCartItems));
        OnPropertyChanged(nameof(IsCartEmpty));
        OnPropertyChanged(nameof(ItemsCountText));
        OnPropertyChanged(nameof(SubtotalDisplay));
    }

    private async void OnCheckoutClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CheckoutPage));
    }

    private async void OnBrowseMenuClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MenuPage");
    }

    private void OnDeleteItemClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is CartItem item)
        {
            CartService.Instance.RemoveFromCart(item);
            RefreshState();
        }
    }

    private void OnIncreaseClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is CartItem item)
        {
            item.Quantity++;
            RefreshState();
        }
    }

    private void OnDecreaseClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is CartItem item)
        {
            if (item.Quantity <= 1)
            {
                CartService.Instance.RemoveFromCart(item);
            }
            else
            {
                item.Quantity--;
            }
            RefreshState();
        }
    }
}