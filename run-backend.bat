@echo off
title Running StudentFood Backend & Ngrok
cd /d "%~dp0"

echo ==========================================
echo [1/2] Starting StudentFood WebAdmin...
echo ==========================================
start "StudentFood WebAdmin" cmd /k "dotnet run --project StudentFood.WebAdmin\StudentFood.WebAdmin.csproj"

echo.
echo ==========================================
echo [2/2] Starting Ngrok Tunnel...
echo ==========================================
start "Ngrok Tunnel" cmd /k "cd /d D:\tools\ngrok && ngrok.exe http http://localhost:5148"

echo.
echo ==========================================
echo Startup scripts initiated!
echo ==========================================
timeout /t 5
