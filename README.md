
# ClipShare

A Windows application for sharing clipboard content between computers over serial connection. Built with C# and .NET 8.

## Features

- 🔄 **Bidirectional clipboard sharing** - Send and receive clipboard text over serial ports
- 🎯 **Global hotkey support** - Trigger clipboard send with customizable keyboard shortcuts (default: Ctrl+Shift+C)
- 🔔 **Toast notifications** - Get notified when clipboard content is sent or received
- � **Multi-language support** - English and French interface (easily extensible)
- �🎨 **System tray integration** - Runs quietly in the background with easy access via tray icon
- ⚙️ **Configurable settings** - Edit all settings via JSON config file
- 📦 **Self-contained** - No .NET runtime installation required

## System Requirements

- Windows 10/11 (x64)
- Serial port (physical or virtual)

## Installation

### Option 1: Windows Installer (Recommended)

1. Download `ClipShare-Setup-1.0.0.exe`
2. Run the installer
3. Choose installation options (desktop icon, startup, etc.)
4. Launch ClipShare from Start Menu or desktop

### Option 2: Portable ZIP

1. Download `ClipShare-v1.0.0-win-x64.zip`
2. Extract to any folder
3. Run `clipshare.exe`

## Usage

### Tray Mode (Default)

Just run `clipshare.exe` - it will start in tray mode with a system tray icon.

**Tray Menu Options:**
- **Mode Selection** - Choose Sender, Receiver, or Both
- **Start/Stop** - Control sender and receiver independently
- **Notifications** - Toggle toast notifications on/off
- **Language** - Switch between English and French
- **Edit Config** - Open configuration file in Notepad
- **Reload Config** - Apply configuration changes without restart
- **Exit** - Close the application

### Command Line Mode

Run with `--tray false` to run in console mode:

**Sender Mode** (captures clipboard and sends):
```bash
clipshare.exe --mode sender --send-port COM1 --baud 115200 --hotkey "Ctrl+Shift+C" --tray false
```

**Receiver Mode** (receives and sets clipboard):
```bash
clipshare.exe --mode receiver --recv-port COM2 --baud 115200 --tray false
```

**Both Modes** (bidirectional):
```bash
clipshare.exe --mode both --send-port COM1 --recv-port COM2 --baud 115200 --tray false
```

### Command Line Options

| Option | Description | Default |
|--------|-------------|---------|
| `--mode` | Operation mode: `sender`, `receiver`, or `both` | `sender` |
| `--send-port` | COM port for sending | `COM3` |
| `--recv-port` | COM port for receiving | `COM4` |
| `--baud` | Baud rate | `115200` |
| `--delay-ms` | Delay after hotkey before reading clipboard (ms) | `200` |
| `--hotkey` | Global hotkey (e.g., "Ctrl+Shift+C") | `Ctrl+Shift+C` |
| `--notifications` | Enable toast notifications | `true` |
| `--preview-chars` | Max characters in notification preview | `100` |
| `--tray` | Run with system tray UI | `true` |

## Configuration

Configuration file is automatically created at: `%APPDATA%\ClipShare\config.json`

**Example configuration:**
```json
{
  "mode": "both",
  "sendPort": "COM1",
  "recvPort": "COM2",
  "baud": 115200,
  "delayMs": 200,
  "hotkey": "Ctrl+Shift+C",
  "notifications": true,
  "previewChars": 100,
  "language": "en"
}
```

### Editing Configuration

1. Right-click tray icon → **Edit Config…**
2. Modify settings in Notepad
3. Save and close
4. Right-click tray icon → **Reload Config**

### Configuration Options

- **mode** - `sender`, `receiver`, or `both`
- **sendPort** - COM port for sending clipboard (e.g., `COM1`)
- **recvPort** - COM port for receiving clipboard (e.g., `COM2`)
- **baud** - Baud rate for serial communication (e.g., `115200`)
- **delayMs** - Milliseconds to wait after hotkey before reading clipboard (allows time for native Ctrl+C to complete)
- **hotkey** - Global hotkey combination (e.g., `Ctrl+Shift+C`, `Ctrl+Alt+V`)
- **notifications** - `true` to show toast notifications, `false` to disable
- **previewChars** - Maximum characters to show in notification preview
- **language** - Interface language: `en` (English) or `fr` (French)

## How It Works

### Sender Mode
1. Press the configured hotkey (default: **Ctrl+Shift+C**)
2. Application reads clipboard content
3. Text is packaged into a binary frame with length encoding
4. Frame is transmitted over serial port
5. Toast notification confirms send (if enabled)

### Receiver Mode
1. Application listens on serial port
2. Incoming data is parsed to extract text frames
3. Text is written to clipboard
4. Toast notification shows received content preview (if enabled)

### Both Mode
Runs sender and receiver simultaneously for bidirectional clipboard sharing.

## Supported Hotkeys

You can use any combination of modifiers with supported keys:

**Modifiers:** `Ctrl`, `Alt`, `Shift`, `Win`

**Keys:** 
- Letters: `A-Z`
- Numbers: `0-9`
- Function keys: `F1-F12`
- Special: `Insert`, `Delete`

**Examples:**
- `Ctrl+Shift+C`
- `Ctrl+Alt+V`
- `Win+Shift+X`
- `Ctrl+F12`

⚠️ **Important:** Don't use `Ctrl+C` as it will block the normal copy operation. Use `Ctrl+Shift+C` or another combination.

## Serial Connection Setup

### Physical Serial Ports
Connect two computers with a null-modem cable between their serial ports.

### Virtual Serial Ports (for testing)
Use tools like:
- **com0com** (Windows) - Creates virtual COM port pairs
- **Virtual Serial Port Driver** - Commercial option with more features

## Protocol Details

ClipShare uses a simple binary protocol:

```
[STX][CLIP][US][TEXT][RS][LENGTH:4bytes][PAYLOAD][ETX]
```

- **STX** (0x02) - Start of transmission
- **CLIP** - Message type identifier
- **US** (0x1F) - Unit separator
- **TEXT** - Content type identifier
- **RS** (0x1E) - Record separator
- **LENGTH** - 4-byte little-endian payload length
- **PAYLOAD** - UTF-8 encoded text
- **ETX** (0x03) - End of transmission

## Building from Source

### Prerequisites
- .NET 8 SDK
- Windows 10/11
- (Optional) Inno Setup for creating installer

### Build Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/sdriton/clipshare.git
   cd clipshare-cs
   ```

2. **Build**
   ```bash
   dotnet build -c Release
   ```

3. **Publish self-contained executable**
   ```bash
   .\publish.ps1
   ```
   Output: `publish\win-x64\clipshare.exe`

4. **(Optional) Create installer**
   ```bash
   iscc setup.iss
   ```
   Output: `installer\ClipShare-Setup-1.0.0.exe`

See [BUILD.md](BUILD.md) for detailed build instructions.

## Troubleshooting

### Application doesn't start
- Check if .NET 8 runtime is installed (not needed for self-contained builds)
- Run from command line to see error messages

### Hotkey doesn't work
- Make sure hotkey is not used by another application
- Avoid `Ctrl+C` - use `Ctrl+Shift+C` instead
- Try a different key combination

### Serial port errors
- Verify COM port exists in Device Manager
- Ensure no other application is using the port
- Check baud rate matches on both sides
- Try different COM ports

### Clipboard not updating
- Check Windows notification area for error messages
- Verify serial connection is working
- Enable notifications to see send/receive status

### Notifications not showing
- Check Windows notification settings
- Toggle notifications in tray menu
- Verify `notifications: true` in config.json

## License

This project is provided as-is for educational and personal use.
