using CornerStore.Models;
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
//category names include the search value (ignore case).
app.MapGet("/api/products", (CornerStoreDbContext db, string search) =>
{
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

app.Run();

//don't move or change this!
public partial class Program { }