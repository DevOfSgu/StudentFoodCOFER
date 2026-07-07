using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentFood.WebAdmin.Data;
using StudentFood.WebAdmin.Models;

namespace StudentFood.WebAdmin.Controllers;

public class CanteenDashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public CanteenDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    private IActionResult? EnsureCanteenAccess(out Canteen canteen)
    {
        canteen = null;
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null || HttpContext.Session.GetString("UserRole") != "canteen")
        {
            return RedirectToAction("Login", "Account");
        }

        canteen = _context.Canteens.FirstOrDefault(c => c.OwnerId == userId);
        if (canteen == null)
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.CanteenName = canteen.Name;
        return null;
    }

    public IActionResult Index()
    {
        var redirect = EnsureCanteenAccess(out var canteen);
        if (redirect != null) return redirect;

        var today = DateTime.UtcNow.Date;

        // Thống kê dữ liệu của căn tin này (Chỉ tính trong ngày hôm nay)
        ViewBag.OrdersToday = _context.Orders.Count(o => o.CanteenId == canteen!.Id && o.CreatedAt >= today);
        ViewBag.PreparingOrders = _context.Orders.Count(o => o.CanteenId == canteen.Id && o.CreatedAt >= today && o.Status == "preparing");
        ViewBag.CompletedOrders = _context.Orders.Count(o => o.CanteenId == canteen.Id && o.CreatedAt >= today && o.Status == "delivered");
        ViewBag.CancelledOrders = _context.Orders.Count(o => o.CanteenId == canteen.Id && o.CreatedAt >= today && o.Status == "cancelled");
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
    public IActionResult CancelOrder(int orderId, string cancelReason)
    {
        var redirect = EnsureCanteenAccess(out var canteen);
        if (redirect != null) return Unauthorized();

        var order = _context.Orders.FirstOrDefault(o => o.Id == orderId && o.CanteenId == canteen!.Id);
        if (order != null && order.Status == "pending")
        {
            order.Status = "cancelled";
            order.CancelReason = cancelReason ?? "Không có lý do";
            order.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult UpdateOrderStatus(int orderId, string status)
    {
        var redirect = EnsureCanteenAccess(out var canteen);
        if (redirect != null) return Unauthorized();

        var order = _context.Orders.FirstOrDefault(o => o.Id == orderId && o.CanteenId == canteen!.Id);
        if (order != null)
        {
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    // GET: CanteenDashboard/Foods
    public async Task<IActionResult> Foods()
    {
        var redirect = EnsureCanteenAccess(out var canteen);
        if (redirect != null) return redirect;

        var foods = await _context.Foods
            .Include(f => f.Category)
            .Where(f => f.CanteenId == canteen!.Id)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return View(foods);
    }

    // GET: CanteenDashboard/CreateFood
    public IActionResult CreateFood()
    {
        var redirect = EnsureCanteenAccess(out _);
        if (redirect != null) return redirect;

        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
        return View();
    }

    // POST: CanteenDashboard/CreateFood
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFood([Bind("CategoryId,Name,Description,Price,ImageUrl,IsAvailable")] Food food, IFormFile? imageFile)
    {
        var redirect = EnsureCanteenAccess(out var canteen);
        if (redirect != null) return redirect;

        if (ModelState.IsValid)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                food.ImageUrl = await SaveFoodImageAsync(imageFile);
            }

            food.CanteenId = canteen!.Id;
            food.CreatedAt = DateTime.UtcNow;
            _context.Add(food);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Foods));
        }
        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", food.CategoryId);
        return View(food);
    }

    // GET: CanteenDashboard/EditFood/5
    public async Task<IActionResult> EditFood(int? id)
    {
        var redirect = EnsureCanteenAccess(out var canteen);
        if (redirect != null) return redirect;

        if (id == null) return NotFound();

        var food = await _context.Foods.FirstOrDefaultAsync(f => f.Id == id && f.CanteenId == canteen!.Id);
        if (food == null) return NotFound();

        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", food.CategoryId);
        return View(food);
    }

    // POST: CanteenDashboard/EditFood/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFood(int id, [Bind("Id,CategoryId,Name,Description,Price,ImageUrl,IsAvailable,CreatedAt")] Food food, IFormFile? imageFile)
    {
        var redirect = EnsureCanteenAccess(out var canteen);
        if (redirect != null) return redirect;

        if (id != food.Id) return NotFound();

        // Ensure the food belongs to this canteen
        var existing = await _context.Foods.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id && f.CanteenId == canteen!.Id);
        if (existing == null) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    food.ImageUrl = await SaveFoodImageAsync(imageFile);
                }
                else
                {
                    food.ImageUrl = existing.ImageUrl;
                }

                food.CanteenId = canteen!.Id;
                _context.Update(food);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Foods.Any(e => e.Id == food.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Foods));
        }
        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", food.CategoryId);
        return View(food);
    }

    // GET: CanteenDashboard/DeleteFood/5
    public async Task<IActionResult> DeleteFood(int? id)
    {
        var redirect = EnsureCanteenAccess(out var canteen);
        if (redirect != null) return redirect;

        if (id == null) return NotFound();

        var food = await _context.Foods
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id && f.CanteenId == canteen!.Id);
        if (food == null) return NotFound();

        return View(food);
    }

    // GET: CanteenDashboard/Reviews
    public async Task<IActionResult> Reviews()
    {
        var redirect = EnsureCanteenAccess(out var canteen);
        if (redirect != null) return redirect;

        var reviews = await _context.Reviews
            .Include(r => r.Food)
            .Include(r => r.Student)
            .Include(r => r.Order)
            .Where(r => r.Food.CanteenId == canteen!.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(reviews);
    }

    // POST: CanteenDashboard/DeleteFood/5
    [HttpPost, ActionName("DeleteFood")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFoodConfirmed(int id)
    {
        var redirect = EnsureCanteenAccess(out var canteen);
        if (redirect != null) return redirect;

        var food = await _context.Foods.FirstOrDefaultAsync(f => f.Id == id && f.CanteenId == canteen!.Id);
        if (food != null)
        {
            _context.Foods.Remove(food);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Foods));
    }

    private async Task<string?> SaveFoodImageAsync(IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
            return null;

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "foods");
        Directory.CreateDirectory(uploadsDir);

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }

        return "/uploads/foods/" + fileName;
    }
}
