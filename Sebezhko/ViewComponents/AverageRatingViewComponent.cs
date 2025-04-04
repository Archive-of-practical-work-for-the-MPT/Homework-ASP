using Microsoft.AspNetCore.Mvc;
using Sebezhko.Models;

namespace Sebezhko.ViewComponents
{
    public class AverageRatingViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public AverageRatingViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke(int productId)
        {
            // Получаем все отзывы для данного товара
            var reviews = _context.Review.Where(r => r.Product_ID == productId).ToList();
            // Вычисляем средний рейтинг
            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            return View(averageRating); 
        }
    }
}
