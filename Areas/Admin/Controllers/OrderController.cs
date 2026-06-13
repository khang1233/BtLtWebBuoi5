using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Baitaptuan5.Models;

namespace Baitaptuan5.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Order
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            IQueryable<Order> query = _context.Orders.Include(o => o.ApplicationUser);

            // If the user is NOT Admin or Employee, filter orders by current user's ID
            if (!User.IsInRole("Admin") && !User.IsInRole("Employee"))
            {
                query = query.Where(o => o.UserId == userId);
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        // GET: /Admin/Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Security Check: Customers/Companies can only view their own orders
            if (!User.IsInRole("Admin") && !User.IsInRole("Employee"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (order.UserId != userId)
                {
                    return Forbid();
                }
            }

            return View(order);
        }
    }
}
