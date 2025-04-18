using System;
using System.Collections.Generic;
using System.Linq;

namespace Vynilstore.Models
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        // Общее количество товаров в корзине
        public int TotalQuantity => Items.Sum(item => item.Quantity);

        // Общая стоимость всех товаров в корзине
        public decimal TotalPrice => Items.Sum(item => item.Total);

        // Добавление товара в корзину
        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.VinylId == item.VinylId);
            if (existingItem != null)
            {
                // Если товар уже есть в корзине, увеличиваем количество
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                // Иначе добавляем новый товар
                Items.Add(item);
            }
        }

        // Удаление товара из корзины
        public void RemoveItem(int vinylId)
        {
            var item = Items.FirstOrDefault(i => i.VinylId == vinylId);
            if (item != null)
            {
                Items.Remove(item);
            }
        }

        // Обновление количества товара
        public void UpdateQuantity(int vinylId, int quantity)
        {
            var item = Items.FirstOrDefault(i => i.VinylId == vinylId);
            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    RemoveItem(vinylId);
                }
            }
        }

        // Очистка корзины
        public void Clear()
        {
            Items.Clear();
        }
    }
} 