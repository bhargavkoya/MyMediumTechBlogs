namespace ECommerceWithRoutingAndMiddleware.CustomMiddlewares
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolutionMiddleware> _logger;

        public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extract tenant from subdomain or header
            var host = context.Request.Host.Host;
            var tenantId = ExtractTenantFromHost(host) ??
                          context.Request.Headers["X-Tenant-ID"].FirstOrDefault() ??
                          "default";

            context.Items["TenantId"] = tenantId;
            _logger.LogDebug("Resolved tenant: {TenantId}", tenantId);

            await _next(context);
        }

        private string? ExtractTenantFromHost(string host)
        {
            var parts = host.Split('.');
            return parts.Length > 2 ? parts[0] : null;
        }
    }
}
