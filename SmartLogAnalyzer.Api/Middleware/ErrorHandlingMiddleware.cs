using Hangfire;
using SmartLogAnalyzer.Core.Models;
using SmartLogAnalyzer.Core.Workers;
using System.Text.Json;

namespace SmartLogAnalyzer.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public ErrorHandlingMiddleware(RequestDelegate next, IBackgroundJobClient backgroundJobClient)
        {
            _next = next;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            var errorLog = new ErrorLog
            {
                ErrorMessage = ex.Message,
                StackTrace = ex.StackTrace ?? string.Empty,
                RoutePath = context.Request.Path
            };

            // Enqueue background job
            _backgroundJobClient.Enqueue<ErrorProcessingWorker>(worker => worker.ProcessErrorAsync(errorLog));

            var result = JsonSerializer.Serialize(new { error = "An internal error has been logged and is being analyzed." });
            await context.Response.WriteAsync(result);
        }
    }
}
