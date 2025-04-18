using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using ApiVynil.Models;
using Vynilstore.ViewModels;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Vynilstore.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://localhost:7119";

        public AccountController(ILogger<AccountController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(_apiBaseUrl);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            try
            {
                // Получаем ID текущего пользователя из клеймов
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login");
                }

                // Запрашиваем данные пользователя из API
                var response = await _httpClient.GetAsync($"/api/Users/{userId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<User>();
                    if (user != null)
                    {
                        // Получаем информацию о роли
                        var roleResponse = await _httpClient.GetAsync($"/api/Roles/{user.RoleId}");
                        string roleName = "Покупатель"; // По умолчанию
                        
                        if (roleResponse.IsSuccessStatusCode)
                        {
                            var role = await roleResponse.Content.ReadFromJsonAsync<Role>();
                            roleName = role?.Name ?? roleName;
                        }

                        var profileViewModel = new ProfileViewModel
                        {
                            Id = user.Id,
                            Login = user.Login,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            BirthDate = user.BirthDate?.ToDateTime(new TimeOnly()),
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            RoleName = roleName
                        };

                        return View(profileViewModel);
                    }
                }
                
                _logger.LogError("Не удалось получить данные пользователя с ID: {userId}", userId);
                TempData["ErrorMessage"] = "Не удалось загрузить профиль. Пожалуйста, попробуйте позже.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError("Исключение при загрузке профиля: {message}", ex.Message);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке профиля. Пожалуйста, попробуйте позже.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Создаем нового пользователя
                    var user = new User
                    {
                        Login = model.Login,
                        Password = model.Password,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        BirthDate = model.BirthDate.HasValue ? DateOnly.FromDateTime(model.BirthDate.Value) : null,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        RoleId = 2, // Роль "Покупатель" по умолчанию
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    // Хэшируем пароль
                    user.HashPassword(model.Password);

                    // Отправляем запрос на создание пользователя в API
                    var response = await _httpClient.PostAsJsonAsync("/api/Users", user);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Пользователь {login} успешно зарегистрирован", model.Login);
                        
                        // Автоматически входим в систему после регистрации
                        await LoginUser(user);
                        
                        TempData["SuccessMessage"] = "Регистрация успешно завершена!";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Ошибка при регистрации пользователя: {statusCode}, {content}", 
                            response.StatusCode, errorContent);
                        
                        ModelState.AddModelError("", "Ошибка при регистрации. Возможно, пользователь с таким логином уже существует.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Исключение при регистрации пользователя: {message}", ex.Message);
                    ModelState.AddModelError("", "Произошла ошибка при регистрации. Пожалуйста, попробуйте позже.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Запрашиваем всех пользователей (в реальном приложении лучше сделать специальный эндпоинт для авторизации)
                    var response = await _httpClient.GetAsync("/api/Users");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var users = await response.Content.ReadFromJsonAsync<List<User>>();
                        var user = users?.FirstOrDefault(u => u.Login == model.Login);
                        
                        if (user != null && user.VerifyPassword(model.Password))
                        {
                            _logger.LogInformation("Пользователь {login} успешно вошел в систему", model.Login);
                            
                            // Входим в систему
                            await LoginUser(user);
                            
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    
                    ModelState.AddModelError("", "Неверный логин или пароль");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Исключение при входе пользователя: {message}", ex.Message);
                    ModelState.AddModelError("", "Произошла ошибка при входе. Пожалуйста, попробуйте позже.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // Вспомогательный метод для входа пользователя
        private async Task LoginUser(User user)
        {
            // Получаем информацию о роли
            var roleResponse = await _httpClient.GetAsync($"/api/Roles/{user.RoleId}");
            string roleName = "Покупатель"; // По умолчанию
            
            if (roleResponse.IsSuccessStatusCode)
            {
                var role = await roleResponse.Content.ReadFromJsonAsync<Role>();
                roleName = role?.Name ?? "Покупатель";
            }
            
            // Создаём Claims для аутентификации
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, roleName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
} 