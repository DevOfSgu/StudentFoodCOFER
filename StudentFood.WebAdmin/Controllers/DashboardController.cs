using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFood.WebAdmin.Data;

namespace StudentFood.WebAdmin.Controllers;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        // Kiểm tra đăng nhập
        if (HttpContext.Session.GetInt32("UserId") == null || HttpContext.Session.GetString("UserRole") != "admin")
        {
            return RedirectToAction("Login", "Account");
        }

        var today = DateTime.UtcNow.Date;

        // Thống kê dữ liệu thực từ database (Today)
        ViewBag.OrdersToday = _context.Orders.Count(o => o.CreatedAt >= today);
        ViewBag.RevenueToday = _context.Orders.Where(o => o.CreatedAt >= today && o.Status == "delivered").Sum(o => (decimal?)o.TotalPrice) ?? 0;
        ViewBag.CommissionToday = _context.Orders.Where(o => o.CreatedAt >= today && o.Status == "delivered").Sum(o => (decimal?)o.CommissionAmount) ?? 0;

        // Students
        ViewBag.TotalStudents = _context.Users.Count(u => u.Role == "student");
        var activeStudents = _context.Users.Count(u => u.Role == "student" && u.Status == "active");
        ViewBag.ActiveStudentPercentage = ViewBag.TotalStudents > 0 ? (activeStudents * 100 / ViewBag.TotalStudents) : 0;

        // Avg Rating
        ViewBag.AvgRating = _context.Reviews.Any() ? _context.Reviews.Average(r => r.Rating) : 0.0;

        // Trending Orders (Top 4 foods)
        var trending = _context.OrderItems
            .Include(oi => oi.Food)
            .GroupBy(oi => new { oi.FoodId, oi.Food.Name, oi.Food.ImageUrl })
            .Select(g => new {
                FoodName = g.Key.Name,
                ImageUrl = g.Key.ImageUrl,
                TotalOrdered = g.Sum(oi => oi.Quantity)
            })
            .OrderByDescending(x => x.TotalOrdered)
            .Take(4)
            .ToList();
        
        // Calculate percentages for trending (just for UI, relative to max)
        var maxTrending = trending.Any() ? trending.Max(t => t.TotalOrdered) : 1;
        ViewBag.TrendingOrders = trending.Select(t => new StudentFood.WebAdmin.Models.TrendingOrderDto {
            Name = t.FoodName,
            ImageUrl = t.ImageUrl,
            Percentage = (t.TotalOrdered * 100) / maxTrending
        }).ToList();

        // Revenue Performance (Last 7 days)
        var last7Days = Enumerable.Range(0, 7).Select(i => today.AddDays(-6 + i)).ToList();
        var chartData = new List<decimal>();
        foreach(var day in last7Days)
        {
            var nextDay = day.AddDays(1);
            var rev = _context.Orders
                .Where(o => o.CreatedAt >= day && o.CreatedAt < nextDay && o.Status == "delivered")
                .Sum(o => (decimal?)o.TotalPrice) ?? 0;
            chartData.Add(rev);
        }
        ViewBag.ChartData = chartData;
        ViewBag.ChartLabels = last7Days.Select(d => d.ToString("ddd")).ToList();

        // Recent Feedback
        ViewBag.RecentFeedback = _context.Reviews
            .Include(r => r.Student)
            .Include(r => r.Food)
            .OrderByDescending(r => r.CreatedAt)
            .Take(2)
            .ToList();

        // 5 đơn hàng mới nhất
        var recentOrders = _context.Orders
            .Include(o => o.Student)
            .Include(o => o.Canteen)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .ToList();

        return View(recentOrders);
    }
}
