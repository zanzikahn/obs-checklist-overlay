@echo off
REM Cleanup Script - Remove Development Documentation Files
REM Run this from the project directory

echo ============================================
echo Cleaning up development files...
echo ============================================
echo.

REM Remove development documentation
if exist "ALTERNATE_LISTS_FIXED.md" del "ALTERNATE_LISTS_FIXED.md"
if exist "AUTO_HEIGHT_FILE_LOGGING.md" del "AUTO_HEIGHT_FILE_LOGGING.md"
if exist "AUTO_SCROLL_COMPLETE.md" del "AUTO_SCROLL_COMPLETE.md"
if exist "BUILD_ERRORS_FIXED.md" del "BUILD_ERRORS_FIXED.md"
if exist "CLEANUP_GUIDE.md" del "CLEANUP_GUIDE.md"
if exist "CRITICAL_FIXES.md" del "CRITICAL_FIXES.md"
if exist "DYNAMIC_HEIGHT_FINAL.md" del "DYNAMIC_HEIGHT_FINAL.md"
if exist "FINAL_HEADER_SCROLL_FIXES.md" del "FINAL_HEADER_SCROLL_FIXES.md"
if exist "FIXES_APPLIED.md" del "FIXES_APPLIED.md"
if exist "INFINITE_SCROLL_COMPLETE.md" del "INFINITE_SCROLL_COMPLETE.md"
if exist "NEW_FEATURES.md" del "NEW_FEATURES.md"
if exist "OVERLAY_FIXED.md" del "OVERLAY_FIXED.md"
if exist "SEAMLESS_SCROLL_REQUIREMENTS.md" del "SEAMLESS_SCROLL_REQUIREMENTS.md"
if exist "SETTINGS_CLEANUP_COMPLETE.md" del "SETTINGS_CLEANUP_COMPLETE.md"
if exist "THREE_FIXES_COMPLETE.md" del "THREE_FIXES_COMPLETE.md"
if exist "TWO_BUGS_FIXED.md" del "TWO_BUGS_FIXED.md"
if exist "TWO_LISTS_SCROLL_FIXED.md" del "TWO_LISTS_SCROLL_FIXED.md"
if exist "UI_FIXES_COMPLETE.md" del "UI_FIXES_COMPLETE.md"
if exist "VIEWPORT_HEIGHT_FEATURE.md" del "VIEWPORT_HEIGHT_FEATURE.md"

echo Development documentation removed.
echo.

REM Remove test and log files
if exist "test-overlay.html" del "test-overlay.html"
if exist "overlay-log.txt" del "overlay-log.txt"

echo Test files removed.
echo.

REM Remove user data file (should be in .gitignore anyway)
if exist "checklist-data.json" (
    echo.
    echo WARNING: checklist-data.json found
    echo This contains your personal checklist data.
    echo It will NOT be committed to Git (in .gitignore)
    echo Do you want to delete it? (Y/N^)
    set /p delete_data="> "
    if /i "%delete_data%"=="Y" del "checklist-data.json"
)

echo.
echo ============================================
echo Cleanup complete!
echo ============================================
echo.
echo Files remaining:
dir /b
echo.
echo Project is now ready for Git!
echo Run SETUP_GIT.bat to initialize the repository.
echo.
pause
