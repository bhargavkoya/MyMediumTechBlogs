using DependencyInjectionInNETCore.FactoryDesignPattern;
using DependencyInjectionInNETCore.Interfaces;
using DependencyInjectionInNETCore.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//registration
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<SmsNotificationService>();
builder.Services.AddScoped<PushNotificationService>();
builder.Services.AddScoped<INotificationFactory, NotificationFactory>();
builder.Services.AddScoped<IProductService, ProductService>();

//keyed service registration
builder.Services.AddKeyedScoped<IPaymentProcessor, StripePaymentProcessor>("stripe");
builder.Services.AddKeyedScoped<IPaymentProcessor, PayPalPaymentProcessor>("paypal");

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

//method injection in minimal API
app.MapGet("/products", (IProductService productService) =>
    productService.GetAllProducts());

app.MapGet("/products/{id}", (int id, IProductService productService) =>
    productService.GetProductById(id));

app.MapPost("/products", async (Product product, IProductService productService) =>
{
    await productService.CreateProductAsync(product);
    return Results.Created($"/products/{product.Id}", product);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
