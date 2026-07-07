using System.ComponentModel.DataAnnotations;

namespace StudentFood.WebAdmin.DTOs;

public class OrderItemCreateRequest
{
    [Required]
    public int FoodId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    public string? Note { get; set; }
}

public class OrderCreateRequest
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = "cash";

    [Required]
    public string DeliveryAddress { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? CustomerName { get; set; }

    public List<OrderItemCreateRequest> Items { get; set; } = [];
}

public class CancelOrderRequest
{
    public string? Reason { get; set; }
}

public class UpdateNoteRequest
{
    [Required]
    [StringLength(500)]
    public string Note { get; set; } = string.Empty;
}

public class OrderReviewRequest
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int FoodId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    public string? Comment { get; set; }
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int FoodId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public string FoodImageUrl { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PriceAtOrder { get; set; }
    public string DisplayPrice => $"{PriceAtOrder:N0}đ";
    public bool HasReview { get; set; }
    public bool CanReview { get; set; }
    public string Note { get; set; } = string.Empty;
}

public class OrderDto
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
    public List<OrderItemDto> Items { get; set; } = [];
}

public class PlaceOrderResponse
{
    public string Message { get; set; } = string.Empty;
    public int EstimatedDeliveryMinutes { get; set; }
    public OrderDto? Order { get; set; }
}