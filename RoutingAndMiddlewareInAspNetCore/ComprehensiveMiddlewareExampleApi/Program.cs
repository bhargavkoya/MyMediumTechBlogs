
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom middleware
builder.Services.AddTransient<CustomLoggingMiddleware>();
builder.Services.AddTransient<CustomAuthenticationMiddleware>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 1. Exception Handling (First in pipeline)
app.UseExceptionHandler("/Error");

// 2. HTTPS Redirection
app.UseHttpsRedirection();

// 3. Static Files
app.UseStaticFiles();

// 4. Custom Logging Middleware
app.UseMiddleware<CustomLoggingMiddleware>();

// 5. Routing
app.UseRouting();

// 6. CORS (after routing, before auth)
app.UseCors();

// 7. Custom Authentication Middleware
app.UseMiddleware<CustomAuthenticationMiddleware>();

// 8. Authorization
app.UseAuthorization();

// 9. Endpoints
app.MapControllers();

// Minimal API endpoints
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck")
   .WithTags("Health");

app.MapGet("/api/products/{id:int}", async (int id, ILogger<Program> logger) =>
{
    logger.LogInformation("Fetching product with ID: {ProductId}", id);

    // Simulate database call
    await Task.Delay(100);

    if (id <= 0)
        return Results.BadRequest("Invalid product ID");

    if (id > 1000)
        return Results.NotFound();

    return Results.Ok(new { Id = id, Name = $"Product {id}", Price = id * 10.99 });
})
.WithName("GetProduct")
.WithSummary("Get a product by ID");

app.Run();

// Custom Logging Middleware
public class CustomLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomLoggingMiddleware> _logger;

    public CustomLoggingMiddleware(RequestDelegate next, ILogger<CustomLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        _logger.LogInformation("Incoming Request: {Method} {Path} at {Timestamp}",
            requestMethod, requestPath, DateTime.UtcNow);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Request {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                requestMethod, requestPath, stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
        }
    }
}

// Custom Authentication Middleware
public class CustomAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomAuthenticationMiddleware> _logger;

    public CustomAuthenticationMiddleware(RequestDelegate next, ILogger<CustomAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();

            // Simple token validation (in real world, use proper JWT validation)
            if (ValidateToken(token))
            {
                context.Items["User"] = "AuthenticatedUser";
                _logger.LogInformation("User authenticated successfully");
            }
            else
            {
                _logger.LogWarning("Invalid token provided");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }

        await _next(context);
    }

    private bool ValidateToken(string token)
    {
        // Simple validation - in real world, validate JWT properly
        return !string.IsNullOrEmpty(token) && token.Length > 10;
    }
}
