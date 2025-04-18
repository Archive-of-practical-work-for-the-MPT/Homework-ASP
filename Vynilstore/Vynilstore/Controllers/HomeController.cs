using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Vynilstore.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ApiVynil.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Vynilstore.ViewModels;

namespace Vynilstore.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly HttpClient _httpClient;
		private readonly string _apiBaseUrl = "https://localhost:7119";

		public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
		{
			_logger = logger;
			_httpClient = httpClientFactory.CreateClient();
			_httpClient.BaseAddress = new Uri(_apiBaseUrl);
		}

		public async Task<IActionResult> Index()
		{
			try
			{
				_logger.LogInformation("Запрос к API: {url}", $"{_apiBaseUrl}/api/Vinyls");
				var response = await _httpClient.GetAsync("/api/Vinyls");
				
				if (response.IsSuccessStatusCode)
				{
					var vinyls = await response.Content.ReadFromJsonAsync<List<Vinyl>>();
					ViewBag.FeaturedVinyls = vinyls?.Take(4).ToList() ?? new List<Vinyl>();
					
					var artistsResponse = await _httpClient.GetAsync("/api/Artists");
					if (artistsResponse.IsSuccessStatusCode)
					{
						var artists = await artistsResponse.Content.ReadFromJsonAsync<List<Artist>>();
						ViewBag.Artists = artists;
						_logger.LogInformation("Успешно получено {count} исполнителей", artists?.Count ?? 0);
					}
					else
					{
						_logger.LogError("Ошибка получения исполнителей: {statusCode}", artistsResponse.StatusCode);
						ViewBag.Artists = new List<Artist>();
					}
					
					_logger.LogInformation("Успешно получено {count} пластинок, отображаем 4", vinyls?.Count ?? 0);
				}
				else
				{
					_logger.LogError("Ошибка получения пластинок: {statusCode}", response.StatusCode);
					ViewBag.FeaturedVinyls = new List<Vinyl>();
					ViewBag.Artists = new List<Artist>();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Исключение при получении данных: {message}", ex.Message);
				ViewBag.FeaturedVinyls = new List<Vinyl>();
				ViewBag.Artists = new List<Artist>();
			}

			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}
		
		public async Task<IActionResult> Catalog()
		{
			try
			{
				_logger.LogInformation("Запрос к API для каталога: {url}", $"{_apiBaseUrl}/api/Vinyls");
				var response = await _httpClient.GetAsync("/api/Vinyls");
				
				if (response.IsSuccessStatusCode)
				{
					var vinyls = await response.Content.ReadFromJsonAsync<List<Vinyl>>();
					ViewBag.Vinyls = vinyls ?? new List<Vinyl>();
					
					// Получаем исполнителей из API
					var artistsResponse = await _httpClient.GetAsync("/api/Artists");
					if (artistsResponse.IsSuccessStatusCode)
					{
						var artists = await artistsResponse.Content.ReadFromJsonAsync<List<Artist>>();
						ViewBag.Artists = artists;
						_logger.LogInformation("Успешно получено {count} исполнителей", artists?.Count ?? 0);
					}
					else
					{
						_logger.LogError("Ошибка получения исполнителей: {statusCode}", artistsResponse.StatusCode);
						ViewBag.Artists = new List<Artist>();
					}
					
					// Получаем отзывы для вычисления средних оценок
					var reviewsResponse = await _httpClient.GetAsync("/api/VinylReviews");
					if (reviewsResponse.IsSuccessStatusCode)
					{
						var reviews = await reviewsResponse.Content.ReadFromJsonAsync<List<VinylReview>>();
						ViewBag.AllReviews = reviews ?? new List<VinylReview>();
						_logger.LogInformation("Успешно получено {count} отзывов", reviews?.Count ?? 0);
					}
					else
					{
						_logger.LogError("Ошибка получения отзывов: {statusCode}", reviewsResponse.StatusCode);
						ViewBag.AllReviews = new List<VinylReview>();
					}
					
					_logger.LogInformation("Успешно получено {count} пластинок для каталога", vinyls?.Count ?? 0);
				}
				else
				{
					_logger.LogError("Ошибка получения пластинок: {statusCode}", response.StatusCode);
					ViewBag.Vinyls = new List<Vinyl>();
					ViewBag.Artists = new List<Artist>();
					ViewBag.AllReviews = new List<VinylReview>();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Исключение при получении данных для каталога: {message}", ex.Message);
				ViewBag.Vinyls = new List<Vinyl>();
				ViewBag.Artists = new List<Artist>();
				ViewBag.AllReviews = new List<VinylReview>();
			}

			return View();
		}

		// Детали пластинки
		[HttpGet]
		public async Task<IActionResult> Details(int id)
		{
			try
			{
				// Запрашиваем пластинку из API
				var vinylResponse = await _httpClient.GetAsync($"/api/Vinyls/{id}");
				
				if (vinylResponse.IsSuccessStatusCode)
				{
					var vinyl = await vinylResponse.Content.ReadFromJsonAsync<Vinyl>();
					if (vinyl != null)
					{
						// Получаем информацию об исполнителе
						var artistResponse = await _httpClient.GetAsync($"/api/Artists/{vinyl.ArtistId}");
						if (artistResponse.IsSuccessStatusCode)
						{
							var artist = await artistResponse.Content.ReadFromJsonAsync<Artist>();
							ViewBag.ArtistName = artist?.Name ?? "Неизвестный исполнитель";
						}

						// Получаем информацию о жанре
						var genreResponse = await _httpClient.GetAsync($"/api/Genres/{vinyl.GenreId}");
						if (genreResponse.IsSuccessStatusCode)
						{
							var genre = await genreResponse.Content.ReadFromJsonAsync<Genre>();
							ViewBag.GenreName = genre?.Name ?? "Неизвестный жанр";
						}

						// Получаем информацию о лейбле
						var labelResponse = await _httpClient.GetAsync($"/api/Labels/{vinyl.LabelId}");
						if (labelResponse.IsSuccessStatusCode)
						{
							var label = await labelResponse.Content.ReadFromJsonAsync<Label>();
							ViewBag.LabelName = label?.Name ?? "Неизвестный лейбл";
						}

						// Запрашиваем отзывы для этой пластинки
						var reviewsResponse = await _httpClient.GetAsync("/api/VinylReviews");
						if (reviewsResponse.IsSuccessStatusCode)
						{
							var allReviews = await reviewsResponse.Content.ReadFromJsonAsync<List<VinylReview>>();
							// Фильтруем отзывы только для текущей пластинки
							var reviews = allReviews?.Where(r => r.VinylId == id).ToList() ?? new List<VinylReview>();
							ViewBag.Reviews = reviews;
							
							// Получаем данные пользователей для отзывов
							var userIds = reviews?.Select(r => r.UserId).Distinct().ToList() ?? new List<int>();
							var usersResponse = await _httpClient.GetAsync("/api/Users");
							if (usersResponse.IsSuccessStatusCode)
							{
								var users = await usersResponse.Content.ReadFromJsonAsync<List<User>>();
								ViewBag.UserData = users?.Where(u => userIds.Contains(u.Id))
									.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}");
							}
						}

						return View(vinyl);
					}
				}
				
				_logger.LogError("Не удалось получить информацию о пластинке с ID: {id}", id);
				return NotFound();
			}
			catch (Exception ex)
			{
				_logger.LogError("Исключение при получении информации о пластинке: {message}", ex.Message);
				return StatusCode(500, "Внутренняя ошибка сервера");
			}
		}

		[Authorize]
		public async Task<IActionResult> Profile()
		{
			try
			{
				// Получаем ID пользователя из Claims
				var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
				if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
				{
					_logger.LogWarning("Не удалось получить ID пользователя из Claims");
					return RedirectToAction("Login", "Account");
				}
				
				// Получаем данные пользователя из API
				_logger.LogInformation("Запрос данных пользователя из API: {url}", $"{_apiBaseUrl}/api/Users/{userId}");
				var response = await _httpClient.GetAsync($"/api/Users/{userId}");
				
				if (response.IsSuccessStatusCode)
				{
					var user = await response.Content.ReadFromJsonAsync<User>();
					if (user != null)
					{
						// Получаем информацию о роли пользователя
						var roleResponse = await _httpClient.GetAsync($"/api/Roles/{user.RoleId}");
						string roleName = "Покупатель";
						
						if (roleResponse.IsSuccessStatusCode)
						{
							var role = await roleResponse.Content.ReadFromJsonAsync<Role>();
							roleName = role?.Name ?? "Покупатель";
						}
						
						// Создаем модель профиля
						var profileViewModel = new ProfileViewModel
						{
							Id = user.Id,
							Login = user.Login,
							FirstName = user.FirstName,
							LastName = user.LastName,
							Email = user.Email ?? "",
							PhoneNumber = user.PhoneNumber ?? "",
							BirthDate = user.BirthDate.HasValue ? 
								new DateTime(user.BirthDate.Value.Year, user.BirthDate.Value.Month, user.BirthDate.Value.Day) : null,
							RoleName = roleName
						};
						
						return View(profileViewModel);
					}
				}
				
				_logger.LogError("Ошибка получения данных пользователя: {statusCode}", response.StatusCode);
				TempData["ErrorMessage"] = "Не удалось загрузить данные профиля";
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				_logger.LogError("Исключение при получении данных профиля: {message}", ex.Message);
				TempData["ErrorMessage"] = "Произошла ошибка при загрузке профиля";
				return RedirectToAction("Index");
			}
		}

		// Добавление отзыва
		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddReview(Vynilstore.ViewModels.AddReviewViewModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					// Получаем ID пользователя из клеймов
					var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
					if (userId == 0)
					{
						return RedirectToAction("Login", "Account");
					}

					// Создаем отзыв
					var review = new VinylReview
					{
						VinylId = model.VinylId,
						UserId = userId,
						Rating = model.Rating,
						ReviewText = model.ReviewText,
						CreatedAt = DateTime.Now
					};

					// Отправляем запрос на создание отзыва
					var response = await _httpClient.PostAsJsonAsync("/api/VinylReviews", review);
					
					if (response.IsSuccessStatusCode)
					{
						TempData["SuccessMessage"] = "Ваш отзыв успешно добавлен!";
					}
					else
					{
						var content = await response.Content.ReadAsStringAsync();
						_logger.LogError("Ошибка при добавлении отзыва: {statusCode}, {content}", 
							response.StatusCode, content);
						TempData["ErrorMessage"] = "Не удалось добавить отзыв. Пожалуйста, попробуйте позже.";
					}
				}
				catch (Exception ex)
				{
					_logger.LogError("Исключение при добавлении отзыва: {message}", ex.Message);
					TempData["ErrorMessage"] = "Произошла ошибка при добавлении отзыва. Пожалуйста, попробуйте позже.";
				}
			}
			else
			{
				TempData["ErrorMessage"] = "Пожалуйста, проверьте введенные данные.";
			}

			return RedirectToAction("Details", new { id = model.VinylId });
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
