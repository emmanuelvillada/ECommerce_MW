using ECommerce_MW.DAL;
using ECommerce_MW.DAL.Entities;
using ECommerce_MW.Helpers;
using ECommerce_MW.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ECommerce_MW.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IUserHelper _userHelper;

        public HomeController(DatabaseContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task<IActionResult> Index()
        {
            List<Product>? products = await _context.Products
               .Include(p => p.ProductImages)
               .Include(p => p.ProductCategories)
               .OrderBy(p => p.Description)
               .ToListAsync();

            ViewBag.UserFullName = GetUserFullName();

            //Begins New change
            HomeViewModel homeViewModel = new() { Products = products };

            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user != null)
            {
                homeViewModel.Quantity = await _context.TemporalSales
                    .Where(ts => ts.User.Id == user.Id)
                    .SumAsync(ts => ts.Quantity);
            }

            return View(homeViewModel);
            //Ends New change
        }

        private string GetUserFullName()
        {
            return _context.Users
                 .Where(u => u.Email == User.Identity.Name)
                 .Select(u => u.FullName)
                 .FirstOrDefault();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

		[Route("error/404")]
		public IActionResult Error404()
        {
            return View();
        }

        public async Task<IActionResult> AddProductInKart(Guid? productId)
        {
            if (productId == null) return NotFound();

            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            Product product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null) return NotFound();

            TemporalSale temporalSale = new()
            {
                Product = product,
                Quantity = 1,
                User = user
            };

            _context.TemporalSales.Add(temporalSale);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}