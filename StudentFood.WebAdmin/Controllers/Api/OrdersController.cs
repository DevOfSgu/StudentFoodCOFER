using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFood.WebAdmin.Data;
using StudentFood.WebAdmin.DTOs;
using StudentFood.WebAdmin.Models;

namespace StudentFood.WebAdmin.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return BadRequest(new { message = "Giỏ hàng trống." });
        }

        var paymentMethod = request.PaymentMethod.Trim().ToLowerInvariant();
        if (paymentMethod is not ("cash" or "transfer"))
        {
            return BadRequest(new { message = "Phương thức thanh toán không hợp lệ." });
        }

        var student = await _context.Users.FirstOrDefaultAsync(user => user.Id == request.StudentId && user.Status == "active");
        if (student == null)
        {
            return BadRequest(new { message = "Không tìm thấy tài khoản sinh viên." });
        }

        var foodIds = request.Items.Select(item => item.FoodId).Distinct().ToList();
        var foods = await _context.Foods
            .Include(food => food.Canteen)
            .Where(food => foodIds.Contains(food.Id))
            .ToListAsync();

        if (foods.Count != foodIds.Count)
        {
            return BadRequest(new { message = "Có món ăn không tồn tại." });
        }

        if (foods.Any(food => !food.IsAvailable || food.Canteen == null || !string.Equals(food.Canteen.Status, "open", StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(new { message = "Có món ăn đã hết hàng hoặc quán đang đóng cửa." });
        }

        var canteenIds = foods.Select(food => food.CanteenId).Distinct().ToList();
        if (canteenIds.Count > 1)
        {
            return BadRequest(new { message = "Vui lòng chỉ đặt món từ một căn tin trong một đơn hàng." });
        }

        var canteen = foods.First().Canteen;
        var subtotal = 0m;
        var totalQuantity = 0;

        var order = new Order
        {
            StudentId = student.Id,
            CanteenId = canteen!.Id,
            Status = "pending",
            DeliveryAddress = request.DeliveryAddress.Trim(),
            PaymentMethod = paymentMethod,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OrderItems = []
        };

        foreach (var item in request.Items)
        {
            var food = foods.First(food => food.Id == item.FoodId);
            subtotal += food.Price * item.Quantity;
            totalQuantity += item.Quantity;

            order.OrderItems.Add(new OrderItem
            {
                FoodId = food.Id,
                Quantity = item.Quantity,
                PriceAtOrder = food.Price,
                Note = item.Note ?? string.Empty
            });
        }

        var deliveryFee = 3000m;
        var tax = subtotal * 0.08m;
        var commissionAmount = subtotal * (decimal)canteen.CommissionRate;
        var estimatedMinutes = 20 + Math.Min(40, totalQuantity * 4);

        order.Subtotal = subtotal;
        order.DeliveryFee = deliveryFee;
        order.CommissionAmount = commissionAmount;
        order.TotalPrice = subtotal + deliveryFee + tax;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var createdOrder = await LoadOrderAsync(order.Id);
        if (createdOrder == null)
        {
            return StatusCode(500, new { message = "Không thể tạo đơn hàng." });
        }

        return Ok(new PlaceOrderResponse
        {
            Message = "Đặt hàng thành công.",
            EstimatedDeliveryMinutes = estimatedMinutes,
            Order = MapOrder(createdOrder, estimatedMinutes)
        });
    }

    [HttpGet("User/{studentId:int}")]
    public async Task<IActionResult> GetUserOrders(int studentId)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(order => order.Canteen)
            .Include(order => order.OrderItems)
                .ThenInclude(orderItem => orderItem.Food)
                    .ThenInclude(food => food.Canteen)
            .Include(order => order.Reviews)
            .Where(order => order.StudentId == studentId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(order => MapOrder(order, EstimateMinutes(order))).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await LoadOrderAsync(id);
        if (order == null)
        {
            return NotFound(new { message = "Không tìm thấy đơn hàng." });
        }

        return Ok(MapOrder(order, EstimateMinutes(order)));
    }

    [HttpPatch("{orderId:int}/items/{orderItemId:int}/note")]
    public async Task<IActionResult> UpdateItemNote(int orderId, int orderItemId, [FromBody] UpdateNoteRequest request)
    {
        var orderItem = await _context.OrderItems
            .Include(oi => oi.Order)
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId && oi.OrderId == orderId);

        if (orderItem == null)
        {
            return NotFound(new { message = "Không tìm thấy món trong đơn hàng." });
        }

        orderItem.Note = request.Note.Trim();
        await _context.SaveChangesAsync();

        return Ok(new { message = "Ghi chú đã được cập nhật." });
    }

    [HttpPost("{orderId:int}/Review")]
    public async Task<IActionResult> CreateReview(int orderId, [FromBody] OrderReviewRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var order = await _context.Orders
            .Include(order => order.OrderItems)
            .Include(order => order.Reviews)
            .FirstOrDefaultAsync(order => order.Id == orderId);

        if (order == null)
        {
            return NotFound(new { message = "Không tìm thấy đơn hàng." });
        }

        if (order.StudentId != request.StudentId)
        {
            return Forbid();
        }

        if (!string.Equals(order.Status, "delivered", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Chỉ có thể đánh giá khi đơn hàng đã giao xong." });
        }

        if (!order.OrderItems.Any(orderItem => orderItem.FoodId == request.FoodId))
        {
            return BadRequest(new { message = "Món ăn này không thuộc đơn hàng." });
        }

        if (await _context.Reviews.AnyAsync(review => review.OrderId == orderId && review.StudentId == request.StudentId && review.FoodId == request.FoodId))
        {
            return BadRequest(new { message = "Món ăn này đã được đánh giá." });
        }

        var review = new Review
        {
            OrderId = orderId,
            StudentId = request.StudentId,
            FoodId = request.FoodId,
            Rating = request.Rating,
            Comment = request.Comment ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đánh giá đã được ghi nhận." });
    }
    [HttpPost("{orderId:int}/ConfirmDelivery")]
    public async Task<IActionResult> ConfirmDelivery(int orderId)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
        {
            return NotFound(new { message = "Không tìm thấy đơn hàng." });
        }

        if (!string.Equals(order.Status, "ready", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Đơn hàng chưa sẵn sàng để xác nhận giao." });
        }

        order.Status = "delivered";
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đã xác nhận nhận hàng thành công." });
    }

    [HttpPost("{orderId:int}/Cancel")]
    public async Task<IActionResult> CancelOrder(int orderId, [FromBody] CancelOrderRequest request)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
        {
            return NotFound(new { message = "Không tìm thấy đơn hàng." });
        }

        if (!string.Equals(order.Status, "pending", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Chỉ có thể hủy đơn hàng đang chờ xác nhận." });
        }

        order.Status = "cancelled";
        order.CancelReason = request.Reason?.Trim() ?? string.Empty;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đơn hàng đã được hủy." });
    }

    private async Task<Order?> LoadOrderAsync(int orderId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(order => order.Canteen)
            .Include(order => order.OrderItems)
                .ThenInclude(orderItem => orderItem.Food)
                    .ThenInclude(food => food.Canteen)
            .Include(order => order.Reviews)
            .FirstOrDefaultAsync(order => order.Id == orderId);
    }

    private static int EstimateMinutes(Order order)
    {
        var totalQuantity = order.OrderItems?.Sum(orderItem => orderItem.Quantity) ?? 0;
        return 20 + Math.Min(40, totalQuantity * 4);
    }

    private static OrderDto MapOrder(Order order, int estimatedMinutes)
    {
        var hasAnyReview = order.Reviews?.Any() ?? false;
        return new OrderDto
        {
            Id = order.Id,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            DeliveryAddress = order.DeliveryAddress,
            Subtotal = order.Subtotal,
            DeliveryFee = order.DeliveryFee,
            CommissionAmount = order.CommissionAmount,
            TotalPrice = order.TotalPrice,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            EstimatedDeliveryMinutes = estimatedMinutes,
            CanteenName = order.Canteen?.Name ?? string.Empty,
            CanteenStatus = order.Canteen?.Status ?? "open",
            HasAnyReview = hasAnyReview,
            CanReview = string.Equals(order.Status, "delivered", StringComparison.OrdinalIgnoreCase),
            StatusText = GetStatusText(order.Status),
            CancelReason = order.CancelReason,                Items = order.OrderItems?.Select(orderItem => new OrderItemDto
            {
                Id = orderItem.Id,
                OrderId = order.Id,
                FoodId = orderItem.FoodId,
                FoodName = orderItem.Food?.Name ?? string.Empty,
                FoodImageUrl = orderItem.Food?.ImageUrl ?? string.Empty,
                Quantity = orderItem.Quantity,
                PriceAtOrder = orderItem.PriceAtOrder,
                Note = orderItem.Note ?? string.Empty,
                HasReview = order.Reviews?.Any(review => review.FoodId == orderItem.FoodId) ?? false,
                CanReview = string.Equals(order.Status, "delivered", StringComparison.OrdinalIgnoreCase) && !(order.Reviews?.Any(review => review.FoodId == orderItem.FoodId) ?? false)
            }).ToList() ?? []
        };
    }

    private static string GetStatusText(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "pending" => "Chờ xác nhận",
            "preparing" => "Đang chuẩn bị",
            "ready" => "Đang giao",
            "delivered" => "Đã nhận hàng",
            "cancelled" => "Đã hủy",
            _ => "Đang xử lý"
        };
    }
}