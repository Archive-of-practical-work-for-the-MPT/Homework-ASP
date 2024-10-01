namespace Sebezhko.Models
{
    public class Orders
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public decimal Sum { get; set; }
        public int User_ID { get; set; }

    }
}
