using Newtonsoft.Json;
using System.Net;

namespace ExceptionHandlingInNET.ExceptionHandler
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                //calls next method in middleware pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                //to handle the exceptions gracefully in the middleware execution
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new { message = "An unexpected error occurred.", details = exception.Message };
            return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}
