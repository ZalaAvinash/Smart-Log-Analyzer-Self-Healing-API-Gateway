# Integration Guide - Smart Log Analyzer

This guide explains how to integrate the Smart Log Analyzer into your own .NET projects.

## Table of Contents
1. [Quick Integration (5 minutes)](#quick-integration)
2. [Full Integration (Recommended)](#full-integration)
3. [Standalone Service Mode](#standalone-service-mode)
4. [Configuration Options](#configuration-options)
5. [Troubleshooting](#troubleshooting)

---

## Quick Integration

### Step 1: Copy Required Projects

Copy these folders into your solution:
```
SmartLogAnalyzer.Core/
SmartLogAnalyzer.Infrastructure/
SmartLogAnalyzer.Api/Middleware/ErrorHandlingMiddleware.cs
```

### Step 2: Add Project References

In your main API `.csproj` file:
```xml
<ItemGroup>
  <ProjectReference Include="..\SmartLogAnalyzer.Core\SmartLogAnalyzer.Core.csproj" />
  <ProjectReference Include="..\SmartLogAnalyzer.Infrastructure\SmartLogAnalyzer.Infrastructure.csproj" />
</ItemGroup>
```

### Step 3: Add Required NuGet Packages

```xml
<ItemGroup>
  <PackageReference Include="Hangfire" Version="1.8.23" />
  <PackageReference Include="Hangfire.Redis.StackExchange" Version="1.12.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.9" />
  <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="10.0.9" />
  <PackageReference Include="Microsoft.SemanticKernel" Version="1.77.0" />
  <PackageReference Include="Microsoft.SemanticKernel.Connectors.OpenAI" Version="1.77.0" />
</ItemGroup>
```

### Step 4: Update Your Program.cs

```csharp
using SmartLogAnalyzer.Api.Middleware;
using SmartLogAnalyzer.Core.Interfaces;
using SmartLogAnalyzer.Core.Hubs;
using SmartLogAnalyzer.Infrastructure.Data;
using SmartLogAnalyzer.Infrastructure.Repositories;
using SmartLogAnalyzer.Infrastructure.Services;
using Hangfire;
using Hangfire.Redis.StackExchange;

var builder = WebApplication.CreateBuilder(args);

// Add Smart Log Analyzer services
builder.Services.AddDbContext<ErrorLogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

// Add Hangfire (only enqueue, Worker processes)
builder.Services.AddHangfire(config => 
    config.UseRedisStorage("your-redis-server:6379,password=your-password"));

// Add SignalR for real-time updates
builder.Services.AddSignalR();

// Add CORS for dashboard
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Add error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseCors();
app.MapHub<ErrorHub>("/errorHub");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ErrorLogDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
```

### Step 5: Create .env File

```env
REDIS_HOST=your-redis-server
REDIS_PORT=6379
REDIS_PASSWORD=your-password
GROQ_API_KEY=gsk_your_groq_key
GROQ_MODEL=llama-3.3-70b-versatile
```

### Step 6: Run the Worker

Start the Worker project in a separate terminal:
```bash
cd SmartLogAnalyzer.Worker
dotnet run
```

---

## Full Integration

For production use, follow these additional steps:

### 1. Create a Shared Database

Update the connection string in both API and Worker to point to the same SQL Server instance.

### 2. Configure Redis

Ensure both API and Worker connect to the same Redis instance.

### 3. Set Up the Worker as a Windows Service

```bash
# Install as Windows Service
sc create SmartLogAnalyzer.Worker binPath="C:\path\to\SmartLogAnalyzer.Worker.exe"
sc start SmartLogAnalyzer.Worker
```

### 4. Deploy the Dashboard

Build the React dashboard:
```bash
cd SmartLogAnalyzer.Dashboard/smart-log-analyzer-dashboard
npm run build
```

Serve the `build` folder using nginx, IIS, or any static file server.

---

## Standalone Service Mode

If you don't want to modify your main application, you can run the Smart Log Analyzer as a standalone service and send errors to it via HTTP.

### 1. Start the Standalone Service

```bash
# Terminal 1
cd SmartLogAnalyzer.Worker
dotnet run

# Terminal 2
cd SmartLogAnalyzer.Api
dotnet run
```

### 2. Send Errors from Your Application

```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class ErrorReportingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;

    public ErrorReportingService(string apiUrl)
    {
        _httpClient = new HttpClient();
        _apiUrl = apiUrl;
    }

    public async Task ReportErrorAsync(Exception ex, string routePath)
    {
        var errorData = new
        {
            ErrorMessage = ex.Message,
            StackTrace = ex.StackTrace,
            RoutePath = routePath,
            Timestamp = DateTime.UtcNow
        };

        var content = new StringContent(
            JsonSerializer.Serialize(errorData),
            Encoding.UTF8,
            "application/json");

        await _httpClient.PostAsync($"{_apiUrl}/api/error/log", content);
    }
}
```

---

## Configuration Options

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `REDIS_HOST` | Redis server hostname | localhost |
| `REDIS_PORT` | Redis server port | 6379 |
| `REDIS_PASSWORD` | Redis password | (empty) |
| `GROQ_API_KEY` | Groq API key | (required) |
| `GROQ_MODEL` | AI model to use | llama-3.3-70b-versatile |
| `DB_SERVER` | SQL Server instance | (localdb)\mssqllocaldb |
| `DB_NAME` | Database name | SmartLogAnalyzerDb |
| `API_PORT` | API port | 5206 |
| `DASHBOARD_PORT` | Dashboard port | 3000 |

### AI Provider Options

The system uses OpenAI-compatible APIs. You can switch providers:

**Groq (Free):**
```env
GROQ_API_KEY=gsk_your_key
GROQ_MODEL=llama-3.3-70b-versatile
```

**OpenAI:**
```env
GROQ_API_KEY=sk_your_openai_key
GROQ_MODEL=gpt-4o
```

**Azure OpenAI:**
Modify the Worker's `Program.cs` to use Azure OpenAI connector instead.

---

## Troubleshooting

### Common Issues

**1. Redis Connection Failed**
- Verify Redis is running: `redis-cli ping`
- Check firewall rules
- Verify password in .env

**2. Database Not Created**
- Ensure SQL Server is running
- Check connection string
- Run `db.Database.EnsureCreated()` manually

**3. AI Analysis Not Working**
- Verify Groq API key is valid
- Check API rate limits
- Review Worker logs for errors

**4. Dashboard Not Updating**
- Verify SignalR hub is mapped
- Check CORS configuration
- Ensure API and Dashboard ports match

### Getting Help

- Check the logs in each service's terminal
- Review the `ErrorLogs` table in SQL Server
- Open an issue on GitHub