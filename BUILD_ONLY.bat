@echo off
echo ================================================
echo OBS Checklist Editor - Build (Fixed)
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

REM Clean previous build
echo Cleaning previous build...
dotnet clean

echo.

REM Build the project
echo Building the project...
dotnet build

if errorlevel 1 (
    echo.
    echo ERROR: Build failed! See errors above.
    echo.
    pause
    exit /b 1
)

echo.
echo ================================================
echo BUILD SUCCESSFUL!
echo ================================================
echo.
echo The application has been compiled successfully.
echo.
echo To run the application:
echo   1. Navigate to: bin\Debug\net6.0-windows\
echo   2. Run: OBSChecklistEditor.exe
echo.
echo Or simply run: dotnet run
echo.

pause
