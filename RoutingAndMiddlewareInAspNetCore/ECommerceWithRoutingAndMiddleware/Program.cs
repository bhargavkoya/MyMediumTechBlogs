
// ===== Enterprise E-Commerce Platform - Complete Implementation =====
// This example demonstrates a real-world enterprise application using
// advanced routing, middleware, and performance optimizations

using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;
using ECommerceWithRoutingAndMiddleware.CustomMiddlewares;
using ECommerceWithRoutingAndMiddleware.Data;
using ECommerceWithRoutingAndMiddleware.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===== SERVICE CONFIGURATION =====


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme); // No changes needed here
// Database
builder.Services.AddDbContext<ECommerceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
    options.AddPolicy("PremiumCustomer", policy =>
        policy.RequireRole("Customer").RequireClaim("subscription", "premium"));
});

// Performance Services
builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Global rate limiting
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    // API specific rate limiting
    options.AddPolicy("ApiPolicy", context =>
        RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: context.User?.Identity?.Name ?? "anonymous",
            factory: partition => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 50,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10,
                ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                TokensPerPeriod = 10,
                AutoReplenishment = true
            }));

    // Premium customer rate limiting
    options.AddPolicy("PremiumPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? "anonymous",
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 1000,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 100
            }));
});

// Output Caching
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(5)));

    options.AddPolicy("ProductCache", builder =>
        builder.Expire(TimeSpan.FromMinutes(30))
               .SetVaryByQuery("category", "page", "search")
               .SetVaryByHeader("Accept-Language"));

    // Replace the problematic line with the following code block:
    options.AddPolicy("UserSpecific", builder =>
       builder.Expire(TimeSpan.FromMinutes(10))
              .VaryByValue((context) =>
                  new KeyValuePair<string, string>("userId",
                      context.User?.Identity?.Name ?? "anonymous")));
});

// Business Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICacheService, CacheService>();

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "E-Commerce API",
        Version = "v1",
        Description = "Enterprise E-Commerce Platform API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins("https://ecommerce.com", "https://admin.ecommerce.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Controllers
builder.Services.AddControllers(options =>
{
    options.CacheProfiles.Add("Default", new CacheProfile
    {
        Duration = 300
    });
    options.CacheProfiles.Add("Long", new CacheProfile
    {
        Duration = 1800
    });
});

var app = builder.Build();

// ===== MIDDLEWARE PIPELINE =====

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API V1");
        c.RoutePrefix = "docs";
    });
}

// Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// Rate Limiting
app.UseRateLimiter();

// Exception Handling
app.UseExceptionHandler("/error");

// HTTPS Redirection
app.UseHttpsRedirection();

// Response Compression
app.UseResponseCompression();

// Output Caching
app.UseOutputCache();

// Static Files with Caching
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int durationInSeconds = 60 * 60 * 24 * 365; // 1 year
        ctx.Context.Response.Headers.CacheControl = $"public,max-age={durationInSeconds}";
        ctx.Context.Response.Headers.Add("Vary", "Accept-Encoding");
    }
});

// CORS
app.UseCors("AllowedOrigins");

// Request Logging
app.UseMiddleware<RequestLoggingMiddleware>();

// Routing
app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Custom Middleware
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseMiddleware<PerformanceTracingMiddleware>();

// ===== ROUTING CONFIGURATION =====

// API Versioning Routes
var apiV1 = app.MapGroup("/api/v1")
    .WithTags("API v1")
    .RequireRateLimiting("ApiPolicy");

var apiV2 = app.MapGroup("/api/v2")
    .WithTags("API v2")
    .RequireRateLimiting("ApiPolicy");

// Product Endpoints
apiV1.MapGet("/products", async (
    IProductService productService,
    [FromQuery] string? category = null,
    [FromQuery] string? search = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20) =>
{
    var products = await productService.GetProductsAsync(category, search, page, pageSize);
    return Results.Ok(products);
})
.CacheOutput("ProductCache")
.WithName("GetProducts")
.WithSummary("Get paginated products with optional filtering");

apiV1.MapGet("/products/{id:int:min(1)}", async (
    int id,
    IProductService productService) =>
{
    var product = await productService.GetProductByIdAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
})
.CacheOutput("ProductCache")
.WithName("GetProductById")
.WithSummary("Get a specific product by ID");

// Premium endpoints with higher rate limits
apiV1.MapGet("/products/recommendations", async (
    HttpContext context,
    IProductService productService) =>
{
    var userId = context.User?.Identity?.Name;
    var recommendations = await productService.GetRecommendationsAsync(userId);
    return Results.Ok(recommendations);
})
.RequireAuthorization("PremiumCustomer")
.RequireRateLimiting("PremiumPolicy")
.CacheOutput("UserSpecific")
.WithName("GetRecommendations")
.WithSummary("Get personalized product recommendations");

// Order Endpoints
apiV1.MapPost("/orders", async (
    CreateOrderRequest request,
    IOrderService orderService,
    ClaimsPrincipal user) =>
{
    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var order = await orderService.CreateOrderAsync(request, userId);
    return Results.Created($"/api/v1/orders/{order.Id}", order);
})
.RequireAuthorization("CustomerOnly")
.WithName("CreateOrder")
.WithSummary("Create a new order");

apiV1.MapGet("/orders", async (
    IOrderService orderService,
    ClaimsPrincipal user,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10) =>
{
    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var orders = await orderService.GetUserOrdersAsync(userId, page, pageSize);
    return Results.Ok(orders);
})
.RequireAuthorization("CustomerOnly")
.CacheOutput("UserSpecific")
.WithName("GetUserOrders")
.WithSummary("Get user's orders");

// Admin Endpoints
var adminApi = app.MapGroup("/api/admin")
    .RequireAuthorization("AdminOnly")
    .WithTags("Admin");

adminApi.MapGet("/analytics/sales", async (
    IOrderService orderService,
    [FromQuery] DateTime? from = null,
    [FromQuery] DateTime? to = null) =>
{
    var analytics = await orderService.GetSalesAnalyticsAsync(from, to);
    return Results.Ok(analytics);
})
.WithName("GetSalesAnalytics")
.WithSummary("Get sales analytics for admin dashboard");

// Health Check Endpoints
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
}))
.WithTags("Health")
.WithName("HealthCheck")
.AllowAnonymous();

app.MapGet("/health/detailed", async (
    ECommerceDbContext dbContext,
    ICacheService cacheService) =>
{
    var healthChecks = new List<HealthCheck>();

    // Database health
    try
    {
        await dbContext.Database.CanConnectAsync();
        healthChecks.Add(new HealthCheck("Database", "Healthy"));
    }
    catch (Exception ex)
    {
        healthChecks.Add(new HealthCheck("Database", "Unhealthy", ex.Message));
    }

    // Cache health
    try
    {
        await cacheService.PingAsync();
        healthChecks.Add(new HealthCheck("Cache", "Healthy"));
    }
    catch (Exception ex)
    {
        healthChecks.Add(new HealthCheck("Cache", "Unhealthy", ex.Message));
    }

    var overallStatus = healthChecks.All(h => h.Status == "Healthy") ? "Healthy" : "Unhealthy";

    return Results.Ok(new
    {
        OverallStatus = overallStatus,
        Checks = healthChecks,
        Timestamp = DateTime.UtcNow
    });
})
.WithTags("Health")
.WithName("DetailedHealthCheck")
.RequireAuthorization("AdminOnly");

// Traditional MVC routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public record HealthCheck(string Component, string Status, string? Message = null);
