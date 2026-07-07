using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using StudentFood.WebAdmin.Models;

namespace StudentFood.WebAdmin.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            // context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // 1. Seed Users if not already seeded
            if (!context.Users.Any(u => u.Email == "admin@studentfood.com"))
            {
                context.Users.Add(new User
                {
                    Email = "admin@studentfood.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                    FullName = "Hệ thống Admin",
                    Role = "admin",
                    PhoneNumber = "0987654321",
                    AvatarUrl = "/img/avatar_admin.png",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (!context.Users.Any(u => u.Email == "canteen@studentfood.com"))
            {
                context.Users.Add(new User
                {
                    Email = "canteen@studentfood.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                    FullName = "Nhân viên Căn tin",
                    Role = "canteen",
                    PhoneNumber = "0912345678",
                    AvatarUrl = "/img/avatar_canteen.png",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Seed 25 Students if no student users exist
            if (!context.Users.Any(u => u.Role == "student"))
            {
                string[] studentNames = new[] 
                {
                    "Nguyễn Văn An", "Trần Thị Bình", "Lê Hoàng Nam", "Phạm Minh Thư", "Hoàng Đức Hải",
                    "Vũ Thanh Hằng", "Phan Anh Tuấn", "Đỗ Ngọc Lan", "Bùi Quốc Khánh", "Ngô Minh Châu",
                    "Đặng Huy Hoàng", "Lý Thu Thảo", "Dương Tấn Đạt", "Lâm Mỹ Lệ", "Võ Hữu Phước",
                    "Đào Minh Quân", "Mai Trúc Quỳnh", "Đinh Hữu Lộc", "Trịnh Thiên Kim", "Phùng Thế Vinh",
                    "Lương Gia Bảo", "Diệp Mỹ Tâm", "Thái Tuấn Kiệt", "Quách Ái Vy", "Tống Gia Huy"
                };

                for (int i = 0; i < studentNames.Length; i++)
                {
                    context.Users.Add(new User
                    {
                        Email = $"student{(i + 1):00}@studentfood.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                        FullName = studentNames[i],
                        Role = "student",
                        PhoneNumber = $"09000000{i:00}",
                        AvatarUrl = $"/img/student{(i + 1)}.png",
                        Status = "active",
                        CreatedAt = DateTime.UtcNow.AddDays(-i)
                    });
                }
            }
            context.SaveChanges();

            // Get the canteen staff user
            var canteenStaff = context.Users.FirstOrDefault(u => u.Role == "canteen");
            if (canteenStaff == null)
            {
                canteenStaff = new User
                {
                    Email = "canteen_fallback@studentfood.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                    FullName = "Nhân viên Căn tin Fallback",
                    Role = "canteen",
                    PhoneNumber = "0912345678",
                    AvatarUrl = "/img/avatar_canteen.png",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(canteenStaff);
                context.SaveChanges();
            }

            // 2. Seed unique Canteen Users and Canteens
            var canteenConfigs = new[]
            {
                new { Email = "canteenA@studentfood.com", Name = "Căn tin A - Khu Trung tâm", Desc = "Phục vụ cơm trưa văn phòng, sinh viên và đồ uống", Rate = 0.1, Status = "open" },
                new { Email = "canteenB@studentfood.com", Name = "Căn tin B - Nhà B11", Desc = "Phục vụ thức ăn nhanh, đồ ăn sáng và ăn nhẹ", Rate = 0.1, Status = "open" },
                new { Email = "canteenC@studentfood.com", Name = "Căn tin C - Thư viện", Desc = "Không gian yên tĩnh phục vụ cafe và bánh ngọt", Rate = 0.08, Status = "open" },
                new { Email = "canteen_miquang@studentfood.com", Name = "Mì Quảng & Phở Việt", Desc = "Món nước truyền thống đậm đà bản sắc Việt", Rate = 0.1, Status = "open" },
                new { Email = "canteen_korean@studentfood.com", Name = "Korean Food Station", Desc = "Cơm cuộn, mì cay và tokbokki nóng hổi", Rate = 0.1, Status = "open" },
                new { Email = "canteen_sushi@studentfood.com", Name = "Sushi & Sashimi Corner", Desc = "Ẩm thực Nhật Bản tươi ngon mỗi ngày", Rate = 0.12, Status = "open" },
                new { Email = "canteen_juice@studentfood.com", Name = "Juice & Bakery Hub", Desc = "Nước ép trái cây tươi mát và bánh mì ngọt", Rate = 0.08, Status = "open" },
                new { Email = "canteen_rice@studentfood.com", Name = "Rice Bowl Spot", Desc = "Cơm thố và cơm đĩa nóng sốt tự chọn", Rate = 0.1, Status = "open" },
                new { Email = "canteen_vegan@studentfood.com", Name = "Vegan Garden", Desc = "Món ăn chay thanh đạm, lành mạnh cho sức khỏe", Rate = 0.05, Status = "open" },
                new { Email = "canteen_seafood@studentfood.com", Name = "Hải Sản Sinh Viên", Desc = "Bún hải sản, mì xào hải sản chất lượng giá rẻ", Rate = 0.1, Status = "open" },
                new { Email = "canteen_chicken@studentfood.com", Name = "Tiệm Gà Rán COFER", Desc = "Gà rán giòn rụm, khoai tây chiên và nước ngọt", Rate = 0.1, Status = "open" },
                new { Email = "canteen_hotpot@studentfood.com", Name = "Lẩu Mini Tự Chọn", Desc = "Lẩu ly, lẩu một người tiện lợi và thơm ngon", Rate = 0.1, Status = "open" }
            };

            foreach (var config in canteenConfigs)
            {
                // Create owner user if not exists
                var owner = context.Users.FirstOrDefault(u => u.Email == config.Email);
                if (owner == null)
                {
                    owner = new User
                    {
                        Email = config.Email,
                        Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                        FullName = $"Chủ {config.Name}",
                        Role = "canteen",
                        PhoneNumber = "0912345678",
                        AvatarUrl = "/img/avatar_canteen.png",
                        Status = "active",
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Users.Add(owner);
                    context.SaveChanges(); // Save to get Id
                }

                // Check if canteen exists
                var existingCanteen = context.Canteens.FirstOrDefault(c => c.Name == config.Name);
                if (existingCanteen == null)
                {
                    var canteen = new Canteen
                    {
                        Name = config.Name,
                        Description = config.Desc,
                        OwnerId = owner.Id,
                        Status = config.Status,
                        CommissionRate = config.Rate
                    };
                    context.Canteens.Add(canteen);
                }
                else
                {
                    // Update OwnerId to the new individual user if it was previously set to fallback/shared staff
                    existingCanteen.OwnerId = owner.Id;
                }
            }
            context.SaveChanges();

            // 3. Seed Categories
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Cơm", IconUrl = "rice" },
                    new Category { Name = "Mì / Phở", IconUrl = "noodles" },
                    new Category { Name = "Thức uống", IconUrl = "drinks" },
                    new Category { Name = "Ăn vặt", IconUrl = "snacks" },
                    new Category { Name = "Tráng miệng", IconUrl = "desserts" }
                };

                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            // Get categories
            var categoryRice = context.Categories.First(c => c.Name == "Cơm");
            var categoryNoodles = context.Categories.First(c => c.Name == "Mì / Phở");
            var categoryDrinks = context.Categories.First(c => c.Name == "Thức uống");
            var categorySnacks = context.Categories.First(c => c.Name == "Ăn vặt");
            var categoryDesserts = context.Categories.First(c => c.Name == "Tráng miệng");

            // Get canteens
            var canteenList = context.Canteens.ToList();

            // 4. Seed Food Items
            if (!context.Foods.Any())
            {
                var foods = new List<Food>
                {
                    // Canteen A
                    new Food { Name = "Cơm sườn bì chả", Description = "Sườn nướng mật ong thơm ngon kèm bì thính và chả chưng trứng", Price = 30000, CategoryId = categoryRice.Id, CanteenId = canteenList[0].Id, IsAvailable = true, ImageUrl = "/foods/com-suon-bi-cha.png" },
                    new Food { Name = "Cơm gà xối mỡ", Description = "Đùi gà chiên giòn rụm kèm cơm chiên tỏi", Price = 35000, CategoryId = categoryRice.Id, CanteenId = canteenList[0].Id, IsAvailable = true, ImageUrl = "/foods/com-ga-xoi-mo.png" },
                    new Food { Name = "Trà sữa thái xanh", Description = "Trà sữa béo ngậy kèm thạch rau câu giòn", Price = 18000, CategoryId = categoryDrinks.Id, CanteenId = canteenList[0].Id, IsAvailable = true, ImageUrl = "/foods/tra-sua-thai-xanh.png" },
                    
                    // Canteen B (Thức ăn nhanh)
                    new Food { Name = "Hamburger gà giòn", Description = "Bánh mì burger kẹp phi lê gà rán giòn và sốt mayonnaise", Price = 25000, CategoryId = categorySnacks.Id, CanteenId = canteenList[1].Id, IsAvailable = true, ImageUrl = "/foods/hamburger-ga-gion.png" },
                    new Food { Name = "Khoai tây chiên lắc phô mai", Description = "Khoai tây chiên giòn rụm lắc bột phô mai mặn ngọt", Price = 15000, CategoryId = categorySnacks.Id, CanteenId = canteenList[1].Id, IsAvailable = true, ImageUrl = "/foods/khoai-tay-chien-pho-mai.jpg" },

                    // Canteen C (Cafe & Bánh)
                    new Food { Name = "Cà phê sữa đá", Description = "Cà phê pha phin truyền thống thơm nồng kết hợp sữa đặc", Price = 12000, CategoryId = categoryDrinks.Id, CanteenId = canteenList[2].Id, IsAvailable = true, ImageUrl = "/foods/ca-phe-sua-da.jpg" },
                    new Food { Name = "Bánh Croissant bơ tỏi", Description = "Bánh sừng bò ngập hương vị bơ tỏi nướng giòn", Price = 20000, CategoryId = categoryDesserts.Id, CanteenId = canteenList[2].Id, IsAvailable = true, ImageUrl = "/foods/croissaint-bo-toi.jpg" },

                    // Mì & Phở
                    new Food { Name = "Mì Quảng gà ta", Description = "Mì quảng tôm thịt gà, ăn kèm bánh tráng nướng và rau sống", Price = 32000, CategoryId = categoryNoodles.Id, CanteenId = canteenList[3].Id, IsAvailable = true, ImageUrl = "/foods/mi-quang-ga-ta.jpg" },
                    new Food { Name = "Phở bò tái nạm", Description = "Bánh phở mềm dai nước dùng hầm xương bò ngọt thanh tự nhiên", Price = 35000, CategoryId = categoryNoodles.Id, CanteenId = canteenList[3].Id, IsAvailable = true, ImageUrl = "/foods/pho-bo-tai-nam.jpg" },

                    // Korean Station
                    new Food { Name = "Mì Cay Samyang", Description = "Mì cay siêu hot kèm xúc xích, phô mai và trứng lòng đào", Price = 28000, CategoryId = categoryNoodles.Id, CanteenId = canteenList[4].Id, IsAvailable = true, ImageUrl = "/foods/mi-cay-samyang.jpg" },
                    new Food { Name = "Kimbap truyền thống", Description = "Cơm cuộn rong biển nhân trứng, xúc xích, củ cải vàng và dưa leo", Price = 22000, CategoryId = categoryRice.Id, CanteenId = canteenList[4].Id, IsAvailable = true, ImageUrl = "/foods/kimbap-truyen-thong.jpg" }
                };

                context.Foods.AddRange(foods);
                context.SaveChanges();
            }
            else
            {
                // Cập nhật ImageUrl cho các món ăn đã có sẵn nếu đang bị null
                var existingFoods = context.Foods.Where(f => string.IsNullOrEmpty(f.ImageUrl)).ToList();
                if (existingFoods.Any())
                {
                    var imageMapping = new Dictionary<string, string>
                    {
                        { "Cơm sườn bì chả", "/foods/com-suon-bi-cha.png" },
                        { "Cơm gà xối mỡ", "/foods/com-ga-xoi-mo.png" },
                        { "Trà sữa thái xanh", "/foods/tra-sua-thai-xanh.png" },
                        { "Hamburger gà giòn", "/foods/hamburger-ga-gion.png" },
                        { "Khoai tây chiên lắc phô mai", "/foods/khoai-tay-chien-pho-mai.jpg" },
                        { "Cà phê sữa đá", "/foods/ca-phe-sua-da.jpg" },
                        { "Bánh Croissant bơ tỏi", "/foods/croissaint-bo-toi.jpg" },
                        { "Mì Quảng gà ta", "/foods/mi-quang-ga-ta.jpg" },
                        { "Phở bò tái nạm", "/foods/pho-bo-tai-nam.jpg" },
                        { "Mì Cay Samyang", "/foods/mi-cay-samyang.jpg" },
                        { "Kimbap truyền thống", "/foods/kimbap-truyen-thong.jpg" }
                    };

                    foreach (var food in existingFoods)
                    {
                        if (imageMapping.ContainsKey(food.Name))
                        {
                            food.ImageUrl = imageMapping[food.Name];
                        }
                    }
                    context.SaveChanges();
                }
            }

            // 5. Seed Orders (with different statuses to make stats look real and rich)
            if (!context.Orders.Any())
            {
                var random = new Random();
                var students = context.Users.Where(u => u.Role == "student").ToList();
                var foods = context.Foods.ToList();

                var orders = new List<Order>();

                // We want a solid number of orders. Let's create 120 delivered orders and some pending/preparing ones
                string[] statuses = new[] { "delivered", "delivered", "delivered", "delivered", "pending", "preparing", "ready", "cancelled" };

                // Seed 150 orders
                for (int i = 0; i < 150; i++)
                {
                    var student = students[random.Next(students.Count)];
                    var food = foods[random.Next(foods.Count)];
                    var qty = random.Next(1, 3);
                    var price = food.Price;
                    var subtotal = price * qty;
                    var deliveryFee = 5000;
                    var commissionRate = 0.1;
                    var commission = subtotal * (decimal)commissionRate;
                    var total = subtotal + deliveryFee;

                    var status = (i < 122) ? "delivered" : statuses[random.Next(statuses.Length)];

                    var order = new Order
                    {
                        StudentId = student.Id,
                        CanteenId = food.CanteenId,
                        Status = status,
                        DeliveryAddress = $"Ký túc xá Phòng {random.Next(101, 509)} - Dãy nhà {((char)random.Next('A', 'E'))}",
                        PaymentMethod = random.Next(2) == 0 ? "cash" : "transfer",
                        Subtotal = subtotal,
                        DeliveryFee = deliveryFee,
                        CommissionAmount = commission,
                        TotalPrice = total,
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)).AddMinutes(-random.Next(1, 1440)),
                        UpdatedAt = DateTime.UtcNow
                    };

                    order.OrderItems = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            FoodId = food.Id,
                            Quantity = qty,
                            PriceAtOrder = price
                        }
                    };

                    orders.Add(order);
                }

                context.Orders.AddRange(orders);
                context.SaveChanges();

                // Add some reviews for these delivered orders
                var deliveredOrders = context.Orders.Include(o => o.OrderItems).Where(o => o.Status == "delivered").Take(30).ToList();
                var reviews = new List<Review>();
                string[] comments = new[] 
                {
                    "Món ăn rất ngon, giao hàng nhanh chóng!",
                    "Giao hàng đúng giờ, đồ ăn còn nóng hổi.",
                    "Hương vị đậm đà, rất vừa miệng, sẽ ủng hộ tiếp.",
                    "Giá cả hợp lý cho sinh viên, đóng gói sạch sẽ.",
                    "Nước dùng phở ngọt thanh, sườn nướng rất thơm."
                };

                foreach (var order in deliveredOrders)
                {
                    var item = order.OrderItems.First();
                    reviews.Add(new Review
                    {
                        OrderId = order.Id,
                        StudentId = order.StudentId,
                        FoodId = item.FoodId,
                        Rating = random.Next(4, 6), // 4 or 5 stars
                        Comment = comments[random.Next(comments.Length)],
                        CreatedAt = order.CreatedAt.AddMinutes(random.Next(30, 120))
                    });
                }
                context.Reviews.AddRange(reviews);
                context.SaveChanges();
            }
        }
    }
}
