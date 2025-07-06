using System.Diagnostics;

namespace ECommerceWithRoutingAndMiddleware.CustomMiddlewares
{
    public class PerformanceTracingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceTracingMiddleware> _logger;

        public PerformanceTracingMiddleware(RequestDelegate next, ILogger<PerformanceTracingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var activity = new Activity("HTTP Request");
            activity.Start();

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                activity.SetTag("http.method", context.Request.Method);
                activity.SetTag("http.status_code", context.Response.StatusCode.ToString());
                activity.SetTag("http.response_time_ms", stopwatch.ElapsedMilliseconds.ToString());

                if (stopwatch.ElapsedMilliseconds > 1000) // Log slow requests
                {
                    _logger.LogWarning("Slow request detected: {Method} {Path} took {ElapsedMs}ms",
                        context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
                }
            }
        }
    }
}
