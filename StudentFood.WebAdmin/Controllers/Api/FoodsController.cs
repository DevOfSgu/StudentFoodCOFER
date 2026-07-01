using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFood.WebAdmin.Data;
using StudentFood.WebAdmin.Models;

namespace StudentFood.WebAdmin.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FoodsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Foods
        [HttpGet]
        public async Task<IActionResult> GetFoods()
        {
            var foods = await _context.Foods
                .Select(f => new
                {
                    f.Id,
                    f.Name,
                    f.Description,
                    f.Price,
                    f.ImageUrl,
                    f.IsAvailable,
                    Category = new
                    {
                        f.Category.Id,
                        f.Category.Name
                    },
                    Canteen = new
                    {
                        f.Canteen.Id,
                        f.Canteen.Name,
                        f.Canteen.Status
                    },
                    AverageRating = _context.Reviews
                        .Where(r => r.FoodId == f.Id)
                        .Select(r => (double?)r.Rating)
                        .Average() ?? 0
                })
                .ToListAsync();
            return Ok(foods);
        }

        // GET: api/Foods/Trending
        [HttpGet("Trending")]
        public async Task<IActionResult> GetTrendingFoods()
        {
            var trendingFoodIds = await _context.OrderItems
                .GroupBy(oi => oi.FoodId)
                .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                .Select(g => g.Key)
                .Take(4)
                .ToListAsync();

            var trendingFoods = await _context.Foods
                .Where(f => trendingFoodIds.Contains(f.Id))
                .Select(f => new
                {
                    f.Id,
                    f.Name,
                    f.Description,
                    f.Price,
                    f.ImageUrl,
                    f.IsAvailable,
                    Category = new
                    {
                        f.Category.Id,
                        f.Category.Name
                    },
                    Canteen = new
                    {
                        f.Canteen.Id,
                        f.Canteen.Name,
                        f.Canteen.Status
                    },
                    AverageRating = _context.Reviews
                        .Where(r => r.FoodId == f.Id)
                        .Select(r => (double?)r.Rating)
                        .Average() ?? 0
                })
                .ToListAsync();

            var result = trendingFoodIds
                .Select(id => trendingFoods.FirstOrDefault(f => f.Id == id))
                .Where(f => f is not null)
                .ToList();

            return Ok(result);
        }

        // GET: api/Foods/Recommended
        [HttpGet("Recommended")]
        public async Task<IActionResult> GetRecommendedFoods()
        {
            var recommendedFoodIds = await _context.Reviews
                .GroupBy(r => r.FoodId)
                .OrderByDescending(g => g.Average(r => r.Rating))
                .Select(g => g.Key)
                .Take(4)
                .ToListAsync();

            var recommendedFoods = await _context.Foods
                .Where(f => recommendedFoodIds.Contains(f.Id))
                .Select(f => new
                {
                    f.Id,
                    f.Name,
                    f.Description,
                    f.Price,
                    f.ImageUrl,
                    f.IsAvailable,
                    Category = new
                    {
                        f.Category.Id,
                        f.Category.Name
                    },
                    Canteen = new
                    {
                        f.Canteen.Id,
                        f.Canteen.Name,
                        f.Canteen.Status
                    },
                    AverageRating = _context.Reviews
                        .Where(r => r.FoodId == f.Id)
                        .Select(r => (double?)r.Rating)
                        .Average() ?? 0
                })
                .ToListAsync();

            var result = recommendedFoodIds
                .Select(id => recommendedFoods.FirstOrDefault(f => f.Id == id))
                .Where(f => f is not null)
                .ToList();

            return Ok(result);
        }
    }
}
