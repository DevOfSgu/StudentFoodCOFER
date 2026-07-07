using StudentFood.MobileApp.Models;
using StudentFood.MobileApp.Services;
using System.Windows.Input;

namespace StudentFood.MobileApp.Pages;

[QueryProperty(nameof(OrderId), "OrderId")]
[QueryProperty(nameof(FoodId), "FoodId")]
public partial class ReviewOrderPage : ContentPage
{
    private readonly OrderService _orderService = new();
    private readonly AuthService _authService = new();
    private int _orderId;
    private int _foodId;
    private OrderHistoryItem? _order;
    private OrderItemSummary? _selectedFood;
    private int _rating = 5;
    private bool _isSubmitting;

    public bool IsSubmitting
    {
        get => _isSubmitting;
        set
        {
            _isSubmitting = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotSubmitting));
            OnPropertyChanged(nameof(SubmitButtonText));
        }
    }

    public bool IsNotSubmitting => !_isSubmitting;
    public string SubmitButtonText => _isSubmitting ? "Đang gửi..." : "Gửi đánh giá";

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

    public int FoodId
    {
        get => _foodId;
        set
        {
            _foodId = value;
            OnPropertyChanged();
            _ = LoadOrderAsync();
        }
    }

    public string SelectedFoodName => _selectedFood?.FoodName ?? "Món ăn";
    public string SelectedFoodImageUrl => _selectedFood?.FullImageUrl ?? string.Empty;
    public string SelectedOrderStatusText => _order?.StatusText ?? string.Empty;
    public string RatingText => $"Bạn đang chấm {Rating} sao";
    public int Rating
    {
        get => _rating;
        set
        {
            _rating = Math.Clamp(value, 1, 5);
            RefreshStars();
        }
    }

    public string Star1BackgroundColor => Rating >= 1 ? "#FFF1E0" : "#F7F3EE";
    public string Star2BackgroundColor => Rating >= 2 ? "#FFF1E0" : "#F7F3EE";
    public string Star3BackgroundColor => Rating >= 3 ? "#FFF1E0" : "#F7F3EE";
    public string Star4BackgroundColor => Rating >= 4 ? "#FFF1E0" : "#F7F3EE";
    public string Star5BackgroundColor => Rating >= 5 ? "#FFF1E0" : "#F7F3EE";
    public string Star1TextColor => Rating >= 1 ? "#FF8A00" : "#8A847E";
    public string Star2TextColor => Rating >= 2 ? "#FF8A00" : "#8A847E";
    public string Star3TextColor => Rating >= 3 ? "#FF8A00" : "#8A847E";
    public string Star4TextColor => Rating >= 4 ? "#FF8A00" : "#8A847E";
    public string Star5TextColor => Rating >= 5 ? "#FF8A00" : "#8A847E";
    public ICommand SetRatingCommand { get; }

    public ReviewOrderPage()
    {
        InitializeComponent();
        SetRatingCommand = new Command<string>(SetRatingFromCommand);
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadOrderAsync();
    }

    private async Task LoadOrderAsync()
    {
        if (OrderId <= 0 || FoodId <= 0)
        {
            return;
        }

        var result = await _orderService.GetOrderAsync(OrderId);
        if (!result.Success || result.Order == null)
        {
            return;
        }

        _order = result.Order;
        _selectedFood = _order.Items.FirstOrDefault(item => item.FoodId == FoodId) ?? _order.Items.FirstOrDefault();

        OnPropertyChanged(nameof(SelectedFoodName));
        OnPropertyChanged(nameof(SelectedFoodImageUrl));
        OnPropertyChanged(nameof(SelectedOrderStatusText));
        RefreshStars();
    }

    private void RefreshStars()
    {
        OnPropertyChanged(nameof(RatingText));
        OnPropertyChanged(nameof(Star1BackgroundColor));
        OnPropertyChanged(nameof(Star2BackgroundColor));
        OnPropertyChanged(nameof(Star3BackgroundColor));
        OnPropertyChanged(nameof(Star4BackgroundColor));
        OnPropertyChanged(nameof(Star5BackgroundColor));
        OnPropertyChanged(nameof(Star1TextColor));
        OnPropertyChanged(nameof(Star2TextColor));
        OnPropertyChanged(nameof(Star3TextColor));
        OnPropertyChanged(nameof(Star4TextColor));
        OnPropertyChanged(nameof(Star5TextColor));
    }

    private void SetRatingFromCommand(string? value)
    {
        if (int.TryParse(value, out var rating))
        {
            Rating = rating;
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        if (_order == null || _selectedFood == null)
        {
            await ThemedMessagePopupPage.ShowAsync("Lỗi", "Không tìm thấy thông tin món ăn để đánh giá.");
            return;
        }

        var studentId = Preferences.Default.Get("user_id", 0);
        if (studentId <= 0)
        {
            await ThemedMessagePopupPage.ShowAsync("Lỗi", "Vui lòng đăng nhập lại.");
            return;
        }

        IsSubmitting = true;

        try
        {
            var result = await _orderService.SubmitReviewAsync(_order.Id, new ReviewRequest
            {
                StudentId = studentId,
                FoodId = _selectedFood.FoodId,
                Rating = Rating,
                Comment = CommentEditor.Text?.Trim()
            });

            if (!result.Success)
            {
                await ThemedMessagePopupPage.ShowAsync("Không thể gửi đánh giá", result.Message ?? "Đã xảy ra lỗi.");
                return;
            }

            // Navigate back FIRST, then show popup from parent page context
            await Shell.Current.GoToAsync("..");
            await ThemedMessagePopupPage.ShowAsync("Đánh giá thành công", result.Message ?? "Cảm ơn bạn đã đánh giá!");
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..", true);
    }
}