# Smart Log Analyzer & Self-Healing API Gateway

## 🏛️ Project Overview
The **Smart Log Analyzer & Self-Healing API Gateway** is a robust, AI-powered solution designed to monitor, diagnose, and suggest fixes for application errors in real-time. Built with a modern .NET 10 backend and a React frontend, it leverages the power of GPT-4o to provide "Self-Healing" capabilities.

## 🚀 Features
- **Real-Time Error Monitoring:** Uses SignalR to push error updates instantly to the dashboard.
- **AI-Powered Analysis:** Integrates with Azure OpenAI (Semantic Kernel) to analyze stack traces and error messages.
- **Self-Healing Suggestions:** Provides human-readable root causes, fix suggestions, and code patches.
- **Error Deduplication:** Uses Redis to cache error hashes, preventing redundant AI calls and saving costs.
- **Background Processing:** Utilizes Hangfire for reliable, asynchronous job processing.
- **Clean Architecture:** Separates concerns into API, Worker, Infrastructure, and Core layers.

## 🛠️ Technology Stack
- **Backend:** ASP.NET Core Web API, Worker Service, Entity Framework Core, SignalR, Hangfire, Redis.
- **AI:** Microsoft Semantic Kernel, Azure OpenAI (GPT-4o).
- **Frontend:** React, TypeScript, SignalR Client.
- **Database:** SQL Server.

## 📂 Solution Structure
```
SmartLogAnalyzer.sln
├── SmartLogAnalyzer.Api          (ASP.NET Core Web API - Gateway)
├── SmartLogAnalyzer.Worker        (Worker Service - Background Processing)
├── SmartLogAnalyzer.Infrastructure (Class Library - Data Access & Services)
├── SmartLogAnalyzer.Core          (Class Library - Models, Interfaces, Hubs)
└── SmartLogAnalyzer.Dashboard     (React App - Live Dashboard)
```

## ⚙️ Prerequisites
1.  **.NET 10 SDK**
2.  **Node.js** (LTS version recommended)
3.  **SQL Server** (LocalDB or any instance)
4.  **Redis Server** (Running on localhost:6379)
5.  **Azure OpenAI Resource** (Endpoint, API Key, and Deployment Name for GPT-4o)

## 🔧 Configuration

### 1. Database & Redis
Update the connection strings in `appsettings.json` for both `SmartLogAnalyzer.Api` and `SmartLogAnalyzer.Worker`.

**Important:** If your Redis server requires a password, include it in the connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartLogAnalyzerDb;Trusted_Connection=True;MultipleActiveResultSets=true",
  "Redis": "localhost:6379,password=your_password"
}
```
If you are running a local Redis instance without a password (e.g., default Docker image), you can use:
```json
"Redis": "localhost:6379"
```

### 2. Azure OpenAI
Update `SmartLogAnalyzer.Worker/appsettings.json` with your Azure OpenAI credentials:
```json
"AzureOpenAI": {
  "DeploymentName": "gpt-4o",
  "Endpoint": "https://your-resource.openai.azure.com/",
  "ApiKey": "your-api-key"
}
```

## ▶️ How to Run

### Step 1: Start Infrastructure
Ensure **Redis** is running on `localhost:6379`. 
**Note:** If your Redis server requires a password, make sure to update the `ConnectionStrings:Redis` in `appsettings.json` files with the correct password (e.g., `localhost:6379,password=your_password`).

### Step 2: Start the Worker Service
```bash
cd SmartLogAnalyzer.Worker
dotnet run
```

### Step 3: Start the API Gateway
```bash
cd SmartLogAnalyzer.Api
dotnet run
```

### Step 4: Start the React Dashboard
```bash
cd SmartLogAnalyzer.Dashboard/smart-log-analyzer-dashboard
npm start
```

## 🧪 Testing the Application
1.  Open your browser and navigate to the React Dashboard (usually `http://localhost:3000`).
2.  Trigger a test error by navigating to the API's trigger endpoint:
    `https://localhost:7001/api/triggererror` (adjust port if necessary).
3.  **Observe the Dashboard:** Within a few seconds, the error will appear in the dashboard.
4.  **AI Analysis:** The error card will expand to show the AI-generated Root Cause, Fix Suggestion, and Code Patch.

## 📝 License
This project is licensed under the MIT License.