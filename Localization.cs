namespace ClipShare;

public static class Localization
{
    private static string _currentLanguage = "en";

    public static string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            _currentLanguage = value;
            LoadLanguage(value);
        }
    }

    private static Dictionary<string, string> _strings = new();

    static Localization()
    {
        LoadLanguage("en");
    }

    private static void LoadLanguage(string lang)
    {
        _strings.Clear();

        switch (lang.ToLower())
        {
            case "fr":
                LoadFrench();
                break;
            case "en":
            default:
                LoadEnglish();
                break;
        }
    }

    public static string Get(string key)
    {
        return _strings.TryGetValue(key, out var value) ? value : key;
    }

    private static void LoadEnglish()
    {
        _strings = new Dictionary<string, string>
        {
            // Tray menu
            ["Status"] = "Status",
            ["StatusStopped"] = "stopped",
            ["Mode"] = "Mode",
            ["ModeSender"] = "Mode: Sender",
            ["ModeReceiver"] = "Mode: Receiver",
            ["ModeBoth"] = "Mode: Both",
            ["StartSender"] = "Start Sender",
            ["StopSender"] = "Stop Sender",
            ["StartReceiver"] = "Start Receiver",
            ["StopReceiver"] = "Stop Receiver",
            ["Notifications"] = "Notifications",
            ["Language"] = "Language",
            ["EditConfig"] = "Edit Config…",
            ["ReloadConfig"] = "Reload Config",
            ["Exit"] = "Exit",
            
            // Tray tooltip
            ["TrayTooltip"] = "ClipShare: Sender/Receiver over Serial",
            
            // Status messages
            ["Sender"] = "Sender",
            ["Receiver"] = "Receiver",
            ["HK"] = "HK",
            
            // Notifications
            ["NotifSent"] = "ClipShare – Sent",
            ["NotifReceived"] = "ClipShare – Received",
            ["NotifNothingToSend"] = "ClipShare – Nothing to send",
            ["NotifSendFailed"] = "ClipShare – Send failed",
            ["NotifReceiveFailed"] = "ClipShare – Receive failed",
            ["NotifClipboardFailed"] = "ClipShare – Clipboard failed",
            ["NotifNoText"] = "Clipboard contains no text.",
            ["NotifChars"] = "chars",
            
            // Console messages
            ["AppStarting"] = "[ClipShare] Application starting...",
            ["LoadingConfig"] = "[ClipShare] Loading configuration...",
            ["ConfigError"] = "[ClipShare] Config error",
            ["OverridingMode"] = "[ClipShare] Overriding mode",
            ["OverridingSendPort"] = "[ClipShare] Overriding send port",
            ["OverridingRecvPort"] = "[ClipShare] Overriding recv port",
            ["OverridingBaud"] = "[ClipShare] Overriding baud",
            ["OverridingDelay"] = "[ClipShare] Overriding delay",
            ["OverridingHotkey"] = "[ClipShare] Overriding hotkey",
            ["OverridingPreview"] = "[ClipShare] Overriding preview chars",
            ["StartingTrayMode"] = "[ClipShare] Starting in tray mode.",
            ["StartingMode"] = "[ClipShare] Starting in {0} mode.",
            ["InitializingSender"] = "[ClipShare] Initializing sender...",
            ["InitializingReceiver"] = "[ClipShare] Initializing receiver...",
            ["SenderError"] = "[ClipShare] Sender error",
            ["ReceiverError"] = "[ClipShare] Receiver error",
            ["UnknownMode"] = "[ClipShare] Unknown mode",
            ["RunningInMode"] = "[ClipShare] Running in {0} mode. Press Ctrl+C to exit.",
            ["ShuttingDown"] = "[ClipShare] Shutting down...",
            
            // Sender messages
            ["SenderPortOpen"] = "[Sender] Port {0} open @ {1} baud. Hotkey: {2}",
            ["SenderClipboardEmpty"] = "[Sender] Clipboard empty (no text).",
            ["SenderSent"] = "[Sender] Sent {0} chars ({1} bytes).",
            ["SenderError2"] = "[Sender] Error: {0}",
            ["SenderClipboardError"] = "[Sender] Clipboard read error: {0}",
            ["SenderSerialError"] = "[Sender] Serial write error: {0}",
            
            // Receiver messages
            ["ReceiverPortOpen"] = "[Receiver] Port {0} open @ {1} baud.",
            ["ReceiverProcessing"] = "[Receiver] Processing received text: {0} chars",
            ["ReceiverShowingNotif"] = "[Receiver] About to show notification. Enabled: {0}",
            ["ReceiverReceived"] = "[Receiver] Received {0} chars; clipboard updated.",
            ["ReceiverClipboardError"] = "[Receiver] Clipboard write error: {0}",
            ["ReceiverSerialError"] = "[Receiver] Serial read error: {0}",
            ["ReceiverStartError"] = "[Receiver] Start error: {0}",
            
            // TrayApp messages
            ["TrayAppHotkeyError"] = "[TrayApp] Hotkey parse error ({0}), falling back to Ctrl+C: {1}",
            ["TrayAppUnknownMode"] = "[TrayApp] Unknown mode: {0}",
            ["TrayAppStartSenderError"] = "[TrayApp] Start sender error: {0}",
            ["TrayAppStartReceiverError"] = "[TrayApp] Start receiver error: {0}",
            ["TrayAppOpenConfigError"] = "[TrayApp] Open config error: {0}",
            ["TrayAppReloadConfigError"] = "[TrayApp] Reload config error: {0}",
            
            // Config messages
            ["ConfigLoadError"] = "[Config] Load error: {0}",
            ["ConfigSaveError"] = "[Config] Save error: {0}",
            ["ConfigOpenError"] = "[Config] Open error: {0}",
            
            // Toast messages
            ["ToastPushError"] = "[Toast] push error: {0}",
        };
    }

    private static void LoadFrench()
    {
        _strings = new Dictionary<string, string>
        {
            // Tray menu
            ["Status"] = "État",
            ["StatusStopped"] = "arrêté",
            ["Mode"] = "Mode",
            ["ModeSender"] = "Mode : Émetteur",
            ["ModeReceiver"] = "Mode : Récepteur",
            ["ModeBoth"] = "Mode : Les deux",
            ["StartSender"] = "Démarrer l'émetteur",
            ["StopSender"] = "Arrêter l'émetteur",
            ["StartReceiver"] = "Démarrer le récepteur",
            ["StopReceiver"] = "Arrêter le récepteur",
            ["Notifications"] = "Notifications",
            ["Language"] = "Langue",
            ["EditConfig"] = "Modifier la config…",
            ["ReloadConfig"] = "Recharger la config",
            ["Exit"] = "Quitter",
            
            // Tray tooltip
            ["TrayTooltip"] = "ClipShare : Émetteur/Récepteur série",
            
            // Status messages
            ["Sender"] = "Émetteur",
            ["Receiver"] = "Récepteur",
            ["HK"] = "RC",
            
            // Notifications
            ["NotifSent"] = "ClipShare – Envoyé",
            ["NotifReceived"] = "ClipShare – Reçu",
            ["NotifNothingToSend"] = "ClipShare – Rien à envoyer",
            ["NotifSendFailed"] = "ClipShare – Échec d'envoi",
            ["NotifReceiveFailed"] = "ClipShare – Échec de réception",
            ["NotifClipboardFailed"] = "ClipShare – Échec du presse-papiers",
            ["NotifNoText"] = "Le presse-papiers ne contient aucun texte.",
            ["NotifChars"] = "caractères",
            
            // Console messages
            ["AppStarting"] = "[ClipShare] Démarrage de l'application...",
            ["LoadingConfig"] = "[ClipShare] Chargement de la configuration...",
            ["ConfigError"] = "[ClipShare] Erreur de configuration",
            ["OverridingMode"] = "[ClipShare] Remplacement du mode",
            ["OverridingSendPort"] = "[ClipShare] Remplacement du port d'envoi",
            ["OverridingRecvPort"] = "[ClipShare] Remplacement du port de réception",
            ["OverridingBaud"] = "[ClipShare] Remplacement du débit",
            ["OverridingDelay"] = "[ClipShare] Remplacement du délai",
            ["OverridingHotkey"] = "[ClipShare] Remplacement du raccourci",
            ["OverridingPreview"] = "[ClipShare] Remplacement des caractères d'aperçu",
            ["StartingTrayMode"] = "[ClipShare] Démarrage en mode barre des tâches.",
            ["StartingMode"] = "[ClipShare] Démarrage en mode {0}.",
            ["InitializingSender"] = "[ClipShare] Initialisation de l'émetteur...",
            ["InitializingReceiver"] = "[ClipShare] Initialisation du récepteur...",
            ["SenderError"] = "[ClipShare] Erreur émetteur",
            ["ReceiverError"] = "[ClipShare] Erreur récepteur",
            ["UnknownMode"] = "[ClipShare] Mode inconnu",
            ["RunningInMode"] = "[ClipShare] Exécution en mode {0}. Appuyez sur Ctrl+C pour quitter.",
            ["ShuttingDown"] = "[ClipShare] Arrêt...",
            
            // Sender messages
            ["SenderPortOpen"] = "[Émetteur] Port {0} ouvert @ {1} bauds. Raccourci : {2}",
            ["SenderClipboardEmpty"] = "[Émetteur] Presse-papiers vide (pas de texte).",
            ["SenderSent"] = "[Émetteur] Envoyé {0} caractères ({1} octets).",
            ["SenderError2"] = "[Émetteur] Erreur : {0}",
            ["SenderClipboardError"] = "[Émetteur] Erreur de lecture du presse-papiers : {0}",
            ["SenderSerialError"] = "[Émetteur] Erreur d'écriture série : {0}",
            
            // Receiver messages
            ["ReceiverPortOpen"] = "[Récepteur] Port {0} ouvert @ {1} bauds.",
            ["ReceiverProcessing"] = "[Récepteur] Traitement du texte reçu : {0} caractères",
            ["ReceiverShowingNotif"] = "[Récepteur] Affichage de la notification. Activé : {0}",
            ["ReceiverReceived"] = "[Récepteur] Reçu {0} caractères ; presse-papiers mis à jour.",
            ["ReceiverClipboardError"] = "[Récepteur] Erreur d'écriture du presse-papiers : {0}",
            ["ReceiverSerialError"] = "[Récepteur] Erreur de lecture série : {0}",
            ["ReceiverStartError"] = "[Récepteur] Erreur de démarrage : {0}",
            
            // TrayApp messages
            ["TrayAppHotkeyError"] = "[TrayApp] Erreur d'analyse du raccourci ({0}), utilisation de Ctrl+C : {1}",
            ["TrayAppUnknownMode"] = "[TrayApp] Mode inconnu : {0}",
            ["TrayAppStartSenderError"] = "[TrayApp] Erreur de démarrage de l'émetteur : {0}",
            ["TrayAppStartReceiverError"] = "[TrayApp] Erreur de démarrage du récepteur : {0}",
            ["TrayAppOpenConfigError"] = "[TrayApp] Erreur d'ouverture de la config : {0}",
            ["TrayAppReloadConfigError"] = "[TrayApp] Erreur de rechargement de la config : {0}",
            
            // Config messages
            ["ConfigLoadError"] = "[Config] Erreur de chargement : {0}",
            ["ConfigSaveError"] = "[Config] Erreur de sauvegarde : {0}",
            ["ConfigOpenError"] = "[Config] Erreur d'ouverture : {0}",
            
            // Toast messages
            ["ToastPushError"] = "[Toast] erreur d'affichage : {0}",
        };
    }
}
