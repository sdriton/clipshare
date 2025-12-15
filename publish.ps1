# ClipShare Build and Publish Script

Write-Host "Building ClipShare..." -ForegroundColor Cyan

# Clean previous builds
if (Test-Path "bin\Release") {
    Remove-Item -Path "bin\Release" -Recurse -Force
}
if (Test-Path "publish") {
    Remove-Item -Path "publish" -Recurse -Force
}

# Build and publish self-contained executable
Write-Host "Publishing self-contained release..." -ForegroundColor Cyan
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\win-x64

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild successful!" -ForegroundColor Green
    Write-Host "Output location: publish\win-x64\clipshare.exe" -ForegroundColor Green
    
    # Copy additional files
    if (Test-Path "assets\icon.ico") {
        New-Item -ItemType Directory -Path "publish\win-x64\assets" -Force | Out-Null
        Copy-Item "assets\icon.ico" "publish\win-x64\assets\icon.ico" -Force
    }
    
    if (Test-Path "README.md") {
        Copy-Item "README.md" "publish\win-x64\README.md" -Force
    }
    
    Write-Host "`nTo create installer, run: iscc setup.iss" -ForegroundColor Yellow
} else {
    Write-Host "`nBuild failed!" -ForegroundColor Red
    exit 1
}
