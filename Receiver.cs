using System.IO.Ports;
using System.Windows.Forms;

namespace ClipShare;

public class Receiver : IDisposable
{
    private readonly string _portName;
    private readonly int _baud;
    private bool _notifications;
    private int _previewChars;

    private SerialPort? _port;
    private readonly FrameParser _parser = new();
    private Thread? _readThread;
    private bool _started;
    private readonly object _lock = new();
    private volatile bool _shouldStop;

    public bool Started => _started;
    public bool Notifications { get => _notifications; set => _notifications = value; }
    public int PreviewChars { get => _previewChars; set => _previewChars = value; }

    public Receiver(string portName, int baud, bool notifications, int previewChars)
    {
        _portName = portName;
        _baud = baud;
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
                Console.WriteLine($"[Receiver] Port {_portName} open @ {_baud} baud.");

                _shouldStop = false;
                _readThread = new Thread(ReadLoop)
                {
                    IsBackground = true,
                    Name = "ReceiverReadThread"
                };
                _readThread.Start();

                _started = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Receiver] Start error: {ex.Message}");
                _port?.Dispose();
                _port = null;
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

            _shouldStop = true;
            
            _port?.Close();
            _port?.Dispose();
            _port = null;

            _readThread?.Join(1000);
            _readThread = null;

            _started = false;
        }
    }

    private void ReadLoop()
    {
        var buffer = new byte[4096];

        while (!_shouldStop)
        {
            try
            {
                if (_port == null || !_port.IsOpen)
                    break;

                int bytesRead = _port.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    continue;

                var chunk = new byte[bytesRead];
                Array.Copy(buffer, chunk, bytesRead);
                _parser.Feed(chunk);

                while (_parser.TryGetNextText(out string text))
                {
                    try
                    {
                        Console.WriteLine(string.Format(Localization.Get("ReceiverProcessing"), text.Length));
                        var textToSet = text; // Capture for lambda
                        var staThread = new Thread(() =>
                        {
                            try
                            {
                                Clipboard.SetText(textToSet, TextDataFormat.UnicodeText);
                            }
                            catch
                            {
                                Clipboard.SetText(textToSet);
                            }
                        });
                        staThread.SetApartmentState(ApartmentState.STA);
                        staThread.Start();
                        staThread.Join();

                        var preview = NotificationHelper.PreviewText(text, _previewChars);
                        Console.WriteLine(string.Format(Localization.Get("ReceiverShowingNotif"), _notifications));
                        NotificationHelper.Show(_notifications, Localization.Get("NotifReceived"), $"{text.Length} {Localization.Get("NotifChars")} ← {_portName}\n{preview}");
                        Console.WriteLine(string.Format(Localization.Get("ReceiverReceived"), text.Length));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format(Localization.Get("ReceiverClipboardError"), ex.Message));
                        NotificationHelper.Show(_notifications, Localization.Get("NotifClipboardFailed"), string.Format(Localization.Get("ReceiverClipboardError"), ex.Message));
                    }
                }
            }
            catch (TimeoutException)
            {
                // Normal timeout, continue
            }
            catch (Exception ex)
            {
                if (!_shouldStop)
                {
                    Console.WriteLine($"[Receiver] Serial read error: {ex.Message}");
                    NotificationHelper.Show(_notifications, "ClipShare – Receive failed", $"Serial read error: {ex.Message}");
                    Thread.Sleep(300);
                }
            }
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
