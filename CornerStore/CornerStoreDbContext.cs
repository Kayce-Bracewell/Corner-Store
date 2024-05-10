using Microsoft.EntityFrameworkCore;
using CornerStore.Models;
public class CornerStoreDbContext : DbContext
{

    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context)
    {

    }

    public DbSet<Cashier> Cashiers {get; set;}
    public DbSet<Product> Products {get; set;}

    public DbSet<Category> Categories {get; set;}

    public DbSet<Order> Orders {get; set;}

    public DbSet<OrderProduct> OrderProducts {get; set;}


    //allows us to configure the schema when migrating as well as seed data
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cashier>().HasData(new Cashier[]
        {
            new Cashier {Id = 1, FirstName = "John", LastName = "Doe"},
            new Cashier {Id = 2, FirstName = "Jane", LastName = "Smith"},
            new Cashier {Id = 3, FirstName = "Sal", LastName = "Murcano"}
        });

        modelBuilder.Entity<Category>().HasData(new Category[]
        {
            new Category {Id = 1, CategoryName = "Electronic"},
            new Category {Id = 2, CategoryName = "Clothing"},
            new Category {Id = 3, CategoryName = "Weapons"},
            new Category {Id = 4, CategoryName = "Sports Gear"}
        });

        modelBuilder.Entity<Product>().HasData(new Product[]
        {
            new Product {Id = 1, ProductName = "T-Shirt", Price = 19.99M, Brand = "Hanes", CategoryId = 2},
            new Product {Id = 2, ProductName = "TV", Price = 699.99M, Brand = "Samsung", CategoryId = 1},
            new Product {Id = 3, ProductName = "Knife", Price = 59.99M, Brand = "Kershaw", CategoryId = 3},
            new Product {Id = 4, ProductName = "Tennis Racket", Price = 79.99M, Brand = "Prince", CategoryId = 4},
            new Product {Id = 5, ProductName = "PC", Price = 899.99M, Brand = "HP", CategoryId = 1},
            new Product {Id = 6, ProductName = "Sweatpants", Price = 29.99M, Brand = "Puma", CategoryId = 2},
            new Product {Id = 7, ProductName = "Explosives", Price = 119.99M, Brand = "ACME", CategoryId = 3},
            new Product {Id = 8, ProductName = "Basketball", Price = 39.99M, Brand = "Nike", CategoryId = 4}
        });

        modelBuilder.Entity<Order>().HasData(new Order[]
        {
            new Order {Id = 1, CashierId = 1, PaidOnDate = DateTime.Now},
            new Order {Id = 2, CashierId = 2, PaidOnDate = DateTime.Now},
            new Order {Id = 3, CashierId = 3, PaidOnDate = DateTime.Now},
            new Order {Id = 4, CashierId = 1, PaidOnDate = DateTime.Now}
        });

        modelBuilder.Entity<OrderProduct>().HasData(new OrderProduct[]
        {
            new OrderProduct {Id = 1, ProductId = 1, OrderId = 1, Quantity = 2},
            new OrderProduct {Id = 2, ProductId = 2, OrderId = 2, Quantity = 3},
            new OrderProduct {Id = 3, ProductId = 6, OrderId = 3, Quantity = 4},
            new OrderProduct {Id = 4, ProductId = 5, OrderId = 4, Quantity = 1}
        });
    }
}