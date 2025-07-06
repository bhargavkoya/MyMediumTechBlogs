namespace ECommerceWithRoutingAndMiddleware.CustomMiddlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestId = Guid.NewGuid().ToString();
            context.Items["RequestId"] = requestId;

            _logger.LogInformation("Request {RequestId} started: {Method} {Path} from {RemoteIp}",
                requestId, context.Request.Method, context.Request.Path,
                context.Connection.RemoteIpAddress);

            await _next(context);

            _logger.LogInformation("Request {RequestId} completed with status {StatusCode}",
                requestId, context.Response.StatusCode);
        }
    }
}
