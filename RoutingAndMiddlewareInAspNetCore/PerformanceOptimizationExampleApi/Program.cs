
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json.Serialization;

// ===== PERFORMANCE OPTIMIZED STARTUP =====
var builder = WebApplication.CreateBuilder(args);

// 1. Configure JSON serialization for better performance
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = false;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// 2. Add memory caching
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Limit cache size
});

// 3. Add distributed caching (Redis in production)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// 4. Add response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "text/json" });
});

// Configure compression levels
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
});

// 5. Add output caching (ASP.NET Core 7+)
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(10)));

    options.AddPolicy("LongCache", builder =>
        builder.Expire(TimeSpan.FromHours(1))
               .SetVaryByQuery("id", "category"));

    options.AddPolicy("ShortCache", builder =>
        builder.Expire(TimeSpan.FromMinutes(2)));
});

// 6. Configure controllers with optimized model binding
builder.Services.AddControllers(options =>
{
    // Disable automatic model validation for better performance
    options.SuppressAsyncSuffixInActionNames = false;

    // Add custom model binder providers for complex scenarios
    options.ModelBinderProviders.Insert(0, new SimpleTypeModelBinderProvider());
});

var app = builder.Build();

// ===== OPTIMIZED MIDDLEWARE PIPELINE =====

// 1. Response compression (early in pipeline)
app.UseResponseCompression();

// 2. Output caching
app.UseOutputCache();

// 3. Static files with caching headers
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int durationInSeconds = 60 * 60 * 24 * 30; // 30 days
        ctx.Context.Response.Headers.CacheControl = $"public,max-age={durationInSeconds}";
    }
});

// 4. Routing
app.UseRouting();

// 5. Custom performance monitoring middleware
app.UseMiddleware<PerformanceMonitoringMiddleware>();

// 6. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// ===== OPTIMIZED ENDPOINTS =====

// Cached endpoint with output caching
app.MapGet("/api/products", async (IProductService productService) =>
{
    var products = await productService.GetAllProductsAsync();
    return Results.Ok(products);
})
.CacheOutput("LongCache")
.WithName("GetAllProducts")
.WithTags("Products");

// Endpoint with manual caching
app.MapGet("/api/categories", async (IMemoryCache cache, ICategoryService categoryService) =>
{
    const string cacheKey = "all_categories";

    if (!cache.TryGetValue(cacheKey, out var categories))
    {
        categories = await categoryService.GetAllCategoriesAsync();

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(5),
            Priority = CacheItemPriority.High
        };

        cache.Set(cacheKey, categories, cacheOptions);
    }

    return Results.Ok(categories);
})
.WithName("GetAllCategories")
.WithTags("Categories");

// Optimized endpoint with async streaming
app.MapGet("/api/large-dataset", async (HttpContext context, IDataService dataService) =>
{
    context.Response.ContentType = "application/json";

    await foreach (var item in dataService.GetLargeDatasetAsync())
    {
        await context.Response.WriteAsync(JsonSerializer.Serialize(item) + "");
        await context.Response.Body.FlushAsync();
    }
})
.WithName("GetLargeDataset")
.WithTags("Data");

app.MapControllers();
app.Run();

// ===== PERFORMANCE MONITORING MIDDLEWARE =====

public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private readonly DiagnosticSource _diagnosticSource;

    public PerformanceMonitoringMiddleware(
        RequestDelegate next,
        ILogger<PerformanceMonitoringMiddleware> logger,
        DiagnosticSource diagnosticSource)
    {
        _next = next;
        _logger = logger;
        _diagnosticSource = diagnosticSource;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var startTime = DateTime.UtcNow;

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var endTime = DateTime.UtcNow;
            var elapsed = sw.ElapsedMilliseconds;

            // Log performance metrics
            _logger.LogInformation(
                "Request {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                elapsed,
                context.Response.StatusCode);

            // Write to diagnostic source for APM tools
            if (_diagnosticSource.IsEnabled("Microsoft.AspNetCore.Request.Performance"))
            {
                _diagnosticSource.Write("Microsoft.AspNetCore.Request.Performance", new
                {
                    HttpContext = context,
                    ElapsedMilliseconds = elapsed,
                    StartTime = startTime,
                    EndTime = endTime
                });
            }

            // Add performance header for debugging
            if (context.Request.Headers.ContainsKey("X-Debug-Performance"))
            {
                context.Response.Headers.Add("X-Response-Time", $"{elapsed}ms");
            }
        }
    }
}

