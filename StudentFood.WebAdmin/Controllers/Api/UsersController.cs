using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFood.WebAdmin.Data;
using StudentFood.WebAdmin.DTOs;

namespace StudentFood.WebAdmin.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id);
        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy tài khoản." });
        }

        return Ok(new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserProfileRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == id);
        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy tài khoản." });
        }

        var email = request.Email?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { message = "Vui lòng nhập email/MSSV." });
        }

        var duplicate = await _context.Users.AnyAsync(other => other.Id != id && other.Email == email);
        if (duplicate)
        {
            return BadRequest(new { message = "Email/MSSV này đã được sử dụng." });
        }

        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.PhoneNumber = request.PhoneNumber?.Trim();

        await _context.SaveChangesAsync();

        return Ok(new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty
        });
    }
}