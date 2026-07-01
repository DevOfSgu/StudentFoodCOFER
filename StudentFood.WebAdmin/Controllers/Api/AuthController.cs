using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentFood.WebAdmin.Data;
using StudentFood.WebAdmin.DTOs;
using StudentFood.WebAdmin.Models;

namespace StudentFood.WebAdmin.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Auth/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra xem Email đã tồn tại chưa
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email này đã được đăng ký!" });
            }

            // Mã hóa mật khẩu
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Username = request.Email,
                Password = passwordHash,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Role = "student", // Mặc định đăng ký từ App là sinh viên
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Trả về thông tin user (ẩn mật khẩu)
            return Ok(new
            {
                message = "Đăng ký thành công!",
                user = new
                {
                    newUser.Id,
                    newUser.Username,
                    newUser.FullName,
                    newUser.Role,
                    newUser.PhoneNumber
                }
            });
        }

        // POST: api/Auth/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Tìm user theo email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Email);
            
            // Kiểm tra user có tồn tại và mật khẩu có khớp không
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác!" });
            }

            if (user.Status != "active")
            {
                return StatusCode(403, new { message = "Tài khoản của bạn đã bị khóa!" });
            }

            // Đăng nhập thành công, trả về thông tin (Thực tế nên trả về JWT Token)
            return Ok(new
            {
                message = "Đăng nhập thành công!",
                user = new
                {
                    user.Id,
                    user.Username,
                    user.FullName,
                    user.Role,
                    user.AvatarUrl,
                    user.PhoneNumber
                },
                // Giả lập token tạm thời (Bạn có thể thêm thư viện JWT sau nếu cần)
                token = "fake-jwt-token-" + user.Id
            });
        }
    }
}