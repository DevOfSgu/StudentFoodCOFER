using Microsoft.AspNetCore.Mvc;
using StudentFood.WebAdmin.Data;
using StudentFood.WebAdmin.Models;

namespace StudentFood.WebAdmin.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        // Nếu đã đăng nhập thì chuyển đến dashboard
        if (HttpContext.Session.GetInt32("UserId") != null)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            // Lưu thông tin vào session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role);

            // Chuyển hướng theo role
            if (user.Role == "admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else if (user.Role == "canteen")
            {
                return RedirectToAction("Index", "CanteenDashboard");
            }
        }

        ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
        return View();
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
