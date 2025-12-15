# Initialize Git repository for ClipShare

Write-Host "Initializing Git repository for ClipShare..." -ForegroundColor Cyan
Write-Host ""

# Check if git is available
if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Host "Git is not installed or not in PATH!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install Git from: https://git-scm.com/download/win" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "After installing Git, run this script again or use init-git.bat" -ForegroundColor Yellow
    pause
    exit 1
}

# Initialize git repository
git init

# Add all files
git add .

# Show status
Write-Host ""
Write-Host "Repository status:" -ForegroundColor Cyan
git status

# Create initial commit
Write-Host ""
$commit = Read-Host "Create initial commit? (Y/N)"
if ($commit -eq 'Y' -or $commit -eq 'y') {
    git commit -m "Initial commit - ClipShare C# implementation"
    Write-Host ""
    Write-Host "Repository initialized and initial commit created!" -ForegroundColor Green
    Write-Host ""
    Write-Host "To push to a remote repository, run:" -ForegroundColor Yellow
    Write-Host "  git remote add origin YOUR_REPOSITORY_URL" -ForegroundColor Gray
    Write-Host "  git branch -M main" -ForegroundColor Gray
    Write-Host "  git push -u origin main" -ForegroundColor Gray
} else {
    Write-Host ""
    Write-Host "Files staged but not committed. Run 'git commit -m `"message`"' when ready." -ForegroundColor Yellow
}

Write-Host ""
pause
