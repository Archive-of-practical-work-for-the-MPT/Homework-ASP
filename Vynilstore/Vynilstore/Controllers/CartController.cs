using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ApiVynil.Models;
using Microsoft.Extensions.Logging;
using Vynilstore.Models;
using Vynilstore.Services;
using Microsoft.AspNetCore.Authorization;

namespace Vynilstore.Controllers
{
    public class CartController : Controller
    {
        private readonly ILogger<CartController> _logger;
        private readonly HttpClient _httpClient;
        private readonly CartService _cartService;
        private readonly string _apiBaseUrl = "https://localhost:7119";

        public CartController(ILogger<CartController> logger, IHttpClientFactory httpClientFactory, CartService cartService)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(_apiBaseUrl);
            _cartService = cartService;
        }

        // Отображение корзины
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            return View(cart);
        }

        // Добавление товара в корзину
        [HttpPost]
        public async Task<IActionResult> AddToCart(int vinylId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                quantity = 1;
            }

            try
            {
                var response = await _httpClient.GetAsync($"/api/Vinyls/{vinylId}");
                if (response.IsSuccessStatusCode)
                {
                    var vinyl = await response.Content.ReadFromJsonAsync<Vinyl>();
                    if (vinyl != null)
                    {
                        // Получаем информацию об исполнителе для отображения в корзине
                        var artistResponse = await _httpClient.GetAsync($"/api/Artists/{vinyl.ArtistId}");
                        string artistName = "Неизвестный исполнитель";
                        
                        if (artistResponse.IsSuccessStatusCode)
                        {
                            var artist = await artistResponse.Content.ReadFromJsonAsync<Artist>();
                            artistName = artist?.Name ?? artistName;
                        }

                        var cart = _cartService.GetCart();
                        var cartItem = new CartItem
                        {
                            VinylId = vinyl.Id,
                            Title = vinyl.Title,
                            ArtistName = artistName,
                            Price = vinyl.Price,
                            Quantity = quantity,
                            CoverImagePath = FixImagePath(vinyl.CoverImagePath)
                        };

                        cart.AddItem(cartItem);
                        _cartService.SaveCart(cart);

                        TempData["SuccessMessage"] = "Товар добавлен в корзину!";
                        
                        // Проверяем, является ли запрос AJAX
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = true, message = "Товар добавлен в корзину", totalItems = cart.TotalQuantity });
                        }
                    }
                }
                else
                {
                    _logger.LogError("Не удалось получить информацию о пластинке с ID: {vinylId}", vinylId);
                    TempData["ErrorMessage"] = "Не удалось добавить товар в корзину. Пожалуйста, попробуйте позже.";
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Не удалось добавить товар в корзину" });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при добавлении товара в корзину: {message}", ex.Message);
                TempData["ErrorMessage"] = "Произошла ошибка при добавлении товара в корзину.";
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Произошла ошибка при добавлении товара в корзину" });
                }
            }

            // Возвращаемся на предыдущую страницу
            return Redirect(Request.Headers["Referer"].ToString() ?? "/");
        }

        // Обновление количества товара в корзине
        [HttpPost]
        public IActionResult UpdateQuantity(int vinylId, int quantity)
        {
            var cart = _cartService.GetCart();
            cart.UpdateQuantity(vinylId, quantity);
            _cartService.SaveCart(cart);

            return RedirectToAction("Index");
        }

        // Удаление товара из корзины
        [HttpPost]
        public IActionResult RemoveFromCart(int vinylId)
        {
            var cart = _cartService.GetCart();
            cart.RemoveItem(vinylId);
            _cartService.SaveCart(cart);

            return RedirectToAction("Index");
        }

        // Очистка корзины
        [HttpPost]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            return RedirectToAction("Index");
        }

        // Оформление заказа
        [HttpGet]
        [Authorize]
        public IActionResult Checkout()
        {
            var cart = _cartService.GetCart();
            if (cart.Items.Count == 0)
            {
                TempData["ErrorMessage"] = "Корзина пуста. Добавьте товары перед оформлением заказа.";
                return RedirectToAction("Index");
            }

            ViewBag.CartTotal = cart.TotalPrice.ToString("C");
            return View();
        }

        // Подтверждение заказа
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            var cart = _cartService.GetCart();
            if (cart.Items.Count == 0)
            {
                TempData["ErrorMessage"] = "Корзина пуста. Добавьте товары перед оформлением заказа.";
                return RedirectToAction("Index");
            }

            try
            {
                // Получаем ID пользователя из клеймов
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Создаем заказ
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    StatusId = 1, // Статус "Новый"
                    TotalAmount = cart.TotalPrice,
                };

                // Отправляем запрос на создание заказа
                var orderResponse = await _httpClient.PostAsJsonAsync("/api/Orders", order);
                if (orderResponse.IsSuccessStatusCode)
                {
                    var createdOrder = await orderResponse.Content.ReadFromJsonAsync<Order>();
                    if (createdOrder != null)
                    {
                        // Добавляем товары в заказ
                        foreach (var item in cart.Items)
                        {
                            var orderItem = new OrderItem
                            {
                                OrderId = createdOrder.Id,
                                VinylId = item.VinylId,
                                Quantity = item.Quantity,
								Price = item.Price
                            };

                            await _httpClient.PostAsJsonAsync("/api/OrderItems", orderItem);
                        }

                        // Очищаем корзину
                        _cartService.ClearCart();

                        TempData["SuccessMessage"] = "Заказ успешно оформлен!";
                        return RedirectToAction("OrderConfirmation", new { orderId = createdOrder.Id });
                    }
                }

                _logger.LogError("Ошибка при оформлении заказа");
                TempData["ErrorMessage"] = "Произошла ошибка при оформлении заказа. Пожалуйста, попробуйте позже.";
            }
            catch (Exception ex)
            {
                _logger.LogError("Исключение при оформлении заказа: {message}", ex.Message);
                TempData["ErrorMessage"] = "Произошла ошибка при оформлении заказа. Пожалуйста, попробуйте позже.";
            }

            return RedirectToAction("Checkout");
        }

        // Подтверждение заказа
        [HttpGet]
        [Authorize]
        public IActionResult OrderConfirmation(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        // Helper method to fix image paths
        private string FixImagePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "/images/no-image.jpg";
                
            if (path.StartsWith("http"))
                return path;
                
            // Remove tilde if present
            if (path.StartsWith("~"))
                path = path.Substring(1);
                
            // Add leading / if not present
            if (!path.StartsWith("/"))
                path = "/" + path;
                
            return path;
        }
    }
} 