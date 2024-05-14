using CornerStore.Models;
using CornerStore.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddNpgsql<CornerStoreDbContext>(builder.Configuration["CornerStoreDbConnectionString"] ?? "testing");

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//endpoints go here
// Endpoint to create a new Cashier
app.MapPost("/api/cashiers", (CornerStoreDbContext db, Cashier cashier) => 
{
    db.Cashiers.Add(cashier);
    db.SaveChanges();
    return Results.Created($"/api/cashiers/{cashier.Id}", cashier);
});

// Get a cashier (include their orders, and the orders' products).
app.MapGet("/api/cashiers/{id}", (CornerStoreDbContext db, int id) =>
{
    Cashier cashier = db.Cashiers
        .Include(c => c.Orders)
            .ThenInclude(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
        .FirstOrDefault(c => c.Id == id);

    if (cashier == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(cashier);
});

// Get all products with categories.
// If the search query string param is present,
// return only products whose names or 
// category names include the search value (ignore case).
app.MapGet("/api/products", (CornerStoreDbContext db, string search) =>
{
    // THIS NEEDS MORE TESTING : RECURSIVE ISSUE
    var foundProducts = db.Products.Include(p => p.Category);

    if (!string.IsNullOrWhiteSpace(search))
    {
        var searchedProducts = foundProducts.Where(p => p.ProductName.Contains(search) || p.Category.CategoryName.Contains(search));
        return Results.Ok(searchedProducts);
    };

    return Results.Ok(foundProducts);
});

// Add a product
app.MapPost("/api/products", (CornerStoreDbContext db, Product product) =>
{
    db.Products.Add(product);
    db.SaveChanges();
    return Results.Created($"/api/products/{product.Id}", product);
});

// Update a product
app.MapPut("/api/products/{id}", (CornerStoreDbContext db, Product updateProduct, int id) =>
{
    Product product = db.Products.FirstOrDefault(p => p.Id == id);

    if (product == null)
    {
        return Results.NotFound();
    }

    product.ProductName = updateProduct.ProductName;
    product.Price = updateProduct.Price;
    product.Brand = updateProduct.Brand;
    product.CategoryId = updateProduct.CategoryId;

    db.SaveChanges();
    return Results.Created($"/api/products/{product.Id}", product);

});

// Get an order details, including the cashier, order products,
// and products on the order with their category.
app.MapGet("/api/orders/{id}", (CornerStoreDbContext db, int id) =>
{
    var orderDetails = db.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
        .FirstOrDefault(o => o.Id == id);

    if (orderDetails == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(orderDetails);

});

// Get all orders.
// Check for a query string param orderDate that only returns orders from a particular day.
// If it is not present, return all orders.
app.MapGet("/api/orders", (CornerStoreDbContext db, DateTime? orderDate) =>
{
    var foundOrders = db.Orders.Include(o => o.OrderProducts).ThenInclude(op => op.Product);

    if (orderDate != null)
    {
        DateTime startOfDay = orderDate.Value.Date;
        DateTime endOfDay = startOfDay.AddDays(1);

        foundOrders.Where(o => o.PaidOnDate >= startOfDay && o.PaidOnDate < endOfDay);
    }

    return foundOrders;
});

// Delete an order
app.MapDelete("/api/orders/{id}", (CornerStoreDbContext db, int id) =>
{
    Order orderToDelete = db.Orders
        .FirstOrDefault(o => o.Id == id);

        if (orderToDelete != null)
        {
            db.Orders.Remove(orderToDelete);
            db.SaveChanges();
            return Results.Ok($"order deleted at ID {id}");
        }

        return Results.NotFound($"no order found with ID {id}");
});

// Create an Order (with products!)
app.MapPost("/api/orders", (CornerStoreDbContext db, CreateOrderDTO orderDto) =>
{
    // ENDPOINT WORKS BUT: there is an issue with the total property of the order class
    var order = new Order
    {
        CashierId = orderDto.CashierId,
        PaidOnDate = DateTime.Now, 
        OrderProducts = orderDto.OrderProducts.Select(op => new OrderProduct
        {
            ProductId = op.ProductId,
            Quantity = op.Quantity
        }).ToList()
    };

    db.Orders.Add(order);
    db.SaveChanges();

    // Calculate total by passing the OrderProducts directly to the CalculateTotal method
    decimal total = 0;
    foreach (var orderProduct in order.OrderProducts)
    {
        if (orderProduct != null && orderProduct.Product != null)
        {
            total += orderProduct.Product.Price * orderProduct.Quantity;
        }
        else
        {
            // skip the calculation
        }
    }

    return Results.Created($"/api/orders/{order.Id}", order);
});

app.Run();

//don't move or change this!
public partial class Program { }