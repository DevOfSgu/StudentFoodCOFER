using System.ComponentModel;
using System.Runtime.CompilerServices;
using StudentFood.MobileApp.Services;

namespace StudentFood.MobileApp.Models;

public class PlaceOrderItemRequest
{
    public int FoodId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Note { get; set; }
}

public class PlaceOrderRequest
{
    public int StudentId { get; set; }
    public string PaymentMethod { get; set; } = "cash";
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? CustomerName { get; set; }
    public List<PlaceOrderItemRequest> Items { get; set; } = [];
}

public class ReviewRequest
{
    public int StudentId { get; set; }
    public int FoodId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class OrderItemSummary : INotifyPropertyChanged
{
    private bool _isEditingNote;
    private string _editableNote = string.Empty;
    private string _note = string.Empty;

    public int Id { get; set; }
    public int OrderId { get; set; }
    public int FoodId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public string FoodImageUrl { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PriceAtOrder { get; set; }
    public bool HasReview { get; set; }
    public bool CanReview { get; set; }

    public string Note
    {
        get => _note;
        set
        {
            if (_note != value)
            {
                _note = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNote));
                OnPropertyChanged(nameof(NoteEditButtonText));
            }
        }
    }

    public bool HasNote => !string.IsNullOrWhiteSpace(Note);

    public bool IsEditingNote
    {
        get => _isEditingNote;
        set
        {
            if (_isEditingNote != value)
            {
                _isEditingNote = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotEditingNote));
            }
        }
    }

    public bool IsNotEditingNote => !_isEditingNote;
    public string NoteEditButtonText => string.IsNullOrWhiteSpace(Note) ? "+ Ghi chú" : "Sửa";

    public string EditableNote
    {
        get => _editableNote;
        set
        {
            if (_editableNote != value)
            {
                _editableNote = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public string DisplayPrice => CartService.FormatCurrency(PriceAtOrder);
    public string FullImageUrl =>
        string.IsNullOrWhiteSpace(FoodImageUrl)
            ? string.Empty
            : FoodImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? FoodImageUrl
                : $"{ApiConfig.ImageBaseUrl}/{FoodImageUrl.TrimStart('/')}";
}

public class OrderHistoryItem
{
    public int Id { get; set; }
    public string Status { get; set; } = "pending";
    public string PaymentMethod { get; set; } = "cash";
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int EstimatedDeliveryMinutes { get; set; }
    public string CanteenName { get; set; } = string.Empty;
    public string CanteenStatus { get; set; } = "open";
    public bool CanReview { get; set; }
    public bool HasAnyReview { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string? CancelReason { get; set; }
    public List<OrderItemSummary> Items { get; set; } = [];

    public string OrderNumberDisplay => $"#{Id:D4}";
    public string DisplayTotalPrice => CartService.FormatCurrency(TotalPrice);
    public string DisplayCreatedAt => CreatedAt.ToLocalTime().ToString("HH:mm dd/MM");
    public string DisplayUpdatedAt => UpdatedAt.ToLocalTime().ToString("HH:mm dd/MM");
    public string DisplayDeliveryEta => $"Khoảng {EstimatedDeliveryMinutes} phút";
    public string FirstItemImageUrl => Items.FirstOrDefault()?.FullImageUrl ?? string.Empty;
    public string PrimaryItemSummary => Items.Count switch
    {
        0 => "Không có món",
        1 => $"{Items[0].FoodName} x{Items[0].Quantity}",
        _ => $"{Items[0].FoodName} + {Items.Count - 1} món khác"
    };

    public double ProgressValue => Status.ToLowerInvariant() switch
    {
        "pending" => 0.2,
        "preparing" => 0.5,
        "ready" => 0.75,
        "delivered" => 1.0,
        "cancelled" => 1.0,
        _ => 0.35
    };

    public string StatusColor => Status.ToLowerInvariant() switch
    {
        "pending" => "#8A847E",
        "preparing" => "#FF8A00",
        "ready" => "#D97706",
        "delivered" => "#1F9D55",
        "cancelled" => "#D14343",
        _ => "#8A847E"
    };

    public string PaymentMethodText => string.Equals(PaymentMethod, "transfer", StringComparison.OrdinalIgnoreCase)
        ? "Chuyển khoản"
        : "Tiền mặt";
}

public class PlaceOrderResponse
{
    public string Message { get; set; } = string.Empty;
    public int EstimatedDeliveryMinutes { get; set; }
    public OrderHistoryItem? Order { get; set; }
}