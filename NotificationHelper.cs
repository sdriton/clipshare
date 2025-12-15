using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace ClipShare;

public static class NotificationHelper
{
    private const string AppId = "ClipShare";

    public static string PreviewText(string text, int maxChars)
    {
        // Collapse whitespace and truncate
        var msg = text.Replace("\r\n", " ")
                      .Replace("\n", " ")
                      .Replace("\t", " ")
                      .Trim();

        if (msg.Length <= maxChars)
            return msg;

        return msg.Substring(0, maxChars) + "…";
    }

    public static void Show(bool enabled, string title, string message)
    {
        if (!enabled)
            return;

        try
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            var stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(title));
            stringElements[1].AppendChild(toastXml.CreateTextNode(message));
            
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier(AppId).Show(toast);
        }
        catch (Exception ex)
        {
            // Don't crash—just log
            Console.WriteLine($"[Toast] push error: {ex.Message}");
        }
    }
}
