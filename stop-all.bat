@echo off
echo Stopping all Smart Log Analyzer services...
echo.

echo Stopping Worker Service...
taskkill /FI "WINDOWTITLE eq Worker Service*" /F 2>nul

echo Stopping API Gateway...
taskkill /FI "WINDOWTITLE eq API Gateway*" /F 2>nul

echo Stopping Dashboard...
taskkill /FI "WINDOWTITLE eq Dashboard*" /F 2>nul

echo.
echo All services stopped!
pause