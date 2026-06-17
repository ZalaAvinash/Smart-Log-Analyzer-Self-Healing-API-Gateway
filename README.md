# Smart Log Analyzer & Self-Healing API Gateway

## 🏛️ Project Overview
The **Smart Log Analyzer & Self-Healing API Gateway** is a robust, AI-powered solution designed to monitor, diagnose, and suggest fixes for application errors in real-time. Built with a modern .NET 10 backend and a React frontend, it leverages the power of AI (via Groq) to provide "Self-Healing" capabilities.

## 🚀 Features
- **Real-Time Error Monitoring:** Uses SignalR to push error updates instantly to the dashboard.
- **AI-Powered Analysis:** Integrates with Groq AI (Semantic Kernel) to analyze stack traces and error messages.
- **Self-Healing Suggestions:** Provides human-readable root causes, fix suggestions, and code patches.
- **Error Deduplication:** Uses Redis to cache error hashes, preventing redundant AI calls and saving costs.
- **Background Processing:** Utilizes Hangfire for reliable, asynchronous job processing.
- **Clean Architecture:** Separates concerns into API, Worker, Infrastructure, and Core layers.
- **Secure Configuration:** All credentials stored in `.env` file (never committed to git).

## 🛠️ Technology Stack
- **Backend:** ASP.NET Core 10, Worker Service, Entity Framework Core, SignalR, Hangfire, Redis.
- **AI:** Microsoft Semantic Kernel, Groq API (Llama 3.3 70B).
- **Frontend:** React, TypeScript, SignalR Client.
- **Database:** SQL Server.

## 📂 Solution Structure
```
SmartLogAnalyzer.slnx
├── SmartLogAnalyzer.Api          (ASP.NET Core Web API - Gateway)
├── SmartLogAnalyzer.Worker        (Worker Service - Background Processing)
├── SmartLogAnalyzer.Infrastructure (Class Library - Data Access & Services)
├── SmartLogAnalyzer.Core          (Class Library - Models, Interfaces, Hubs)
├── SmartLogAnalyzer.Dashboard     (React App - Live Dashboard)
├── .env                           (Environment credentials - GITIGNORED)
├── .env.example                   (Template for environment variables)
├── start-all.bat                  (Start all services with one click)
├── stop-all.bat                   (Stop all services with one click)
└── start-dashboard.bat            (Start only the dashboard)
```

## ⚙️ Prerequisites
1. **.NET 10 SDK**
2. **Node.js** (LTS version recommended)
3. **SQL Server** (LocalDB or any instance)
4. **Redis Server** (Running and accessible)
5. **Groq API Key** (Free at https://console.groq.com)

## 🔧 Quick Start

### 1. Clone and Configure
```bash
git clone https://github.com/ZalaAvinash/Smart-Log-Analyzer-Self-Healing-API-Gateway.git
cd Smart-Log-Analyzer-Self-Healing-API-Gateway

# Copy environment template
cp .env.example .env

# Edit .env with your credentials
```

### 2. Set Up Environment Variables
Edit the `.env` file with your credentials:
```env
# Redis Configuration
REDIS_HOST=your-redis-server
REDIS_PORT=6379
REDIS_PASSWORD=your-redis-password

# Groq AI Configuration
GROQ_API_KEY=gsk_your_groq_api_key_here
GROQ_MODEL=llama-3.3-70b-versatile

# Database Configuration
DB_SERVER=(localdb)\mssqllocaldb
DB_NAME=SmartLogAnalyzerDb

# API Configuration
API_PORT=5206
DASHBOARD_PORT=3000
```

### 3. Start All Services
```bash
# Windows - One click start
start-all.bat

# Or start manually in separate terminals:
```

**Terminal 1 - Worker Service:**
```bash
cd SmartLogAnalyzer.Worker
dotnet run
```

**Terminal 2 - API Gateway:**
```bash
cd SmartLogAnalyzer.Api
dotnet run
```

**Terminal 3 - Dashboard:**
```bash
cd SmartLogAnalyzer.Dashboard/smart-log-analyzer-dashboard
npm start
```

### 4. Test
1. Open browser: `http://localhost:3000`
2. Trigger test error: `http://localhost:5206/api/triggererror`
3. Watch the dashboard update with AI analysis!

## 🧪 Testing the Application
1. Open your browser and navigate to the React Dashboard (`http://localhost:3000`).
2. Trigger a test error by navigating to the API's trigger endpoint: `http://localhost:5206/api/triggererror`.
3. **Observe the Dashboard:** Within a few seconds, the error will appear in the dashboard.
4. **AI Analysis:** The error card will expand to show the AI-generated Root Cause, Fix Suggestion, and Code Patch.

## 🔌 Integration Guide - Use in Your Own Project

You can integrate the Smart Log Analyzer into any .NET project. Here's how:

### Option 1: Add as Project References

1. **Copy these projects** into your solution:
   - `SmartLogAnalyzer.Core`
   - `SmartLogAnalyzer.Infrastructure`

2. **Add project references** to your main API project:
   ```xml
   <ProjectReference Include="..\SmartLogAnalyzer.Core\SmartLogAnalyzer.Core.csproj" />
   <ProjectReference Include="..\SmartLogAnalyzer.Infrastructure\SmartLogAnalyzer.Infrastructure.csproj" />
   ```

3. **Add the error handling middleware** to your `Program.cs`:
   ```csharp
   using SmartLogAnalyzer.Api.Middleware;
   using SmartLogAnalyzer.Core.Interfaces;
   using SmartLogAnalyzer.Infrastructure.Services;
   
   // Add services
   builder.Services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
   builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
   builder.Services.AddDbContext<ErrorLogDbContext>(options =>
       options.UseSqlServer(connectionString));
   
   // Add Hangfire
   builder.Services.AddHangfire(config => config.UseRedisStorage(redisConnectionString));
   
   // Add middleware
   app.UseMiddleware<ErrorHandlingMiddleware>();
   ```

4. **Add the Worker project** as a separate executable for background processing.

### Option 2: Use as a NuGet Package (Future)
You can package the Core and Infrastructure libraries as NuGet packages for easy distribution.

### Option 3: Standalone Service
Run the Smart Log Analyzer as a standalone service and configure it to receive errors from your main application via HTTP API.

### Required Packages
```xml
<PackageReference Include="Hangfire" Version="1.8.23" />
<PackageReference Include="Hangfire.Redis.StackExchange" Version="1.12.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.9" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="10.0.9" />
<PackageReference Include="Microsoft.SemanticKernel" Version="1.77.0" />
<PackageReference Include="Microsoft.SemanticKernel.Connectors.OpenAI" Version="1.77.0" />
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.2.11" />
```

### AI Provider Configuration
The system supports any OpenAI-compatible API. Configure in `.env`:
```env
# Groq (Free)
GROQ_API_KEY=gsk_your_key
GROQ_MODEL=llama-3.3-70b-versatile

# Or Azure OpenAI
# AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
# AZURE_OPENAI_API_KEY=your-key
# AZURE_OPENAI_DEPLOYMENT=gpt-4o
```

## 📝 License
This project is licensed under the MIT License.