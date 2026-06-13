using Baitaptuan5.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Baitaptuan5.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /admin or /admin/dashboard (Admin Dashboard)
        [HttpGet]
        [Route("")]
        [Route("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            // Statistics for sales website
            var totalProducts = await _context.Products.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalUsers = await _userManager.Users.CountAsync();

            // Count products per category
            var productsByCategory = await _context.Products
                .Include(p => p.Category)
                .GroupBy(p => p.Category!.Name)
                .Select(g => new { Category = g.Key ?? "Chưa phân loại", Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);

            // Newest products
            var newestProducts = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalCategories = totalCategories;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.ProductsByCategory = productsByCategory;
            ViewBag.NewestProducts = newestProducts;

            return View();
        }
    }
}

