@echo off
echo ================================================
echo OBS Checklist Editor - Build and Run
echo ================================================
echo.

REM Check if .NET is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK is not installed!
    echo.
    echo Please install .NET 6.0 or later from:
    echo https://dotnet.microsoft.com/download
    echo.
    pause
    exit /b 1
)

echo .NET SDK detected!
echo.

REM Build the project
echo Building the project...
dotnet build

if errorlevel 1 (
    echo.
    echo ERROR: Build failed!
    echo.
    pause
    exit /b 1
)

echo.
echo Build successful!
echo.

REM Run the application
echo Launching OBS Checklist Editor...
echo.
dotnet run

pause
