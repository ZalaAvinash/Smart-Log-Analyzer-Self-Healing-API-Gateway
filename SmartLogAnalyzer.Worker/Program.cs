using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using SmartLogAnalyzer.Core.Interfaces;
using SmartLogAnalyzer.Core.Workers;
using SmartLogAnalyzer.Infrastructure.Data;
using SmartLogAnalyzer.Infrastructure.Repositories;
using SmartLogAnalyzer.Infrastructure.Services;
using SmartLogAnalyzer.Worker.Services;

// Load .env file before anything else
EnvLoader.Load();

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    // Database - from env with fallback
    var dbServer = EnvLoader.Get("DB_SERVER") ?? "(localdb)\\mssqllocaldb";
    var dbName = EnvLoader.Get("DB_NAME") ?? "SmartLogAnalyzerDb";
    var connectionString = $"Server={dbServer};Database={dbName};Trusted_Connection=True;MultipleActiveResultSets=true";
    services.AddDbContext<ErrorLogDbContext>(options =>
        options.UseSqlServer(connectionString));

    // Redis - from env with fallback
    var redisHost = EnvLoader.Get("REDIS_HOST") ?? "localhost";
    var redisPort = EnvLoader.Get("REDIS_PORT") ?? "6379";
    var redisPassword = EnvLoader.Get("REDIS_PASSWORD") ?? "";
    var redisConnectionString = $"{redisHost}:{redisPort},password={redisPassword}";

    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "SmartLogAnalyzer:";
    });

    // Repositories & Services
    services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
    services.AddSingleton<IRedisCacheService, RedisCacheService>();
    
    // Semantic Kernel - Using Groq (Free API) - from env with fallback
    var groqApiKey = EnvLoader.Get("GROQ_API_KEY") ?? "your-groq-api-key";
    var groqModel = EnvLoader.Get("GROQ_MODEL") ?? "llama-3.3-70b-versatile";
    
    services.AddSingleton<Kernel>(sp =>
    {
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId: groqModel, apiKey: groqApiKey, endpoint: new Uri("https://api.groq.com/openai/v1"))
            .Build();
        return kernel;
    });
    services.AddScoped<IAiAnalysisService, AiAnalysisService>();

    // Hangfire
    services.AddHangfire(config => config.UseRedisStorage(redisConnectionString));
    services.AddHangfireServer();

    // SignalR
    services.AddSignalR();

    // Worker
    services.AddScoped<ErrorProcessingWorker>();
});

var host = builder.Build();

// Ensure DB is created
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ErrorLogDbContext>();
    db.Database.EnsureCreated();
}

await host.RunAsync();
