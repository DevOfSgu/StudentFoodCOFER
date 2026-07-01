using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentFood.WebAdmin.Data;
using StudentFood.WebAdmin.Models;

namespace StudentFood.WebAdmin.Controllers
{
    public class CanteensController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CanteensController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Canteens
        public async Task<IActionResult> Index()
        {
            var canteens = await _context.Canteens.Include(c => c.Owner).ToListAsync();

            var statsMap = await _context.Orders
                .Where(o => o.Status == "delivered")
                .GroupBy(o => o.CanteenId)
                .Select(g => new
                {
                    CanteenId = g.Key,
                    TotalOrders = g.Count(),
                    TotalRevenue = g.Sum(o => o.Subtotal - o.CommissionAmount),
                    TotalCommission = g.Sum(o => o.CommissionAmount)
                })
                .ToDictionaryAsync(x => x.CanteenId, x => new CanteenStatsDto
                {
                    TotalOrders = x.TotalOrders,
                    TotalRevenue = x.TotalRevenue,
                    TotalCommission = x.TotalCommission
                });

            ViewBag.StatsMap = statsMap;
            return View(canteens);
        }

        // GET: Canteens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var canteen = await _context.Canteens
                .Include(c => c.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (canteen == null)
            {
                return NotFound();
            }

            return View(canteen);
        }

        // GET: Canteens/Create
        public IActionResult Create()
        {
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "FullName");
            return View();
        }

        // POST: Canteens/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OwnerId,Name,Description,Status,CommissionRate")] Canteen canteen)
        {
            if (ModelState.IsValid)
            {
                _context.Add(canteen);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "FullName", canteen.OwnerId);
            return View(canteen);
        }

        // GET: Canteens/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var canteen = await _context.Canteens.FindAsync(id);
            if (canteen == null)
            {
                return NotFound();
            }
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "FullName", canteen.OwnerId);
            return View(canteen);
        }

        // POST: Canteens/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OwnerId,Name,Description,Status,CommissionRate")] Canteen canteen)
        {
            if (id != canteen.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(canteen);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CanteenExists(canteen.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "FullName", canteen.OwnerId);
            return View(canteen);
        }

        // GET: Canteens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var canteen = await _context.Canteens
                .Include(c => c.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (canteen == null)
            {
                return NotFound();
            }

            return View(canteen);
        }

        // POST: Canteens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var canteen = await _context.Canteens.FindAsync(id);
            if (canteen != null)
            {
                _context.Canteens.Remove(canteen);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CanteenExists(int id)
        {
            return _context.Canteens.Any(e => e.Id == id);
        }
    }

    public class CanteenStatsDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommission { get; set; }
    }
}
