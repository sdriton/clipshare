
# Run on Windows
Copy the built .exe to your Windows machine (or Windows VM):

Receiver side (the machine that will get the text and set its clipboard):

```` bash
.\clipshare.exe --mode receiver --recv-port COM2 --baud 115200
````

Sender side (the machine that captures Ctrl+C and sends text):

```` bash
.\clipshare.exe --mode sender --send-port COM1 --baud 115200 --hotkey "Ctrl+C" --delay-ms 200
````

# Configuration

Config file is created at first run at the location: %APPDATA%\ClipShare\config.json

Edit via the tray menu → Edit Config… (Notepad opens).
Click Reload Config after editing to apply changes.

```` json

{
  "mode": "sender",
  "sendPort": "COM1",
  "recvPort": "COM2",
  "baud": 115200,
  "delayMs": 200,
  "tray": true,
  "hotkey": "Ctrl+Shift+C"
}

````

{
  "mode": "both",
  "sendPort": "COM1",
  "recvPort": "COM2",
  "baud": 115200,
  "delayMs": 200,
  "hotkey": "Ctrl+C",
  "notifications": true,
  ­"tray": true,
  "previewChars": 100
}