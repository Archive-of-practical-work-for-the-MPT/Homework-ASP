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
using System.Globalization;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

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
            Cart cart = new Cart();
            if (HttpContext.Session.Keys.Contains("Cart"))
            {
                cart = JsonSerializer.Deserialize<Cart>(HttpContext.Session.GetString("Cart"));
                cart.CartLines.Clear(); // Clear the entire list
                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
            }

            return Redirect("~/Home/Cart");
        }

        public IActionResult Cart()
        {
            Cart cart = new Cart();

            if (HttpContext.Session.Keys.Contains("Cart"))

                cart = JsonSerializer.Deserialize<Cart>(HttpContext.Session.GetString("Cart"));

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrderCart()
        {
            Cart cart = new Cart();

            if (HttpContext.Session.Keys.Contains("Cart"))
                cart = JsonSerializer.Deserialize<Cart>(HttpContext.Session.GetString("Cart"));

            List<Products> products = cart.CartLines.ToList();

            string userIdString = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdString))
            {
                return Redirect("~/Home/SignIn");
            }

            // Создаём новый заказ
            Orders order = new Orders();
            order.Date = DateTime.Now;
            order.User_ID = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            order.Sum = 0; // Пока 0, мы обновим ее после добавления деталей


            await _appDbContext.Orders.AddAsync(order);
            await _appDbContext.SaveChangesAsync(); // Сохраняем заказ чтобы получить его ID


            // Идём по товарам в корзине и создаём детали заказа
            foreach (Products product in products)
            {
                OrdersDetails orderDetail = new OrdersDetails();
                orderDetail.Orders_ID = order.ID; // ID созданного заказа
                orderDetail.Product_ID = product.ID; // ID товара

                await _appDbContext.OrdersDetails.AddAsync(orderDetail);

                order.Sum += product.Price; // Добавляем цену к общей сумме заказа
            }

            await _appDbContext.SaveChangesAsync(); // Сохраняем детали заказа и обновленную сумму заказа
            

            HttpContext.Session.Remove("Cart");
            
            return Redirect("~/Home/Catalog");
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
                HttpContext.Session.SetString("ProfileID", user.ID.ToString());
                HttpContext.Session.SetString("AuthUser", user.ID.ToString());
            }
            else
            {
                ViewBag.User_ID = person.ID;
                ViewBag.ProfileID = user.ID;
                HttpContext.Session.SetString("ProfileID", user.ID.ToString());
                HttpContext.Session.SetString("UserID", person.ID.ToString());
            }


            if (role.Role == "admin")
            {
                ViewBag.UserRole = "Администратор";
                return View("Admin");
            }
            else if (role.Role == "manager")
            {
                var orders = _appDbContext.Orders.ToList();
                var ordersDetails = _appDbContext.OrdersDetails.ToList();

                var ordersByDay = orders
                    .GroupBy(o => o.Date.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        TotalOrders = g.Count()

                    })
                    .ToList();

                var prData = _appDbContext.Products.ToList();

                var ordersByProducts = ordersDetails
                    .GroupBy(o => o.Product_ID)
                    .Select(g => new {
                        Product_ID = g.Key,
                        TotalOrders = g.Count()
                    })
                    .Join(prData,
                        order => order.Product_ID,
                        product => product.ID,
                        (order, product) => new {
                            BookTitle = product.Name_Album,
                            TotalOrders = order.TotalOrders
                        })
                    .ToList();

                ViewBag.OrdersByDay = ordersByDay;
                ViewBag.OrdersByBook = ordersByProducts;

                ViewBag.UserRole = "Менеджер";
                return View("Manager");
            }
            else
            {
                ViewBag.UserRole = "Покупатель";
                return View("Profile");
            }
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            var orders = _appDbContext.Orders.ToList();
            var ordersDetails = _appDbContext.OrdersDetails.ToList();

            var ordersByDay = orders
                .GroupBy(o => o.Date.Date)
                .OrderBy(g => g.Key)
                .Select(g => new {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    TotalOrders = g.Count()

                })
                .ToList();

            var prData = _appDbContext.Products.ToList();

            var ordersByProducts = ordersDetails
                .GroupBy(o => o.Product_ID)
                .Select(g => new {
                    Product_ID = g.Key,
                    TotalOrders = g.Count()
                })
                .Join(prData,
                    order => order.Product_ID,
                    product => product.ID,
                    (order, product) => new {
                        BookTitle = product.Name_Album,
                        TotalOrders = order.TotalOrders
                    })
                .ToList();

            using (var workbook = new ExcelPackage())
            {
                var worksheetDays = workbook.Workbook.Worksheets.Add("Количество заказов по дням");
                worksheetDays.Cells[1, 1].Value = "Date";
                worksheetDays.Cells[1, 2].Value = "Total Orders";
                // Заполняем данными
                worksheetDays.Cells.LoadFromCollection(ordersByDay, false);
                worksheetDays.Cells[1, 1, ordersByDay.Count + 1, 2].AutoFitColumns();

                //Создаем диаграмму
                var chartDays = worksheetDays.Drawings.AddChart("OrdersByDayChart", eChartType.ColumnClustered);
                chartDays.SetPosition(ordersByDay.Count + 3, 0, 0, 0);
                chartDays.SetSize(600, 300);
                chartDays.Title.Text = "Количество заказов по дням";

                // Добавляем данные к графику
                var serieDays = chartDays.Series.Add(worksheetDays.Cells[1, 2, ordersByDay.Count + 1, 2],
                    worksheetDays.Cells[1, 1, ordersByDay.Count + 1, 1]);
                chartDays.Legend.Position = eLegendPosition.Bottom;

                var worksheetProducts = workbook.Workbook.Worksheets.Add("Распределение продаж товаров");
                worksheetProducts.Cells[1, 1].Value = "Vinyl Title";
                worksheetProducts.Cells[1, 2].Value = "Total Orders";
                // Заполняем данными
                worksheetProducts.Cells.LoadFromCollection(ordersByProducts, false);
                worksheetProducts.Cells[1, 1, ordersByProducts.Count + 1, 2].AutoFitColumns();

                //Создаем диаграмму
                var chartBooks = worksheetProducts.Drawings.AddChart("OrdersByProductChart", eChartType.Pie);
                chartBooks.SetPosition(ordersByProducts.Count + 3, 0, 0, 0);
                chartBooks.SetSize(600, 300);
                chartBooks.Title.Text = "Распределение продаж товаров";

                var serieBooks = chartBooks.Series.Add(worksheetProducts.Cells[1, 2, ordersByProducts.Count + 1, 2],
                    worksheetProducts.Cells[1, 1, ordersByProducts.Count + 1, 1]);
                chartBooks.Legend.Position = eLegendPosition.Bottom;

                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "SalesAnalytics.xlsx");
                }
            }
        }
    }

}
