# ClipShare Installation Package

## Building the Installation Package

### Option 1: Self-Contained Executable (Recommended for quick deployment)

Run the publish script:
```powershell
.\publish.ps1
```

This creates a self-contained executable at `publish\win-x64\clipshare.exe` that includes all dependencies. You can distribute this single folder.

### Option 2: Windows Installer (Recommended for distribution)

1. **Download and Install Inno Setup**
   - Download from: https://jrsoftware.org/isdl.php
   - Install Inno Setup Compiler

2. **Build the application**
   ```powershell
   .\publish.ps1
   ```

3. **Create the installer**
   ```powershell
   iscc setup.iss
   ```

4. **Find the installer**
   The installer will be created at: `installer\ClipShare-Setup-1.0.0.exe`

## Manual Build Steps

If you prefer to build manually:

```powershell
# Build release version
dotnet build -c Release

# Publish self-contained (includes .NET runtime)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish\win-x64

# Or publish framework-dependent (requires .NET 8 installed)
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish\win-x64-fd
```

## Distribution Options

1. **Portable** - Just ZIP the `publish\win-x64` folder
2. **Installer** - Use the generated `.exe` from Inno Setup
3. **Microsoft Store** - Package as MSIX (requires additional setup)

## What's Included

- ClipShare executable
- Icon file
- Configuration support (created on first run at `%APPDATA%\ClipShare\config.json`)
- README documentation
