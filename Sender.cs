using System.Runtime.InteropServices;
using System.IO.Ports;
using System.Windows.Forms;

namespace ClipShare;

public class HotkeyInfo
{
    public uint Modifiers { get; set; }
    public uint VirtualKey { get; set; }
    public string Label { get; set; } = "";
}

public static class HotkeyParser
{
    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;

    public static HotkeyInfo Parse(string hotkeyString)
    {
        var parts = hotkeyString.ToLower().Split('+');
        uint modifiers = 0;
        string? key = null;

        foreach (var part in parts)
        {
            var p = part.Trim();
            switch (p)
            {
                case "ctrl":
                case "control":
                    modifiers |= MOD_CONTROL;
                    break;
                case "alt":
                    modifiers |= MOD_ALT;
                    break;
                case "shift":
                    modifiers |= MOD_SHIFT;
                    break;
                case "win":
                case "super":
                    modifiers |= MOD_WIN;
                    break;
                default:
                    key = p;
                    break;
            }
        }

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException($"Invalid hotkey: {hotkeyString}");

        var vk = KeyToVirtualKey(key);
        if (vk == 0)
            throw new ArgumentException($"Unsupported key: {key}");

        return new HotkeyInfo
        {
            Modifiers = modifiers,
            VirtualKey = vk,
            Label = hotkeyString
        };
    }

    private static uint KeyToVirtualKey(string key)
    {
        if (key.Length == 1)
        {
            char c = key[0];
            if (c >= 'a' && c <= 'z')
                return (uint)(c - 'a' + 'A');
            if (c >= '0' && c <= '9')
                return (uint)c;
        }

        return key.ToLower() switch
        {
            "c" => 'C',
            "v" => 'V',
            "x" => 'X',
            "insert" => 0x2D,
            "delete" => 0x2E,
            "f1" => 0x70,
            "f2" => 0x71,
            "f3" => 0x72,
            "f4" => 0x73,
            "f5" => 0x74,
            "f6" => 0x75,
            "f7" => 0x76,
            "f8" => 0x77,
            "f9" => 0x78,
            "f10" => 0x79,
            "f11" => 0x7A,
            "f12" => 0x7B,
            _ => 0
        };
    }
}

public class Sender : IDisposable
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int HOTKEY_ID = 1;
    private const int WM_HOTKEY = 0x0312;

    private readonly string _portName;
    private readonly int _baud;
    private readonly int _delayMs;
    private readonly HotkeyInfo _hotkey;
    private bool _notifications;
    private int _previewChars;

    private SerialPort? _port;
    private HotkeyMessageWindow? _messageWindow;
    private bool _started;
    private readonly object _lock = new();

    public bool Started => _started;
    public bool Notifications { get => _notifications; set => _notifications = value; }
    public int PreviewChars { get => _previewChars; set => _previewChars = value; }

    public Sender(string portName, int baud, int delayMs, HotkeyInfo hotkey, bool notifications, int previewChars)
    {
        _portName = portName;
        _baud = baud;
        _delayMs = delayMs;
        _hotkey = hotkey;
        _notifications = notifications;
        _previewChars = previewChars;
    }

    public void Start()
    {
        lock (_lock)
        {
            if (_started)
                return;

            try
            {
                _port = new SerialPort(_portName, _baud, Parity.None, 8, StopBits.One);
                _port.Open();
                Console.WriteLine($"[Sender] Port {_portName} open @ {_baud} baud. Hotkey: {_hotkey.Label}");

                _messageWindow = new HotkeyMessageWindow(OnHotkeyPressed);
                if (!RegisterHotKey(_messageWindow.Handle, HOTKEY_ID, _hotkey.Modifiers, _hotkey.VirtualKey))
                {
                    throw new Exception("Failed to register hotkey");
                }

                _started = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Sender] Start error: {ex.Message}");
                _port?.Dispose();
                _port = null;
                _messageWindow?.Dispose();
                _messageWindow = null;
                throw;
            }
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (!_started)
                return;

            if (_messageWindow != null)
            {
                UnregisterHotKey(_messageWindow.Handle, HOTKEY_ID);
                _messageWindow.Dispose();
                _messageWindow = null;
            }

            _port?.Close();
            _port?.Dispose();
            _port = null;

            _started = false;
        }
    }

    private void OnHotkeyPressed()
    {
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(_delayMs);

                string text = "";
                var staThread = new Thread(() =>
                {
                    try
                    {
                        text = Clipboard.GetText(TextDataFormat.UnicodeText);
                        if (string.IsNullOrEmpty(text))
                        {
                            text = Clipboard.GetText(TextDataFormat.Text);
                        }
                    }
                    catch
                    {
                        text = "";
                    }
                });
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();
                staThread.Join();

                if (string.IsNullOrEmpty(text))
                {
                    Console.WriteLine(Localization.Get("SenderClipboardEmpty"));
                    NotificationHelper.Show(_notifications, Localization.Get("NotifNothingToSend"), Localization.Get("NotifNoText"));
                    return;
                }

                var frame = Protocol.BuildTextFrame(text);
                _port?.Write(frame, 0, frame.Length);

                var preview = NotificationHelper.PreviewText(text, _previewChars);
                NotificationHelper.Show(_notifications, Localization.Get("NotifSent"), $"{text.Length} {Localization.Get("NotifChars")} → {_portName}\n{preview}");
                Console.WriteLine(string.Format(Localization.Get("SenderSent"), text.Length, frame.Length));
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format(Localization.Get("SenderError2"), ex.Message));
                NotificationHelper.Show(_notifications, Localization.Get("NotifSendFailed"), string.Format(Localization.Get("SenderError2"), ex.Message));
            }
        });
    }

    public void Dispose()
    {
        Stop();
    }

    private class HotkeyMessageWindow : NativeWindow, IDisposable
    {
        private readonly Action _hotkeyCallback;

        public HotkeyMessageWindow(Action hotkeyCallback)
        {
            _hotkeyCallback = hotkeyCallback;
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                _hotkeyCallback();
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            DestroyHandle();
        }
    }
}
