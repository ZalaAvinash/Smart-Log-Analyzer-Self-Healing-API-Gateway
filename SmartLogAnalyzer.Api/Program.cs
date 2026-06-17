using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.EntityFrameworkCore;
using SmartLogAnalyzer.Api.Middleware;
using SmartLogAnalyzer.Core.Hubs;
using SmartLogAnalyzer.Core.Interfaces;
using SmartLogAnalyzer.Infrastructure.Data;
using SmartLogAnalyzer.Infrastructure.Repositories;
using SmartLogAnalyzer.Infrastructure.Services;

// Load .env file before anything else
EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database - from env with fallback to appsettings
var dbServer = EnvLoader.Get("DB_SERVER") ?? "(localdb)\\mssqllocaldb";
var dbName = EnvLoader.Get("DB_NAME") ?? "SmartLogAnalyzerDb";
var connectionString = $"Server={dbServer};Database={dbName};Trusted_Connection=True;MultipleActiveResultSets=true";
builder.Services.AddDbContext<ErrorLogDbContext>(options =>
    options.UseSqlServer(connectionString));

// Redis - from env with fallback to appsettings
var redisHost = EnvLoader.Get("REDIS_HOST") ?? "localhost";
var redisPort = EnvLoader.Get("REDIS_PORT") ?? "6379";
var redisPassword = EnvLoader.Get("REDIS_PASSWORD") ?? "";
var redisConnectionString = $"{redisHost}:{redisPort},password={redisPassword}";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "SmartLogAnalyzer:";
});

// Repositories & Services
builder.Services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

// Hangfire (only enqueue jobs, Worker service will process them)
builder.Services.AddHangfire(config => config.UseRedisStorage(redisConnectionString));
// Note: HangfireServer is NOT added here - Worker service handles processing

// SignalR
builder.Services.AddSignalR();

// CORS
var dashboardPort = EnvLoader.Get("DASHBOARD_PORT") ?? "3000";
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins($"http://localhost:{dashboardPort}")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

// Middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();
app.MapHub<ErrorHub>("/errorHub");

// Ensure DB is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ErrorLogDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
