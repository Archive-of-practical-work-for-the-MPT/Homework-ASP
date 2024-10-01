namespace Sebezhko.Models
{
    public class Products
    {
        public int ID { get; set; }
        public string Name_Album { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public int RPM { get; set; }
        public string Color { get; set; }
        public int Year { get; set; }
        public string Path_Image { get; set; }
        public int Label_ID { get; set; }
        public int Musician_ID { get; set; }
        public int Genre_ID { get; set; }

    }
}
