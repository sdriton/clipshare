using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClipShare;

public class Config
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "sender";

    [JsonPropertyName("sendPort")]
    public string SendPort { get; set; } = "COM3";

    [JsonPropertyName("recvPort")]
    public string RecvPort { get; set; } = "COM4";

    [JsonPropertyName("baud")]
    public int Baud { get; set; } = 115200;

    [JsonPropertyName("delayMs")]
    public int DelayMs { get; set; } = 200;

    [JsonPropertyName("hotkey")]
    public string Hotkey { get; set; } = "Ctrl+Shift+C";

    [JsonPropertyName("notifications")]
    public bool Notifications { get; set; } = true;

    [JsonPropertyName("previewChars")]
    public int PreviewChars { get; set; } = 100;

    [JsonPropertyName("language")]
    public string Language { get; set; } = "en";

    [JsonIgnore]
    public DateTime Timestamp { get; set; }

    public static Config Default()
    {
        return new Config
        {
            Mode = "sender",
            SendPort = "COM3",
            RecvPort = "COM4",
            Baud = 115200,
            DelayMs = 200,
            Hotkey = "Ctrl+Shift+C",
            Notifications = true,
            PreviewChars = 100,
            Language = "en",
            Timestamp = DateTime.Now
        };
    }

    public static string GetConfigDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "ClipShare");
    }

    public static string GetConfigPath()
    {
        return Path.Combine(GetConfigDirectory(), "config.json");
    }

    public static Config Load()
    {
        var path = GetConfigPath();
        if (!File.Exists(path))
        {
            var defaultCfg = Default();
            defaultCfg.Save();
            return defaultCfg;
        }

        try
        {
            var json = File.ReadAllText(path);
            var cfg = JsonSerializer.Deserialize<Config>(json) ?? Default();
            if (cfg.PreviewChars <= 0)
                cfg.PreviewChars = 100;
            if (string.IsNullOrEmpty(cfg.Language))
                cfg.Language = "en";
            Localization.CurrentLanguage = cfg.Language;
            cfg.Timestamp = DateTime.Now;
            return cfg;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Config] Load error: {ex.Message}");
            return Default();
        }
    }

    public void Save()
    {
        try
        {
            var dir = GetConfigDirectory();
            Directory.CreateDirectory(dir);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(GetConfigPath(), json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Config] Save error: {ex.Message}");
        }
    }

    public static void OpenInNotepad()
    {
        try
        {
            var path = GetConfigPath();
            System.Diagnostics.Process.Start("notepad.exe", path);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Config] Open error: {ex.Message}");
        }
    }
}
