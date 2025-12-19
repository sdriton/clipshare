using System.Windows.Forms;
using ClipShare;

Logger.Initialize();
Logger.Log("[ClipShare] Application starting...");

// Parse command line arguments
var cmdArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();

string? mode = null;
string? sendPort = null;
string? recvPort = null;
int? baud = null;
int? delayMs = null;
string? hotkeyStr = null;
int? previewChars = null;
bool? notifications = null;
bool tray = true;

for (int i = 0; i < cmdArgs.Length; i++)
{
    var arg = cmdArgs[i];
    switch (arg.ToLower())
    {
        case "--mode":
        case "-mode":
            mode = i + 1 < cmdArgs.Length ? cmdArgs[++i] : null;
            break;
        case "--send-port":
        case "-send-port":
            sendPort = i + 1 < cmdArgs.Length ? cmdArgs[++i] : null;
            break;
        case "--recv-port":
        case "-recv-port":
            recvPort = i + 1 < cmdArgs.Length ? cmdArgs[++i] : null;
            break;
        case "--baud":
        case "-baud":
            if (i + 1 < cmdArgs.Length && int.TryParse(cmdArgs[++i], out int b))
                baud = b;
            break;
        case "--delay-ms":
        case "-delay-ms":
            if (i + 1 < cmdArgs.Length && int.TryParse(cmdArgs[++i], out int d))
                delayMs = d;
            break;
        case "--hotkey":
        case "-hotkey":
            hotkeyStr = i + 1 < cmdArgs.Length ? cmdArgs[++i] : null;
            break;
        case "--preview-chars":
        case "-preview-chars":
            if (i + 1 < cmdArgs.Length && int.TryParse(cmdArgs[++i], out int p))
                previewChars = p;
            break;
        case "--notifications":
        case "-notifications":
            if (i + 1 < cmdArgs.Length && bool.TryParse(cmdArgs[++i], out bool n))
                notifications = n;
            break;
        case "--tray":
        case "-tray":
            if (i + 1 < cmdArgs.Length && bool.TryParse(cmdArgs[++i], out bool t))
                tray = t;
            break;
    }
}

Logger.Log("[ClipShare] Loading configuration...");
var config = Config.Load();

// Apply overrides
if (mode != null)
{
    Logger.Log($"[ClipShare] Overriding mode: {mode}");
    config.Mode = mode;
}
if (sendPort != null)
{
    Logger.Log($"[ClipShare] Overriding send port: {sendPort}");
    config.SendPort = sendPort;
}
if (recvPort != null)
{
    Logger.Log($"[ClipShare] Overriding recv port: {recvPort}");
    config.RecvPort = recvPort;
}
if (baud.HasValue)
{
    Logger.Log($"[ClipShare] Overriding baud: {baud}");
    config.Baud = baud.Value;
}
if (delayMs.HasValue)
{
    Logger.Log($"[ClipShare] Overriding delay: {delayMs} ms");
    config.DelayMs = delayMs.Value;
}
if (hotkeyStr != null)
{
    Logger.Log($"[ClipShare] Overriding hotkey: {hotkeyStr}");
    config.Hotkey = hotkeyStr;
}
if (previewChars.HasValue)
{
    Logger.Log($"[ClipShare] Overriding preview chars: {previewChars}");
    config.PreviewChars = previewChars.Value;
}
if (notifications.HasValue)
{
    config.Notifications = notifications.Value;
}

config.Save();

if (tray)
{
    Logger.Log("[ClipShare] Starting in tray mode.");
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.Run(new TrayApp(config));
}
else
{
    // Non-tray: run per mode in foreground (console)
    Logger.Log($"[ClipShare] Starting in {config.Mode} mode.");
    
    HotkeyInfo hotkey;
    try
    {
        hotkey = HotkeyParser.Parse(config.Hotkey);
    }
    catch (Exception ex)
    {
        Logger.LogError($"[ClipShare] Hotkey parse error ({config.Hotkey}), fallback Ctrl+C: {ex.Message}");
        hotkey = HotkeyParser.Parse("Ctrl+C");
    }

    using var sender = new Sender(config.SendPort, config.Baud, config.DelayMs, hotkey, config.Notifications, config.PreviewChars);
    using var receiver = new Receiver(config.RecvPort, config.Baud, config.Notifications, config.PreviewChars);

    switch (config.Mode.ToLower())
    {
        case "sender":
            Logger.Log("[ClipShare] Initializing sender...");
            sender.Start();
            break;
        case "receiver":
            Logger.Log("[ClipShare] Initializing receiver...");
            receiver.Start();
            break;
        case "both":
            Logger.Log("[ClipShare] Initializing receiver...");
            receiver.Start();
            Logger.Log("[ClipShare] Initializing sender...");
            sender.Start();
            break;
        default:
            Logger.LogError($"[ClipShare] Unknown mode: {config.Mode}");
            return;
    }

    Logger.Log($"[ClipShare] Running in {config.Mode} mode. Press Ctrl+C to exit.");
    
    // Keep alive - wait for Ctrl+C
    var exitEvent = new ManualResetEvent(false);
    Console.CancelKeyPress += (s, e) =>
    {
        e.Cancel = true;
        exitEvent.Set();
    };
    exitEvent.WaitOne();
    
    Logger.Log("[ClipShare] Shutting down...");
}
