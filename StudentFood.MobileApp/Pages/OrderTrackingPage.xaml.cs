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
            OnPropertyChanged(nameof(CanCancelOrder));
            OnPropertyChanged(nameof(CanConfirmDelivery));
            OnPropertyChanged(nameof(IsOrderCancelled));
            OnPropertyChanged(nameof(HasCancelReason));
            RefreshStatusSteps();
        }
    }

    public bool CanReviewCurrentOrder => CurrentOrder?.CanReview == true;
    public bool CanCancelOrder => CurrentOrder != null && string.Equals(CurrentOrder.Status, "pending", StringComparison.OrdinalIgnoreCase);
    public bool CanConfirmDelivery => CurrentOrder != null && string.Equals(CurrentOrder.Status, "ready", StringComparison.OrdinalIgnoreCase);
    public bool IsOrderCancelled => CurrentOrder != null && string.Equals(CurrentOrder.Status, "cancelled", StringComparison.OrdinalIgnoreCase);
    public bool HasCancelReason => IsOrderCancelled && !string.IsNullOrWhiteSpace(CurrentOrder?.CancelReason);

    public ICommand RefreshCommand { get; }
    public ICommand ReviewFoodCommand { get; }
    public ICommand EditNoteCommand { get; }
    public ICommand SaveNoteCommand { get; }
    public ICommand CancelEditNoteCommand { get; }
    public ICommand CancelOrderCommand { get; }
    public ICommand ConfirmDeliveryCommand { get; }

    public OrderTrackingPage()
    {
        InitializeComponent();
        RefreshCommand = new Command(async () => await LoadOrderAsync());
        ReviewFoodCommand = new Command<OrderItemSummary>(async item => await OnReviewFoodAsync(item));
        EditNoteCommand = new Command<OrderItemSummary>(OnEditNote);
        SaveNoteCommand = new Command<OrderItemSummary>(async item => await OnSaveNoteAsync(item));
        CancelEditNoteCommand = new Command<OrderItemSummary>(OnCancelEditNote);
        CancelOrderCommand = new Command(async () => await OnCancelOrderAsync());
        ConfirmDeliveryCommand = new Command(async () => await OnConfirmDeliveryAsync());
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

        var isCancelled = string.Equals(CurrentOrder.Status, "cancelled", StringComparison.OrdinalIgnoreCase);

        if (isCancelled)
        {
            // Khi đã hủy: chỉ đánh dấu bước đầu tiên, thêm bước "Đã hủy" màu đỏ
            StatusSteps.Add(new OrderStatusStep("Chờ xác nhận", "Đã nhận đơn", true));
            StatusSteps.Add(new CancelledStatusStep());
        }
        else
        {
            var currentStage = CurrentOrder.Status.ToLowerInvariant() switch
            {
                "pending" => 0,
                "preparing" => 1,
                "ready" => 2,
                "delivered" => 3,
                _ => 0
            };

            var steps = new[]
            {
                new OrderStatusStep("Chờ xác nhận", "Quán đã nhận đơn", currentStage >= 0),
                new OrderStatusStep("Đang chuẩn bị", "Nhân viên đang làm món", currentStage >= 1),
                new OrderStatusStep("Sẵn sàng", "Món đã sẵn sàng, vui lòng đến nhận", currentStage >= 2),
                new OrderStatusStep("Đã nhận hàng", "Khách đã nhận món", currentStage >= 3)
            };

            foreach (var step in steps)
            {
                StatusSteps.Add(step);
            }
        }

        OnPropertyChanged(nameof(CanReviewCurrentOrder));
    }

    private void OnEditNote(OrderItemSummary? item)
    {
        if (item == null) return;
        item.EditableNote = item.Note;
        item.IsEditingNote = true;
    }

    private async Task OnSaveNoteAsync(OrderItemSummary? item)
    {
        if (item == null || CurrentOrder == null) return;
        if (string.IsNullOrWhiteSpace(item.EditableNote))
        {
            item.EditableNote = string.Empty;
        }

        var result = await _orderService.UpdateItemNoteAsync(CurrentOrder.Id, item.Id, item.EditableNote?.Trim() ?? "");
        if (!result.Success)
        {
            await ThemedMessagePopupPage.ShowAsync("Lỗi", result.Message);
            return;
        }

        item.IsEditingNote = false;
        item.Note = item.EditableNote?.Trim() ?? "";

        await ThemedMessagePopupPage.ShowAsync("Đã lưu", "Ghi chú đã được cập nhật.");
        await LoadOrderAsync(); // Refresh to get latest data
    }

    private void OnCancelEditNote(OrderItemSummary? item)
    {
        if (item == null) return;
        item.IsEditingNote = false;
        item.EditableNote = item.Note;
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

    private async Task OnConfirmDeliveryAsync()
    {
        if (CurrentOrder == null || !CanConfirmDelivery) return;

        var confirmed = await ThemedConfirmPopupPage.ShowAsync(
            "Xác nhận đã nhận hàng",
            $"Bạn đã nhận đơn #{CurrentOrder.Id:D4}?",
            "Chưa",
            "Đã nhận hàng");

        if (!confirmed) return;

        var result = await _orderService.ConfirmDeliveryAsync(CurrentOrder.Id);
        if (!result.Success)
        {
            await ThemedMessagePopupPage.ShowAsync("Lỗi", result.Message);
            return;
        }

        await ThemedMessagePopupPage.ShowAsync("Đã nhận hàng", "Cảm ơn bạn! Chúc bạn ngon miệng.");
        await LoadOrderAsync();
    }

    private async Task OnCancelOrderAsync()
    {
        if (CurrentOrder == null || !CanCancelOrder) return;

        var reason = await ThemedPromptPopupPage.ShowAsync(
            "Hủy đơn hàng",
            $"Bạn có chắc muốn hủy đơn #{CurrentOrder.Id:D4}? Vui lòng nhập lý do:",
            "VD: Đổi ý, muốn thêm món...",
            "Không hủy",
            "Xác nhận hủy");

        if (string.IsNullOrEmpty(reason)) return;

        var result = await _orderService.CancelOrderAsync(CurrentOrder.Id, reason);
        if (!result.Success)
        {
            await ThemedMessagePopupPage.ShowAsync("Lỗi", result.Message);
            return;
        }

        await ThemedMessagePopupPage.ShowAsync("Đã hủy đơn", result.Message);
        await LoadOrderAsync();
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

public class CancelledStatusStep : OrderStatusStep
{
    public CancelledStatusStep() : base("Đã hủy", "Đơn hàng đã bị hủy", true) { }

    public new string CircleColor => "#D14343";
    public new string TextColor => "#D14343";
}