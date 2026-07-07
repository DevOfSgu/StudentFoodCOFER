using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFood.WebAdmin.Data;

namespace StudentFood.WebAdmin.Controllers;

public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(string? status, string? search, string? dateFrom, string? dateTo, int page = 1)
    {
        const int pageSize = 10;

        // Kiểm tra đăng nhập
        if (HttpContext.Session.GetInt32("UserId") == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Query gốc KHÔNG filter theo status (dùng cho stats)
        var baseQuery = _context.Orders
            .Include(o => o.Student)
            .Include(o => o.Canteen)
            .AsQueryable();

        // Query có filter (dùng cho danh sách + phân trang)
        var query = baseQuery;

        // Lọc theo trạng thái (chỉ áp dụng cho danh sách, KHÔNG cho stats)
        if (!string.IsNullOrEmpty(status) && status != "all")
        {
            query = query.Where(o => o.Status == status);
        }

        // Tìm kiếm theo mã đơn, tên sinh viên, căn tin
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(o =>
                o.Id.ToString().Contains(s) ||
                (o.Student != null && o.Student.FullName.ToLower().Contains(s)) ||
                (o.Canteen != null && o.Canteen.Name.ToLower().Contains(s))
            );
            baseQuery = baseQuery.Where(o =>
                o.Id.ToString().Contains(s) ||
                (o.Student != null && o.Student.FullName.ToLower().Contains(s)) ||
                (o.Canteen != null && o.Canteen.Name.ToLower().Contains(s))
            );
        }

        // Lọc theo ngày
        if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out var fromDate))
        {
            query = query.Where(o => o.CreatedAt >= fromDate);
            baseQuery = baseQuery.Where(o => o.CreatedAt >= fromDate);
        }
        if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out var toDate))
        {
            query = query.Where(o => o.CreatedAt <= toDate.AddDays(1));
            baseQuery = baseQuery.Where(o => o.CreatedAt <= toDate.AddDays(1));
        }

        var queryFiltered = query.OrderByDescending(o => o.CreatedAt);

        // Đếm tổng số để phân trang
        var totalCount = queryFiltered.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        var orders = queryFiltered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Thống kê (tính từ baseQuery - KHÔNG filter theo status)
        // Dùng CountAsync riêng thay vì ToList để tránh load hết dữ liệu
        ViewBag.TotalOrders = baseQuery.Count();
        ViewBag.PendingOrders = baseQuery.Count(o => o.Status == "pending");
        ViewBag.PreparingOrders = baseQuery.Count(o => o.Status == "preparing");
        ViewBag.ReadyOrders = baseQuery.Count(o => o.Status == "ready");
        ViewBag.DeliveredOrders = baseQuery.Count(o => o.Status == "delivered");
        ViewBag.CancelledOrders = baseQuery.Count(o => o.Status == "cancelled");

        ViewBag.CurrentStatus = status ?? "all";
        ViewBag.SearchTerm = search ?? "";
        ViewBag.DateFrom = dateFrom ?? "";
        ViewBag.DateTo = dateTo ?? "";

        // Phân trang
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(orders);
    }

    // GET: Orders/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (id == null)
        {
            return NotFound();
        }

        var order = await _context.Orders
            .Include(o => o.Student)
            .Include(o => o.Canteen)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Food)
                    .ThenInclude(f => f.Category)
            .Include(o => o.Reviews)
                .ThenInclude(r => r.Student)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpPost]
    public IActionResult UpdateStatus(int orderId, string status)
    {
        if (HttpContext.Session.GetString("UserRole") != "admin")
        {
            return Unauthorized();
        }

        var order = _context.Orders.Find(orderId);
        if (order != null)
        {
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
        }

        return RedirectToAction("Details", new { id = orderId });
    }
}