// ===== OPTIMIZED SERVICES =====

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
}

public class ProductService : IProductService
{
    private readonly IMemoryCache _cache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IMemoryCache cache,
        IDistributedCache distributedCache,
        ILogger<ProductService> logger)
    {
        _cache = cache;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        const string cacheKey = "all_products";

        // Try L1 cache (memory)
        if (_cache.TryGetValue(cacheKey, out IEnumerable<Product>? products))
        {
            _logger.LogDebug("Products retrieved from L1 cache");
            return products!;
        }

        // Try L2 cache (distributed)
        var distributedProducts = await _distributedCache.GetStringAsync(cacheKey);
        if (distributedProducts != null)
        {
            products = JsonSerializer.Deserialize<IEnumerable<Product>>(distributedProducts);

            // Store in L1 cache
            _cache.Set(cacheKey, products, TimeSpan.FromMinutes(5));

            _logger.LogDebug("Products retrieved from L2 cache");
            return products!;
        }

        // Fetch from database
        products = await FetchProductsFromDatabaseAsync();

        // Store in both caches
        var serializedProducts = JsonSerializer.Serialize(products);
        await _distributedCache.SetStringAsync(cacheKey, serializedProducts,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });

        _cache.Set(cacheKey, products, TimeSpan.FromMinutes(5));

        _logger.LogDebug("Products retrieved from database and cached");
        return products;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        var cacheKey = $"product_{id}";

        if (_cache.TryGetValue(cacheKey, out Product? product))
        {
            return product;
        }

        product = await FetchProductFromDatabaseAsync(id);

        if (product != null)
        {
            _cache.Set(cacheKey, product, TimeSpan.FromMinutes(10));
        }

        return product;
    }

    private async Task<IEnumerable<Product>> FetchProductsFromDatabaseAsync()
    {
        // Simulate database call
        await Task.Delay(100);
        return new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 10.99m },
            new Product { Id = 2, Name = "Product 2", Price = 20.99m }
        };
    }

    private async Task<Product?> FetchProductFromDatabaseAsync(int id)
    {
        // Simulate database call
        await Task.Delay(50);
        return new Product { Id = id, Name = $"Product {id}", Price = id * 10.99m };
    }
}

// ===== OPTIMIZED CONTROLLER =====

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "category", "page" })]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
        [FromQuery] string? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Input validation
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var products = await _productService.GetAllProductsAsync();

        // Apply filtering and paging efficiently
        var filteredProducts = products
            .Where(p => category == null || p.Category == category)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return Ok(filteredProducts);
    }

    // Fix for CS0246: The type or namespace name 'VaryByParam' could not be found  
    // The issue is that 'VaryByParam' is not a valid property of the ResponseCacheAttribute.  
    // Replace 'VaryByParam' with 'VaryByQueryKeys', which is a valid property for ResponseCacheAttribute.  

    [HttpGet("{id:int:min(1)}")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id" })]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }
}

// ===== DATA MODELS =====

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}

public interface ICategoryService
{
    Task<IEnumerable<string>> GetAllCategoriesAsync();
}

public interface IDataService
{
    IAsyncEnumerable<DataItem> GetLargeDatasetAsync();
}

public class DataItem
{
    public int Id { get; set; }
    public string Data { get; set; } = string.Empty;
}
