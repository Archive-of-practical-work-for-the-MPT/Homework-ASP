namespace Sebezhko.Models
{
    public class Review
    {
        public int ID { get; set; }
        public int Product_ID { get; set; }
        public int User_ID { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual Products Products { get; set; }
        public Users User { get; set; }
    }
}
