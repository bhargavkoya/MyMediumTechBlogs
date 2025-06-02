using System.Net;
using System.Text;
using System.Text.Json;

namespace HttpServerInDotnet.HTTPListnerApproach
{
    public class HttpListenerServer
    {
        private readonly HttpListener _listener;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Dictionary<string, Func<HttpListenerContext, Task>> _routes;

        public HttpListenerServer(string[] prefixes)
        {
            _listener = new HttpListener();
            _cancellationTokenSource = new CancellationTokenSource();
            _routes = new Dictionary<string, Func<HttpListenerContext, Task>>();

            // Add URL prefixes
            foreach (string prefix in prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }

            SetupDefaultRoutes();
        }

        private void SetupDefaultRoutes()
        {
            // GET routes
            _routes["GET /"] = HandleHomeRoute;
            _routes["GET /api/users"] = HandleGetUsers;
            _routes["GET /api/users/{id}"] = HandleGetUserById;
            
            // POST routes
            _routes["POST /api/users"] = HandleCreateUser;
            _routes["POST /api/data"] = HandlePostData;
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine("HTTP Server started on:");
            foreach (string prefix in _listener.Prefixes)
            {
                Console.WriteLine($"  {prefix}");
            }

            // Handle requests concurrently
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++) //10 concurrent connections at a time
            {
                tasks.Add(HandleIncomingConnections());
            }

            await Task.WhenAll(tasks);
        }

        private async Task HandleIncomingConnections()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    // Handle each request asynchronously
                    _ = Task.Run(() => ProcessRequest(context));
                }
                catch (ObjectDisposedException)
                {
                    // Listener was stopped
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting context: {ex.Message}");
                }
            }
        }

        private async Task ProcessRequest(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                // Add CORS headers
                AddCorsHeaders(response);

                // Handle OPTIONS preflight request for CORS
                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 200;
                    response.Close();
                    return;
                }

                Console.WriteLine($"{request.HttpMethod} {request.Url.LocalPath}");

                // Route the request
                string routeKey = $"{request.HttpMethod} {request.Url.LocalPath}";
                
                if (_routes.TryGetValue(routeKey, out var handler))
                {
                    await handler(context);
                }
                else if (IsParameterizedRoute(request, out var paramHandler))
                {
                    await paramHandler(context);
                }
                else
                {
                    await HandleNotFound(context);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing request: {ex.Message}");
                try
                {
                    context.Response.StatusCode = 500;
                    await WriteJsonResponse(context.Response, new { error = "Internal server error" });
                }
                catch
                {
                    // Ignore if response is already closed
                }
            }
        }

        private bool IsParameterizedRoute(HttpListenerRequest request, out Func<HttpListenerContext, Task> handler)
        {
            handler = null;
            var path = request.Url.LocalPath;
            var method = request.HttpMethod;

            if (method == "GET" && path.StartsWith("/api/users/") && path.Length > "/api/users/".Length)
            {
                handler = HandleGetUserById;
                return true;
            }

            return false;
        }

        private void AddCorsHeaders(HttpListenerResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        }

        // Route Handlers
        private async Task HandleHomeRoute(HttpListenerContext context)
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>HTTP Server</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; }
        .endpoint { background: #f5f5f5; padding: 10px; margin: 10px 0; border-left: 4px solid #007acc; }
    </style>
</head>
<body>
    <h1>HTTP Server Running</h1>
    <h2>Available Endpoints in this server:</h2>
    <div class='endpoint'><strong>GET /</strong> - This page</div>
    <div class='endpoint'><strong>GET /api/users</strong> - Get all users</div>
    <div class='endpoint'><strong>GET /api/users/{id}</strong> - Get user by ID</div>
    <div class='endpoint'><strong>POST /api/users</strong> - Create new user</div>
    <div class='endpoint'><strong>POST /api/data</strong> - Post JSON data</div>
</body>
</html>";
            
            context.Response.ContentType = "text/html";
            await WriteResponse(context.Response, html);
        }

        private async Task HandleGetUsers(HttpListenerContext context)
        {
            var users = new[]
            {
                new { Id = 1, Name = "John Doe", Email = "john@example.com" },
                new { Id = 2, Name = "Jane Smith", Email = "jane@example.com" },
                new { Id = 3, Name = "Bob Johnson", Email = "bob@example.com" }
            };

            await WriteJsonResponse(context.Response, users);
        }

        private async Task HandleGetUserById(HttpListenerContext context)
        {
            var path = context.Request.Url.LocalPath;
            var segments = path.Split('/');
            
            if (segments.Length >= 4 && int.TryParse(segments[3], out int userId))
            {
                var user = new { Id = userId, Name = $"User {userId}", Email = $"user{userId}@example.com" };
                await WriteJsonResponse(context.Response, user);
            }
            else
            {
                context.Response.StatusCode = 400;
                await WriteJsonResponse(context.Response, new { error = "Invalid user ID" });
            }
        }

        private async Task HandleCreateUser(HttpListenerContext context)
        {
            try
            {
                // Read POST body
                string jsonString = await ReadRequestBody(context.Request);
                
                if (string.IsNullOrEmpty(jsonString))
                {
                    context.Response.StatusCode = 400;
                    await WriteJsonResponse(context.Response, new { error = "Request body is required" });
                    return;
                }

                // Parse JSON
                var userData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
                
                // Simulate creating user
                var newUser = new 
                { 
                    Id = new Random().Next(1000, 9999),
                    Name = userData.GetValueOrDefault("name", "Unknown").ToString(),
                    Email = userData.GetValueOrDefault("email", "unknown@example.com").ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                context.Response.StatusCode = 201;
                await WriteJsonResponse(context.Response, newUser);
            }
            catch (JsonException)
            {
                context.Response.StatusCode = 400;
                await WriteJsonResponse(context.Response, new { error = "Invalid JSON in request body" });
            }
        }

        private async Task HandlePostData(HttpListenerContext context)
        {
            try
            {
                string requestBody = await ReadRequestBody(context.Request);
                
                // Echo back the received data with additional info
                var responseData = new
                {
                    message = "Data received successfully",
                    receivedAt = DateTime.UtcNow,
                    contentType = context.Request.ContentType,
                    contentLength = context.Request.ContentLength64,
                    data = requestBody
                };

                await WriteJsonResponse(context.Response, responseData);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 400;
                await WriteJsonResponse(context.Response, new { error = ex.Message });
            }
        }

        private async Task HandleNotFound(HttpListenerContext context)
        {
            context.Response.StatusCode = 404;
            await WriteJsonResponse(context.Response, new { error = "Endpoint not found" });
        }

        // Helper Methods
        private async Task<string> ReadRequestBody(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
                return string.Empty;

            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            return await reader.ReadToEndAsync();
        }

        private async Task WriteResponse(HttpListenerResponse response, string content)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
        }

        private async Task WriteJsonResponse(HttpListenerResponse response, object data)
        {
            response.ContentType = "application/json";
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
            await WriteResponse(response, json);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _listener?.Stop();
            _listener?.Close();
        }
    }
}