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

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    // Database
    services.AddDbContext<ErrorLogDbContext>(options =>
        options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

    // Redis
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = context.Configuration.GetConnectionString("Redis");
        options.InstanceName = "SmartLogAnalyzer:";
    });

    // Repositories & Services
    services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
    services.AddSingleton<IRedisCacheService, RedisCacheService>();
    
    // Semantic Kernel
    services.AddSingleton<Kernel>(sp =>
    {
        var deploymentName = context.Configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4o";
        var endpoint = context.Configuration["AzureOpenAI:Endpoint"] ?? "https://your-resource.openai.azure.com/";
        var apiKey = context.Configuration["AzureOpenAI:ApiKey"] ?? "your-api-key";
        
        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey)
            .Build();
        return kernel;
    });
    services.AddScoped<IAiAnalysisService, AiAnalysisService>();

    // Hangfire
    services.AddHangfire(config => config.UseRedisStorage(context.Configuration.GetConnectionString("Redis")));
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
