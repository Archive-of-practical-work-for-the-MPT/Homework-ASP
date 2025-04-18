using System;
using ApiVynil.Models;

namespace Vynilstore.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int VinylId { get; set; }
        public string Title { get; set; }
        public string ArtistName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string CoverImagePath { get; set; }
        
        // Общая стоимость этого товара в корзине
        public decimal Total => Price * Quantity;
    }
} 