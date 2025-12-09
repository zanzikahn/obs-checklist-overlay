@echo off
REM Git Repository Setup Script for OBS Checklist Overlay
REM Run this from the project directory

echo ============================================
echo Git Repository Setup
echo ============================================
echo.

REM Check if git is installed
git --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Git is not installed or not in PATH
    echo Please install Git from https://git-scm.com/download/win
    pause
    exit /b 1
)

echo Git found: 
git --version
echo.

REM Initialize repository if not already initialized
if not exist ".git" (
    echo Initializing Git repository...
    git init
    echo.
) else (
    echo Git repository already initialized.
    echo.
)

REM Configure git if not already done (optional - user can skip)
echo Checking Git configuration...
git config user.name >nul 2>&1
if errorlevel 1 (
    echo.
    echo Git user not configured. Please enter your details:
    set /p username="Enter your name: "
    set /p email="Enter your email: "
    git config user.name "%username%"
    git config user.email "%email%"
    echo Configuration saved.
    echo.
)

REM Add all files
echo Adding files to Git...
git add .gitignore
git add LICENSE
git add README.md
git add *.cs
git add *.csproj
git add *.bat
git add overlay.html
git add preview.html
git add checklist-data-examples.json
git add QUICKSTART.md
echo.

REM Show status
echo Git Status:
git status
echo.

REM Create initial commit
echo Creating initial commit...
git commit -m "Initial commit: OBS Checklist Overlay

- Windows Forms C# editor with drag-and-drop
- HTML/CSS/JS overlay for OBS
- Multi-list support
- Auto-scroll (seamless infinite and alternate modes)
- Theme editor with live preview
- Progress bars and counters
- Built 100%% with Claude (Sonnet 4)"
echo.

echo ============================================
echo Repository initialized successfully!
echo ============================================
echo.
echo Next steps:
echo 1. Create a new repository on GitHub (https://github.com/new)
echo 2. Name it: obs-checklist-overlay
echo 3. Do NOT initialize with README (we already have one)
echo 4. Run these commands:
echo.
echo    git remote add origin https://github.com/YOUR_USERNAME/obs-checklist-overlay.git
echo    git branch -M main
echo    git push -u origin main
echo.
echo Replace YOUR_USERNAME with your GitHub username
echo.
pause
