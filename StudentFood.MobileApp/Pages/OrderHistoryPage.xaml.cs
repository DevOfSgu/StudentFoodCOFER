using System.Collections.ObjectModel;
using StudentFood.MobileApp.Models;
using StudentFood.MobileApp.Services;

namespace StudentFood.MobileApp.Pages;

public partial class OrderHistoryPage : ContentPage
{
    private readonly OrderService _orderService = new();

    public ObservableCollection<OrderHistoryItem> Orders { get; } = [];
    public bool HasOrders => Orders.Count > 0;
    public bool IsEmpty => !HasOrders;

    public OrderHistoryPage()
    {
        InitializeComponent();
        BindingContext = this;
        Orders.CollectionChanged += (_, __) =>
        {
            OnPropertyChanged(nameof(HasOrders));
            OnPropertyChanged(nameof(IsEmpty));
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadOrdersAsync();
    }

    private async Task LoadOrdersAsync()
    {
        var studentId = Preferences.Default.Get("user_id", 0);
        if (studentId <= 0) return;

        var result = await _orderService.GetOrdersAsync(studentId);
        if (!result.Success) return;

        Orders.Clear();
        foreach (var order in result.Orders)
        {
            Orders.Add(order);
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnTrackOrderClicked(object sender, EventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is OrderHistoryItem order)
        {
            await Shell.Current.GoToAsync(nameof(OrderTrackingPage), new Dictionary<string, object>
            {
                { "OrderId", order.Id }
            });
        }
    }
}
