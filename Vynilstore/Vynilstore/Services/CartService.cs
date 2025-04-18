using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Vynilstore.Models;

namespace Vynilstore.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _cartKey = "ShoppingCart";

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Получение корзины из куки
        public ShoppingCart GetCart()
        {
            var context = _httpContextAccessor.HttpContext;
            var cartJson = context.Request.Cookies[_cartKey];

            if (string.IsNullOrEmpty(cartJson))
            {
                return new ShoppingCart();
            }

            try
            {
                return JsonSerializer.Deserialize<ShoppingCart>(cartJson);
            }
            catch
            {
                // В случае ошибки десериализации возвращаем пустую корзину
                return new ShoppingCart();
            }
        }

        // Сохранение корзины в куки
        public void SaveCart(ShoppingCart cart)
        {
            var context = _httpContextAccessor.HttpContext;
            var cartJson = JsonSerializer.Serialize(cart);

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true,
                IsEssential = true, // Необходимо для работы корзины
                SameSite = SameSiteMode.Lax
            };

            context.Response.Cookies.Delete(_cartKey);
            context.Response.Cookies.Append(_cartKey, cartJson, cookieOptions);
        }

        // Очистка корзины
        public void ClearCart()
        {
            var context = _httpContextAccessor.HttpContext;
            context.Response.Cookies.Delete(_cartKey);
        }
    }
} 