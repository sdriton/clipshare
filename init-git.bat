@echo off
echo Initializing Git repository for ClipShare...
echo.

REM Initialize git repository
git init

REM Configure git user if not already set (optional - remove if you want to use global config)
REM git config user.name "Your Name"
REM git config user.email "your.email@example.com"

REM Add all files
git add .

REM Show status
echo.
echo Repository status:
git status

REM Create initial commit
echo.
set /p COMMIT="Create initial commit? (Y/N): "
if /i "%COMMIT%"=="Y" (
    git commit -m "Initial commit - ClipShare C# implementation"
    echo.
    echo Repository initialized and initial commit created!
    echo.
    echo To push to a remote repository, run:
    echo   git remote add origin YOUR_REPOSITORY_URL
    echo   git branch -M main
    echo   git push -u origin main
) else (
    echo.
    echo Files staged but not committed. Run 'git commit -m "message"' when ready.
)

echo.
pause
