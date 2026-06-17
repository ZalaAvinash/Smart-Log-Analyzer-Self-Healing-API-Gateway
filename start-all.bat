@echo off
echo Starting Smart Log Analyzer and Self-Healing API Gateway...
echo.

echo Starting Worker Service...
start "Worker Service" cmd /c "cd /d ""D:\Avinash\Projects\Smart Log Analyzer & Self-Healing API Gateway\SmartLogAnalyzer.Worker"" && dotnet run"

echo Waiting for Worker to start...
timeout /t 5 /nobreak > nul

echo Starting API Gateway...
start "API Gateway" cmd /c "cd /d ""D:\Avinash\Projects\Smart Log Analyzer & Self-Healing API Gateway\SmartLogAnalyzer.Api"" && dotnet run"

echo Waiting for API to start...
timeout /t 5 /nobreak > nul

echo Starting Dashboard...
start "Dashboard" cmd /c "cd /d ""D:\Avinash\Projects\Smart Log Analyzer & Self-Healing API Gateway\SmartLogAnalyzer.Dashboard\smart-log-analyzer-dashboard"" && node node_modules\react-scripts\bin\react-scripts.js start"

echo.
echo All services started!
echo.
echo API:      http://localhost:5206
echo Dashboard: http://localhost:3000
echo.
echo To trigger a test error, visit: http://localhost:5206/api/triggererror
echo.
pause
