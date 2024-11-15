namespace Sebezhko.Models
{
    public class Cart
    {
        public Cart()
        {
            CartLines = new List<Products>();
        }

        public List<Products> CartLines { get; set; }

        public decimal FinalPrice
        {
            get
            {
                decimal sum = 0;
                foreach (Products product in CartLines)
                {
                    sum += product.Price;
                }
                return sum;
            }
        }
    }
}
