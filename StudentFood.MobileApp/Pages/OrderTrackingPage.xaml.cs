using System.Collections.ObjectModel;
using System.Windows.Input;
using StudentFood.MobileApp.Models;
using StudentFood.MobileApp.Services;

namespace StudentFood.MobileApp.Pages;

[QueryProperty(nameof(OrderId), "OrderId")]
public partial class OrderTrackingPage : ContentPage
{
    private readonly OrderService _orderService = new();
    private readonly AuthService _authService = new();
    private IDispatcherTimer? _refreshTimer;
    private bool _isLoading;
    private int _orderId;
    private OrderHistoryItem? _currentOrder;

    public ObservableCollection<OrderStatusStep> StatusSteps { get; } = [];

    public int OrderId
    {
        get => _orderId;
        set
        {
            _orderId = value;
            OnPropertyChanged();
            _ = LoadOrderAsync();
        }
    }

    public OrderHistoryItem? CurrentOrder
    {
        get => _currentOrder;
        set
        {
            _currentOrder = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanReviewCurrentOrder));
            RefreshStatusSteps();
        }
    }

    public bool CanReviewCurrentOrder => CurrentOrder?.CanReview == true;

    public ICommand RefreshCommand { get; }
    public ICommand ReviewFoodCommand { get; }

    public OrderTrackingPage()
    {
        InitializeComponent();
        RefreshCommand = new Command(async () => await LoadOrderAsync());
        ReviewFoodCommand = new Command<OrderItemSummary>(async item => await OnReviewFoodAsync(item));
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        StartAutoRefresh();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopAutoRefresh();
    }

    private async Task LoadOrderAsync()
    {
        if (_isLoading || OrderId <= 0)
        {
            return;
        }

        try
        {
            _isLoading = true;
            var result = await _orderService.GetOrderAsync(OrderId);
            if (result.Success && result.Order != null)
            {
                CurrentOrder = result.Order;
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void RefreshStatusSteps()
    {
        StatusSteps.Clear();

        if (CurrentOrder == null)
        {
            return;
        }

        var currentStage = CurrentOrder.Status.ToLowerInvariant() switch
        {
            "pending" => 0,
            "preparing" => 1,
            "ready" => 2,
            "delivered" => 3,
            "cancelled" => 3,
            _ => 0
        };

        var steps = new[]
        {
            new OrderStatusStep("Chờ xác nhận", "Quán đã nhận đơn", currentStage >= 0),
            new OrderStatusStep("Đang chuẩn bị", "Nhân viên đang làm món", currentStage >= 1),
            new OrderStatusStep("Sẵn sàng giao", "Đơn sắp được gửi tới bạn", currentStage >= 2),
            new OrderStatusStep("Đã nhận hàng", "Khách đã nhận món", currentStage >= 3)
        };

        foreach (var step in steps)
        {
            StatusSteps.Add(step);
        }

        OnPropertyChanged(nameof(CanReviewCurrentOrder));
    }

    private async Task OnReviewFoodAsync(OrderItemSummary? item)
    {
        if (CurrentOrder == null || item == null || item.HasReview || !CurrentOrder.CanReview)
        {
            return;
        }

        await Shell.Current.GoToAsync(nameof(ReviewOrderPage), new Dictionary<string, object>
        {
            { "OrderId", CurrentOrder.Id },
            { "FoodId", item.FoodId }
        });
    }

    private void StartAutoRefresh()
    {
        if (_refreshTimer != null)
        {
            return;
        }

        _refreshTimer = Dispatcher.CreateTimer();
        _refreshTimer.Interval = TimeSpan.FromSeconds(15);
        _refreshTimer.Tick += async (_, _) => await LoadOrderAsync();
        _refreshTimer.Start();
    }

    private void StopAutoRefresh()
    {
        if (_refreshTimer == null)
        {
            return;
        }

        _refreshTimer.Stop();
        _refreshTimer = null;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..", true);
    }
}

public class OrderStatusStep
{
    public OrderStatusStep(string title, string description, bool isComplete)
    {
        Title = title;
        Description = description;
        IsComplete = isComplete;
    }

    public string Title { get; }
    public string Description { get; }
    public bool IsComplete { get; }
    public string CircleColor => IsComplete ? "#FF8A00" : "#E8D8C6";
    public string TextColor => IsComplete ? "#1F1B16" : "#8A847E";
}