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
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrdersDetails> OrdersDetails { get; set; }
        public DbSet<Review> Review { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { 
            Database.EnsureCreated();

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Products) // Связь с продуктом
                .WithMany(p => p.Reviews) // Коллекция отзывов в продукте
                .HasForeignKey(r => r.Product_ID); // Внешний ключ

            modelBuilder.Entity<Review>()
               .HasOne(r => r.User) // Связь с продуктом
               .WithMany() // Коллекция отзывов в продукте
               .HasForeignKey(r => r.User_ID); // Внешний ключ
        }
    }
}
