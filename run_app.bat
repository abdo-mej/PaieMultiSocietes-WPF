@echo off
cd /d "%~dp0PaieMultiSocietesPro2025"
echo Starting Paie Multi-Societes WPF Final Corrected Pro...
dotnet restore
dotnet build
if %errorlevel% neq 0 pause & exit /b %errorlevel%
dotnet run
pause
