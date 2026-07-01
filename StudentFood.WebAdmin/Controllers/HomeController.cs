using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudentFood.WebAdmin.Data;
using StudentFood.WebAdmin.Models;

namespace StudentFood.WebAdmin.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        // Truy vấn dữ liệu thực tế từ database
        ViewBag.ActiveStudents = _context.Users.Count(u => u.Role == "student");
        ViewBag.OrdersDelivered = _context.Orders.Count(o => o.Status == "delivered");
        ViewBag.PartnerShops = _context.Canteens.Count();
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
