using System.Windows.Forms;
using System.Drawing;

namespace ClipShare;

public class TrayApp : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _contextMenu;
    
    private ToolStripMenuItem _statusItem = null!;
    private ToolStripMenuItem _modeSenderItem = null!;
    private ToolStripMenuItem _modeReceiverItem = null!;
    private ToolStripMenuItem _modeBothItem = null!;
    private ToolStripMenuItem _startSenderItem = null!;
    private ToolStripMenuItem _stopSenderItem = null!;
    private ToolStripMenuItem _startReceiverItem = null!;
    private ToolStripMenuItem _stopReceiverItem = null!;
    private ToolStripMenuItem _notifToggleItem = null!;
    private ToolStripMenuItem _editConfigItem = null!;
    private ToolStripMenuItem _reloadConfigItem = null!;
    private ToolStripMenuItem _exitItem = null!;

    private Config _config;
    private Sender? _sender;
    private Receiver? _receiver;
    private HotkeyInfo _hotkey;

    public TrayApp(Config config)
    {
        _config = config;

        // Parse hotkey
        try
        {
            _hotkey = HotkeyParser.Parse(_config.Hotkey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TrayApp] Hotkey parse error ({_config.Hotkey}), falling back to Ctrl+C: {ex.Message}");
            _hotkey = HotkeyParser.Parse("Ctrl+C");
        }

        _sender = new Sender(_config.SendPort, _config.Baud, _config.DelayMs, _hotkey, _config.Notifications, _config.PreviewChars);
        _receiver = new Receiver(_config.RecvPort, _config.Baud, _config.Notifications, _config.PreviewChars);

        // Create context menu
        _contextMenu = new ContextMenuStrip();
        BuildMenu();

        // Create tray icon
        _trayIcon = new NotifyIcon
        {
            Text = "ClipShare: Sender/Receiver over Serial",
            Visible = true,
            ContextMenuStrip = _contextMenu
        };

        // Try to load icon
        string iconPath = Path.Combine("assets", "icon.ico");
        if (File.Exists(iconPath))
        {
            _trayIcon.Icon = new Icon(iconPath);
        }
        else
        {
            // Use default system icon if custom icon not found
            _trayIcon.Icon = SystemIcons.Application;
        }

        // Apply initial mode
        ApplyMode(_config.Mode);
        UpdateStatus();
    }

    private void BuildMenu()
    {
        _statusItem = new ToolStripMenuItem("Status: stopped");
        _modeSenderItem = new ToolStripMenuItem("Mode: Sender", null, OnModeSender) { CheckOnClick = false };
        _modeReceiverItem = new ToolStripMenuItem("Mode: Receiver", null, OnModeReceiver) { CheckOnClick = false };
        _modeBothItem = new ToolStripMenuItem("Mode: Both", null, OnModeBoth) { CheckOnClick = false };

        _startSenderItem = new ToolStripMenuItem("Start Sender", null, OnStartSender);
        _stopSenderItem = new ToolStripMenuItem("Stop Sender", null, OnStopSender);
        _startReceiverItem = new ToolStripMenuItem("Start Receiver", null, OnStartReceiver);
        _stopReceiverItem = new ToolStripMenuItem("Stop Receiver", null, OnStopReceiver);

        _notifToggleItem = new ToolStripMenuItem("Notifications", null, OnToggleNotifications)
        {
            Checked = _config.Notifications
        };

        _editConfigItem = new ToolStripMenuItem("Edit Config…", null, OnEditConfig);
        _reloadConfigItem = new ToolStripMenuItem("Reload Config", null, OnReloadConfig);
        _exitItem = new ToolStripMenuItem("Exit", null, OnExit);

        _contextMenu.Items.Add(_statusItem);
        _contextMenu.Items.Add(_modeSenderItem);
        _contextMenu.Items.Add(_modeReceiverItem);
        _contextMenu.Items.Add(_modeBothItem);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(_startSenderItem);
        _contextMenu.Items.Add(_stopSenderItem);
        _contextMenu.Items.Add(_startReceiverItem);
        _contextMenu.Items.Add(_stopReceiverItem);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(_notifToggleItem);
        _contextMenu.Items.Add(_editConfigItem);
        _contextMenu.Items.Add(_reloadConfigItem);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(_exitItem);
    }

    private void ApplyMode(string mode)
    {
        switch (mode.ToLower())
        {
            case "sender":
                _sender?.Start();
                break;
            case "receiver":
                _receiver?.Start();
                break;
            case "both":
                _receiver?.Start();
                _sender?.Start();
                break;
            default:
                Console.WriteLine($"[TrayApp] Unknown mode: {mode}");
                break;
        }
    }

    private void UpdateStatus()
    {
        var parts = new List<string>();
        if (_sender?.Started == true)
            parts.Add($"Sender[{_config.SendPort}@{_config.Baud}] HK:{_hotkey.Label}");
        if (_receiver?.Started == true)
            parts.Add($"Receiver[{_config.RecvPort}@{_config.Baud}]");

        var statusText = parts.Count == 0 ? "stopped" : string.Join(" | ", parts);
        _statusItem.Text = $"Mode: {_config.Mode} | {statusText}";

        // Update mode checkmarks
        _modeSenderItem.Checked = _config.Mode.Equals("sender", StringComparison.OrdinalIgnoreCase);
        _modeReceiverItem.Checked = _config.Mode.Equals("receiver", StringComparison.OrdinalIgnoreCase);
        _modeBothItem.Checked = _config.Mode.Equals("both", StringComparison.OrdinalIgnoreCase);
    }

    private void OnModeSender(object? sender, EventArgs e)
    {
        _config.Mode = "sender";
        _config.Save();
        _receiver?.Stop();
        try
        {
            _sender?.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TrayApp] Start sender error: {ex.Message}");
        }
        UpdateStatus();
    }

    private void OnModeReceiver(object? sender, EventArgs e)
    {
        _config.Mode = "receiver";
        _config.Save();
        _sender?.Stop();
        try
        {
            _receiver?.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TrayApp] Start receiver error: {ex.Message}");
        }
        UpdateStatus();
    }

    private void OnModeBoth(object? sender, EventArgs e)
    {
        _config.Mode = "both";
        _config.Save();
        try
        {
            _receiver?.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TrayApp] Start receiver error: {ex.Message}");
        }
        try
        {
            _sender?.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TrayApp] Start sender error: {ex.Message}");
        }
        UpdateStatus();
    }

    private void OnStartSender(object? sender, EventArgs e)
    {
        try
        {
            _sender?.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TrayApp] Start sender error: {ex.Message}");
        }
        UpdateStatus();
    }

    private void OnStopSender(object? sender, EventArgs e)
    {
        _sender?.Stop();
        UpdateStatus();
    }

    private void OnStartReceiver(object? sender, EventArgs e)
    {
        try
        {
            _receiver?.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TrayApp] Start receiver error: {ex.Message}");
        }
        UpdateStatus();
    }

    private void OnStopReceiver(object? sender, EventArgs e)
    {
        _receiver?.Stop();
        UpdateStatus();
    }

    private void OnToggleNotifications(object? sender, EventArgs e)
    {
        _notifToggleItem.Checked = !_notifToggleItem.Checked;
        _config.Notifications = _notifToggleItem.Checked;
        _config.Save();
        
        if (_sender != null)
            _sender.Notifications = _config.Notifications;
        if (_receiver != null)
            _receiver.Notifications = _config.Notifications;
    }

    private void OnEditConfig(object? sender, EventArgs e)
    {
        try
        {
            Config.OpenInNotepad();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TrayApp] Open config error: {ex.Message}");
        }
    }

    private void OnReloadConfig(object? sender, EventArgs e)
    {
        try
        {
            var newConfig = Config.Load();
            _config = newConfig;

            // Stop existing
            _sender?.Stop();

            // Parse new hotkey
            try
            {
                _hotkey = HotkeyParser.Parse(_config.Hotkey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrayApp] Hotkey parse error ({_config.Hotkey}), fallback Ctrl+C: {ex.Message}");
                _hotkey = HotkeyParser.Parse("Ctrl+C");
            }

            // Recreate sender
            _sender = new Sender(_config.SendPort, _config.Baud, _config.DelayMs, _hotkey, _config.Notifications, _config.PreviewChars);

            // Recreate receiver
            bool wasReceiverStarted = _receiver?.Started == true;
            _receiver?.Stop();
            _receiver = new Receiver(_config.RecvPort, _config.Baud, _config.Notifications, _config.PreviewChars);
            if (wasReceiverStarted)
                _receiver.Start();

            // Apply mode
            ApplyMode(_config.Mode);
            UpdateStatus();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TrayApp] Reload config error: {ex.Message}");
        }
    }

    private void OnExit(object? sender, EventArgs e)
    {
        _sender?.Stop();
        _receiver?.Stop();
        _trayIcon.Visible = false;
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sender?.Dispose();
            _receiver?.Dispose();
            _trayIcon?.Dispose();
            _contextMenu?.Dispose();
        }
        base.Dispose(disposing);
    }
}
