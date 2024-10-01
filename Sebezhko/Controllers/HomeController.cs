using Microsoft.AspNetCore.Mvc;
using Sebezhko.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Sebezhko.Controllers
{
    public class HomeController : Controller
    {
        private AppDbContext _appDbContext;

        public HomeController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;

        }
        // public IActionResult About() => View();

        public async Task<IActionResult> Index()
        {
            return View(await _appDbContext.Users.ToListAsync());
        }

        public async Task<IActionResult> Catalog()
        {
            var products = await _appDbContext.Products.ToListAsync();
            return View(products);
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
    }
}