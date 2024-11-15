using Microsoft.AspNetCore.Mvc;
using Sebezhko.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Sebezhko.Controllers
{
    public class HomeController : Controller
    {
        private AppDbContext _appDbContext;
        private readonly ILogger<HomeController> _logger;


        public HomeController(ILogger<HomeController> logger, AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _appDbContext.Accounts.ToListAsync());
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

        public IActionResult AddToCart()
        {
            int ID = Convert.ToInt32(Request.Query["ID"]);

            Cart cart = new Cart();

            if (HttpContext.Session.Keys.Contains("Cart")) {
                cart = JsonSerializer.Deserialize<Cart>(HttpContext.Session.GetString("Cart"));
            }

            cart.CartLines.Add(_appDbContext.Products.Find(ID));

            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize<Cart>(cart));

            return Redirect("~/Home/Catalog");
        }

        public IActionResult RemoveFromCart()
        {
            int number = Convert.ToInt32(Request.Query["Number"]);

            Cart cart = new Cart();

            if (HttpContext.Session.Keys.Contains("Cart")) 

                cart = JsonSerializer.Deserialize<Cart>(HttpContext.Session.GetString("Cart"));

            cart.CartLines.RemoveAt(number);

            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize<Cart>(cart));

            return Redirect("~/Home/Cart");
        }

        public IActionResult RemoveAllFromCart()
        {
            int ID = Convert.ToInt32(Request.Query["ID"]);

            Cart cart = new Cart();
            if (HttpContext.Session.Keys.Contains("Cart"))
                cart = JsonSerializer.Deserialize<Cart>(HttpContext.Session.GetString("Cart"));
            cart.CartLines.RemoveAll(item => item.ID == ID);
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

            return Redirect("~/Home/Cart");
        }

        public IActionResult Cart()
        {
            Cart cart = new Cart();

            if (HttpContext.Session.Keys.Contains("Cart"))

                cart = JsonSerializer.Deserialize<Cart>(HttpContext.Session.GetString("Cart"));

            return View(cart);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult SignIn()
        {
            if (HttpContext.Session.Keys.Contains("AuthUser"))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                Accounts user = await _appDbContext.Accounts.FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == model.Password);
                if (user != null)
                {
                    HttpContext.Session.SetString("AuthUser", model.Login);
                    HttpContext.Session.SetString("UserRole", user.Role_ID.ToString());
                    await Authenticate(model.Login);
                    return RedirectToAction("Profile", "Home");

                }
                ModelState.AddModelError("", "Некоректный логин или пароль");

            }
            return RedirectToAction("SignIn", "Home");
        }

        private async Task Authenticate(string email)
        {
            var claims = new List<Claim>
             {
                 new Claim(ClaimsIdentity.DefaultNameClaimType, email)
             };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Session.Remove("AuthUser");
            return RedirectToAction("SignIn");
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(Accounts user)
        {
            user.Role_ID = 2;
            if (ModelState.IsValid)
            {
                if (await _appDbContext.Accounts.AnyAsync(u => u.Login == user.Login))
                {
                    ModelState.AddModelError("", "Пользователь с таким email уже существует.");
                    return View(user);
                }
                await _appDbContext.Accounts.AddAsync(user);
                await _appDbContext.SaveChangesAsync();
                return RedirectToAction("SignIn");
            }
            return View(user);
        }

        public async Task<IActionResult> Profile()
        {
            var login = HttpContext.Session.GetString("AuthUser");
            if (login == null)
            {
                return RedirectToAction("SignIn");
            }


            var user = await _appDbContext.Accounts.FirstOrDefaultAsync(u => u.Login == login);

            if (user == null)
            {
                return NotFound();
            }



            var role = await _appDbContext.Roles.FirstOrDefaultAsync(r => r.ID == user.Role_ID);
            Users person = await _appDbContext.Users.FirstOrDefaultAsync(p => p.Account_ID == user.ID);
            if (person == null)
            {
                ViewBag.Name = user.Login;
            }
            else
            {
                ViewBag.Name = person.Name;
            }


            if (role.Role == "admin")
            {
                ViewBag.UserRole = "Администратор";
                return View("Admin");
            }
            else if (role.Role == "manager")
            {
                ViewBag.UserRole = "Менеджер";
                return View("Manager");
            }
            else
            {
                ViewBag.UserRole = "Покупатель";
                return View("Profile");
            }
        }

    }

}
