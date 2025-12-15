using System.Windows.Forms;
using ClipShare;

Console.WriteLine("[ClipShare] Application starting...");

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

Console.WriteLine("[ClipShare] Loading configuration...");
var config = Config.Load();

// Apply overrides
if (mode != null)
{
    Console.WriteLine($"[ClipShare] Overriding mode: {mode}");
    config.Mode = mode;
}
if (sendPort != null)
{
    Console.WriteLine($"[ClipShare] Overriding send port: {sendPort}");
    config.SendPort = sendPort;
}
if (recvPort != null)
{
    Console.WriteLine($"[ClipShare] Overriding recv port: {recvPort}");
    config.RecvPort = recvPort;
}
if (baud.HasValue)
{
    Console.WriteLine($"[ClipShare] Overriding baud: {baud}");
    config.Baud = baud.Value;
}
if (delayMs.HasValue)
{
    Console.WriteLine($"[ClipShare] Overriding delay: {delayMs} ms");
    config.DelayMs = delayMs.Value;
}
if (hotkeyStr != null)
{
    Console.WriteLine($"[ClipShare] Overriding hotkey: {hotkeyStr}");
    config.Hotkey = hotkeyStr;
}
if (previewChars.HasValue)
{
    Console.WriteLine($"[ClipShare] Overriding preview chars: {previewChars}");
    config.PreviewChars = previewChars.Value;
}
if (notifications.HasValue)
{
    config.Notifications = notifications.Value;
}

config.Save();

if (tray)
{
    Console.WriteLine("[ClipShare] Starting in tray mode.");
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.Run(new TrayApp(config));
}
else
{
    // Non-tray: run per mode in foreground (console)
    Console.WriteLine($"[ClipShare] Starting in {config.Mode} mode.");
    
    HotkeyInfo hotkey;
    try
    {
        hotkey = HotkeyParser.Parse(config.Hotkey);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ClipShare] Hotkey parse error ({config.Hotkey}), fallback Ctrl+C: {ex.Message}");
        hotkey = HotkeyParser.Parse("Ctrl+C");
    }

    using var sender = new Sender(config.SendPort, config.Baud, config.DelayMs, hotkey, config.Notifications, config.PreviewChars);
    using var receiver = new Receiver(config.RecvPort, config.Baud, config.Notifications, config.PreviewChars);

    switch (config.Mode.ToLower())
    {
        case "sender":
            Console.WriteLine("[ClipShare] Initializing sender...");
            sender.Start();
            break;
        case "receiver":
            Console.WriteLine("[ClipShare] Initializing receiver...");
            receiver.Start();
            break;
        case "both":
            Console.WriteLine("[ClipShare] Initializing receiver...");
            receiver.Start();
            Console.WriteLine("[ClipShare] Initializing sender...");
            sender.Start();
            break;
        default:
            Console.WriteLine($"[ClipShare] Unknown mode: {config.Mode}");
            return;
    }

    Console.WriteLine($"[ClipShare] Running in {config.Mode} mode. Press Ctrl+C to exit.");
    
    // Keep alive - wait for Ctrl+C
    var exitEvent = new ManualResetEvent(false);
    Console.CancelKeyPress += (s, e) =>
    {
        e.Cancel = true;
        exitEvent.Set();
    };
    exitEvent.WaitOne();
    
    Console.WriteLine("[ClipShare] Shutting down...");
}
