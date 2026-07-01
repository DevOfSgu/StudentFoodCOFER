using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFood.WebAdmin.Data;

namespace StudentFood.WebAdmin.Controllers;

public class CanteenDashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public CanteenDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        // Kiểm tra đăng nhập
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null || HttpContext.Session.GetString("UserRole") != "canteen")
        {
            return RedirectToAction("Login", "Account");
        }

        // Lấy căn tin của nhân viên này
        var canteen = _context.Canteens.FirstOrDefault(c => c.OwnerId == userId);
        if (canteen == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var today = DateTime.UtcNow.Date;

        // Thống kê dữ liệu của căn tin này (Chỉ tính trong ngày hôm nay)
        ViewBag.CanteenName = canteen.Name;
        ViewBag.OrdersToday = _context.Orders.Count(o => o.CanteenId == canteen.Id && o.CreatedAt >= today);
        ViewBag.PreparingOrders = _context.Orders.Count(o => o.CanteenId == canteen.Id && o.CreatedAt >= today && o.Status == "preparing");
        ViewBag.CompletedOrders = _context.Orders.Count(o => o.CanteenId == canteen.Id && o.CreatedAt >= today && o.Status == "delivered");
        ViewBag.RevenueToday = _context.Orders
            .Where(o => o.CanteenId == canteen.Id && o.CreatedAt >= today && o.Status == "delivered")
            .Sum(o => (decimal?)(o.Subtotal - o.CommissionAmount)) ?? 0;

        // Đơn hàng của căn tin này (Chỉ lấy các đơn chưa giao xong để hiển thị Kanban)
        var orders = _context.Orders
            .Include(o => o.Student)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Food)
            .Where(o => o.CanteenId == canteen.Id && o.Status != "delivered")
            .OrderBy(o => o.CreatedAt)
            .ToList();

        return View(orders);
    }

    [HttpPost]
    public IActionResult UpdateOrderStatus(int orderId, string status)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null || HttpContext.Session.GetString("UserRole") != "canteen")
        {
            return Unauthorized();
        }

        var canteen = _context.Canteens.FirstOrDefault(c => c.OwnerId == userId);
        if (canteen == null) return Unauthorized();

        var order = _context.Orders.FirstOrDefault(o => o.Id == orderId && o.CanteenId == canteen.Id);
        if (order != null)
        {
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }
}
