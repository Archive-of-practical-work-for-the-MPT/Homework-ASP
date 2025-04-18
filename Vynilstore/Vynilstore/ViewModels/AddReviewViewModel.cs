using System;
using System.ComponentModel.DataAnnotations;

namespace Vynilstore.ViewModels
{
    public class AddReviewViewModel
    {
        [Required(ErrorMessage = "Выберите оценку товара")]
        [Range(1, 5, ErrorMessage = "Оценка должна быть от 1 до 5 звезд")]
        public int Rating { get; set; }

        [StringLength(500, ErrorMessage = "Текст отзыва не должен превышать 500 символов")]
        public string ReviewText { get; set; }

        [Required]
        public int VinylId { get; set; }
    }
} 