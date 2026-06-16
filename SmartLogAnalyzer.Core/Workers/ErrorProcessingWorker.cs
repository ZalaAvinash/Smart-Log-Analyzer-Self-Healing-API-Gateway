using Hangfire;
using Microsoft.AspNetCore.SignalR;
using SmartLogAnalyzer.Core.Hubs;
using SmartLogAnalyzer.Core.Interfaces;
using SmartLogAnalyzer.Core.Models;
using System.Security.Cryptography;
using System.Text;

namespace SmartLogAnalyzer.Core.Workers
{
    public class ErrorProcessingWorker
    {
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IAiAnalysisService _aiAnalysisService;
        private readonly IHubContext<ErrorHub> _hubContext;

        public ErrorProcessingWorker(
            IErrorLogRepository errorLogRepository,
            IRedisCacheService redisCacheService,
            IAiAnalysisService aiAnalysisService,
            IHubContext<ErrorHub> hubContext)
        {
            _errorLogRepository = errorLogRepository;
            _redisCacheService = redisCacheService;
            _aiAnalysisService = aiAnalysisService;
            _hubContext = hubContext;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task ProcessErrorAsync(ErrorLog errorLog)
        {
            // 1. Generate Hash
            var stackTraceHash = ComputeHash(errorLog.StackTrace);

            // 2. Check Redis
            if (await _redisCacheService.KeyExistsAsync(stackTraceHash))
            {
                // Duplicate error, just update count in DB
                var existingLog = await _errorLogRepository.AddOrUpdateErrorLogAsync(errorLog);
                await _hubContext.Clients.All.SendAsync("ReceiveErrorUpdate", System.Text.Json.JsonSerializer.Serialize(existingLog));
                return;
            }

            // 3. New Error: Save to Redis
            await _redisCacheService.SetKeyAsync(stackTraceHash, "1", TimeSpan.FromHours(24));

            // 4. Call AI
            var analyzedLog = await _aiAnalysisService.AnalyzeErrorAsync(errorLog);

            // 5. Save to DB
            var savedLog = await _errorLogRepository.AddOrUpdateErrorLogAsync(analyzedLog);

            // 6. Push to Dashboard
            await _hubContext.Clients.All.SendAsync("ReceiveErrorUpdate", System.Text.Json.JsonSerializer.Serialize(savedLog));
        }

        private string ComputeHash(string input)
        {
            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}