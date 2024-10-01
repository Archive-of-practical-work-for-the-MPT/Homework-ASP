namespace Sebezhko.Models
{
    public record class Users
    {
        public int ID { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string Name { get; set; }
        public int Account_ID { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }

    }
}
