using Microsoft.EntityFrameworkCore;

namespace Sebezhko.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Musicians> Musicians { get; set; }
        public DbSet<Genres> Genres { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Accounts> Accounts { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrdersDetails> OrdersDetails { get; set; }



        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { 
            Database.EnsureCreated();
        }
    }
}
