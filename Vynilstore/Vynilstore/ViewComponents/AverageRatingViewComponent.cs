using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ApiVynil.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Vynilstore.ViewComponents
{
    public class AverageRatingViewComponent : ViewComponent
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AverageRatingViewComponent> _logger;
        private readonly string _apiBaseUrl = "https://localhost:7119";

        public AverageRatingViewComponent(IHttpClientFactory httpClientFactory, ILogger<AverageRatingViewComponent> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(_apiBaseUrl);
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(int vinylId)
        {
            try
            {
                // Запрашиваем отзывы для этой пластинки
                _logger.LogInformation("Запрос отзывов для пластинки: {vinylId}", vinylId);
                var response = await _httpClient.GetAsync("/api/VinylReviews");
                
                if (response.IsSuccessStatusCode)
                {
                    var allReviews = await response.Content.ReadFromJsonAsync<List<VinylReview>>();
                    
                    // Фильтруем отзывы только для текущей пластинки
                    var reviews = allReviews?.Where(r => r.VinylId == vinylId).ToList();
                    
                    if (reviews != null && reviews.Count > 0)
                    {
                        // Вычисляем средний рейтинг
                        var averageRating = reviews.Average(r => r.Rating);
                        var reviewsCount = reviews.Count;
                        
                        _logger.LogInformation("Средний рейтинг для пластинки {vinylId}: {avgRating}, отзывов: {count}", 
                            vinylId, averageRating, reviewsCount);
                        return View(new Tuple<double, int>(averageRating, reviewsCount));
                    }
                }
                else
                {
                    _logger.LogWarning("Ошибка при получении отзывов: {statusCode}", response.StatusCode);
                }
                
                // Если отзывов нет или произошла ошибка
                return View(new Tuple<double, int>(0, 0));
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при получении среднего рейтинга: {message}", ex.Message);
                return View(new Tuple<double, int>(0, 0));
            }
        }
    }
} 