using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baitaptuan5.Models
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Ensure database is created/migrated
            await context.Database.MigrateAsync();

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Seed Roles (removed Student role)
            string[] roleNames = { SD.Role_Admin, SD.Role_Customer, SD.Role_Company, SD.Role_Employee };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Seed Custom Users
            var usersToSeed = new[]
            {
                new { Email = "admin@gmail.com", Role = SD.Role_Admin, Pass = "Admin@123", Name = "Admin User" },
                new { Email = "customer@gmail.com", Role = SD.Role_Customer, Pass = "Customer@123", Name = "Customer User" },
                new { Email = "employee@gmail.com", Role = SD.Role_Employee, Pass = "Employee@123", Name = "Employee User" },
                new { Email = "employee@gmai.com", Role = SD.Role_Employee, Pass = "Employee@123", Name = "Employee User Alt" },
                new { Email = "company@gmail.com", Role = SD.Role_Company, Pass = "Company@123", Name = "Company User" }
            };

            foreach (var u in usersToSeed)
            {
                var existingUser = await userManager.FindByEmailAsync(u.Email);
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = u.Email,
                        Email = u.Email,
                        EmailConfirmed = true,
                        FullName = u.Name,
                        Address = "Mặc định",
                        Age = "25"
                    };
                    var createResult = await userManager.CreateAsync(user, u.Pass);
                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, u.Role);
                    }
                }
            }

            // Legacy admin
            var adminUser = await userManager.FindByEmailAsync("admin@shop.com");
            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@shop.com",
                    Email = "admin@shop.com",
                    EmailConfirmed = true,
                    FullName = "Admin Quản Trị Viên",
                    Address = "123 Đường Ba Tháng Hai, Quận 10, TP.HCM",
                    Age = "30"
                };
                var createPowerUser = await userManager.CreateAsync(admin, "Admin@123");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, SD.Role_Admin);
                }
            }

            // Detect and clear corrupted/garbled seed data from previous runs if any
            var hasCorruptedData = await context.Categories.AnyAsync(c => c.Name.Contains("Ä"));
            if (hasCorruptedData)
            {
                context.OrderDetails.RemoveRange(context.OrderDetails);
                context.Orders.RemoveRange(context.Orders);
                context.Products.RemoveRange(context.Products);
                context.Categories.RemoveRange(context.Categories);
                await context.SaveChangesAsync();
            }

            // 3. Seed Categories
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Điện thoại & Tablet" },
                    new Category { Name = "Laptop & Thiết bị IT" },
                    new Category { Name = "Phụ kiện công nghệ" },
                    new Category { Name = "Sách chuyên ngành" }
                };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // 4. Seed Products (10 items for pagination testing)
            if (!await context.Products.AnyAsync())
            {
                var phoneCategory = await context.Categories.FirstAsync(c => c.Name == "Điện thoại & Tablet");
                var laptopCategory = await context.Categories.FirstAsync(c => c.Name == "Laptop & Thiết bị IT");
                var accessoryCategory = await context.Categories.FirstAsync(c => c.Name == "Phụ kiện công nghệ");
                var bookCategory = await context.Categories.FirstAsync(c => c.Name == "Sách chuyên ngành");

                var products = new List<Product>
                {
                    // Category: Điện thoại & Tablet (3)
                    new Product
                    {
                        Name = "iPhone 15 Pro Max 256GB",
                        Price = 29990000m,
                        Description = "Điện thoại di động iPhone 15 Pro Max chính hãng Apple Việt Nam với khung viền Titanium siêu bền.",
                        ImageUrl = "https://images.unsplash.com/photo-1695048133142-1a20484d2569?w=500&auto=format&fit=crop&q=60",
                        CategoryId = phoneCategory.Id
                    },
                    new Product
                    {
                        Name = "Samsung Galaxy S24 Ultra",
                        Price = 27490000m,
                        Description = "Flagship mới nhất từ Samsung tích hợp bút S-Pen thông minh và công nghệ Galaxy AI tiên tiến.",
                        ImageUrl = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=500&auto=format&fit=crop&q=60",
                        CategoryId = phoneCategory.Id
                    },
                    new Product
                    {
                        Name = "iPad Air 5 M1 Wi-Fi",
                        Price = 14990000m,
                        Description = "Máy tính bảng iPad Air thế hệ 5 trang bị vi xử lý Apple M1 mạnh mẽ đáp ứng tốt nhu cầu học tập và đồ họa.",
                        ImageUrl = "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?w=500&auto=format&fit=crop&q=60",
                        CategoryId = phoneCategory.Id
                    },

                    // Category: Laptop & Thiết bị IT (3)
                    new Product
                    {
                        Name = "MacBook Pro 14 inch M3 Pro",
                        Price = 49990000m,
                        Description = "Laptop cao cấp Apple MacBook Pro 14 với chip M3 Pro, 18GB RAM và 512GB SSD màu Space Black.",
                        ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=500&auto=format&fit=crop&q=60",
                        CategoryId = laptopCategory.Id
                    },
                    new Product
                    {
                        Name = "Dell XPS 15 9530",
                        Price = 42990000m,
                        Description = "Ultrabook cao cấp dành cho creator trang bị màn hình OLED rực rỡ và vi xử lý Intel Core i7 thế hệ mới.",
                        ImageUrl = "https://images.unsplash.com/photo-1593642632823-8f785ba67e45?w=500&auto=format&fit=crop&q=60",
                        CategoryId = laptopCategory.Id
                    },
                    new Product
                    {
                        Name = "ASUS ROG Zephyrus G14",
                        Price = 35990000m,
                        Description = "Laptop gaming mỏng nhẹ hàng đầu trang bị CPU AMD Ryzen 9 và GPU NVIDIA RTX 4060 cực mạnh mẽ.",
                        ImageUrl = "https://images.unsplash.com/photo-1603302576837-37561b2e2302?w=500&auto=format&fit=crop&q=60",
                        CategoryId = laptopCategory.Id
                    },

                    // Category: Phụ kiện công nghệ (2)
                    new Product
                    {
                        Name = "Tai nghe Apple AirPods Pro 2 USB-C",
                        Price = 5790000m,
                        Description = "Tai nghe True Wireless cao cấp của Apple hỗ trợ chống ồn chủ động ANC thế hệ mới.",
                        ImageUrl = "https://images.unsplash.com/photo-1600294037681-c80b4cb5b434?w=500&auto=format&fit=crop&q=60",
                        CategoryId = accessoryCategory.Id
                    },
                    new Product
                    {
                        Name = "Chuột không dây Logitech MX Master 3S",
                        Price = 2490000m,
                        Description = "Chuột công thái học cao cấp dành cho văn phòng và lập trình viên với nút cuộn MagSpeed siêu tốc.",
                        ImageUrl = "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?w=500&auto=format&fit=crop&q=60",
                        CategoryId = accessoryCategory.Id
                    },

                    // Category: Sách chuyên ngành (2)
                    new Product
                    {
                        Name = "Sách Lập Trình C# 12 và .NET 8",
                        Price = 250000m,
                        Description = "Giáo trình hướng dẫn chi tiết về cú pháp lập trình C# 12 và xây dựng ứng dụng với .NET 8.",
                        ImageUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=500&auto=format&fit=crop&q=60",
                        CategoryId = bookCategory.Id
                    },
                    new Product
                    {
                        Name = "Sách Thiết Kế Hệ Thống Microservices",
                        Price = 320000m,
                        Description = "Cuốn sách cung cấp kiến thức nền tảng và nâng cao về kiến trúc hệ thống dịch vụ nhỏ (Microservices).",
                        ImageUrl = "https://images.unsplash.com/photo-1532012197267-da84d127e765?w=500&auto=format&fit=crop&q=60",
                        CategoryId = bookCategory.Id
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }
    }
}
