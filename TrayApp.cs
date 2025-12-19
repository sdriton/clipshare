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
    private ToolStripMenuItem _languageItem = null!;
    private ToolStripMenuItem _langEnglishItem = null!;
    private ToolStripMenuItem _langFrenchItem = null!;
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
            Logger.LogError($"[TrayApp] Hotkey parse error ({_config.Hotkey}), falling back to Ctrl+C: {ex.Message}");
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
            Text = Localization.Get("TrayTooltip"),
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
        _statusItem = new ToolStripMenuItem(Localization.Get("Status") + ": " + Localization.Get("StatusStopped"));
        _modeSenderItem = new ToolStripMenuItem(Localization.Get("ModeSender"), null, OnModeSender) { CheckOnClick = false };
        _modeReceiverItem = new ToolStripMenuItem(Localization.Get("ModeReceiver"), null, OnModeReceiver) { CheckOnClick = false };
        _modeBothItem = new ToolStripMenuItem(Localization.Get("ModeBoth"), null, OnModeBoth) { CheckOnClick = false };

        _startSenderItem = new ToolStripMenuItem(Localization.Get("StartSender"), null, OnStartSender);
        _stopSenderItem = new ToolStripMenuItem(Localization.Get("StopSender"), null, OnStopSender);
        _startReceiverItem = new ToolStripMenuItem(Localization.Get("StartReceiver"), null, OnStartReceiver);
        _stopReceiverItem = new ToolStripMenuItem(Localization.Get("StopReceiver"), null, OnStopReceiver);

        _notifToggleItem = new ToolStripMenuItem(Localization.Get("Notifications"), null, OnToggleNotifications)
        {
            Checked = _config.Notifications
        };

        _languageItem = new ToolStripMenuItem(Localization.Get("Language"));
        _langEnglishItem = new ToolStripMenuItem("English", null, OnLanguageEnglish) { Checked = _config.Language == "en" };
        _langFrenchItem = new ToolStripMenuItem("Français", null, OnLanguageFrench) { Checked = _config.Language == "fr" };
        _languageItem.DropDownItems.Add(_langEnglishItem);
        _languageItem.DropDownItems.Add(_langFrenchItem);

        _editConfigItem = new ToolStripMenuItem(Localization.Get("EditConfig"), null, OnEditConfig);
        _reloadConfigItem = new ToolStripMenuItem(Localization.Get("ReloadConfig"), null, OnReloadConfig);
        var viewLogItem = new ToolStripMenuItem(Localization.Get("ViewLogFile"), null, OnViewLogFile);
        _exitItem = new ToolStripMenuItem(Localization.Get("Exit"), null, OnExit);

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
        _contextMenu.Items.Add(_languageItem);
        _contextMenu.Items.Add(_editConfigItem);
        _contextMenu.Items.Add(_reloadConfigItem);
        _contextMenu.Items.Add(viewLogItem);
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
                Logger.LogError($"[TrayApp] Unknown mode: {mode}");
                break;
        }
    }

    private void UpdateStatus()
    {
        var parts = new List<string>();
        if (_sender?.Started == true)
            parts.Add($"{Localization.Get("Sender")}[{_config.SendPort}@{_config.Baud}] {Localization.Get("HK")}:{_hotkey.Label}");
        if (_receiver?.Started == true)
            parts.Add($"{Localization.Get("Receiver")}[{_config.RecvPort}@{_config.Baud}]");

        var statusText = parts.Count == 0 ? Localization.Get("StatusStopped") : string.Join(" | ", parts);
        _statusItem.Text = $"{Localization.Get("Mode")}: {_config.Mode} | {statusText}";

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
            Logger.LogError($"[TrayApp] Start sender error: {ex.Message}");
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
            Logger.LogError($"[TrayApp] Start receiver error: {ex.Message}");
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
            Logger.LogError($"[TrayApp] Start receiver error: {ex.Message}");
        }
        try
        {
            _sender?.Start();
        }
        catch (Exception ex)
        {
            Logger.LogError($"[TrayApp] Start sender error: {ex.Message}");
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
            Logger.LogError($"[TrayApp] Start sender error: {ex.Message}");
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
            Logger.LogError($"[TrayApp] Start receiver error: {ex.Message}");
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
            Logger.LogError($"[TrayApp] Open config error: {ex.Message}");
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
                Logger.LogError($"[TrayApp] Hotkey parse error ({_config.Hotkey}), fallback Ctrl+C: {ex.Message}");
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
            
            // Refresh menu with new language
            RefreshMenu();
            UpdateStatus();
        }
        catch (Exception ex)
        {
            Logger.LogError($"[TrayApp] Reload config error: {ex.Message}");
        }
    }

    private void OnViewLogFile(object? sender, EventArgs e)
    {
        try
        {
            var logPath = Logger.GetLogFilePath();
            if (File.Exists(logPath))
                System.Diagnostics.Process.Start("notepad.exe", logPath);
            else
                Logger.LogWarning($"[TrayApp] Log file not found: {logPath}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"[TrayApp] Open log file error: {ex.Message}");
        }
    }

    private void OnLanguageEnglish(object? sender, EventArgs e)
    {
        _config.Language = "en";
        Localization.CurrentLanguage = "en";
        _config.Save();
        RefreshMenu();
        UpdateStatus();
    }

    private void OnLanguageFrench(object? sender, EventArgs e)
    {
        _config.Language = "fr";
        Localization.CurrentLanguage = "fr";
        _config.Save();
        RefreshMenu();
        UpdateStatus();
    }

    private void RefreshMenu()
    {
        _statusItem.Text = Localization.Get("Status") + ": " + Localization.Get("StatusStopped");
        _modeSenderItem.Text = Localization.Get("ModeSender");
        _modeReceiverItem.Text = Localization.Get("ModeReceiver");
        _modeBothItem.Text = Localization.Get("ModeBoth");
        _startSenderItem.Text = Localization.Get("StartSender");
        _stopSenderItem.Text = Localization.Get("StopSender");
        _startReceiverItem.Text = Localization.Get("StartReceiver");
        _stopReceiverItem.Text = Localization.Get("StopReceiver");
        _notifToggleItem.Text = Localization.Get("Notifications");
        _languageItem.Text = Localization.Get("Language");
        _editConfigItem.Text = Localization.Get("EditConfig");
        _reloadConfigItem.Text = Localization.Get("ReloadConfig");
        _exitItem.Text = Localization.Get("Exit");
        
        _langEnglishItem.Checked = _config.Language == "en";
        _langFrenchItem.Checked = _config.Language == "fr";
        
        _trayIcon.Text = Localization.Get("TrayTooltip");
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
