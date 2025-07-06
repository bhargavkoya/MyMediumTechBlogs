
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Text.RegularExpressions;

// ===== CONVENTIONAL ROUTING SETUP =====
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
    options.AppendTrailingSlash = false;
});

// Register custom route constraints
builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap.Add("customslug", typeof(SlugConstraint));
    options.ConstraintMap.Add("weekday", typeof(WeekdayConstraint));
});

// Register route transformer
builder.Services.AddSingleton<IOutboundParameterTransformer, SlugifyParameterTransformer>();

var app = builder.Build();

// ===== CONVENTIONAL ROUTING =====
app.MapControllerRoute(
    name: "blog-post",
    pattern: "blog/{year:int:min(2020):max(2030)}/{month:int:range(1,12)}/{slug:customslug}",
    defaults: new { controller = "Blog", action = "Post" });

app.MapControllerRoute(
    name: "admin-area",
    pattern: "admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" });

app.MapControllerRoute(
    name: "api-versioned",
    pattern: "api/v{version:apiVersion}/{controller}/{action?}",
    defaults: new { action = "Get" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ===== MINIMAL API WITH ADVANCED ROUTING =====

// Route with multiple constraints
app.MapGet("/products/{id:int:min(1)}", (int id) =>
    Results.Ok(new { Id = id, Name = $"Product {id}" }))
   .WithName("GetProductById");

// Route with custom constraint
app.MapGet("/events/{day:weekday}", (string day) =>
    Results.Ok(new { EventDay = day, Message = $"Events for {day}" }))
   .WithName("GetEventsByWeekday");

// Route with regex constraint
app.MapGet(@"/files/{filename:regex(^[a-zA-Z0-9._-]+\.(jpg|png|gif)$)}",
    (string filename) => Results.Ok(new { Filename = filename }))
   .WithName("GetImageFile");

// Complex route with optional segments
app.MapGet("/catalog/{category}/{subcategory?}/{page:int?}",
    (string category, string? subcategory, int? page) =>
    {
        return Results.Ok(new
        {
            Category = category,
            Subcategory = subcategory ?? "all",
            Page = page ?? 1
        });
    })
   .WithName("BrowseCatalog");

// Update the route group to remove the problematic method call  
var apiV1 = app.MapGroup("/api/v1")
              .WithTags("API v1"); // Removed .WithOpenApi() as it is not supported by RouteGroupBuilder  

apiV1.MapGet("/users/{id:guid}", (Guid id) =>
    Results.Ok(new { UserId = id, Name = "John Doe" }));

apiV1.MapPost("/users", (CreateUserRequest request) =>
    Results.Created($"/api/v1/users/{Guid.NewGuid()}", request));

// Dynamic route with parameter transformer
app.MapControllerRoute(
    name: "product-details",
    pattern: "products/{productName:slugify}",
    defaults: new { controller = "Products", action = "Details" });

app.Run();

// ===== CUSTOM CONSTRAINTS =====

public class SlugConstraint : IRouteConstraint
{
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey,
        RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (values.TryGetValue(routeKey, out var value) && value != null)
        {
            var slug = value.ToString();
            // Slug should contain only lowercase letters, numbers, and hyphens
            return Regex.IsMatch(slug, @"^[a-z0-9-]+$");
        }
        return false;
    }
}

public class WeekdayConstraint : IRouteConstraint
{
    private readonly string[] _weekdays = { "monday", "tuesday", "wednesday", "thursday", "friday" };

    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey,
        RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (values.TryGetValue(routeKey, out var value) && value != null)
        {
            var day = value.ToString()?.ToLowerInvariant();
            return _weekdays.Contains(day);
        }
        return false;
    }
}

// ===== PARAMETER TRANSFORMER =====

public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value == null) return null;

        var str = value.ToString();
        if (string.IsNullOrEmpty(str)) return str;

        // Convert to lowercase and replace spaces with hyphens
        return Regex.Replace(str, @"\s+", "-").ToLowerInvariant();
    }
}

// ===== CONTROLLER EXAMPLES =====

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet("{id:int:min(1)}")]
    public IActionResult GetProduct(int id)
    {
        return Ok(new { Id = id, Name = $"Product {id}" });
    }

    [HttpGet("search")]
    public IActionResult Search([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        return Ok(new { Query = q, Page = page, Size = size });
    }
}

[Route("blog")]
public class BlogController : Controller
{
    [Route("{year:int}/{month:int}/{slug}")]
    public IActionResult Post(int year, int month, string slug)
    {
        return Ok(new { Year = year, Month = month, Slug = slug });
    }

    [Route("category/{category:alpha}")]
    public IActionResult Category(string category)
    {
        return Ok(new { Category = category });
    }
}

// ===== MULTI-TENANT ROUTING =====

public class TenantRouteConstraint : IRouteConstraint
{
    private readonly string[] _validTenants = { "tenant1", "tenant2", "tenant3" };

    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey,
        RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (values.TryGetValue(routeKey, out var value) && value != null)
        {
            var tenant = value.ToString()?.ToLowerInvariant();
            return _validTenants.Contains(tenant);
        }
        return false;
    }
}

// ===== DTOs =====

public record CreateUserRequest(string Name, string Email);
