/*
 * OffNet 1.5.1
 * Lightweight Windows 10/11 network-device tray controller.
 *
 * Project: https://github.com/zeittresor/OffNet
 * Author/project reference: https://github.com/zeittresor
 *
 * SPDX-License-Identifier: MIT
 * Copyright (c) 2026 zeittresor
 *
 * MIT License - see LICENSE included with this source package.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Net.NetworkInformation;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace OffNet
{
    internal static class Program
    {
        internal const string AppName = "OffNet";
        internal const string Version = "1.5.1";
        internal const string ProjectUrl = "https://github.com/zeittresor/OffNet";

        [STAThread]
        private static void Main()
        {
            AppSettings settings = ConfigurationStore.LoadSettings();
            Localization.SetLanguage(settings.Language);

            bool createdNew;
            using (Mutex mutex = new Mutex(true, @"Local\OffNet_zeittresor", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show(
                        Localization.T("AlreadyRunning"),
                        AppName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                if (!AdminHelper.IsAdministrator())
                {
                    MessageBox.Show(
                        Localization.T("AdminRequired"),
                        AppName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                using (OffNetApplicationContext context = new OffNetApplicationContext(settings))
                {
                    Application.Run(context);
                }
            }
        }
    }

    internal static class AdminHelper
    {
        internal static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }

    internal sealed class AppSettings
    {
        internal string Language = "en";
        internal int ActiveColorArgb = Color.LimeGreen.ToArgb();
        internal int DisabledColorArgb = Color.Red.ToArgb();
        internal int OfflineColorArgb = Color.Gold.ToArgb();

        // Optional tray traffic overview. Disabled by default to keep the tray menu slim.
        internal bool ShowTrafficOverview = false;
        internal bool ShowTaskbarTrafficMeter = false;
        internal string TaskbarMeterFontName = "Segoe UI Semibold";
        internal bool TaskbarTextShadow = false;
        internal int TrafficHistorySeconds = 60;
        internal int TrafficSamplePoints = 75;
        internal int TrafficScaleRefreshSeconds = 5;
        internal string StatisticsTheme = "Dark";
        internal string ApplicationTheme = "Muffin";
        internal int DownloadColorArgb = Color.DodgerBlue.ToArgb();
        internal int UploadColorArgb = Color.DarkOrange.ToArgb();
        internal int OfflineActivityColorArgb = Color.MediumPurple.ToArgb();

        internal AppSettings Clone()
        {
            AppSettings copy = new AppSettings();
            copy.Language = Language;
            copy.ActiveColorArgb = ActiveColorArgb;
            copy.DisabledColorArgb = DisabledColorArgb;
            copy.OfflineColorArgb = OfflineColorArgb;
            copy.ShowTrafficOverview = ShowTrafficOverview;
            copy.ShowTaskbarTrafficMeter = ShowTaskbarTrafficMeter;
            copy.TaskbarMeterFontName = TaskbarMeterFontName;
            copy.TaskbarTextShadow = TaskbarTextShadow;
            copy.TrafficHistorySeconds = TrafficHistorySeconds;
            copy.TrafficSamplePoints = TrafficSamplePoints;
            copy.TrafficScaleRefreshSeconds = TrafficScaleRefreshSeconds;
            copy.StatisticsTheme = StatisticsTheme;
            copy.ApplicationTheme = ApplicationTheme;
            copy.DownloadColorArgb = DownloadColorArgb;
            copy.UploadColorArgb = UploadColorArgb;
            copy.OfflineActivityColorArgb = OfflineActivityColorArgb;
            return copy;
        }
    }

    internal static class Localization
    {
        private static string currentLanguage = "en";

        private static readonly Dictionary<string, string> En =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"AlreadyRunning", "OffNet is already running in the Windows notification area."},
                {"AdminRequired", "OffNet requires administrator privileges because it enables and disables network devices directly at the Windows PnP/driver level.\r\n\r\nPlease start OffNet again and confirm the UAC prompt."},
                {"NotPresent", "Not present"},
                {"Disabled", "Disabled"},
                {"Enabled", "Enabled"},
                {"ActiveProblem", "Active / problem {0}"},
                {"Hardware", "Hardware"},
                {"VirtualSystem", "Virtual/System"},
                {"MainTitle", "OffNet — network devices at PnP/driver level"},
                {"MainDescription", "Checked under ‘Tray control’ = managed by tray actions. Select a row = one-time action using the buttons below."},
                {"MainLegend", "Tray:  active + Internet   |   disabled   |   blinking = active, but no Internet"},
                {"ShowNonPresent", "Show non-present devices"},
                {"Reload", "Reload"},
                {"ColTray", "Tray control"},
                {"ColStatus", "Status"},
                {"ColType", "Type"},
                {"ColDevice", "Device"},
                {"ColManufacturer", "Manufacturer"},
                {"ColInstance", "Device instance ID"},
                {"SelectedEnable", "Enable selected"},
                {"SelectedDisable", "Disable selected"},
                {"Reconnect", "Reconnect"},
                {"DeviceManager", "Device Manager"},
                {"Ready", "Ready."},
                {"ReadingDevices", "Reading network devices..."},
                {"FoundDevices", "{0} network device(s) found. Managed by tray: {1}."},
                {"ReadError", "Error while reading network devices."},
                {"TraySelectionSaved", "Tray selection saved: {0} device(s)."},
                {"SelectDevice", "Select at least one network device."},
                {"DisableSelectedConfirm", "Really disable the selected network devices persistently at PnP/driver level?\r\n\r\nAn active network or remote session may be disconnected immediately."},
                {"DisableTitle", "OffNet — Disable"},
                {"DeviceNotFound", "Device not found (CONFIGRET={0}):\r\n{1}"},
                {"OperationFailed", "{0} failed (CONFIGRET={1}):\r\n{2}"},
                {"EnableVerb", "Enable"},
                {"DisableVerb", "Disable"},
                {"SetupDiFailed", "SetupDiGetClassDevs failed. Win32 error: {0}"},
                {"EnumFailed", "SetupDiEnumDeviceInfo failed. Win32 error: {0}"},
                {"IpconfigFailed", "ipconfig.exe could not be started."},
                {"TrayActivate", "Activate"},
                {"TrayDisable", "Disable"},
                {"TrayReconnect", "Reconnect"},
                {"TrayOpen", "Open OffNet..."},
                {"TrayOptions", "Options..."},
                {"TrayExit", "Exit"},
                {"FirstRun", "Select which network devices should be managed by the tray actions."},
                {"NoManaged", "No network devices are marked for tray control."},
                {"ActivateDone", "Managed network devices were enabled."},
                {"DisableManagedConfirm", "Really disable the network devices managed by OffNet persistently at PnP/driver level?\r\n\r\nLAN, Wi-Fi, VPN or a remote session may be disconnected immediately."},
                {"DisableDone", "Managed network devices were disabled persistently."},
                {"ReconnectStart", "Reconnect is running: DHCP release/renew for IPv4 and IPv6..."},
                {"ReconnectStarted", "DHCP release/renew has started."},
                {"ReconnectDone", "Reconnect completed: DHCP release/renew was executed."},
                {"ReconnectCompleted", "Reconnect completed."},
                {"ReconnectError", "Reconnect finished with an error."},
                {"TrayStatusActive", "OffNet - device/driver active + Internet"},
                {"TrayStatusDisabled", "OffNet - device/driver disabled"},
                {"TrayStatusNoSelection", "OffNet - no tray devices selected"},
                {"TrayStatusOffline", "OffNet - active, but no Internet"},
                {"TrayStatusUnknown", "OffNet - status unknown"},
                {"StatusActivate", "Tray action: Activate executed."},
                {"StatusDisable", "Tray action: persistent Disable executed."},
                {"OptionsTitle", "OffNet Options"},
                {"Language", "Language"},
                {"ApplicationTheme", "Application theme"},
                {"TrayColors", "Tray circle colors"},
                {"ActiveInternetColor", "Active + Internet"},
                {"DisabledColor", "Device/driver disabled"},
                {"OfflineColor", "Active, no Internet"},
                {"ChooseColor", "Choose..."},
                {"RestoreDefaults", "Restore defaults"},
                {"OpenProject", "Open project page"},
                {"OptionsHint", "The offline color blinks between the selected color and a lighter shade."},
                {"StartWithWindows", "Start OffNet automatically when I sign in to Windows"},
                {"TrafficOverviewGroup", "Traffic overview"},
                {"ShowTrafficOverview", "Show traffic overview in the tray menu"},
                {"ShowTaskbarTrafficMeter", "Show permanent throughput meter on the Windows taskbar"},
                {"TaskbarMeterFont", "Taskbar meter font"},
                {"TaskbarTextShadow", "Text shadow in taskbar meter"},
                {"TrafficHistory", "History window"},
                {"TrafficSamplePoints", "Measurement points"},
                {"TrafficScaleRefresh", "Dynamic scale update"},
                {"Seconds", "seconds"},
                {"StatisticsTheme", "Statistics theme"},
                {"Minutes", "minutes"},
                {"DownloadColor", "Download"},
                {"UploadColor", "Upload"},
                {"OfflineActivityColor", "Offline activity"},
                {"TrafficOptionsHint", "History, measurement points, scale timing and curve colors are shared by both graphs. A statistics theme styles the tray graph, while the permanent taskbar meter stays transparent and follows the Windows taskbar. Traffic sampling and the taskbar meter pause automatically while a true fullscreen application is in the foreground. Offline activity shows traffic on other active adapters while all OffNet-managed adapters are disabled."},
                {"TrafficGraphTitle", "Network throughput — last {0}"},
                {"TrafficDownload", "Download"},
                {"TrafficUpload", "Upload"},
                {"TrafficOfflineActivity", "Offline activity"},
                {"TrafficCollecting", "Collecting traffic history..."},

                {"StartupChangeFailed", "The Windows startup setting could not be changed.\r\n\r\n{0}"},
                {"OK", "OK"},
                {"Cancel", "Cancel"},
                {"English", "English"},
                {"German", "German"},
                {"French", "French"},
                {"ProjectOpenFailed", "The project page could not be opened."}
            };

        private static readonly Dictionary<string, string> De =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"AlreadyRunning", "OffNet läuft bereits im Windows-Tray."},
                {"AdminRequired", "OffNet benötigt Administratorrechte, weil Netzwerkgeräte direkt auf Windows-PnP-/Treiberebene aktiviert und deaktiviert werden.\r\n\r\nBitte starte OffNet erneut und bestätige die UAC-Abfrage."},
                {"NotPresent", "Nicht vorhanden"},
                {"Disabled", "Deaktiviert"},
                {"Enabled", "Aktiviert"},
                {"ActiveProblem", "Aktiv / Problem {0}"},
                {"Hardware", "Hardware"},
                {"VirtualSystem", "Virtuell/System"},
                {"MainTitle", "OffNet — Netzwerkgeräte auf PnP-/Treiberebene"},
                {"MainDescription", "Haken bei „Tray steuern“ = vom Tray verwaltet. Zeile markieren = einmalige Aktion über die Buttons unten."},
                {"MainLegend", "Tray:  aktiv + Internet   |   deaktiviert   |   blinkend = aktiv, aber kein Internet"},
                {"ShowNonPresent", "Nicht vorhandene Geräte anzeigen"},
                {"Reload", "Neu laden"},
                {"ColTray", "Tray steuern"},
                {"ColStatus", "Status"},
                {"ColType", "Typ"},
                {"ColDevice", "Gerät"},
                {"ColManufacturer", "Hersteller"},
                {"ColInstance", "Geräteinstanz-ID"},
                {"SelectedEnable", "Markierte aktivieren"},
                {"SelectedDisable", "Markierte deaktivieren"},
                {"Reconnect", "Reconnect"},
                {"DeviceManager", "Geräte-Manager"},
                {"Ready", "Bereit."},
                {"ReadingDevices", "Netzwerkgeräte werden eingelesen..."},
                {"FoundDevices", "{0} Netzwerkgerät(e) gefunden. Vom Tray verwaltet: {1}."},
                {"ReadError", "Fehler beim Einlesen der Netzwerkgeräte."},
                {"TraySelectionSaved", "Tray-Auswahl gespeichert: {0} Gerät(e)."},
                {"SelectDevice", "Bitte mindestens ein Netzwerkgerät auswählen."},
                {"DisableSelectedConfirm", "Die ausgewählten Netzwerkgeräte wirklich persistent auf PnP-/Treiberebene deaktivieren?\r\n\r\nEine laufende Netzwerk- oder Remote-Verbindung kann sofort abbrechen."},
                {"DisableTitle", "OffNet — Deaktivieren"},
                {"DeviceNotFound", "Gerät nicht gefunden (CONFIGRET={0}):\r\n{1}"},
                {"OperationFailed", "{0} fehlgeschlagen (CONFIGRET={1}):\r\n{2}"},
                {"EnableVerb", "Aktivieren"},
                {"DisableVerb", "Deaktivieren"},
                {"SetupDiFailed", "SetupDiGetClassDevs ist fehlgeschlagen. Win32-Fehler: {0}"},
                {"EnumFailed", "SetupDiEnumDeviceInfo ist fehlgeschlagen. Win32-Fehler: {0}"},
                {"IpconfigFailed", "ipconfig.exe konnte nicht gestartet werden."},
                {"TrayActivate", "Aktivieren"},
                {"TrayDisable", "Deaktivieren"},
                {"TrayReconnect", "Neu verbinden"},
                {"TrayOpen", "OffNet öffnen..."},
                {"TrayOptions", "Optionen..."},
                {"TrayExit", "Beenden"},
                {"FirstRun", "Wähle aus, welche Netzwerkgeräte von den Tray-Aktionen verwaltet werden sollen."},
                {"NoManaged", "Keine Netzwerkgeräte sind für die Tray-Steuerung markiert."},
                {"ActivateDone", "Verwaltete Netzwerkgeräte wurden aktiviert."},
                {"DisableManagedConfirm", "Die von OffNet verwalteten Netzwerkgeräte wirklich persistent auf PnP-/Treiberebene deaktivieren?\r\n\r\nLAN, WLAN, VPN oder eine Remote-Sitzung können dabei sofort getrennt werden."},
                {"DisableDone", "Verwaltete Netzwerkgeräte wurden persistent deaktiviert."},
                {"ReconnectStart", "Reconnect läuft: DHCP Release/Renew für IPv4 und IPv6..."},
                {"ReconnectStarted", "DHCP Release/Renew wurde gestartet."},
                {"ReconnectDone", "Reconnect abgeschlossen: DHCP Release/Renew wurde ausgeführt."},
                {"ReconnectCompleted", "Reconnect abgeschlossen."},
                {"ReconnectError", "Reconnect wurde mit einem Fehler beendet."},
                {"TrayStatusActive", "OffNet - Gerät/Treiber aktiv + Internet"},
                {"TrayStatusDisabled", "OffNet - Gerät/Treiber deaktiviert"},
                {"TrayStatusNoSelection", "OffNet - keine Tray-Geräte gewählt"},
                {"TrayStatusOffline", "OffNet - aktiv, aber kein Internet"},
                {"TrayStatusUnknown", "OffNet - Status unbekannt"},
                {"StatusActivate", "Tray-Aktion: Aktivieren ausgeführt."},
                {"StatusDisable", "Tray-Aktion: persistent Deaktivieren ausgeführt."},
                {"OptionsTitle", "OffNet Optionen"},
                {"Language", "Sprache"},
                {"ApplicationTheme", "Anwendungs-Theme"},
                {"TrayColors", "Farben der Tray-Kreise"},
                {"ActiveInternetColor", "Aktiv + Internet"},
                {"DisabledColor", "Gerät/Treiber deaktiviert"},
                {"OfflineColor", "Aktiv, kein Internet"},
                {"ChooseColor", "Auswählen..."},
                {"RestoreDefaults", "Standard wiederherstellen"},
                {"OpenProject", "Projektseite öffnen"},
                {"OptionsHint", "Die Offline-Farbe blinkt zwischen der gewählten Farbe und einem helleren Farbton."},
                {"StartWithWindows", "OffNet bei der Windows-Anmeldung automatisch starten"},
                {"TrafficOverviewGroup", "Datenverkehr-Übersicht"},
                {"ShowTrafficOverview", "Datenverkehr-Übersicht im Tray-Menü anzeigen"},
                {"ShowTaskbarTrafficMeter", "Permanente Datendurchsatz-Anzeige in der Windows-Taskleiste anzeigen"},
                {"TaskbarMeterFont", "Schriftart der Taskleisten-Anzeige"},
                {"TaskbarTextShadow", "Textschatten in der Taskleisten-Anzeige"},
                {"TrafficHistory", "Verlaufsfenster"},
                {"TrafficSamplePoints", "Messpunkte"},
                {"TrafficScaleRefresh", "Dynamische Skalenanpassung"},
                {"Seconds", "Sekunden"},
                {"StatisticsTheme", "Statistik-Theme"},
                {"Minutes", "Minuten"},
                {"DownloadColor", "Download"},
                {"UploadColor", "Upload"},
                {"OfflineActivityColor", "Offline-Aktivität"},
                {"TrafficOptionsHint", "Verlauf, Messpunkte, Skalierungsintervall und Kurvenfarben gelten für beide Diagramme. Ein Statistik-Theme gestaltet den Tray-Graphen; die permanente Taskleisten-Anzeige bleibt transparent und folgt der Windows-Taskleiste. Bei einer echten Vollbildanwendung werden Traffic-Sampling und Taskleisten-Anzeige automatisch pausiert. Offline-Aktivität zeigt Datenverkehr auf anderen aktiven Adaptern, während alle von OffNet verwalteten Adapter deaktiviert sind."},
                {"TrafficGraphTitle", "Netzwerkdurchsatz — letzte {0}"},
                {"TrafficDownload", "Download"},
                {"TrafficUpload", "Upload"},
                {"TrafficOfflineActivity", "Offline-Aktivität"},
                {"TrafficCollecting", "Datenverkehrsverlauf wird gesammelt..."},

                {"StartupChangeFailed", "Die Windows-Autostart-Einstellung konnte nicht geändert werden.\r\n\r\n{0}"},
                {"OK", "OK"},
                {"Cancel", "Abbrechen"},
                {"English", "Englisch"},
                {"German", "Deutsch"},
                {"French", "Französisch"},
                {"ProjectOpenFailed", "Die Projektseite konnte nicht geöffnet werden."}
            };

        private static readonly Dictionary<string, string> Fr =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"AlreadyRunning", "OffNet est déjà en cours d’exécution dans la zone de notification Windows."},
                {"AdminRequired", "OffNet nécessite des droits administrateur car il active et désactive les périphériques réseau directement au niveau PnP/pilote de Windows.\r\n\r\nVeuillez redémarrer OffNet et confirmer l’invite UAC."},
                {"NotPresent", "Non présent"},
                {"Disabled", "Désactivé"},
                {"Enabled", "Activé"},
                {"ActiveProblem", "Actif / problème {0}"},
                {"Hardware", "Matériel"},
                {"VirtualSystem", "Virtuel/Système"},
                {"MainTitle", "OffNet — périphériques réseau au niveau PnP/pilote"},
                {"MainDescription", "Coché dans « Contrôle tray » = géré par le tray. Sélectionner une ligne = action unique via les boutons ci-dessous."},
                {"MainLegend", "Tray :  actif + Internet   |   désactivé   |   clignotant = actif, mais sans Internet"},
                {"ShowNonPresent", "Afficher les périphériques non présents"},
                {"Reload", "Actualiser"},
                {"ColTray", "Contrôle tray"},
                {"ColStatus", "État"},
                {"ColType", "Type"},
                {"ColDevice", "Périphérique"},
                {"ColManufacturer", "Fabricant"},
                {"ColInstance", "ID d’instance du périphérique"},
                {"SelectedEnable", "Activer la sélection"},
                {"SelectedDisable", "Désactiver la sélection"},
                {"Reconnect", "Reconnecter"},
                {"DeviceManager", "Gestionnaire de périphériques"},
                {"Ready", "Prêt."},
                {"ReadingDevices", "Lecture des périphériques réseau..."},
                {"FoundDevices", "{0} périphérique(s) réseau trouvé(s). Géré(s) par le tray : {1}."},
                {"ReadError", "Erreur lors de la lecture des périphériques réseau."},
                {"TraySelectionSaved", "Sélection du tray enregistrée : {0} périphérique(s)."},
                {"SelectDevice", "Sélectionnez au moins un périphérique réseau."},
                {"DisableSelectedConfirm", "Désactiver réellement et de manière persistante les périphériques réseau sélectionnés au niveau PnP/pilote ?\r\n\r\nUne connexion réseau ou une session distante active peut être interrompue immédiatement."},
                {"DisableTitle", "OffNet — Désactiver"},
                {"DeviceNotFound", "Périphérique introuvable (CONFIGRET={0}) :\r\n{1}"},
                {"OperationFailed", "Échec de l’opération {0} (CONFIGRET={1}) :\r\n{2}"},
                {"EnableVerb", "Activer"},
                {"DisableVerb", "Désactiver"},
                {"SetupDiFailed", "Échec de SetupDiGetClassDevs. Erreur Win32 : {0}"},
                {"EnumFailed", "Échec de SetupDiEnumDeviceInfo. Erreur Win32 : {0}"},
                {"IpconfigFailed", "ipconfig.exe n’a pas pu être démarré."},
                {"TrayActivate", "Activer"},
                {"TrayDisable", "Désactiver"},
                {"TrayReconnect", "Reconnecter"},
                {"TrayOpen", "Ouvrir OffNet..."},
                {"TrayOptions", "Options..."},
                {"TrayExit", "Quitter"},
                {"FirstRun", "Sélectionnez les périphériques réseau qui doivent être gérés par les actions du tray."},
                {"NoManaged", "Aucun périphérique réseau n’est marqué pour le contrôle du tray."},
                {"ActivateDone", "Les périphériques réseau gérés ont été activés."},
                {"DisableManagedConfirm", "Désactiver réellement et de manière persistante les périphériques réseau gérés par OffNet au niveau PnP/pilote ?\r\n\r\nLe LAN, le Wi-Fi, le VPN ou une session distante peuvent être interrompus immédiatement."},
                {"DisableDone", "Les périphériques réseau gérés ont été désactivés de manière persistante."},
                {"ReconnectStart", "Reconnexion en cours : DHCP release/renew pour IPv4 et IPv6..."},
                {"ReconnectStarted", "Le DHCP release/renew a démarré."},
                {"ReconnectDone", "Reconnexion terminée : DHCP release/renew exécuté."},
                {"ReconnectCompleted", "Reconnexion terminée."},
                {"ReconnectError", "La reconnexion s’est terminée avec une erreur."},
                {"TrayStatusActive", "OffNet - périphérique/pilote actif + Internet"},
                {"TrayStatusDisabled", "OffNet - périphérique/pilote désactivé"},
                {"TrayStatusNoSelection", "OffNet - aucun périphérique tray sélectionné"},
                {"TrayStatusOffline", "OffNet - actif, mais sans Internet"},
                {"TrayStatusUnknown", "OffNet - état inconnu"},
                {"StatusActivate", "Action tray : activation exécutée."},
                {"StatusDisable", "Action tray : désactivation persistante exécutée."},
                {"OptionsTitle", "Options OffNet"},
                {"Language", "Langue"},
                {"ApplicationTheme", "Thème de l’application"},
                {"TrayColors", "Couleurs des cercles du tray"},
                {"ActiveInternetColor", "Actif + Internet"},
                {"DisabledColor", "Périphérique/pilote désactivé"},
                {"OfflineColor", "Actif, sans Internet"},
                {"ChooseColor", "Choisir..."},
                {"RestoreDefaults", "Valeurs par défaut"},
                {"OpenProject", "Ouvrir la page du projet"},
                {"OptionsHint", "La couleur hors ligne clignote entre la couleur choisie et une teinte plus claire."},
                {"StartWithWindows", "Démarrer OffNet automatiquement à l’ouverture de session Windows"},
                {"TrafficOverviewGroup", "Aperçu du trafic"},
                {"ShowTrafficOverview", "Afficher l’aperçu du trafic dans le menu de la zone de notification"},
                {"ShowTaskbarTrafficMeter", "Afficher en permanence le débit dans la barre des tâches Windows"},
                {"TaskbarMeterFont", "Police de l’affichage de la barre des tâches"},
                {"TaskbarTextShadow", "Ombre du texte dans l’affichage de la barre des tâches"},
                {"TrafficHistory", "Fenêtre d’historique"},
                {"TrafficSamplePoints", "Points de mesure"},
                {"TrafficScaleRefresh", "Mise à jour dynamique de l’échelle"},
                {"Seconds", "secondes"},
                {"StatisticsTheme", "Thème des statistiques"},
                {"Minutes", "minutes"},
                {"DownloadColor", "Téléchargement"},
                {"UploadColor", "Envoi"},
                {"OfflineActivityColor", "Activité hors ligne"},
                {"TrafficOptionsHint", "L’historique, les points de mesure, l’intervalle d’échelle et les couleurs des courbes sont partagés par les deux graphiques. Un thème de statistiques habille le graphique du menu, tandis que l’affichage permanent de la barre des tâches reste transparent et suit Windows. L’échantillonnage du trafic et l’affichage de la barre des tâches sont automatiquement suspendus lorsqu’une véritable application plein écran est au premier plan. L’activité hors ligne montre le trafic sur d’autres adaptateurs actifs lorsque tous les adaptateurs gérés par OffNet sont désactivés."},
                {"TrafficGraphTitle", "Débit réseau — dernières {0}"},
                {"TrafficDownload", "Téléchargement"},
                {"TrafficUpload", "Envoi"},
                {"TrafficOfflineActivity", "Activité hors ligne"},
                {"TrafficCollecting", "Collecte de l’historique du trafic..."},

                {"StartupChangeFailed", "Le paramètre de démarrage automatique de Windows n’a pas pu être modifié.\r\n\r\n{0}"},
                {"OK", "OK"},
                {"Cancel", "Annuler"},
                {"English", "Anglais"},
                {"German", "Allemand"},
                {"French", "Français"},
                {"ProjectOpenFailed", "La page du projet n’a pas pu être ouverte."}
            };

        internal static string CurrentLanguage
        {
            get { return currentLanguage; }
        }

        internal static void SetLanguage(string language)
        {
            string value = String.IsNullOrWhiteSpace(language)
                ? "en"
                : language.Trim().ToLowerInvariant();

            if (value != "de" && value != "fr")
                value = "en";

            currentLanguage = value;
        }

        internal static string T(string key)
        {
            Dictionary<string, string> dict = En;
            if (currentLanguage == "de") dict = De;
            else if (currentLanguage == "fr") dict = Fr;

            string value;
            if (dict.TryGetValue(key, out value)) return value;
            if (En.TryGetValue(key, out value)) return value;
            return key;
        }

        internal static string F(string key, params object[] args)
        {
            return String.Format(CultureInfo.CurrentCulture, T(key), args);
        }
    }

    internal sealed class DeviceInfo
    {
        internal string Name;
        internal string Manufacturer;
        internal string InstanceId;
        internal string NetCfgInstanceId;
        internal bool IsHardware;
        internal bool IsPresent;
        internal bool IsDisabled;
        internal uint ProblemCode;

        internal string StateText
        {
            get
            {
                if (!IsPresent) return Localization.T("NotPresent");
                if (IsDisabled) return Localization.T("Disabled");
                if (ProblemCode != 0) return Localization.F("ActiveProblem", ProblemCode);
                return Localization.T("Enabled");
            }
        }

        internal string KindText
        {
            get { return Localization.T(IsHardware ? "Hardware" : "VirtualSystem"); }
        }
    }

    internal sealed class ManagedState
    {
        internal int ConfiguredCount;
        internal int EnabledCount;
        internal int DisabledCount;
        internal int MissingCount;
        internal bool InternetConnected;

        internal TrayState State
        {
            get
            {
                if (EnabledCount <= 0) return TrayState.Red;
                if (InternetConnected) return TrayState.Green;
                return TrayState.Yellow;
            }
        }
    }

    internal enum TrayState
    {
        Green,
        Red,
        Yellow
    }

    internal static class ConfigurationStore
    {
        private static readonly string ConfigDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OffNet");

        private static readonly string ManagedDevicesFile =
            Path.Combine(ConfigDirectory, "managed_devices.txt");

        private static readonly string SettingsFile =
            Path.Combine(ConfigDirectory, "settings.ini");

        internal static bool HasManagedDeviceConfiguration
        {
            get { return File.Exists(ManagedDevicesFile); }
        }

        internal static HashSet<string> LoadManagedIds()
        {
            HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (!File.Exists(ManagedDevicesFile)) return result;
                string[] lines = File.ReadAllLines(ManagedDevicesFile, Encoding.UTF8);
                foreach (string raw in lines)
                {
                    string line = raw == null ? String.Empty : raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;
                    result.Add(line);
                }
            }
            catch { }
            return result;
        }

        internal static void SaveManagedIds(IEnumerable<string> ids)
        {
            Directory.CreateDirectory(ConfigDirectory);
            List<string> lines = new List<string>();
            lines.Add("# OffNet managed PnP network-device instance IDs");
            lines.Add("# " + Program.ProjectUrl);

            HashSet<string> unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string id in ids)
            {
                if (String.IsNullOrWhiteSpace(id)) continue;
                if (unique.Add(id.Trim())) lines.Add(id.Trim());
            }
            File.WriteAllLines(ManagedDevicesFile, lines.ToArray(), new UTF8Encoding(false));
        }

        internal static AppSettings LoadSettings()
        {
            AppSettings settings = new AppSettings();
            try
            {
                if (!File.Exists(SettingsFile)) return settings;
                string[] lines = File.ReadAllLines(SettingsFile, Encoding.UTF8);
                foreach (string raw in lines)
                {
                    string line = raw == null ? String.Empty : raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;
                    int split = line.IndexOf('=');
                    if (split <= 0) continue;
                    string key = line.Substring(0, split).Trim().ToLowerInvariant();
                    string value = line.Substring(split + 1).Trim();
                    int colorValue;
                    int intValue;
                    bool boolValue;
                    if (key == "language") settings.Language = value;
                    else if (key == "active_color" && Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out colorValue)) settings.ActiveColorArgb = colorValue;
                    else if (key == "disabled_color" && Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out colorValue)) settings.DisabledColorArgb = colorValue;
                    else if (key == "offline_color" && Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out colorValue)) settings.OfflineColorArgb = colorValue;
                    else if (key == "show_traffic_overview" && Boolean.TryParse(value, out boolValue)) settings.ShowTrafficOverview = boolValue;
                    else if (key == "show_taskbar_traffic_meter" && Boolean.TryParse(value, out boolValue)) settings.ShowTaskbarTrafficMeter = boolValue;
                    else if (key == "taskbar_meter_font" && !String.IsNullOrWhiteSpace(value)) settings.TaskbarMeterFontName = value;
                    else if (key == "taskbar_text_shadow" && Boolean.TryParse(value, out boolValue)) settings.TaskbarTextShadow = boolValue;
                    else if (key == "traffic_history_seconds" && Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out intValue)) settings.TrafficHistorySeconds = Math.Max(10, Math.Min(600, ((intValue + 5) / 10) * 10));
                    else if (key == "traffic_history_minutes" && Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out intValue)) settings.TrafficHistorySeconds = Math.Max(10, Math.Min(600, intValue * 60));
                    else if (key == "traffic_sample_points" && Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out intValue)) settings.TrafficSamplePoints = Math.Max(10, Math.Min(300, intValue));
                    else if (key == "traffic_scale_refresh_seconds" && Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out intValue)) settings.TrafficScaleRefreshSeconds = Math.Max(1, Math.Min(30, intValue));
                    else if (key == "statistics_theme" && !String.IsNullOrWhiteSpace(value)) settings.StatisticsTheme = value;
                    else if (key == "application_theme" && !String.IsNullOrWhiteSpace(value)) settings.ApplicationTheme = value;
                    else if (key == "download_color" && Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out colorValue)) settings.DownloadColorArgb = colorValue;
                    else if (key == "upload_color" && Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out colorValue)) settings.UploadColorArgb = colorValue;
                    else if (key == "offline_activity_color" && Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out colorValue)) settings.OfflineActivityColorArgb = colorValue;
                }
            }
            catch { }

            Localization.SetLanguage(settings.Language);
            settings.Language = Localization.CurrentLanguage;
            return settings;
        }

        internal static void SaveSettings(AppSettings settings)
        {
            Directory.CreateDirectory(ConfigDirectory);
            List<string> lines = new List<string>();
            lines.Add("# OffNet settings");
            lines.Add("# " + Program.ProjectUrl);
            lines.Add("language=" + settings.Language);
            lines.Add("active_color=" + settings.ActiveColorArgb.ToString(CultureInfo.InvariantCulture));
            lines.Add("disabled_color=" + settings.DisabledColorArgb.ToString(CultureInfo.InvariantCulture));
            lines.Add("offline_color=" + settings.OfflineColorArgb.ToString(CultureInfo.InvariantCulture));
            lines.Add("show_traffic_overview=" + (settings.ShowTrafficOverview ? "true" : "false"));
            lines.Add("show_taskbar_traffic_meter=" + (settings.ShowTaskbarTrafficMeter ? "true" : "false"));
            lines.Add("taskbar_meter_font=" + (String.IsNullOrWhiteSpace(settings.TaskbarMeterFontName) ? "Segoe UI Semibold" : settings.TaskbarMeterFontName));
            lines.Add("taskbar_text_shadow=" + (settings.TaskbarTextShadow ? "true" : "false"));
            lines.Add("traffic_history_seconds=" + settings.TrafficHistorySeconds.ToString(CultureInfo.InvariantCulture));
            lines.Add("traffic_sample_points=" + settings.TrafficSamplePoints.ToString(CultureInfo.InvariantCulture));
            lines.Add("traffic_scale_refresh_seconds=" + settings.TrafficScaleRefreshSeconds.ToString(CultureInfo.InvariantCulture));
            lines.Add("statistics_theme=" + (String.IsNullOrWhiteSpace(settings.StatisticsTheme) ? "Dark" : settings.StatisticsTheme));
            lines.Add("application_theme=" + (String.IsNullOrWhiteSpace(settings.ApplicationTheme) ? "Muffin" : settings.ApplicationTheme));
            lines.Add("download_color=" + settings.DownloadColorArgb.ToString(CultureInfo.InvariantCulture));
            lines.Add("upload_color=" + settings.UploadColorArgb.ToString(CultureInfo.InvariantCulture));
            lines.Add("offline_activity_color=" + settings.OfflineActivityColorArgb.ToString(CultureInfo.InvariantCulture));
            File.WriteAllLines(SettingsFile, lines.ToArray(), new UTF8Encoding(false));
        }
    }

    internal static class NativeMethods
    {
        internal static readonly Guid GUID_DEVCLASS_NET = new Guid("4D36E972-E325-11CE-BFC1-08002BE10318");
        internal const uint DIGCF_PRESENT = 0x00000002;
        internal const uint SPDRP_DEVICEDESC = 0x00000000;
        internal const uint SPDRP_MFG = 0x0000000B;
        internal const uint SPDRP_FRIENDLYNAME = 0x0000000C;
        internal const uint SPDRP_DRIVER = 0x00000009;
        internal const uint CM_DISABLE_PERSIST = 0x00000001;
        internal const uint CR_SUCCESS = 0x00000000;
        internal const uint CM_PROB_DISABLED = 22;
        internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVINFO_DATA
        {
            internal uint cbSize;
            internal Guid ClassGuid;
            internal uint DevInst;
            internal IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            internal int Left;
            internal int Top;
            internal int Right;
            internal int Bottom;

            internal int Width { get { return Right - Left; } }
            internal int Height { get { return Bottom - Top; } }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct MONITORINFO
        {
            internal int cbSize;
            internal RECT rcMonitor;
            internal RECT rcWork;
            internal uint dwFlags;
        }

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, uint Flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiGetDeviceInstanceId(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, StringBuilder DeviceInstanceId, int DeviceInstanceIdSize, out int RequiredSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiGetDeviceRegistryProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, uint Property, out uint PropertyRegDataType, byte[] PropertyBuffer, uint PropertyBufferSize, out uint RequiredSize);

        [DllImport("setupapi.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
        internal static extern uint CM_Locate_DevNode(out uint pdnDevInst, string pDeviceID, uint ulFlags);

        [DllImport("cfgmgr32.dll")]
        internal static extern uint CM_Get_DevNode_Status(out uint pulStatus, out uint pulProblemNumber, uint dnDevInst, uint ulFlags);

        [DllImport("cfgmgr32.dll")]
        internal static extern uint CM_Disable_DevNode(uint dnDevInst, uint ulFlags);

        [DllImport("cfgmgr32.dll")]
        internal static extern uint CM_Enable_DevNode(uint dnDevInst, uint ulFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        internal static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        internal const uint SWP_NOSIZE = 0x0001;
        internal const uint SWP_NOMOVE = 0x0002;
        internal const uint SWP_NOACTIVATE = 0x0010;
        internal const uint SWP_SHOWWINDOW = 0x0040;
        internal const uint SWP_NOOWNERZORDER = 0x0200;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindWindowEx(
            IntPtr hwndParent,
            IntPtr hwndChildAfter,
            string lpszClass,
            string lpszWindow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(
            IntPtr hWnd,
            out RECT lpRect);

        internal const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        internal static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern IntPtr MonitorFromWindow(
            IntPtr hwnd,
            uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetMonitorInfo(
            IntPtr hMonitor,
            ref MONITORINFO lpmi);

        [DllImport("wininet.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool InternetGetConnectedState(out int lpdwConnection, int dwReserved);
    }

    internal static class DeviceManager
    {
        internal static List<DeviceInfo> EnumerateNetworkDevices(bool includeNonPresent)
        {
            List<DeviceInfo> devices = new List<DeviceInfo>();
            Guid classGuid = NativeMethods.GUID_DEVCLASS_NET;
            uint flags = includeNonPresent ? 0U : NativeMethods.DIGCF_PRESENT;
            IntPtr set = NativeMethods.SetupDiGetClassDevs(ref classGuid, IntPtr.Zero, IntPtr.Zero, flags);

            if (set == NativeMethods.INVALID_HANDLE_VALUE)
                throw new InvalidOperationException(Localization.F("SetupDiFailed", Marshal.GetLastWin32Error()));

            try
            {
                uint index = 0;
                while (true)
                {
                    NativeMethods.SP_DEVINFO_DATA data = new NativeMethods.SP_DEVINFO_DATA();
                    data.cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SP_DEVINFO_DATA));

                    if (!NativeMethods.SetupDiEnumDeviceInfo(set, index, ref data))
                    {
                        int error = Marshal.GetLastWin32Error();
                        if (error == 259) break;
                        throw new InvalidOperationException(Localization.F("EnumFailed", error));
                    }

                    DeviceInfo device = new DeviceInfo();
                    device.InstanceId = GetInstanceId(set, ref data);
                    device.Name = GetStringProperty(set, ref data, NativeMethods.SPDRP_FRIENDLYNAME);
                    if (String.IsNullOrWhiteSpace(device.Name)) device.Name = GetStringProperty(set, ref data, NativeMethods.SPDRP_DEVICEDESC);
                    if (String.IsNullOrWhiteSpace(device.Name)) device.Name = device.InstanceId;
                    device.Manufacturer = GetStringProperty(set, ref data, NativeMethods.SPDRP_MFG);
                    device.NetCfgInstanceId = GetNetCfgInstanceId(set, ref data);
                    device.IsHardware = LooksLikePhysicalHardware(device.InstanceId);

                    uint status;
                    uint problem;
                    uint cr = NativeMethods.CM_Get_DevNode_Status(out status, out problem, data.DevInst, 0);
                    if (cr == NativeMethods.CR_SUCCESS)
                    {
                        device.IsPresent = true;
                        device.ProblemCode = problem;
                        device.IsDisabled = problem == NativeMethods.CM_PROB_DISABLED;
                    }
                    else
                    {
                        device.IsPresent = false;
                        device.ProblemCode = 0;
                        device.IsDisabled = false;
                    }

                    devices.Add(device);
                    index++;
                }
            }
            finally
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(set);
            }

            devices.Sort(delegate(DeviceInfo a, DeviceInfo b)
            {
                int aRank = a.IsHardware ? 0 : 1;
                int bRank = b.IsHardware ? 0 : 1;
                int kind = aRank.CompareTo(bRank);
                if (kind != 0) return kind;
                return String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });

            return devices;
        }

        private static string GetInstanceId(IntPtr set, ref NativeMethods.SP_DEVINFO_DATA data)
        {
            int required;
            NativeMethods.SetupDiGetDeviceInstanceId(set, ref data, null, 0, out required);
            if (required <= 0) return String.Empty;
            StringBuilder builder = new StringBuilder(required + 1);
            if (!NativeMethods.SetupDiGetDeviceInstanceId(set, ref data, builder, builder.Capacity, out required)) return String.Empty;
            return builder.ToString();
        }

        private static string GetStringProperty(IntPtr set, ref NativeMethods.SP_DEVINFO_DATA data, uint property)
        {
            uint regType;
            uint required;
            byte[] buffer = new byte[2048];
            bool ok = NativeMethods.SetupDiGetDeviceRegistryProperty(set, ref data, property, out regType, buffer, (uint)buffer.Length, out required);
            if (!ok || required == 0) return String.Empty;
            int length = (int)Math.Min((uint)buffer.Length, required);
            if (length <= 0) return String.Empty;
            string value = Encoding.Unicode.GetString(buffer, 0, length);
            return value.TrimEnd('\0').Trim();
        }

        private static string GetNetCfgInstanceId(IntPtr set, ref NativeMethods.SP_DEVINFO_DATA data)
        {
            try
            {
                string driverKey = GetStringProperty(set, ref data, NativeMethods.SPDRP_DRIVER);
                if (String.IsNullOrWhiteSpace(driverKey)) return String.Empty;

                string path = @"SYSTEM\CurrentControlSet\Control\Class\" + driverKey;
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(path))
                {
                    if (key == null) return String.Empty;
                    object value = key.GetValue("NetCfgInstanceId");
                    return value == null ? String.Empty : NormalizeNetworkInterfaceId(value.ToString());
                }
            }
            catch
            {
                return String.Empty;
            }
        }

        internal static string NormalizeNetworkInterfaceId(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return String.Empty;
            return value.Trim().Trim('{', '}').ToUpperInvariant();
        }

        internal static void GetManagedInterfaceSelectors(
            HashSet<string> managedDeviceIds,
            out HashSet<string> networkInterfaceIds,
            out HashSet<string> descriptions)
        {
            networkInterfaceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            descriptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (managedDeviceIds == null || managedDeviceIds.Count == 0) return;

            List<DeviceInfo> devices = EnumerateNetworkDevices(false);
            foreach (DeviceInfo device in devices)
            {
                if (!managedDeviceIds.Contains(device.InstanceId)) continue;

                if (!String.IsNullOrWhiteSpace(device.NetCfgInstanceId))
                    networkInterfaceIds.Add(NormalizeNetworkInterfaceId(device.NetCfgInstanceId));

                if (!String.IsNullOrWhiteSpace(device.Name))
                    descriptions.Add(device.Name.Trim());
            }
        }

        internal static bool LooksLikePhysicalHardware(string instanceId)
        {
            if (String.IsNullOrWhiteSpace(instanceId)) return false;
            string id = instanceId.ToUpperInvariant();
            return id.StartsWith(@"PCI\", StringComparison.Ordinal) ||
                   id.StartsWith(@"USB\", StringComparison.Ordinal) ||
                   id.StartsWith(@"PCMCIA\", StringComparison.Ordinal) ||
                   id.StartsWith(@"SD\", StringComparison.Ordinal) ||
                   id.StartsWith(@"BTH\", StringComparison.Ordinal);
        }

        internal static DeviceInfo GetDeviceState(string instanceId)
        {
            DeviceInfo result = new DeviceInfo();
            result.InstanceId = instanceId;
            result.Name = instanceId;
            result.Manufacturer = String.Empty;
            result.IsHardware = LooksLikePhysicalHardware(instanceId);

            uint devInst;
            uint cr = NativeMethods.CM_Locate_DevNode(out devInst, instanceId, 0);
            if (cr != NativeMethods.CR_SUCCESS)
            {
                result.IsPresent = false;
                return result;
            }

            uint status;
            uint problem;
            cr = NativeMethods.CM_Get_DevNode_Status(out status, out problem, devInst, 0);
            if (cr != NativeMethods.CR_SUCCESS)
            {
                result.IsPresent = false;
                return result;
            }

            result.IsPresent = true;
            result.ProblemCode = problem;
            result.IsDisabled = problem == NativeMethods.CM_PROB_DISABLED;
            return result;
        }

        internal static void SetDeviceEnabled(string instanceId, bool enable)
        {
            uint devInst;
            uint cr = NativeMethods.CM_Locate_DevNode(out devInst, instanceId, 0);
            if (cr != NativeMethods.CR_SUCCESS)
                throw new InvalidOperationException(Localization.F("DeviceNotFound", cr, instanceId));

            if (enable) cr = NativeMethods.CM_Enable_DevNode(devInst, 0);
            else cr = NativeMethods.CM_Disable_DevNode(devInst, NativeMethods.CM_DISABLE_PERSIST);

            if (cr != NativeMethods.CR_SUCCESS)
            {
                string operation = Localization.T(enable ? "EnableVerb" : "DisableVerb");
                throw new InvalidOperationException(Localization.F("OperationFailed", operation, cr, instanceId));
            }
        }

        internal static ManagedState GetManagedState(IEnumerable<string> instanceIds)
        {
            ManagedState state = new ManagedState();
            foreach (string id in instanceIds)
            {
                if (String.IsNullOrWhiteSpace(id)) continue;
                state.ConfiguredCount++;
                DeviceInfo device = GetDeviceState(id);
                if (!device.IsPresent) { state.MissingCount++; continue; }
                if (device.IsDisabled) state.DisabledCount++;
                else state.EnabledCount++;
            }
            if (state.EnabledCount > 0) state.InternetConnected = ConnectivityManager.IsConnectedToInternet();
            return state;
        }
    }

    internal static class ConnectivityManager
    {
        private static readonly Guid NetworkListManagerClsid = new Guid("DCB00C01-570F-4A9B-8D69-199FDBA5723B");

        internal static bool IsConnectedToInternet()
        {
            object networkListManager = null;
            try
            {
                Type type = Type.GetTypeFromCLSID(NetworkListManagerClsid, true);
                networkListManager = Activator.CreateInstance(type);
                object result = networkListManager.GetType().InvokeMember("IsConnectedToInternet", BindingFlags.GetProperty, null, networkListManager, null);
                return Convert.ToBoolean(result);
            }
            catch
            {
                int flags;
                return NativeMethods.InternetGetConnectedState(out flags, 0);
            }
            finally
            {
                if (networkListManager != null && Marshal.IsComObject(networkListManager))
                {
                    try { Marshal.FinalReleaseComObject(networkListManager); } catch { }
                }
            }
        }

        internal static void ReconnectAsync(Action<string> completed)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                string error = null;
                try
                {
                    RunIpConfig("/release *");
                    RunIpConfig("/release6 *");
                    RunIpConfig("/renew *");
                    RunIpConfig("/renew6 *");
                }
                catch (Exception ex) { error = ex.Message; }
                if (completed != null) completed(error);
            });
        }

        private static void RunIpConfig(string arguments)
        {
            string system = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string exe = Path.Combine(system, "ipconfig.exe");
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = exe;
            info.Arguments = arguments;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

            using (Process process = Process.Start(info))
            {
                if (process == null) throw new InvalidOperationException(Localization.T("IpconfigFailed"));
                process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0 && !String.IsNullOrWhiteSpace(stderr)) throw new InvalidOperationException(stderr.Trim());
            }
        }
    }

    internal static class IconFactory
    {
        internal static Icon CreateCircle(Color fill)
        {
            using (Bitmap bitmap = new Bitmap(32, 32))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
            using (SolidBrush brush = new SolidBrush(fill))
            using (Pen border = new Pen(Color.FromArgb(70, 70, 70), 2.0f))
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);
                graphics.FillEllipse(shadow, 5, 6, 23, 23);
                graphics.FillEllipse(brush, 3, 3, 25, 25);
                graphics.DrawEllipse(border, 3, 3, 25, 25);
                IntPtr handle = bitmap.GetHicon();
                try
                {
                    using (Icon temp = Icon.FromHandle(handle)) return (Icon)temp.Clone();
                }
                finally { NativeMethods.DestroyIcon(handle); }
            }
        }

        internal static Color Lighter(Color color)
        {
            int r = color.R + (255 - color.R) / 2;
            int g = color.G + (255 - color.G) / 2;
            int b = color.B + (255 - color.B) / 2;
            return Color.FromArgb(color.A, r, g, b);
        }
    }

    internal static class AppIconProvider
    {
        private static readonly Icon sharedIcon = LoadIcon();

        internal static Icon Shared
        {
            get { return sharedIcon; }
        }

        private static Icon LoadIcon()
        {
            try
            {
                Icon extracted =
                    Icon.ExtractAssociatedIcon(
                        Application.ExecutablePath);

                if (extracted != null)
                    return extracted;
            }
            catch
            {
            }

            try
            {
                string localIcon =
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "OffNet.ico");

                if (File.Exists(localIcon))
                    return new Icon(localIcon);
            }
            catch
            {
            }

            return SystemIcons.Application;
        }
    }

    internal static class ShellHelper
    {
        internal static void OpenProjectPage()
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = Program.ProjectUrl;
                info.UseShellExecute = true;
                Process.Start(info);
            }
            catch
            {
                MessageBox.Show(Localization.T("ProjectOpenFailed"), Program.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    internal sealed class LanguageChoice
    {
        internal readonly string Code;
        internal readonly string Name;
        internal LanguageChoice(string code, string name) { Code = code; Name = name; }
        public override string ToString() { return Name; }
    }

    internal static class StartupManager
    {
        private const string TaskName = "OffNet AutoStart";
        private const int TaskCreateOrUpdate = 6;
        private const int TaskLogonInteractiveToken = 3;
        private const int TaskRunLevelHighest = 1;
        private const int TaskTriggerLogon = 9;
        private const int TaskActionExecute = 0;

        internal static bool IsEnabled()
        {
            object service = null;
            object root = null;
            object task = null;
            object definition = null;
            object actions = null;
            object action = null;

            try
            {
                dynamic scheduler = CreateScheduler(out service);
                scheduler.Connect();
                dynamic rootFolder = scheduler.GetFolder("\\");
                root = rootFolder;

                dynamic registeredTask = rootFolder.GetTask(TaskName);
                task = registeredTask;
                if (registeredTask == null || !Convert.ToBoolean(registeredTask.Enabled))
                    return false;

                dynamic taskDefinition = registeredTask.Definition;
                definition = taskDefinition;
                dynamic taskActions = taskDefinition.Actions;
                actions = taskActions;
                if (Convert.ToInt32(taskActions.Count) < 1)
                    return false;

                dynamic execAction = taskActions.Item(1);
                action = execAction;
                string configuredPath = Convert.ToString(execAction.Path);
                if (String.IsNullOrWhiteSpace(configuredPath))
                    return false;

                return PathsEqual(configuredPath, GetCurrentExecutablePath());
            }
            catch
            {
                return false;
            }
            finally
            {
                ReleaseCom(action);
                ReleaseCom(actions);
                ReleaseCom(definition);
                ReleaseCom(task);
                ReleaseCom(root);
                ReleaseCom(service);
            }
        }

        internal static void SetEnabled(bool enabled)
        {
            object service = null;
            object root = null;
            object definition = null;
            object registration = null;
            object principal = null;
            object settings = null;
            object triggers = null;
            object trigger = null;
            object actions = null;
            object action = null;
            object registeredTask = null;

            try
            {
                dynamic scheduler = CreateScheduler(out service);
                scheduler.Connect();
                dynamic rootFolder = scheduler.GetFolder("\\");
                root = rootFolder;

                if (!enabled)
                {
                    try
                    {
                        rootFolder.DeleteTask(TaskName, 0);
                    }
                    catch (COMException)
                    {
                        // Missing tasks are equivalent to disabled autostart.
                    }
                    return;
                }

                dynamic taskDefinition = scheduler.NewTask(0);
                definition = taskDefinition;

                dynamic registrationInfo = taskDefinition.RegistrationInfo;
                registration = registrationInfo;
                registrationInfo.Description =
                    "Starts OffNet at Windows sign-in with the privileges required for PnP/driver control. " +
                    Program.ProjectUrl;
                registrationInfo.Author = "zeittresor";

                dynamic taskPrincipal = taskDefinition.Principal;
                principal = taskPrincipal;
                taskPrincipal.UserId = WindowsIdentity.GetCurrent().Name;
                taskPrincipal.LogonType = TaskLogonInteractiveToken;
                taskPrincipal.RunLevel = TaskRunLevelHighest;

                dynamic taskSettings = taskDefinition.Settings;
                settings = taskSettings;
                taskSettings.Enabled = true;
                taskSettings.StartWhenAvailable = true;
                taskSettings.DisallowStartIfOnBatteries = false;
                taskSettings.StopIfGoingOnBatteries = false;
                taskSettings.ExecutionTimeLimit = "PT0S";

                dynamic taskTriggers = taskDefinition.Triggers;
                triggers = taskTriggers;
                dynamic logonTrigger = taskTriggers.Create(TaskTriggerLogon);
                trigger = logonTrigger;
                logonTrigger.UserId = WindowsIdentity.GetCurrent().Name;
                logonTrigger.Enabled = true;

                dynamic taskActions = taskDefinition.Actions;
                actions = taskActions;
                dynamic execAction = taskActions.Create(TaskActionExecute);
                action = execAction;

                string executablePath = GetCurrentExecutablePath();
                if (String.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
                {
                    throw new FileNotFoundException(
                        "The current OffNet executable could not be found.",
                        executablePath);
                }

                // The Task Scheduler ExecAction requires a plain executable path.
                // OffNet does not need a working directory, so do not set one here.
                execAction.Path = executablePath;

                dynamic result = rootFolder.RegisterTaskDefinition(
                    TaskName,
                    taskDefinition,
                    TaskCreateOrUpdate,
                    null,
                    null,
                    TaskLogonInteractiveToken,
                    null);
                registeredTask = result;
            }
            finally
            {
                ReleaseCom(registeredTask);
                ReleaseCom(action);
                ReleaseCom(actions);
                ReleaseCom(trigger);
                ReleaseCom(triggers);
                ReleaseCom(settings);
                ReleaseCom(principal);
                ReleaseCom(registration);
                ReleaseCom(definition);
                ReleaseCom(root);
                ReleaseCom(service);
            }
        }

        private static dynamic CreateScheduler(out object serviceObject)
        {
            Type schedulerType = Type.GetTypeFromProgID("Schedule.Service", true);
            serviceObject = Activator.CreateInstance(schedulerType);
            return serviceObject;
        }

        private static string GetCurrentExecutablePath()
        {
            try
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    if (process.MainModule != null &&
                        !String.IsNullOrWhiteSpace(process.MainModule.FileName))
                    {
                        return Path.GetFullPath(process.MainModule.FileName);
                    }
                }
            }
            catch
            {
            }

            try
            {
                return Path.GetFullPath(Application.ExecutablePath);
            }
            catch
            {
                return Application.ExecutablePath;
            }
        }

        private static bool PathsEqual(string left, string right)
        {
            try
            {
                string a = Path.GetFullPath(left.Trim().Trim('"'));
                string b = Path.GetFullPath(right.Trim().Trim('"'));
                return String.Equals(a, b, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return String.Equals(left.Trim().Trim('"'), right.Trim().Trim('"'), StringComparison.OrdinalIgnoreCase);
            }
        }

        private static void ReleaseCom(object value)
        {
            if (value == null || !Marshal.IsComObject(value))
                return;

            try { Marshal.FinalReleaseComObject(value); }
            catch { }
        }
    }


    internal sealed class AppThemePalette
    {
        internal Color Background;
        internal Color Surface;
        internal Color Input;
        internal Color Text;
        internal Color SecondaryText;
        internal Color Border;
        internal Color Accent;
        internal Color Selection;
        internal Color Success;
        internal Color Danger;
        internal Color Muted;

        internal static AppThemePalette FromName(string name)
        {
            string key = String.IsNullOrWhiteSpace(name) ? "Muffin" : name.Trim();
            AppThemePalette p = new AppThemePalette();

            if (String.Equals(key, "Light", StringComparison.OrdinalIgnoreCase))
            {
                p.Background = Color.FromArgb(245, 246, 248);
                p.Surface = Color.White;
                p.Input = Color.White;
                p.Text = Color.FromArgb(28, 30, 34);
                p.SecondaryText = Color.FromArgb(86, 90, 98);
                p.Border = Color.FromArgb(190, 194, 202);
                p.Accent = Color.FromArgb(46, 106, 210);
                p.Selection = Color.FromArgb(203, 222, 250);
            }
            else if (String.Equals(key, "Dark", StringComparison.OrdinalIgnoreCase))
            {
                p.Background = Color.FromArgb(28, 29, 32);
                p.Surface = Color.FromArgb(36, 38, 42);
                p.Input = Color.FromArgb(44, 46, 51);
                p.Text = Color.FromArgb(242, 243, 245);
                p.SecondaryText = Color.FromArgb(181, 184, 190);
                p.Border = Color.FromArgb(79, 82, 90);
                p.Accent = Color.FromArgb(89, 145, 255);
                p.Selection = Color.FromArgb(49, 77, 122);
            }
            else if (String.Equals(key, "Sepia", StringComparison.OrdinalIgnoreCase))
            {
                p.Background = Color.FromArgb(235, 224, 199);
                p.Surface = Color.FromArgb(247, 239, 219);
                p.Input = Color.FromArgb(255, 249, 233);
                p.Text = Color.FromArgb(62, 46, 30);
                p.SecondaryText = Color.FromArgb(103, 78, 50);
                p.Border = Color.FromArgb(174, 148, 107);
                p.Accent = Color.FromArgb(141, 92, 41);
                p.Selection = Color.FromArgb(222, 194, 150);
            }
            else if (String.Equals(key, "Ocean", StringComparison.OrdinalIgnoreCase))
            {
                p.Background = Color.FromArgb(11, 31, 43);
                p.Surface = Color.FromArgb(17, 43, 58);
                p.Input = Color.FromArgb(22, 53, 70);
                p.Text = Color.FromArgb(229, 247, 250);
                p.SecondaryText = Color.FromArgb(157, 203, 214);
                p.Border = Color.FromArgb(55, 103, 121);
                p.Accent = Color.FromArgb(48, 181, 206);
                p.Selection = Color.FromArgb(31, 91, 112);
            }
            else if (String.Equals(key, "Matrix", StringComparison.OrdinalIgnoreCase))
            {
                p.Background = Color.FromArgb(3, 12, 5);
                p.Surface = Color.FromArgb(8, 23, 10);
                p.Input = Color.FromArgb(12, 31, 15);
                p.Text = Color.FromArgb(139, 255, 151);
                p.SecondaryText = Color.FromArgb(82, 191, 94);
                p.Border = Color.FromArgb(36, 104, 45);
                p.Accent = Color.FromArgb(41, 214, 64);
                p.Selection = Color.FromArgb(23, 80, 31);
            }
            else if (String.Equals(key, "Hellfire", StringComparison.OrdinalIgnoreCase))
            {
                p.Background = Color.FromArgb(27, 7, 5);
                p.Surface = Color.FromArgb(43, 12, 8);
                p.Input = Color.FromArgb(58, 17, 11);
                p.Text = Color.FromArgb(255, 235, 211);
                p.SecondaryText = Color.FromArgb(232, 161, 111);
                p.Border = Color.FromArgb(119, 45, 24);
                p.Accent = Color.FromArgb(236, 76, 31);
                p.Selection = Color.FromArgb(104, 37, 20);
            }
            else if (String.Equals(key, "Purple", StringComparison.OrdinalIgnoreCase))
            {
                p.Background = Color.FromArgb(27, 20, 38);
                p.Surface = Color.FromArgb(39, 29, 54);
                p.Input = Color.FromArgb(50, 37, 69);
                p.Text = Color.FromArgb(244, 235, 255);
                p.SecondaryText = Color.FromArgb(192, 166, 224);
                p.Border = Color.FromArgb(99, 74, 132);
                p.Accent = Color.FromArgb(160, 101, 224);
                p.Selection = Color.FromArgb(77, 51, 107);
            }
            else if (String.Equals(key, "Aurora", StringComparison.OrdinalIgnoreCase))
            {
                p.Background = Color.FromArgb(10, 28, 29);
                p.Surface = Color.FromArgb(16, 41, 39);
                p.Input = Color.FromArgb(21, 52, 48);
                p.Text = Color.FromArgb(229, 255, 248);
                p.SecondaryText = Color.FromArgb(143, 221, 198);
                p.Border = Color.FromArgb(61, 121, 106);
                p.Accent = Color.FromArgb(73, 220, 175);
                p.Selection = Color.FromArgb(38, 91, 78);
            }
            else // Muffin - the original neutral/light OffNet appearance
            {
                p.Background = Color.FromArgb(240, 240, 240);
                p.Surface = Color.FromArgb(248, 248, 248);
                p.Input = Color.White;
                p.Text = Color.FromArgb(32, 32, 32);
                p.SecondaryText = Color.FromArgb(96, 96, 96);
                p.Border = Color.FromArgb(182, 182, 182);
                p.Accent = Color.FromArgb(0, 120, 215);
                p.Selection = Color.FromArgb(205, 228, 247);
            }

            p.Success = IsDark(p.Background)
                ? Color.FromArgb(103, 231, 132)
                : Color.FromArgb(28, 120, 59);

            p.Danger = IsDark(p.Background)
                ? Color.FromArgb(255, 126, 119)
                : Color.FromArgb(178, 43, 37);

            p.Muted = p.SecondaryText;
            return p;
        }

        internal static bool IsDark(Color color)
        {
            int luminance =
                (color.R * 299 +
                 color.G * 587 +
                 color.B * 114) / 1000;
            return luminance < 128;
        }

        internal static Color BestTextFor(Color background)
        {
            return IsDark(background)
                ? Color.White
                : Color.FromArgb(24, 24, 24);
        }
    }

    internal sealed class OffNetToolStripColorTable : ProfessionalColorTable
    {
        private readonly AppThemePalette p;

        internal OffNetToolStripColorTable(AppThemePalette palette)
        {
            p = palette;
            UseSystemColors = false;
        }

        public override Color ToolStripDropDownBackground { get { return p.Surface; } }
        public override Color MenuBorder { get { return p.Border; } }
        public override Color MenuItemBorder { get { return p.Accent; } }
        public override Color MenuItemSelected { get { return p.Selection; } }
        public override Color MenuItemSelectedGradientBegin { get { return p.Selection; } }
        public override Color MenuItemSelectedGradientEnd { get { return p.Selection; } }
        public override Color MenuItemPressedGradientBegin { get { return p.Selection; } }
        public override Color MenuItemPressedGradientMiddle { get { return p.Selection; } }
        public override Color MenuItemPressedGradientEnd { get { return p.Selection; } }
        public override Color SeparatorDark { get { return p.Border; } }
        public override Color SeparatorLight { get { return p.Surface; } }
        public override Color ImageMarginGradientBegin { get { return p.Surface; } }
        public override Color ImageMarginGradientMiddle { get { return p.Surface; } }
        public override Color ImageMarginGradientEnd { get { return p.Surface; } }
    }

    internal static class AppThemeManager
    {
        internal static void Apply(Control root, string themeName)
        {
            if (root == null)
                return;

            AppThemePalette palette =
                AppThemePalette.FromName(themeName);

            ApplyRecursive(root, palette);

            DataGridView grid = root as DataGridView;
            if (grid != null)
                ApplyGrid(grid, palette);
        }

        private static void ApplyRecursive(
            Control control,
            AppThemePalette palette)
        {
            Form form = control as Form;
            GroupBox group = control as GroupBox;
            Panel panel = control as Panel;
            TableLayoutPanel table = control as TableLayoutPanel;
            Label label = control as Label;
            CheckBox check = control as CheckBox;
            Button button = control as Button;
            ComboBox combo = control as ComboBox;
            TextBox textBox = control as TextBox;
            NumericUpDown numeric = control as NumericUpDown;
            TrackBar track = control as TrackBar;
            DataGridView grid = control as DataGridView;

            if (form != null || panel != null || table != null)
            {
                control.BackColor = palette.Background;
                control.ForeColor = palette.Text;
            }
            else if (group != null)
            {
                control.BackColor = palette.Surface;
                control.ForeColor = palette.Text;
            }
            else if (label != null || check != null || track != null)
            {
                control.BackColor = control.Parent is GroupBox
                    ? palette.Surface
                    : palette.Background;
                control.ForeColor = palette.Text;

                if (check != null)
                    check.UseVisualStyleBackColor = false;
            }
            else if (button != null)
            {
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = palette.Border;
                button.FlatAppearance.MouseOverBackColor = palette.Selection;
                button.BackColor = palette.Surface;
                button.ForeColor = palette.Text;
                button.UseVisualStyleBackColor = false;
            }
            else if (combo != null || textBox != null || numeric != null)
            {
                control.BackColor = palette.Input;
                control.ForeColor = palette.Text;
            }
            else if (grid != null)
            {
                ApplyGrid(grid, palette);
            }
            else
            {
                control.BackColor = palette.Background;
                control.ForeColor = palette.Text;
            }

            foreach (Control child in control.Controls)
            {
                ApplyRecursive(child, palette);
            }
        }

        internal static void ApplyGrid(
            DataGridView grid,
            AppThemePalette palette)
        {
            if (grid == null)
                return;

            grid.EnableHeadersVisualStyles = false;
            grid.BackgroundColor = palette.Background;
            grid.GridColor = palette.Border;
            grid.DefaultCellStyle.BackColor = palette.Input;
            grid.DefaultCellStyle.ForeColor = palette.Text;
            grid.DefaultCellStyle.SelectionBackColor = palette.Accent;
            grid.DefaultCellStyle.SelectionForeColor =
                AppThemePalette.BestTextFor(palette.Accent);
            grid.ColumnHeadersDefaultCellStyle.BackColor = palette.Surface;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = palette.Text;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = palette.Surface;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = palette.Text;
        }

        internal static void ApplyToolStrip(
            ToolStrip strip,
            string themeName)
        {
            if (strip == null)
                return;

            AppThemePalette p = AppThemePalette.FromName(themeName);
            strip.BackColor = p.Surface;
            strip.ForeColor = p.Text;
            strip.Renderer =
                new ToolStripProfessionalRenderer(
                    new OffNetToolStripColorTable(p));

            foreach (ToolStripItem item in strip.Items)
            {
                item.BackColor = p.Surface;
                item.ForeColor = p.Text;

                ToolStripControlHost host = item as ToolStripControlHost;
                if (host != null &&
                    host.Control != null &&
                    !(host.Control is TrafficGraphControl))
                {
                    Apply(host.Control, themeName);
                }
            }
        }
    }

    internal static class FullscreenDetector
    {
        internal static bool IsForegroundFullscreen(IntPtr offNetWindow)
        {
            try
            {
                IntPtr foreground = NativeMethods.GetForegroundWindow();

                if (foreground == IntPtr.Zero ||
                    foreground == offNetWindow ||
                    foreground == NativeMethods.GetShellWindow() ||
                    !NativeMethods.IsWindowVisible(foreground))
                {
                    return false;
                }

                IntPtr taskbar = NativeMethods.FindWindow("Shell_TrayWnd", null);
                if (foreground == taskbar)
                    return false;

                NativeMethods.RECT windowRect;
                if (!NativeMethods.GetWindowRect(foreground, out windowRect))
                    return false;

                IntPtr monitor =
                    NativeMethods.MonitorFromWindow(
                        foreground,
                        NativeMethods.MONITOR_DEFAULTTONEAREST);

                if (monitor == IntPtr.Zero)
                    return false;

                NativeMethods.MONITORINFO info =
                    new NativeMethods.MONITORINFO();
                info.cbSize =
                    Marshal.SizeOf(typeof(NativeMethods.MONITORINFO));

                if (!NativeMethods.GetMonitorInfo(monitor, ref info))
                    return false;

                const int tolerance = 3;

                return
                    windowRect.Left <= info.rcMonitor.Left + tolerance &&
                    windowRect.Top <= info.rcMonitor.Top + tolerance &&
                    windowRect.Right >= info.rcMonitor.Right - tolerance &&
                    windowRect.Bottom >= info.rcMonitor.Bottom - tolerance;
            }
            catch
            {
                return false;
            }
        }
    }

    internal sealed class TrafficScaleState
    {
        private double currentMaximum = 1.0;
        private DateTime nextRefreshUtc = DateTime.MinValue;

        internal void Reset()
        {
            currentMaximum = 1.0;
            nextRefreshUtc = DateTime.MinValue;
        }

        internal double GetMaximum(
            List<TrafficSample> samples,
            int refreshSeconds)
        {
            DateTime now = DateTime.UtcNow;
            int seconds = Math.Max(1, Math.Min(30, refreshSeconds));

            if (now < nextRefreshUtc && currentMaximum > 0.0)
                return currentMaximum;

            DateTime start = now.AddSeconds(-seconds);
            double peak = 0.0;

            if (samples != null)
            {
                foreach (TrafficSample sample in samples)
                {
                    if (sample.TimestampUtc < start)
                        continue;

                    peak = Math.Max(peak, sample.DownloadMbps);
                    peak = Math.Max(peak, sample.UploadMbps);

                    if (sample.ManagedDisabled)
                        peak = Math.Max(peak, sample.OfflineActivityMbps);
                }
            }

            currentMaximum = NiceMaximum(peak * 1.10);
            nextRefreshUtc = now.AddSeconds(seconds);
            return currentMaximum;
        }

        private static double NiceMaximum(double value)
        {
            if (value <= 0.08)
                return 0.1;

            double exponent =
                Math.Pow(10.0, Math.Floor(Math.Log10(value)));
            double normalized = value / exponent;
            double nice;

            if (normalized <= 1.0) nice = 1.0;
            else if (normalized <= 1.5) nice = 1.5;
            else if (normalized <= 2.0) nice = 2.0;
            else if (normalized <= 3.0) nice = 3.0;
            else if (normalized <= 5.0) nice = 5.0;
            else if (normalized <= 7.5) nice = 7.5;
            else nice = 10.0;

            return nice * exponent;
        }
    }

    internal sealed class OptionsForm : Form
    {
        private readonly ComboBox languageBox;
        private readonly Label languageLabel;
        private readonly Label applicationThemeLabel;
        private readonly ComboBox applicationThemeBox;

        private readonly GroupBox colorsGroup;
        private readonly Label activeLabel;
        private readonly Label disabledLabel;
        private readonly Label offlineLabel;
        private readonly Label hintLabel;
        private Button activeButton;
        private Button disabledButton;
        private Button offlineButton;

        private readonly GroupBox trafficGroup;
        private readonly CheckBox trafficOverviewCheckBox;
        private readonly CheckBox taskbarTrafficCheckBox;
        private readonly Label taskbarFontLabel;
        private readonly ComboBox taskbarFontBox;
        private readonly CheckBox taskbarTextShadowCheckBox;
        private readonly Label trafficHistoryLabel;
        private readonly TrackBar trafficHistorySlider;
        private readonly Label trafficHistoryValue;
        private readonly Label trafficSamplePointsLabel;
        private readonly TrackBar trafficSamplePointsSlider;
        private readonly Label trafficSamplePointsValue;
        private readonly Label trafficScaleRefreshLabel;
        private readonly TrackBar trafficScaleRefreshSlider;
        private readonly Label trafficScaleRefreshValue;
        private readonly Label statisticsThemeLabel;
        private readonly ComboBox statisticsThemeBox;
        private readonly Label downloadLabel;
        private readonly Label uploadLabel;
        private readonly Label offlineActivityLabel;
        private readonly Label trafficHintLabel;
        private Button downloadButton;
        private Button uploadButton;
        private Button offlineActivityButton;

        private readonly CheckBox startupCheckBox;
        private readonly Button defaultsButton;
        private readonly Button projectButton;
        private readonly Button okButton;
        private readonly Button cancelButton;
        private readonly ColorDialog colorDialog;

        private Color activeColor;
        private Color disabledColor;
        private Color offlineColor;
        private Color downloadColor;
        private Color uploadColor;
        private Color offlineActivityColor;
        private bool applyingLanguage;

        internal AppSettings ResultSettings { get; private set; }
        internal bool ResultStartWithWindows { get; private set; }

        internal OptionsForm(AppSettings current, bool startWithWindows)
        {
            ResultSettings = current.Clone();

            activeColor = Color.FromArgb(current.ActiveColorArgb);
            disabledColor = Color.FromArgb(current.DisabledColorArgb);
            offlineColor = Color.FromArgb(current.OfflineColorArgb);
            downloadColor = Color.FromArgb(current.DownloadColorArgb);
            uploadColor = Color.FromArgb(current.UploadColorArgb);
            offlineActivityColor = Color.FromArgb(current.OfflineActivityColorArgb);

            ResultStartWithWindows = startWithWindows;

            Icon = AppIconProvider.Shared;
            ShowIcon = true;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Width = 700;
            Height = 920;
            MinimumSize = new Size(660, 560);
            AutoScroll = true;
            AutoScrollMinSize = new Size(660, 960);
            Font = new Font("Segoe UI", 9.0f);

            languageLabel = new Label();
            languageLabel.AutoSize = true;
            languageLabel.Location = new Point(18, 22);
            Controls.Add(languageLabel);

            languageBox = new ComboBox();
            languageBox.DropDownStyle = ComboBoxStyle.DropDownList;
            languageBox.Location = new Point(230, 18);
            languageBox.Width = 405;
            Controls.Add(languageBox);

            applicationThemeLabel = new Label();
            applicationThemeLabel.AutoSize = true;
            applicationThemeLabel.Location = new Point(18, 58);
            Controls.Add(applicationThemeLabel);

            applicationThemeBox = new ComboBox();
            applicationThemeBox.DropDownStyle = ComboBoxStyle.DropDownList;
            applicationThemeBox.Location = new Point(230, 54);
            applicationThemeBox.Width = 405;
            applicationThemeBox.Items.AddRange(new object[]
            {
                "Muffin",
                "Light",
                "Dark",
                "Sepia",
                "Ocean",
                "Matrix",
                "Hellfire",
                "Purple",
                "Aurora"
            });
            applicationThemeBox.SelectedItem =
                String.IsNullOrWhiteSpace(current.ApplicationTheme)
                    ? "Muffin"
                    : current.ApplicationTheme;
            if (applicationThemeBox.SelectedIndex < 0)
                applicationThemeBox.SelectedItem = "Muffin";
            Controls.Add(applicationThemeBox);

            colorsGroup = new GroupBox();
            colorsGroup.Location = new Point(16, 94);
            colorsGroup.Size = new Size(628, 190);
            Controls.Add(colorsGroup);

            activeLabel = AddColorRow(colorsGroup, 28, out activeButton);
            disabledLabel = AddColorRow(colorsGroup, 72, out disabledButton);
            offlineLabel = AddColorRow(colorsGroup, 116, out offlineButton);

            hintLabel = new Label();
            hintLabel.AutoSize = false;
            hintLabel.Location = new Point(16, 153);
            hintLabel.Size = new Size(592, 30);
            colorsGroup.Controls.Add(hintLabel);

            activeButton.Click += delegate { activeColor = PickColor(activeColor); UpdateColorButtons(); };
            disabledButton.Click += delegate { disabledColor = PickColor(disabledColor); UpdateColorButtons(); };
            offlineButton.Click += delegate { offlineColor = PickColor(offlineColor); UpdateColorButtons(); };

            trafficGroup = new GroupBox();
            trafficGroup.Location = new Point(16, 294);
            trafficGroup.Size = new Size(628, 535);
            Controls.Add(trafficGroup);

            trafficOverviewCheckBox = new CheckBox();
            trafficOverviewCheckBox.AutoSize = false;
            trafficOverviewCheckBox.Location = new Point(16, 24);
            trafficOverviewCheckBox.Size = new Size(592, 26);
            trafficOverviewCheckBox.Checked = current.ShowTrafficOverview;
            trafficGroup.Controls.Add(trafficOverviewCheckBox);

            taskbarTrafficCheckBox = new CheckBox();
            taskbarTrafficCheckBox.AutoSize = false;
            taskbarTrafficCheckBox.Location = new Point(16, 52);
            taskbarTrafficCheckBox.Size = new Size(592, 26);
            taskbarTrafficCheckBox.Checked = current.ShowTaskbarTrafficMeter;
            trafficGroup.Controls.Add(taskbarTrafficCheckBox);

            taskbarFontLabel = new Label();
            taskbarFontLabel.AutoSize = false;
            taskbarFontLabel.Location = new Point(16, 86);
            taskbarFontLabel.Size = new Size(300, 24);
            trafficGroup.Controls.Add(taskbarFontLabel);

            taskbarFontBox = new ComboBox();
            taskbarFontBox.DropDownStyle = ComboBoxStyle.DropDownList;
            taskbarFontBox.Location = new Point(390, 82);
            taskbarFontBox.Size = new Size(210, 24);
            trafficGroup.Controls.Add(taskbarFontBox);

            taskbarTextShadowCheckBox = new CheckBox();
            taskbarTextShadowCheckBox.AutoSize = false;
            taskbarTextShadowCheckBox.Location = new Point(16, 114);
            taskbarTextShadowCheckBox.Size = new Size(592, 26);
            taskbarTextShadowCheckBox.Checked = current.TaskbarTextShadow;
            trafficGroup.Controls.Add(taskbarTextShadowCheckBox);

            trafficHistoryLabel = new Label();
            trafficHistoryLabel.AutoSize = false;
            trafficHistoryLabel.Location = new Point(16, 150);
            trafficHistoryLabel.Size = new Size(190, 24);
            trafficGroup.Controls.Add(trafficHistoryLabel);

            trafficHistorySlider = new TrackBar();
            trafficHistorySlider.Minimum = 1;
            trafficHistorySlider.Maximum = 60;
            trafficHistorySlider.TickFrequency = 5;
            trafficHistorySlider.SmallChange = 1;
            trafficHistorySlider.LargeChange = 6;
            trafficHistorySlider.AutoSize = false;
            trafficHistorySlider.Location = new Point(230, 144);
            trafficHistorySlider.Size = new Size(300, 34);
            trafficHistorySlider.Value =
                Math.Max(1, Math.Min(60, current.TrafficHistorySeconds / 10));
            trafficGroup.Controls.Add(trafficHistorySlider);

            trafficHistoryValue = new Label();
            trafficHistoryValue.AutoSize = false;
            trafficHistoryValue.TextAlign = ContentAlignment.MiddleRight;
            trafficHistoryValue.Location = new Point(538, 147);
            trafficHistoryValue.Size = new Size(70, 24);
            trafficGroup.Controls.Add(trafficHistoryValue);

            trafficHistorySlider.ValueChanged += delegate
            {
                trafficHistoryValue.Text =
                    FormatHistoryDuration(trafficHistorySlider.Value * 10);
            };

            trafficHistoryValue.Text =
                FormatHistoryDuration(trafficHistorySlider.Value * 10);

            trafficSamplePointsLabel = new Label();
            trafficSamplePointsLabel.AutoSize = false;
            trafficSamplePointsLabel.Location = new Point(16, 180);
            trafficSamplePointsLabel.Size = new Size(200, 24);
            trafficGroup.Controls.Add(trafficSamplePointsLabel);

            trafficSamplePointsSlider = new TrackBar();
            trafficSamplePointsSlider.Minimum = 10;
            trafficSamplePointsSlider.Maximum = 300;
            trafficSamplePointsSlider.TickFrequency = 25;
            trafficSamplePointsSlider.SmallChange = 10;
            trafficSamplePointsSlider.LargeChange = 25;
            trafficSamplePointsSlider.AutoSize = false;
            trafficSamplePointsSlider.Location = new Point(230, 175);
            trafficSamplePointsSlider.Size = new Size(300, 34);
            trafficSamplePointsSlider.Value = Math.Max(10, Math.Min(300, current.TrafficSamplePoints));
            trafficGroup.Controls.Add(trafficSamplePointsSlider);

            trafficSamplePointsValue = new Label();
            trafficSamplePointsValue.AutoSize = false;
            trafficSamplePointsValue.TextAlign = ContentAlignment.MiddleRight;
            trafficSamplePointsValue.Location = new Point(538, 177);
            trafficSamplePointsValue.Size = new Size(70, 24);
            trafficSamplePointsValue.Text = trafficSamplePointsSlider.Value.ToString(CultureInfo.InvariantCulture);
            trafficGroup.Controls.Add(trafficSamplePointsValue);

            trafficSamplePointsSlider.ValueChanged += delegate
            {
                trafficSamplePointsValue.Text =
                    trafficSamplePointsSlider.Value.ToString(CultureInfo.InvariantCulture);
            };

            trafficScaleRefreshLabel = new Label();
            trafficScaleRefreshLabel.AutoSize = false;
            trafficScaleRefreshLabel.Location = new Point(16, 218);
            trafficScaleRefreshLabel.Size = new Size(200, 24);
            trafficGroup.Controls.Add(trafficScaleRefreshLabel);

            trafficScaleRefreshSlider = new TrackBar();
            trafficScaleRefreshSlider.Minimum = 1;
            trafficScaleRefreshSlider.Maximum = 30;
            trafficScaleRefreshSlider.TickFrequency = 5;
            trafficScaleRefreshSlider.SmallChange = 1;
            trafficScaleRefreshSlider.LargeChange = 5;
            trafficScaleRefreshSlider.AutoSize = false;
            trafficScaleRefreshSlider.Location = new Point(230, 212);
            trafficScaleRefreshSlider.Size = new Size(300, 34);
            trafficScaleRefreshSlider.Value =
                Math.Max(1, Math.Min(30, current.TrafficScaleRefreshSeconds));
            trafficGroup.Controls.Add(trafficScaleRefreshSlider);

            trafficScaleRefreshValue = new Label();
            trafficScaleRefreshValue.AutoSize = false;
            trafficScaleRefreshValue.TextAlign = ContentAlignment.MiddleRight;
            trafficScaleRefreshValue.Location = new Point(538, 215);
            trafficScaleRefreshValue.Size = new Size(70, 24);
            trafficScaleRefreshValue.Text =
                trafficScaleRefreshSlider.Value.ToString(CultureInfo.CurrentCulture) +
                " s";
            trafficGroup.Controls.Add(trafficScaleRefreshValue);

            trafficScaleRefreshSlider.ValueChanged += delegate
            {
                trafficScaleRefreshValue.Text =
                    trafficScaleRefreshSlider.Value.ToString(CultureInfo.CurrentCulture) +
                    " s";
            };

            statisticsThemeLabel = new Label();
            statisticsThemeLabel.AutoSize = false;
            statisticsThemeLabel.Location = new Point(16, 256);
            statisticsThemeLabel.Size = new Size(190, 24);
            trafficGroup.Controls.Add(statisticsThemeLabel);

            statisticsThemeBox = new ComboBox();
            statisticsThemeBox.DropDownStyle = ComboBoxStyle.DropDownList;
            statisticsThemeBox.Location = new Point(230, 252);
            statisticsThemeBox.Size = new Size(200, 24);
            statisticsThemeBox.Items.AddRange(new object[]
            {
                "Light",
                "Dark",
                "Sepia",
                "Ocean",
                "Matrix",
                "Hellfire",
                "Purple",
                "Aurora"
            });
            statisticsThemeBox.SelectedItem =
                String.IsNullOrWhiteSpace(current.StatisticsTheme)
                    ? "Dark"
                    : current.StatisticsTheme;
            if (statisticsThemeBox.SelectedIndex < 0)
                statisticsThemeBox.SelectedItem = "Dark";
            trafficGroup.Controls.Add(statisticsThemeBox);

            downloadLabel = AddColorRow(trafficGroup, 294, out downloadButton);
            uploadLabel = AddColorRow(trafficGroup, 334, out uploadButton);
            offlineActivityLabel = AddColorRow(trafficGroup, 374, out offlineActivityButton);

            downloadButton.Click += delegate { downloadColor = PickColor(downloadColor); UpdateColorButtons(); };
            uploadButton.Click += delegate { uploadColor = PickColor(uploadColor); UpdateColorButtons(); };
            offlineActivityButton.Click += delegate { offlineActivityColor = PickColor(offlineActivityColor); UpdateColorButtons(); };

            trafficHintLabel = new Label();
            trafficHintLabel.AutoSize = false;
            trafficHintLabel.Location = new Point(16, 414);
            trafficHintLabel.Size = new Size(592, 108);
            trafficGroup.Controls.Add(trafficHintLabel);

            startupCheckBox = new CheckBox();
            startupCheckBox.AutoSize = false;
            startupCheckBox.Location = new Point(18, 842);
            startupCheckBox.Size = new Size(620, 34);
            startupCheckBox.Checked = startWithWindows;
            Controls.Add(startupCheckBox);

            defaultsButton = new Button();
            defaultsButton.Location = new Point(16, 886);
            defaultsButton.Size = new Size(165, 32);
            defaultsButton.Click += delegate
            {
                activeColor = Color.LimeGreen;
                disabledColor = Color.Red;
                offlineColor = Color.Gold;

                downloadColor = Color.DodgerBlue;
                uploadColor = Color.DarkOrange;
                offlineActivityColor = Color.MediumPurple;
                trafficHistorySlider.Value = 6;
                trafficSamplePointsSlider.Value = 75;
                trafficScaleRefreshSlider.Value = 5;
                statisticsThemeBox.SelectedItem = "Dark";
                applicationThemeBox.SelectedItem = "Muffin";
                taskbarTextShadowCheckBox.Checked = false;
                trafficOverviewCheckBox.Checked = false;
                taskbarTrafficCheckBox.Checked = false;
                SelectTaskbarFont("Segoe UI Semibold");

                UpdateColorButtons();
            };
            Controls.Add(defaultsButton);

            projectButton = new Button();
            projectButton.Location = new Point(190, 886);
            projectButton.Size = new Size(190, 32);
            projectButton.Click += delegate { ShellHelper.OpenProjectPage(); };
            Controls.Add(projectButton);

            okButton = new Button();
            okButton.Location = new Point(454, 886);
            okButton.Size = new Size(90, 32);
            okButton.Click += delegate
            {
                // Commit first, then close. This makes runtime language changes
                // deterministic and independent of any later startup-task work.
                CommitSettings();
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(okButton);

            cancelButton = new Button();
            cancelButton.Location = new Point(554, 886);
            cancelButton.Size = new Size(90, 32);
            cancelButton.DialogResult = DialogResult.Cancel;
            Controls.Add(cancelButton);

            AcceptButton = okButton;
            CancelButton = cancelButton;

            colorDialog = new ColorDialog();
            colorDialog.FullOpen = true;

            FillLanguages(current.Language);
            FillTaskbarFonts(current.TaskbarMeterFontName);
            applicationThemeBox.SelectedIndexChanged += delegate
            {
                ApplySelectedApplicationTheme();
            };

            Shown += delegate
            {
                FitToWorkingArea();
            };

            languageBox.SelectedIndexChanged += delegate
            {
                if (applyingLanguage) return;

                LanguageChoice choice = languageBox.SelectedItem as LanguageChoice;
                if (choice == null) return;

                Localization.SetLanguage(choice.Code);
                ApplyLocalization();
                FillLanguages(choice.Code);
                ApplySelectedApplicationTheme();
            };

            ApplyLocalization();
            ApplySelectedApplicationTheme();
            UpdateColorButtons();
        }

        private void FitToWorkingArea()
        {
            Screen screen = Screen.FromControl(this);
            Rectangle area = screen.WorkingArea;

            int targetHeight =
                Math.Max(
                    MinimumSize.Height,
                    Math.Min(1040, area.Height - 100));

            int maximumSafeHeight = Math.Max(480, area.Height - 100);
            targetHeight = Math.Min(targetHeight, maximumSafeHeight);

            Height = targetHeight;
            Width = Math.Min(Math.Max(700, Width), Math.Max(700, area.Width - 100));
            Left = area.Left + Math.Max(50, (area.Width - Width) / 2);
            Top = area.Top + Math.Max(50, (area.Height - Height) / 2);
        }

        private void ApplySelectedApplicationTheme()
        {
            string name =
                applicationThemeBox == null ||
                applicationThemeBox.SelectedItem == null
                    ? "Muffin"
                    : applicationThemeBox.SelectedItem.ToString();

            AppThemeManager.Apply(this, name);

            AppThemePalette palette =
                AppThemePalette.FromName(name);
            hintLabel.ForeColor = palette.SecondaryText;
            trafficHintLabel.ForeColor = palette.SecondaryText;

            UpdateColorButtons();
        }

        private Label AddColorRow(Control parent, int y, out Button button)
        {
            Label label = new Label();
            label.AutoSize = false;
            label.Location = new Point(14, y + 6);
            label.Size = new Size(355, 24);
            parent.Controls.Add(label);

            button = new Button();
            button.Location = new Point(390, y);
            button.Size = new Size(210, 30);
            parent.Controls.Add(button);

            return label;
        }

        private void FillLanguages(string selectCode)
        {
            applyingLanguage = true;
            try
            {
                languageBox.Items.Clear();
                languageBox.Items.Add(new LanguageChoice("en", Localization.T("English")));
                languageBox.Items.Add(new LanguageChoice("de", Localization.T("German")));
                languageBox.Items.Add(new LanguageChoice("fr", Localization.T("French")));

                for (int i = 0; i < languageBox.Items.Count; i++)
                {
                    LanguageChoice item = languageBox.Items[i] as LanguageChoice;
                    if (item != null && item.Code == selectCode)
                    {
                        languageBox.SelectedIndex = i;
                        break;
                    }
                }

                if (languageBox.SelectedIndex < 0)
                    languageBox.SelectedIndex = 0;
            }
            finally
            {
                applyingLanguage = false;
            }
        }

        private static string FormatHistoryDuration(int seconds)
        {
            seconds = Math.Max(10, Math.Min(600, seconds));

            if (seconds < 60)
                return seconds.ToString(CultureInfo.CurrentCulture) + " s";

            if ((seconds % 60) == 0)
                return (seconds / 60).ToString(CultureInfo.CurrentCulture) + " min";

            return (seconds / 60).ToString(CultureInfo.CurrentCulture) +
                   ":" +
                   (seconds % 60).ToString("00", CultureInfo.CurrentCulture) +
                   " min";
        }

        private void FillTaskbarFonts(string selectedFont)
        {
            string wanted = String.IsNullOrWhiteSpace(selectedFont)
                ? "Segoe UI Semibold"
                : selectedFont;

            taskbarFontBox.BeginUpdate();

            try
            {
                taskbarFontBox.Items.Clear();

                try
                {
                    using (System.Drawing.Text.InstalledFontCollection fonts =
                        new System.Drawing.Text.InstalledFontCollection())
                    {
                        foreach (FontFamily family in fonts.Families)
                        {
                            if (!String.IsNullOrWhiteSpace(family.Name))
                                taskbarFontBox.Items.Add(family.Name);
                        }
                    }
                }
                catch
                {
                    taskbarFontBox.Items.Add("Segoe UI");
                    taskbarFontBox.Items.Add("Segoe UI Semibold");
                    taskbarFontBox.Items.Add("Tahoma");
                    taskbarFontBox.Items.Add("Arial");
                    taskbarFontBox.Items.Add("Consolas");
                }

                SelectTaskbarFont(wanted);
            }
            finally
            {
                taskbarFontBox.EndUpdate();
            }
        }

        private void SelectTaskbarFont(string fontName)
        {
            if (taskbarFontBox == null)
                return;

            string wanted = String.IsNullOrWhiteSpace(fontName)
                ? "Segoe UI Semibold"
                : fontName;

            for (int i = 0; i < taskbarFontBox.Items.Count; i++)
            {
                string item = taskbarFontBox.Items[i] as string;

                if (String.Equals(
                    item,
                    wanted,
                    StringComparison.OrdinalIgnoreCase))
                {
                    taskbarFontBox.SelectedIndex = i;
                    return;
                }
            }

            // Preserve a configured font name even if Windows temporarily cannot
            // enumerate it. The renderer still has a safe fallback.
            taskbarFontBox.Items.Insert(0, wanted);
            taskbarFontBox.SelectedIndex = 0;
        }

        private Color PickColor(Color current)
        {
            colorDialog.Color = current;
            return colorDialog.ShowDialog(this) == DialogResult.OK ? colorDialog.Color : current;
        }

        private void UpdateColorButtons()
        {
            SetColorButton(activeButton, activeColor);
            SetColorButton(disabledButton, disabledColor);
            SetColorButton(offlineButton, offlineColor);

            SetColorButton(downloadButton, downloadColor);
            SetColorButton(uploadButton, uploadColor);
            SetColorButton(offlineActivityButton, offlineActivityColor);
        }

        private static void SetColorButton(Button button, Color color)
        {
            button.BackColor = color;
            button.ForeColor = AppThemePalette.BestTextFor(color);
            button.Text = String.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }

        private void ApplyLocalization()
        {
            Text = Localization.T("OptionsTitle");

            languageLabel.Text = Localization.T("Language");
            applicationThemeLabel.Text = Localization.T("ApplicationTheme");

            colorsGroup.Text = Localization.T("TrayColors");
            activeLabel.Text = Localization.T("ActiveInternetColor");
            disabledLabel.Text = Localization.T("DisabledColor");
            offlineLabel.Text = Localization.T("OfflineColor");
            hintLabel.Text = Localization.T("OptionsHint");

            trafficGroup.Text = Localization.T("TrafficOverviewGroup");
            trafficOverviewCheckBox.Text = Localization.T("ShowTrafficOverview");
            taskbarTrafficCheckBox.Text = Localization.T("ShowTaskbarTrafficMeter");
            taskbarFontLabel.Text = Localization.T("TaskbarMeterFont");
            taskbarTextShadowCheckBox.Text = Localization.T("TaskbarTextShadow");
            trafficHistoryLabel.Text = Localization.T("TrafficHistory");
            trafficSamplePointsLabel.Text = Localization.T("TrafficSamplePoints");
            trafficScaleRefreshLabel.Text = Localization.T("TrafficScaleRefresh");
            statisticsThemeLabel.Text = Localization.T("StatisticsTheme");
            downloadLabel.Text = Localization.T("DownloadColor");
            uploadLabel.Text = Localization.T("UploadColor");
            offlineActivityLabel.Text = Localization.T("OfflineActivityColor");
            trafficHintLabel.Text = Localization.T("TrafficOptionsHint");

            startupCheckBox.Text = Localization.T("StartWithWindows");
            defaultsButton.Text = Localization.T("RestoreDefaults");
            projectButton.Text = Localization.T("OpenProject");
            okButton.Text = Localization.T("OK");
            cancelButton.Text = Localization.T("Cancel");
        }

        private void CommitSettings()
        {
            LanguageChoice choice = languageBox.SelectedItem as LanguageChoice;

            ResultSettings.Language = choice == null ? "en" : choice.Code;
            ResultSettings.ActiveColorArgb = activeColor.ToArgb();
            ResultSettings.DisabledColorArgb = disabledColor.ToArgb();
            ResultSettings.OfflineColorArgb = offlineColor.ToArgb();

            ResultSettings.ShowTrafficOverview = trafficOverviewCheckBox.Checked;
            ResultSettings.ShowTaskbarTrafficMeter = taskbarTrafficCheckBox.Checked;
            ResultSettings.TaskbarMeterFontName =
                taskbarFontBox.SelectedItem == null
                    ? "Segoe UI Semibold"
                    : taskbarFontBox.SelectedItem.ToString();
            ResultSettings.TaskbarTextShadow = taskbarTextShadowCheckBox.Checked;
            ResultSettings.TrafficHistorySeconds = trafficHistorySlider.Value * 10;
            ResultSettings.TrafficSamplePoints = trafficSamplePointsSlider.Value;
            ResultSettings.TrafficScaleRefreshSeconds = trafficScaleRefreshSlider.Value;
            ResultSettings.StatisticsTheme =
                statisticsThemeBox.SelectedItem == null
                    ? "Dark"
                    : statisticsThemeBox.SelectedItem.ToString();
            ResultSettings.ApplicationTheme =
                applicationThemeBox.SelectedItem == null
                    ? "Muffin"
                    : applicationThemeBox.SelectedItem.ToString();
            ResultSettings.DownloadColorArgb = downloadColor.ToArgb();
            ResultSettings.UploadColorArgb = uploadColor.ToArgb();
            ResultSettings.OfflineActivityColorArgb = offlineActivityColor.ToArgb();

            ResultStartWithWindows = startupCheckBox.Checked;
        }
    }

    internal sealed class MainWindow : Form
    {
        private readonly DataGridView grid;
        private readonly CheckBox showNonPresent;
        private readonly Label statusLabel;
        private readonly Label titleLabel;
        private readonly Label descriptionLabel;
        private readonly Label legendLabel;
        private readonly Button refreshButton;
        private readonly Button enableButton;
        private readonly Button disableButton;
        private readonly Button reconnectButton;
        private readonly Button deviceManagerButton;
        private string currentTheme = "Muffin";
        private bool loading;

        internal event EventHandler ManagedDevicesChanged;
        internal event EventHandler ReconnectRequested;

        internal MainWindow()
        {
            Text = "OffNet " + Program.Version;
            Icon = AppIconProvider.Shared;
            ShowIcon = true;
            StartPosition = FormStartPosition.CenterScreen;
            Width = 1120;
            Height = 680;
            MinimumSize = new Size(850, 520);
            Font = new Font("Segoe UI", 9.0f);
            ShowInTaskbar = true;

            Panel header = new Panel();
            header.Dock = DockStyle.Fill;

            titleLabel = new Label();
            titleLabel.Font = new Font("Segoe UI Semibold", 15.0f);
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(14, 10);
            header.Controls.Add(titleLabel);

            descriptionLabel = new Label();
            descriptionLabel.ForeColor = Color.DimGray;
            descriptionLabel.AutoSize = true;
            descriptionLabel.Location = new Point(17, 44);
            header.Controls.Add(descriptionLabel);

            legendLabel = new Label();
            legendLabel.AutoSize = true;
            legendLabel.Location = new Point(17, 70);
            header.Controls.Add(legendLabel);

            showNonPresent = new CheckBox();
            showNonPresent.AutoSize = true;
            showNonPresent.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            showNonPresent.Location = new Point(825, 18);
            showNonPresent.CheckedChanged += delegate { RefreshDevices(); };
            header.Controls.Add(showNonPresent);

            refreshButton = new Button();
            refreshButton.Width = 120;
            refreshButton.Height = 30;
            refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            refreshButton.Location = new Point(955, 56);
            refreshButton.Click += delegate { RefreshDevices(); };
            header.Controls.Add(refreshButton);

            grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = true;
            grid.AutoGenerateColumns = false;
            grid.BackgroundColor = SystemColors.Window;
            grid.BorderStyle = BorderStyle.Fixed3D;

            DataGridViewCheckBoxColumn managedColumn = new DataGridViewCheckBoxColumn();
            managedColumn.Width = 105;
            managedColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            grid.Columns.Add(managedColumn);

            DataGridViewTextBoxColumn stateColumn = new DataGridViewTextBoxColumn();
            stateColumn.Width = 145;
            stateColumn.ReadOnly = true;
            grid.Columns.Add(stateColumn);

            DataGridViewTextBoxColumn kindColumn = new DataGridViewTextBoxColumn();
            kindColumn.Width = 115;
            kindColumn.ReadOnly = true;
            grid.Columns.Add(kindColumn);

            DataGridViewTextBoxColumn nameColumn = new DataGridViewTextBoxColumn();
            nameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            nameColumn.FillWeight = 35;
            nameColumn.ReadOnly = true;
            grid.Columns.Add(nameColumn);

            DataGridViewTextBoxColumn manufacturerColumn = new DataGridViewTextBoxColumn();
            manufacturerColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            manufacturerColumn.FillWeight = 22;
            manufacturerColumn.ReadOnly = true;
            grid.Columns.Add(manufacturerColumn);

            DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn();
            idColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            idColumn.FillWeight = 43;
            idColumn.ReadOnly = true;
            grid.Columns.Add(idColumn);

            grid.CurrentCellDirtyStateChanged += delegate
            {
                if (grid.IsCurrentCellDirty) grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            grid.CellValueChanged += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (loading) return;
                if (e.RowIndex >= 0 && e.ColumnIndex == 0)
                {
                    SaveManagedSelection();
                    EventHandler handler = ManagedDevicesChanged;
                    if (handler != null) handler(this, EventArgs.Empty);
                }
            };

            Panel bottom = new Panel();
            bottom.Dock = DockStyle.Fill;

            enableButton = new Button();
            enableButton.Width = 175;
            enableButton.Height = 32;
            enableButton.Location = new Point(12, 10);
            enableButton.Click += delegate { SetSelectedDevices(true); };
            bottom.Controls.Add(enableButton);

            disableButton = new Button();
            disableButton.Width = 185;
            disableButton.Height = 32;
            disableButton.Location = new Point(195, 10);
            disableButton.Click += delegate { SetSelectedDevices(false); };
            bottom.Controls.Add(disableButton);

            reconnectButton = new Button();
            reconnectButton.Width = 120;
            reconnectButton.Height = 32;
            reconnectButton.Location = new Point(388, 10);
            reconnectButton.Click += delegate
            {
                EventHandler handler = ReconnectRequested;
                if (handler != null) handler(this, EventArgs.Empty);
            };
            bottom.Controls.Add(reconnectButton);

            deviceManagerButton = new Button();
            deviceManagerButton.Width = 150;
            deviceManagerButton.Height = 32;
            deviceManagerButton.Location = new Point(516, 10);
            deviceManagerButton.Click += delegate { Process.Start("devmgmt.msc"); };
            bottom.Controls.Add(deviceManagerButton);

            statusLabel = new Label();
            statusLabel.AutoEllipsis = true;
            statusLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            statusLabel.Location = new Point(14, 57);
            statusLabel.Size = new Size(1060, 22);
            bottom.Controls.Add(statusLabel);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 3;
            layout.Margin = new Padding(0);
            layout.Padding = new Padding(0);
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100.0f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112.0f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100.0f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 94.0f));
            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(grid, 0, 1);
            layout.Controls.Add(bottom, 0, 2);
            Controls.Add(layout);

            ApplyLocalization();
        }

        internal void ApplyLocalization()
        {
            Text = "OffNet " + Program.Version;
            titleLabel.Text = Localization.T("MainTitle");
            descriptionLabel.Text = Localization.T("MainDescription");
            legendLabel.Text = Localization.T("MainLegend");
            showNonPresent.Text = Localization.T("ShowNonPresent");
            refreshButton.Text = Localization.T("Reload");
            grid.Columns[0].HeaderText = Localization.T("ColTray");
            grid.Columns[1].HeaderText = Localization.T("ColStatus");
            grid.Columns[2].HeaderText = Localization.T("ColType");
            grid.Columns[3].HeaderText = Localization.T("ColDevice");
            grid.Columns[4].HeaderText = Localization.T("ColManufacturer");
            grid.Columns[5].HeaderText = Localization.T("ColInstance");
            enableButton.Text = Localization.T("SelectedEnable");
            disableButton.Text = Localization.T("SelectedDisable");
            reconnectButton.Text = Localization.T("Reconnect");
            deviceManagerButton.Text = Localization.T("DeviceManager");
            if (String.IsNullOrWhiteSpace(statusLabel.Text)) statusLabel.Text = Localization.T("Ready");
        }

        internal void ApplyTheme(string themeName)
        {
            currentTheme =
                String.IsNullOrWhiteSpace(themeName)
                    ? "Muffin"
                    : themeName;

            AppThemeManager.Apply(this, currentTheme);

            AppThemePalette palette =
                AppThemePalette.FromName(currentTheme);

            descriptionLabel.ForeColor = palette.SecondaryText;
            AppThemeManager.ApplyGrid(grid, palette);

            foreach (DataGridViewRow row in grid.Rows)
            {
                string state =
                    row.Cells.Count > 1 && row.Cells[1].Value != null
                        ? row.Cells[1].Value.ToString()
                        : String.Empty;

                if (String.Equals(
                    state,
                    Localization.T("Disabled"),
                    StringComparison.OrdinalIgnoreCase))
                {
                    row.DefaultCellStyle.ForeColor = palette.Danger;
                }
                else if (String.Equals(
                    state,
                    Localization.T("Enabled"),
                    StringComparison.OrdinalIgnoreCase))
                {
                    row.DefaultCellStyle.ForeColor = palette.Success;
                }
                else
                {
                    row.DefaultCellStyle.ForeColor = palette.Text;
                }
            }
        }

        internal void RefreshDevices()
        {
            try
            {
                loading = true;
                statusLabel.Text = Localization.T("ReadingDevices");
                Application.DoEvents();
                List<DeviceInfo> devices = DeviceManager.EnumerateNetworkDevices(showNonPresent.Checked);
                HashSet<string> managed = ConfigurationStore.LoadManagedIds();

                if (!ConfigurationStore.HasManagedDeviceConfiguration)
                {
                    foreach (DeviceInfo device in devices)
                    {
                        if (device.IsPresent && device.IsHardware) managed.Add(device.InstanceId);
                    }
                    if (managed.Count == 0)
                    {
                        foreach (DeviceInfo device in devices) if (device.IsPresent) managed.Add(device.InstanceId);
                    }
                    ConfigurationStore.SaveManagedIds(managed);
                }

                grid.Rows.Clear();
                foreach (DeviceInfo device in devices)
                {
                    bool isManaged = managed.Contains(device.InstanceId);
                    int rowIndex = grid.Rows.Add(isManaged, device.StateText, device.KindText, device.Name, device.Manufacturer, device.InstanceId);
                    DataGridViewRow row = grid.Rows[rowIndex];
                    row.Tag = device.InstanceId;
                    AppThemePalette palette =
                        AppThemePalette.FromName(currentTheme);

                    if (!device.IsPresent)
                        row.DefaultCellStyle.ForeColor = palette.Muted;
                    else if (device.IsDisabled)
                        row.DefaultCellStyle.ForeColor = palette.Danger;
                    else
                        row.DefaultCellStyle.ForeColor = palette.Success;
                }

                statusLabel.Text = Localization.F("FoundDevices", devices.Count, managed.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Program.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = Localization.T("ReadError");
            }
            finally { loading = false; }
        }

        internal void SetStatusText(string text)
        {
            if (InvokeRequired) { BeginInvoke(new Action<string>(SetStatusText), text); return; }
            statusLabel.Text = text;
        }

        private void SaveManagedSelection()
        {
            List<string> ids = new List<string>();
            foreach (DataGridViewRow row in grid.Rows)
            {
                bool selected = row.Cells[0].Value != null && Convert.ToBoolean(row.Cells[0].Value);
                if (selected && row.Tag != null) ids.Add(row.Tag.ToString());
            }
            ConfigurationStore.SaveManagedIds(ids);
            statusLabel.Text = Localization.F("TraySelectionSaved", ids.Count);
        }

        private List<string> GetSelectedDeviceIds()
        {
            List<string> ids = new List<string>();
            HashSet<string> unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataGridViewRow row in grid.SelectedRows)
            {
                if (row.Tag == null) continue;
                string id = row.Tag.ToString();
                if (unique.Add(id)) ids.Add(id);
            }
            return ids;
        }

        private void SetSelectedDevices(bool enable)
        {
            List<string> ids = GetSelectedDeviceIds();
            if (ids.Count == 0)
            {
                MessageBox.Show(Localization.T("SelectDevice"), Program.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!enable)
            {
                DialogResult answer = MessageBox.Show(Localization.T("DisableSelectedConfirm"), Localization.T("DisableTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (answer != DialogResult.Yes) return;
            }

            List<string> errors = new List<string>();
            foreach (string id in ids)
            {
                try { DeviceManager.SetDeviceEnabled(id, enable); }
                catch (Exception ex) { errors.Add(ex.Message); }
            }

            Thread.Sleep(250);
            RefreshDevices();
            EventHandler changed = ManagedDevicesChanged;
            if (changed != null) changed(this, EventArgs.Empty);
            if (errors.Count > 0) MessageBox.Show(String.Join("\r\n\r\n", errors.ToArray()), Program.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    internal sealed class TrafficSample
    {
        internal DateTime TimestampUtc;
        internal double DownloadMbps;
        internal double UploadMbps;
        internal double OfflineActivityMbps;
        internal bool ManagedDisabled;
    }

    internal sealed class TrafficCounterSnapshot
    {
        internal long BytesReceived;
        internal long BytesSent;
    }

    internal sealed class TrafficMonitor
    {
        private readonly object sync = new object();
        private readonly List<TrafficSample> samples = new List<TrafficSample>();
        private readonly Dictionary<string, TrafficCounterSnapshot> previousCounters =
            new Dictionary<string, TrafficCounterSnapshot>(StringComparer.OrdinalIgnoreCase);

        private HashSet<string> managedInterfaceIds =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> managedDescriptions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private string managedSignature = String.Empty;
        private DateTime nextSelectorRefreshUtc = DateTime.MinValue;
        private DateTime lastSampleUtc = DateTime.MinValue;
        private int historySeconds = 60;
        private int maxSamplePoints = 75;

        internal int HistorySeconds
        {
            get
            {
                lock (sync) return historySeconds;
            }
        }

        internal void SetHistorySeconds(int seconds)
        {
            lock (sync)
            {
                historySeconds = Math.Max(10, Math.Min(600, ((seconds + 5) / 10) * 10));
                TrimHistory(DateTime.UtcNow);
            }
        }

        internal void SetSamplePoints(int points)
        {
            lock (sync)
            {
                maxSamplePoints = Math.Max(10, Math.Min(300, points));
                TrimHistory(DateTime.UtcNow);
            }
        }

        internal void ResetCounters()
        {
            lock (sync)
            {
                previousCounters.Clear();
                lastSampleUtc = DateTime.MinValue;
            }
        }

        internal void Sample(HashSet<string> managedDeviceIds, bool managedDisabled)
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                EnsureManagedSelectors(managedDeviceIds, now);

                double elapsedSeconds;
                lock (sync)
                {
                    elapsedSeconds = lastSampleUtc == DateTime.MinValue
                        ? 0.0
                        : (now - lastSampleUtc).TotalSeconds;
                    lastSampleUtc = now;
                }

                double maximumReasonableGapSeconds =
                    Math.Max(
                        15.0,
                        Math.Max(10, historySeconds) * 2.0);

                if (elapsedSeconds <= 0.0 ||
                    elapsedSeconds > maximumReasonableGapSeconds)
                {
                    elapsedSeconds = 0.0;
                }

                double downloadBytes = 0.0;
                double uploadBytes = 0.0;
                double offlineBytes = 0.0;

                HashSet<string> seen =
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface networkInterface in interfaces)
                {
                    if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                        continue;

                    string normalizedId =
                        DeviceManager.NormalizeNetworkInterfaceId(networkInterface.Id);

                    if (String.IsNullOrWhiteSpace(normalizedId))
                        normalizedId = networkInterface.Id;

                    seen.Add(normalizedId);

                    long bytesReceived;
                    long bytesSent;

                    try
                    {
                        IPInterfaceStatistics statistics =
                            networkInterface.GetIPStatistics();
                        bytesReceived = statistics.BytesReceived;
                        bytesSent = statistics.BytesSent;
                    }
                    catch
                    {
                        continue;
                    }

                    TrafficCounterSnapshot previous = null;

                    lock (sync)
                    {
                        previousCounters.TryGetValue(normalizedId, out previous);
                        previousCounters[normalizedId] =
                            new TrafficCounterSnapshot
                            {
                                BytesReceived = bytesReceived,
                                BytesSent = bytesSent
                            };
                    }

                    if (elapsedSeconds <= 0.0 || previous == null)
                        continue;

                    long receivedDelta =
                        bytesReceived >= previous.BytesReceived
                            ? bytesReceived - previous.BytesReceived
                            : 0;

                    long sentDelta =
                        bytesSent >= previous.BytesSent
                            ? bytesSent - previous.BytesSent
                            : 0;

                    bool isManaged =
                        managedInterfaceIds.Contains(normalizedId) ||
                        managedDescriptions.Contains(networkInterface.Description ?? String.Empty) ||
                        managedDescriptions.Contains(networkInterface.Name ?? String.Empty);

                    if (isManaged)
                    {
                        downloadBytes += receivedDelta;
                        uploadBytes += sentDelta;
                    }
                    else if (managedDisabled &&
                             networkInterface.OperationalStatus == OperationalStatus.Up)
                    {
                        // The third curve intentionally combines RX + TX. It exists to
                        // reveal traffic on adapters that OffNet did NOT disable while the
                        // user expects the managed adapters to be offline.
                        offlineBytes += receivedDelta + sentDelta;
                    }
                }

                lock (sync)
                {
                    List<string> stale = new List<string>();
                    foreach (string id in previousCounters.Keys)
                    {
                        if (!seen.Contains(id)) stale.Add(id);
                    }

                    foreach (string id in stale)
                        previousCounters.Remove(id);

                    double divisor =
                        elapsedSeconds > 0.0
                            ? elapsedSeconds * 1000000.0
                            : 1.0;

                    TrafficSample sample = new TrafficSample();
                    sample.TimestampUtc = now;
                    sample.DownloadMbps = elapsedSeconds > 0.0
                        ? (downloadBytes * 8.0) / divisor
                        : 0.0;
                    sample.UploadMbps = elapsedSeconds > 0.0
                        ? (uploadBytes * 8.0) / divisor
                        : 0.0;
                    sample.OfflineActivityMbps = elapsedSeconds > 0.0
                        ? (offlineBytes * 8.0) / divisor
                        : 0.0;
                    sample.ManagedDisabled = managedDisabled;

                    samples.Add(sample);
                    TrimHistory(now);
                }
            }
            catch
            {
                // Traffic telemetry is an optional overview feature and must never
                // interfere with OffNet's device control.
            }
        }

        internal List<TrafficSample> GetSnapshot()
        {
            lock (sync)
            {
                List<TrafficSample> copy =
                    new List<TrafficSample>(samples.Count);

                foreach (TrafficSample sample in samples)
                {
                    TrafficSample cloned = new TrafficSample();
                    cloned.TimestampUtc = sample.TimestampUtc;
                    cloned.DownloadMbps = sample.DownloadMbps;
                    cloned.UploadMbps = sample.UploadMbps;
                    cloned.OfflineActivityMbps = sample.OfflineActivityMbps;
                    cloned.ManagedDisabled = sample.ManagedDisabled;
                    copy.Add(cloned);
                }

                return copy;
            }
        }

        private void EnsureManagedSelectors(
            HashSet<string> managedDeviceIds,
            DateTime now)
        {
            string signature = BuildSignature(managedDeviceIds);

            if (signature == managedSignature &&
                now < nextSelectorRefreshUtc)
            {
                return;
            }

            HashSet<string> interfaceIds;
            HashSet<string> descriptions;

            DeviceManager.GetManagedInterfaceSelectors(
                managedDeviceIds,
                out interfaceIds,
                out descriptions);

            managedInterfaceIds = interfaceIds;
            managedDescriptions = descriptions;
            managedSignature = signature;
            nextSelectorRefreshUtc = now.AddSeconds(30);
        }

        private static string BuildSignature(HashSet<string> ids)
        {
            if (ids == null || ids.Count == 0)
                return String.Empty;

            List<string> values = new List<string>();

            foreach (string id in ids)
            {
                if (!String.IsNullOrWhiteSpace(id))
                    values.Add(id.Trim());
            }

            values.Sort(StringComparer.OrdinalIgnoreCase);
            return String.Join("|", values.ToArray());
        }

        private void TrimHistory(DateTime now)
        {
            DateTime minimum =
                now.AddSeconds(-Math.Max(10, Math.Min(600, historySeconds)));

            int removeCount = 0;

            while (removeCount < samples.Count &&
                   samples[removeCount].TimestampUtc < minimum)
            {
                removeCount++;
            }

            if (removeCount > 0)
                samples.RemoveRange(0, removeCount);

            int pointLimit =
                Math.Max(10, Math.Min(300, maxSamplePoints));

            if (samples.Count > pointLimit)
            {
                samples.RemoveRange(
                    0,
                    samples.Count - pointLimit);
            }
        }
    }

    internal sealed class StatisticsThemePalette
    {
        internal Color Background;
        internal Color Grid;
        internal Color Text;
        internal Color SecondaryText;
        internal Color Border;

        internal static StatisticsThemePalette FromName(string name)
        {
            string key = String.IsNullOrWhiteSpace(name) ? "Dark" : name.Trim();
            StatisticsThemePalette p = new StatisticsThemePalette();

            if (String.Equals(key, "Light", StringComparison.OrdinalIgnoreCase))
            {
                p.Background = Color.FromArgb(248, 248, 248);
                p.Grid = Color.FromArgb(215, 215, 215);
                p.Text = Color.FromArgb(30, 30, 30);
                p.SecondaryText = Color.FromArgb(90, 90, 90);
                p.Border = Color.FromArgb(180, 180, 180);
            }
            else if (String.Equals(key, "Sepia", StringComparison.OrdinalIgnoreCase))
            {
                p.Background = Color.FromArgb(244, 236, 214);
                p.Grid = Color.FromArgb(205, 190, 155);
                p.Text = Color.FromArgb(73, 57, 38);
                p.SecondaryText = Color.FromArgb(115, 92, 62);
                p.Border = Color.FromArgb(174, 151, 112);
            }
            else if (String.Equals(key, "Ocean", StringComparison.OrdinalIgnoreCase))
            {
                p.Background = Color.FromArgb(13, 34, 46);
                p.Grid = Color.FromArgb(32, 79, 99);
                p.Text = Color.FromArgb(218, 242, 247);
                p.SecondaryText = Color.FromArgb(133, 190, 207);
                p.Border = Color.FromArgb(47, 106, 130);
            }
            else if (String.Equals(key, "Matrix", StringComparison.OrdinalIgnoreCase))
            {
                p.Background = Color.FromArgb(5, 15, 7);
                p.Grid = Color.FromArgb(18, 68, 24);
                p.Text = Color.FromArgb(104, 255, 122);
                p.SecondaryText = Color.FromArgb(58, 174, 70);
                p.Border = Color.FromArgb(31, 110, 41);
            }
            else if (String.Equals(key, "Hellfire", StringComparison.OrdinalIgnoreCase))
            {
                p.Background = Color.FromArgb(28, 8, 5);
                p.Grid = Color.FromArgb(95, 27, 14);
                p.Text = Color.FromArgb(255, 223, 184);
                p.SecondaryText = Color.FromArgb(229, 117, 60);
                p.Border = Color.FromArgb(145, 48, 22);
            }
            else if (String.Equals(key, "Purple", StringComparison.OrdinalIgnoreCase))
            {
                p.Background = Color.FromArgb(27, 20, 38);
                p.Grid = Color.FromArgb(72, 52, 99);
                p.Text = Color.FromArgb(238, 226, 255);
                p.SecondaryText = Color.FromArgb(176, 143, 219);
                p.Border = Color.FromArgb(105, 75, 143);
            }
            else if (String.Equals(key, "Aurora", StringComparison.OrdinalIgnoreCase))
            {
                p.Background = Color.FromArgb(12, 29, 31);
                p.Grid = Color.FromArgb(37, 82, 76);
                p.Text = Color.FromArgb(222, 255, 247);
                p.SecondaryText = Color.FromArgb(112, 215, 184);
                p.Border = Color.FromArgb(64, 125, 110);
            }
            else
            {
                p.Background = Color.FromArgb(24, 24, 24);
                p.Grid = Color.FromArgb(58, 58, 58);
                p.Text = Color.FromArgb(236, 236, 236);
                p.SecondaryText = Color.FromArgb(165, 165, 165);
                p.Border = Color.FromArgb(85, 85, 85);
            }

            return p;
        }
    }

    internal sealed class TrafficGraphControl : Control
    {
        private readonly TrafficMonitor monitor;
        private readonly TrafficScaleState scaleState = new TrafficScaleState();
        private AppSettings settings;

        internal TrafficGraphControl(TrafficMonitor trafficMonitor, AppSettings appSettings)
        {
            monitor = trafficMonitor;
            settings = appSettings.Clone();

            Size = new Size(430, 210);
            MinimumSize = new Size(430, 210);
            MaximumSize = new Size(430, 210);
            Margin = new Padding(0);
            StatisticsThemePalette palette =
                StatisticsThemePalette.FromName(settings.StatisticsTheme);
            BackColor = palette.Background;
            ForeColor = palette.Text;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint,
                true);
        }

        internal void UpdateSettings(AppSettings appSettings)
        {
            settings = appSettings.Clone();
            scaleState.Reset();
            StatisticsThemePalette palette =
                StatisticsThemePalette.FromName(settings.StatisticsTheme);
            BackColor = palette.Background;
            ForeColor = palette.Text;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode =
                System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Rectangle bounds = ClientRectangle;
            StatisticsThemePalette palette =
                StatisticsThemePalette.FromName(settings.StatisticsTheme);
            e.Graphics.Clear(palette.Background);

            using (Font titleFont = new Font(Font, FontStyle.Bold))
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    Localization.F(
                        "TrafficGraphTitle",
                        FormatHistoryDuration(settings.TrafficHistorySeconds)),
                    titleFont,
                    new Rectangle(10, 7, bounds.Width - 20, 22),
                    ForeColor,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis);
            }

            List<TrafficSample> snapshot = monitor.GetSnapshot();
            TrafficSample current =
                snapshot.Count > 0
                    ? snapshot[snapshot.Count - 1]
                    : null;

            Color downloadColor = Color.FromArgb(settings.DownloadColorArgb);
            Color uploadColor = Color.FromArgb(settings.UploadColorArgb);
            Color offlineColor = Color.FromArgb(settings.OfflineActivityColorArgb);

            DrawLegendValue(
                e.Graphics,
                10,
                32,
                downloadColor,
                Localization.T("TrafficDownload"),
                current == null ? 0.0 : current.DownloadMbps);

            DrawLegendValue(
                e.Graphics,
                150,
                32,
                uploadColor,
                Localization.T("TrafficUpload"),
                current == null ? 0.0 : current.UploadMbps);

            DrawLegendValue(
                e.Graphics,
                276,
                32,
                offlineColor,
                Localization.T("TrafficOfflineActivity"),
                current == null || !current.ManagedDisabled ? 0.0 : current.OfflineActivityMbps);

            Rectangle plot =
                new Rectangle(44, 58, bounds.Width - 56, bounds.Height - 83);

            using (Pen borderPen = new Pen(palette.Border))
            using (Pen gridPen = new Pen(palette.Grid))
            {
                e.Graphics.DrawRectangle(borderPen, plot);

                for (int i = 1; i < 4; i++)
                {
                    int y = plot.Top + (plot.Height * i / 4);
                    e.Graphics.DrawLine(
                        gridPen,
                        plot.Left,
                        y,
                        plot.Right,
                        y);
                }
            }

            if (snapshot.Count < 2)
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    Localization.T("TrafficCollecting"),
                    Font,
                    plot,
                    palette.SecondaryText,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis);
            }
            else
            {
                double graphMax =
                    scaleState.GetMaximum(
                        snapshot,
                        settings.TrafficScaleRefreshSeconds);

                DrawCurve(
                    e.Graphics,
                    plot,
                    snapshot,
                    graphMax,
                    downloadColor,
                    0,
                    settings.TrafficHistorySeconds);

                DrawCurve(
                    e.Graphics,
                    plot,
                    snapshot,
                    graphMax,
                    uploadColor,
                    1,
                    settings.TrafficHistorySeconds);

                DrawCurve(
                    e.Graphics,
                    plot,
                    snapshot,
                    graphMax,
                    offlineColor,
                    2,
                    settings.TrafficHistorySeconds);

                TextRenderer.DrawText(
                    e.Graphics,
                    FormatRate(graphMax),
                    Font,
                    new Rectangle(0, plot.Top - 7, 40, 18),
                    ForeColor,
                    TextFormatFlags.Right |
                    TextFormatFlags.VerticalCenter);

                TextRenderer.DrawText(
                    e.Graphics,
                    "0",
                    Font,
                    new Rectangle(0, plot.Bottom - 9, 40, 18),
                    ForeColor,
                    TextFormatFlags.Right |
                    TextFormatFlags.VerticalCenter);
            }

            string leftTime =
                "-" + FormatHistoryDuration(settings.TrafficHistorySeconds);

            TextRenderer.DrawText(
                e.Graphics,
                leftTime,
                Font,
                new Rectangle(plot.Left, plot.Bottom + 4, 90, 18),
                palette.SecondaryText,
                TextFormatFlags.Left);

            TextRenderer.DrawText(
                e.Graphics,
                "0 min",
                Font,
                new Rectangle(plot.Right - 80, plot.Bottom + 4, 80, 18),
                palette.SecondaryText,
                TextFormatFlags.Right);
        }

        private void DrawLegendValue(
            Graphics graphics,
            int x,
            int y,
            Color color,
            string label,
            double value)
        {
            using (SolidBrush brush = new SolidBrush(color))
            {
                graphics.FillRectangle(brush, x, y + 4, 10, 10);
            }

            string text =
                label + " " +
                value.ToString(value < 10.0 ? "0.00" : "0.0", CultureInfo.CurrentCulture) +
                " Mbit/s";

            TextRenderer.DrawText(
                graphics,
                text,
                Font,
                new Rectangle(x + 15, y, 138, 20),
                ForeColor,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis);
        }

        private static string FormatHistoryDuration(int seconds)
        {
            seconds = Math.Max(10, Math.Min(600, seconds));

            if (seconds < 60)
                return seconds.ToString(CultureInfo.CurrentCulture) + " s";

            if ((seconds % 60) == 0)
                return (seconds / 60).ToString(CultureInfo.CurrentCulture) + " min";

            return (seconds / 60).ToString(CultureInfo.CurrentCulture) +
                   ":" +
                   (seconds % 60).ToString("00", CultureInfo.CurrentCulture) +
                   " min";
        }

        private static void DrawCurve(
            Graphics graphics,
            Rectangle plot,
            List<TrafficSample> samples,
            double graphMax,
            Color color,
            int series,
            int historySeconds)
        {
            if (samples.Count < 2 || graphMax <= 0.0)
                return;

            DateTime end = DateTime.UtcNow;

            // Always keep the X axis fixed to the configured rolling window.
            // During the first seconds/minutes after startup only the right side fills in.
            DateTime windowStart =
                end.AddSeconds(-Math.Max(10, Math.Min(600, historySeconds)));

            List<PointF> points = new List<PointF>();

            using (Pen pen = new Pen(color, 1.8f))
            {
                pen.LineJoin =
                    System.Drawing.Drawing2D.LineJoin.Round;

                foreach (TrafficSample sample in samples)
                {
                    // The third curve is deliberately visible only during periods
                    // in which all OffNet-managed adapters were disabled.
                    if (series == 2 && !sample.ManagedDisabled)
                    {
                        DrawPointSegment(graphics, pen, points);
                        points.Clear();
                        continue;
                    }

                    double ageFraction =
                        (sample.TimestampUtc - windowStart).TotalSeconds /
                        Math.Max(1.0, (end - windowStart).TotalSeconds);

                    if (ageFraction < 0.0 || ageFraction > 1.0)
                    {
                        if (series == 2)
                        {
                            DrawPointSegment(graphics, pen, points);
                            points.Clear();
                        }
                        continue;
                    }

                    double value = 0.0;

                    if (series == 0)
                        value = sample.DownloadMbps;
                    else if (series == 1)
                        value = sample.UploadMbps;
                    else
                        value = sample.OfflineActivityMbps;

                    value = Math.Max(0.0, Math.Min(graphMax, value));

                    float x =
                        plot.Left +
                        (float)(ageFraction * plot.Width);

                    float y =
                        plot.Bottom -
                        (float)((value / graphMax) * plot.Height);

                    points.Add(new PointF(x, y));
                }

                DrawPointSegment(graphics, pen, points);
            }
        }

        private static void DrawPointSegment(
            Graphics graphics,
            Pen pen,
            List<PointF> points)
        {
            if (points.Count >= 2)
                graphics.DrawLines(pen, points.ToArray());
        }

        private static double NiceMaximum(double value)
        {
            if (value <= 0.08)
                return 0.1;

            double exponent =
                Math.Pow(10.0, Math.Floor(Math.Log10(value)));

            double normalized = value / exponent;
            double nice;

            if (normalized <= 1.0)
                nice = 1.0;
            else if (normalized <= 2.0)
                nice = 2.0;
            else if (normalized <= 5.0)
                nice = 5.0;
            else
                nice = 10.0;

            return nice * exponent;
        }

        private static string FormatRate(double value)
        {
            if (value < 1.0)
                return value.ToString("0.0", CultureInfo.CurrentCulture);

            if (value < 10.0)
                return value.ToString("0.0", CultureInfo.CurrentCulture);

            return value.ToString("0", CultureInfo.CurrentCulture);
        }
    }


    internal static class TaskbarThemeHelper
    {
        internal static Color GetTextColor()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        object value = key.GetValue("SystemUsesLightTheme");
                        if (value != null && Convert.ToInt32(value, CultureInfo.InvariantCulture) != 0)
                            return Color.FromArgb(28, 28, 28);
                    }
                }
            }
            catch
            {
            }

            return Color.WhiteSmoke;
        }

        internal static Color GetShadowColor(Color foreground)
        {
            int luminance =
                (foreground.R * 299 +
                 foreground.G * 587 +
                 foreground.B * 114) / 1000;

            return luminance > 128
                ? Color.FromArgb(150, 0, 0, 0)
                : Color.FromArgb(150, 255, 255, 255);
        }
    }

    internal sealed class TaskbarTrafficForm : Form
    {
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        private readonly TrafficMonitor monitor;
        private readonly TrafficScaleState scaleState = new TrafficScaleState();
        private readonly System.Windows.Forms.Timer positionTimer;
        private AppSettings settings;
        private TrayState currentState = TrayState.Yellow;

        // An unusual chroma-key color keeps the entire background transparent,
        // so the real Windows taskbar (solid, accent, acrylic, light or dark)
        // remains visible underneath the meter.
        private readonly Color transparencyColor =
            Color.FromArgb(1, 2, 3);

        internal TaskbarTrafficForm(
            TrafficMonitor trafficMonitor,
            AppSettings appSettings)
        {
            monitor = trafficMonitor;
            settings = appSettings.Clone();

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            BackColor = transparencyColor;
            TransparencyKey = transparencyColor;
            Size = new Size(300, 42);
            DoubleBuffered = true;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint,
                true);

            positionTimer = new System.Windows.Forms.Timer();
            positionTimer.Interval = 500;
            positionTimer.Tick += delegate
            {
                if (Visible)
                {
                    RepositionToTaskbar();
                    KeepAboveTaskbar();
                }
            };
            positionTimer.Start();
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                parameters.ExStyle |=
                    WS_EX_TOOLWINDOW |
                    WS_EX_NOACTIVATE |
                    WS_EX_TRANSPARENT;
                return parameters;
            }
        }

        internal void UpdateSettings(AppSettings appSettings)
        {
            settings = appSettings.Clone();
            scaleState.Reset();
            Invalidate();
        }

        internal void SetTrayState(TrayState state)
        {
            currentState = state;
            Invalidate();
        }

        internal void SetMeterVisible(bool visible)
        {
            if (!visible)
            {
                if (positionTimer.Enabled)
                    positionTimer.Stop();

                Hide();
                return;
            }

            if (!positionTimer.Enabled)
                positionTimer.Start();

            RepositionToTaskbar();

            if (!Visible)
                Show();

            RepositionToTaskbar();
            KeepAboveTaskbar();
            Invalidate();
        }

        internal void RefreshTraffic()
        {
            if (Visible)
            {
                KeepAboveTaskbar();
                Invalidate();
            }
        }

        private void KeepAboveTaskbar()
        {
            try
            {
                if (!Visible || !IsHandleCreated)
                    return;

                NativeMethods.SetWindowPos(
                    Handle,
                    NativeMethods.HWND_TOPMOST,
                    0,
                    0,
                    0,
                    0,
                    NativeMethods.SWP_NOMOVE |
                    NativeMethods.SWP_NOSIZE |
                    NativeMethods.SWP_NOACTIVATE |
                    NativeMethods.SWP_SHOWWINDOW |
                    NativeMethods.SWP_NOOWNERZORDER);
            }
            catch
            {
                // Visual safeguard only. Z-order failures must never affect
                // network-device control or traffic sampling.
            }
        }

        private void RepositionToTaskbar()
        {
            try
            {
                IntPtr taskbar =
                    NativeMethods.FindWindow("Shell_TrayWnd", null);

                if (taskbar == IntPtr.Zero)
                    return;

                NativeMethods.RECT taskbarRect;

                if (!NativeMethods.GetWindowRect(
                    taskbar,
                    out taskbarRect))
                {
                    return;
                }

                int taskbarWidth = taskbarRect.Width;
                int taskbarHeight = taskbarRect.Height;

                // Windows 11 only supports a horizontal taskbar. Windows 10 can
                // still be vertical; in that uncommon case we keep a compact
                // panel near the taskbar instead of stretching it vertically.
                bool horizontal =
                    taskbarWidth >= taskbarHeight;

                if (horizontal)
                {
                    int desiredHeight =
                        Math.Max(34, Math.Min(56, taskbarHeight - 2));

                    int desiredWidth =
                        Math.Max(330, Math.Min(420, taskbarWidth / 4));

                    int x =
                        taskbarRect.Right -
                        desiredWidth -
                        190;

                    IntPtr trayNotify =
                        NativeMethods.FindWindowEx(
                            taskbar,
                            IntPtr.Zero,
                            "TrayNotifyWnd",
                            null);

                    if (trayNotify == IntPtr.Zero)
                    {
                        IntPtr rebar =
                            NativeMethods.FindWindowEx(
                                taskbar,
                                IntPtr.Zero,
                                "ReBarWindow32",
                                null);

                        if (rebar != IntPtr.Zero)
                        {
                            trayNotify =
                                NativeMethods.FindWindowEx(
                                    rebar,
                                    IntPtr.Zero,
                                    "TrayNotifyWnd",
                                    null);
                        }
                    }

                    NativeMethods.RECT trayRect;

                    if (trayNotify != IntPtr.Zero &&
                        NativeMethods.GetWindowRect(
                            trayNotify,
                            out trayRect) &&
                        trayRect.Left >
                            taskbarRect.Left +
                            desiredWidth +
                            20)
                    {
                        x = trayRect.Left - desiredWidth - 6;
                    }

                    int minimumX =
                        taskbarRect.Left + 8;
                    int maximumX =
                        taskbarRect.Right -
                        desiredWidth -
                        8;

                    x = Math.Max(
                        minimumX,
                        Math.Min(maximumX, x));

                    int y =
                        taskbarRect.Top +
                        Math.Max(
                            0,
                            (taskbarHeight - desiredHeight) / 2);

                    Bounds = new Rectangle(
                        x,
                        y,
                        desiredWidth,
                        desiredHeight);
                }
                else
                {
                    // Fallback for a Windows 10 vertical taskbar. The meter stays
                    // adjacent to the taskbar so the graph remains readable.
                    int desiredWidth = 270;
                    int desiredHeight = 48;
                    Rectangle primary =
                        Screen.PrimaryScreen.Bounds;

                    bool taskbarOnLeft =
                        taskbarRect.Left <= primary.Left + 2;

                    int x = taskbarOnLeft
                        ? taskbarRect.Right + 2
                        : taskbarRect.Left - desiredWidth - 2;

                    int y =
                        Math.Max(
                            taskbarRect.Top + 8,
                            taskbarRect.Bottom -
                            desiredHeight -
                            80);

                    Bounds = new Rectangle(
                        x,
                        y,
                        desiredWidth,
                        desiredHeight);
                }
            }
            catch
            {
                // The meter is optional. Positioning failures must never affect
                // device control or the normal OffNet tray icon.
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(transparencyColor);
            e.Graphics.SmoothingMode =
                System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            List<TrafficSample> snapshot =
                monitor.GetSnapshot();

            TrafficSample current =
                snapshot.Count > 0
                    ? snapshot[snapshot.Count - 1]
                    : null;

            double download =
                current == null ? 0.0 : current.DownloadMbps;

            double upload =
                current == null ? 0.0 : current.UploadMbps;

            double offline =
                current == null || !current.ManagedDisabled
                    ? 0.0
                    : current.OfflineActivityMbps;

            Color downloadColor =
                Color.FromArgb(settings.DownloadColorArgb);
            Color uploadColor =
                Color.FromArgb(settings.UploadColorArgb);
            Color offlineColor =
                Color.FromArgb(settings.OfflineActivityColorArgb);

            int rightEdge = Width - 4;
            int valueRowHeight =
                Math.Max(17, Math.Min(21, Height / 2));

            int graphTop =
                Math.Min(Height - 13, valueRowHeight);

            Rectangle graph =
                new Rectangle(
                    3,
                    graphTop,
                    Math.Max(60, rightEdge - 4),
                    Math.Max(12, Height - graphTop - 3));

            float valueFontSize =
                Height <= 40 ? 8.0f : 8.7f;

            using (Font valueFont =
                CreateTaskbarFont(valueFontSize))
            using (Font scaleFont =
                CreateTaskbarFont(
                    Math.Max(7.0f, valueFontSize - 1.1f)))
            {
                int valuesWidth =
                    Math.Max(150, rightEdge - 2);

                int cellWidth =
                    Math.Max(68, valuesWidth / 3);

                DrawValueCell(
                    e.Graphics,
                    valueFont,
                    new Rectangle(
                        1,
                        0,
                        cellWidth,
                        valueRowHeight),
                    "▼",
                    download,
                    downloadColor,
                    settings.TaskbarTextShadow);

                DrawValueCell(
                    e.Graphics,
                    valueFont,
                    new Rectangle(
                        1 + cellWidth,
                        0,
                        cellWidth,
                        valueRowHeight),
                    "▲",
                    upload,
                    uploadColor,
                    settings.TaskbarTextShadow);

                DrawValueCell(
                    e.Graphics,
                    valueFont,
                    new Rectangle(
                        1 + cellWidth * 2,
                        0,
                        Math.Max(
                            62,
                            valuesWidth - cellWidth * 2),
                        valueRowHeight),
                    "◆",
                    offline,
                    offlineColor,
                    settings.TaskbarTextShadow);

                double graphMaximum =
                    scaleState.GetMaximum(
                        snapshot,
                        settings.TrafficScaleRefreshSeconds);

                Color taskbarText =
                    TaskbarThemeHelper.GetTextColor();

                Color taskbarShadow =
                    TaskbarThemeHelper.GetShadowColor(
                        taskbarText);

                using (Pen baselinePen =
                    new Pen(
                        Color.FromArgb(
                            90,
                            taskbarText),
                        1.0f))
                {
                    e.Graphics.DrawLine(
                        baselinePen,
                        graph.Left,
                        graph.Bottom - 1,
                        graph.Right,
                        graph.Bottom - 1);
                }

                DrawCurve(
                    e.Graphics,
                    graph,
                    snapshot,
                    graphMaximum,
                    downloadColor,
                    0,
                    settings.TrafficHistorySeconds);

                DrawCurve(
                    e.Graphics,
                    graph,
                    snapshot,
                    graphMaximum,
                    uploadColor,
                    1,
                    settings.TrafficHistorySeconds);

                DrawCurve(
                    e.Graphics,
                    graph,
                    snapshot,
                    graphMaximum,
                    offlineColor,
                    2,
                    settings.TrafficHistorySeconds);

                string maximumText =
                    FormatMaximum(graphMaximum) +
                    " Mbit/s";

                Rectangle scaleRect =
                    new Rectangle(
                        graph.Left + 2,
                        graph.Top,
                        Math.Max(40, graph.Width - 4),
                        Math.Max(12, graph.Height));

                DrawShadowedText(
                    e.Graphics,
                    maximumText,
                    scaleFont,
                    scaleRect,
                    taskbarText,
                    taskbarShadow,
                    settings.TaskbarTextShadow,
                    TextFormatFlags.Right |
                    TextFormatFlags.Top |
                    TextFormatFlags.SingleLine |
                    TextFormatFlags.EndEllipsis);
            }

        }

        private Font CreateTaskbarFont(float size)
        {
            string name =
                settings == null ||
                String.IsNullOrWhiteSpace(settings.TaskbarMeterFontName)
                    ? "Segoe UI Semibold"
                    : settings.TaskbarMeterFontName;

            try
            {
                return new Font(
                    name,
                    size,
                    FontStyle.Regular,
                    GraphicsUnit.Point);
            }
            catch
            {
                try
                {
                    return new Font(
                        "Segoe UI Semibold",
                        size,
                        FontStyle.Regular,
                        GraphicsUnit.Point);
                }
                catch
                {
                    return new Font(
                        "Segoe UI",
                        size,
                        FontStyle.Regular,
                        GraphicsUnit.Point);
                }
            }
        }

        private static void DrawValueCell(
            Graphics graphics,
            Font font,
            Rectangle bounds,
            string symbol,
            double value,
            Color color,
            bool drawShadow)
        {
            string text =
                symbol +
                " " +
                FormatCurrentRate(value);

            Color shadow =
                TaskbarThemeHelper.GetShadowColor(color);

            DrawShadowedText(
                graphics,
                text,
                font,
                bounds,
                color,
                shadow,
                drawShadow,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine |
                TextFormatFlags.EndEllipsis);
        }

        private static void DrawShadowedText(
            Graphics graphics,
            string text,
            Font font,
            Rectangle bounds,
            Color foreground,
            Color shadow,
            bool drawShadow,
            TextFormatFlags flags)
        {
            if (drawShadow)
            {
                Rectangle shadowBounds =
                    new Rectangle(
                        bounds.X + 1,
                        bounds.Y + 1,
                        bounds.Width,
                        bounds.Height);

                TextRenderer.DrawText(
                    graphics,
                    text,
                    font,
                    shadowBounds,
                    shadow,
                    flags);
            }

            TextRenderer.DrawText(
                graphics,
                text,
                font,
                bounds,
                foreground,
                flags);
        }

        private static void DrawCurve(
            Graphics graphics,
            Rectangle plot,
            List<TrafficSample> samples,
            double graphMaximum,
            Color color,
            int series,
            int historySeconds)
        {
            if (samples.Count < 2 ||
                graphMaximum <= 0.0)
            {
                return;
            }

            DateTime end =
                DateTime.UtcNow;

            DateTime start =
                end.AddSeconds(
                    -Math.Max(
                        10,
                        Math.Min(
                            600,
                            historySeconds)));

            List<PointF> segment =
                new List<PointF>();

            using (Pen pen =
                new Pen(color, 1.5f))
            {
                pen.LineJoin =
                    System.Drawing.Drawing2D.LineJoin.Round;

                foreach (TrafficSample sample in samples)
                {
                    if (series == 2 &&
                        !sample.ManagedDisabled)
                    {
                        DrawSegment(
                            graphics,
                            pen,
                            segment);

                        segment.Clear();
                        continue;
                    }

                    double fraction =
                        (sample.TimestampUtc - start).TotalSeconds /
                        Math.Max(
                            1.0,
                            (end - start).TotalSeconds);

                    if (fraction < 0.0 ||
                        fraction > 1.0)
                    {
                        if (series == 2)
                        {
                            DrawSegment(
                                graphics,
                                pen,
                                segment);

                            segment.Clear();
                        }

                        continue;
                    }

                    double value;

                    if (series == 0)
                        value = sample.DownloadMbps;
                    else if (series == 1)
                        value = sample.UploadMbps;
                    else
                        value = sample.OfflineActivityMbps;

                    value =
                        Math.Max(
                            0.0,
                            Math.Min(
                                graphMaximum,
                                value));

                    float x =
                        plot.Left +
                        (float)(
                            fraction *
                            Math.Max(
                                1,
                                plot.Width - 1));

                    float y =
                        plot.Bottom -
                        1 -
                        (float)(
                            (value /
                             graphMaximum) *
                            Math.Max(
                                1,
                                plot.Height - 2));

                    segment.Add(
                        new PointF(
                            x,
                            y));
                }

                DrawSegment(
                    graphics,
                    pen,
                    segment);
            }
        }

        private static void DrawSegment(
            Graphics graphics,
            Pen pen,
            List<PointF> segment)
        {
            if (segment.Count >= 2)
            {
                graphics.DrawLines(
                    pen,
                    segment.ToArray());
            }
        }

        private static double NiceMaximum(double value)
        {
            if (value <= 0.08)
                return 0.1;

            double exponent =
                Math.Pow(
                    10.0,
                    Math.Floor(
                        Math.Log10(value)));

            double normalized =
                value / exponent;

            double nice;

            if (normalized <= 1.0)
                nice = 1.0;
            else if (normalized <= 2.0)
                nice = 2.0;
            else if (normalized <= 5.0)
                nice = 5.0;
            else
                nice = 10.0;

            return nice * exponent;
        }

        private static string FormatCurrentRate(double value)
        {
            return value.ToString(
                "0.00",
                CultureInfo.CurrentCulture) +
                " Mbit";
        }

        private static string FormatMaximum(double value)
        {
            if (value < 10.0)
            {
                return value.ToString(
                    "0.0",
                    CultureInfo.CurrentCulture);
            }

            return value.ToString(
                "0",
                CultureInfo.CurrentCulture);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing &&
                positionTimer != null)
            {
                positionTimer.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    internal sealed class OffNetApplicationContext : ApplicationContext, IDisposable
    {
        private readonly NotifyIcon trayIcon;
        private readonly ContextMenuStrip trayMenu;
        private readonly ToolStripMenuItem activateItem;
        private readonly ToolStripMenuItem disableItem;
        private readonly ToolStripMenuItem reconnectItem;
        private readonly ToolStripMenuItem openItem;
        private readonly ToolStripMenuItem optionsItem;
        private readonly ToolStripMenuItem exitItem;
        private readonly MainWindow mainWindow;
        private readonly System.Windows.Forms.Timer statusTimer;
        private readonly System.Windows.Forms.Timer blinkTimer;
        private readonly System.Windows.Forms.Timer trafficTimer;
        private readonly System.Windows.Forms.Timer fullscreenTimer;
        private readonly TrafficMonitor trafficMonitor;
        private readonly TrafficGraphControl trafficGraph;
        private readonly TaskbarTrafficForm taskbarTrafficForm;
        private readonly ToolStripControlHost trafficHost;
        private readonly ToolStripSeparator trafficSeparator;

        private AppSettings settings;
        private Icon activeIcon;
        private Icon disabledIcon;
        private Icon offlineIconA;
        private Icon offlineIconB;
        private TrayState currentState;
        private bool blinkPhase;
        private bool fullscreenActive;
        private bool exiting;

        internal OffNetApplicationContext(AppSettings initialSettings)
        {
            settings = initialSettings.Clone();
            Localization.SetLanguage(settings.Language);
            CreateIcons();

            trafficMonitor = new TrafficMonitor();
            trafficMonitor.SetHistorySeconds(settings.TrafficHistorySeconds);
            trafficMonitor.SetSamplePoints(settings.TrafficSamplePoints);
            trafficGraph = new TrafficGraphControl(trafficMonitor, settings);
            taskbarTrafficForm = new TaskbarTrafficForm(trafficMonitor, settings);

            trayMenu = new ContextMenuStrip();
            // Explicit AutoClose is important for a tray menu opened manually by
            // either mouse button: clicking anywhere outside the menu dismisses it.
            trayMenu.AutoClose = true;
            trayMenu.ShowImageMargin = false;
            trayMenu.ShowCheckMargin = false;

            trafficHost = new ToolStripControlHost(trafficGraph);
            trafficHost.AutoSize = false;
            trafficHost.Size = trafficGraph.Size;
            trafficHost.Margin = new Padding(2);
            trayMenu.Items.Add(trafficHost);

            trafficSeparator = new ToolStripSeparator();
            trayMenu.Items.Add(trafficSeparator);

            activateItem = new ToolStripMenuItem();
            activateItem.Click += delegate { ActivateManagedDevices(); };
            trayMenu.Items.Add(activateItem);

            disableItem = new ToolStripMenuItem();
            disableItem.Click += delegate { DisableManagedDevices(); };
            trayMenu.Items.Add(disableItem);

            reconnectItem = new ToolStripMenuItem();
            reconnectItem.Click += delegate { Reconnect(); };
            trayMenu.Items.Add(reconnectItem);
            trayMenu.Items.Add(new ToolStripSeparator());

            openItem = new ToolStripMenuItem();
            openItem.Click += delegate { ShowMainWindow(); };
            trayMenu.Items.Add(openItem);

            optionsItem = new ToolStripMenuItem();
            optionsItem.Click += delegate { ShowOptions(); };
            trayMenu.Items.Add(optionsItem);

            trayMenu.Items.Add(new ToolStripSeparator());

            exitItem = new ToolStripMenuItem();
            exitItem.Click += delegate { ExitOffNet(); };
            trayMenu.Items.Add(exitItem);

            bool firstRun = !ConfigurationStore.HasManagedDeviceConfiguration;
            mainWindow = new MainWindow();
            mainWindow.ManagedDevicesChanged += delegate
            {
                trafficMonitor.ResetCounters();
                UpdateTrayState();
            };
            mainWindow.ReconnectRequested += delegate { Reconnect(); };
            mainWindow.FormClosing += delegate(object sender, FormClosingEventArgs e)
            {
                if (!exiting)
                {
                    e.Cancel = true;
                    mainWindow.Hide();
                    mainWindow.ShowInTaskbar = false;
                }
            };
            IntPtr mainWindowHandle = mainWindow.Handle;
            mainWindow.ApplyTheme(settings.ApplicationTheme);

            trayIcon = new NotifyIcon();
            trayIcon.Visible = true;
            trayIcon.Icon = offlineIconA;
            trayIcon.Text = "OffNet";

            // Handle both buttons ourselves. This avoids two different menu paths
            // (NotifyIcon.ContextMenuStrip vs. manual Show) and gives left/right
            // click identical close-on-outside-click behaviour.
            trayIcon.MouseUp += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left ||
                    e.Button == MouseButtons.Right)
                {
                    ShowTrayMenu();
                }
            };

            trayIcon.DoubleClick += delegate { ShowMainWindow(); };

            fullscreenActive =
                FullscreenDetector.IsForegroundFullscreen(mainWindow.Handle);

            UpdateTrafficOverviewVisibility();
            UpdateTaskbarTrafficVisibility();

            ApplyLocalization();
            mainWindow.RefreshDevices();
            UpdateTrayState();

            if (firstRun)
            {
                mainWindow.Show();
                mainWindow.Activate();
                ShowBalloon("OffNet", Localization.T("FirstRun"), ToolTipIcon.Info);
            }
            else
            {
                mainWindow.Hide();
                mainWindow.ShowInTaskbar = false;
            }

            statusTimer = new System.Windows.Forms.Timer();
            statusTimer.Interval = 4000;
            statusTimer.Tick += delegate { UpdateTrayState(); };
            statusTimer.Start();

            blinkTimer = new System.Windows.Forms.Timer();
            blinkTimer.Interval = 500;
            blinkTimer.Tick += delegate { UpdateBlink(); };
            blinkTimer.Start();

            trafficTimer = new System.Windows.Forms.Timer();
            trafficTimer.Interval = CalculateTrafficSampleIntervalMilliseconds(settings);
            trafficTimer.Tick += delegate { SampleTraffic(); };

            fullscreenTimer = new System.Windows.Forms.Timer();
            fullscreenTimer.Interval = 1000;
            fullscreenTimer.Tick += delegate { UpdateFullscreenState(); };
            fullscreenTimer.Start();

            UpdateTrafficMonitoringState();

            if (!fullscreenActive &&
                (settings.ShowTrafficOverview ||
                 settings.ShowTaskbarTrafficMeter))
            {
                SampleTraffic();
            }
        }

        private HashSet<string> ManagedIds { get { return ConfigurationStore.LoadManagedIds(); } }

        private void ApplyLocalization()
        {
            activateItem.Text = Localization.T("TrayActivate");
            disableItem.Text = Localization.T("TrayDisable");
            reconnectItem.Text = Localization.T("TrayReconnect");
            openItem.Text = Localization.T("TrayOpen");
            optionsItem.Text = Localization.T("TrayOptions");
            exitItem.Text = Localization.T("TrayExit");
            mainWindow.ApplyLocalization();
            mainWindow.ApplyTheme(settings.ApplicationTheme);
            AppThemeManager.ApplyToolStrip(
                trayMenu,
                settings.ApplicationTheme);
            trafficGraph.UpdateSettings(settings);
            taskbarTrafficForm.UpdateSettings(settings);
            UpdateTrayState();
        }

        private void ShowMainWindow()
        {
            mainWindow.ApplyLocalization();
            mainWindow.ApplyTheme(settings.ApplicationTheme);
            mainWindow.RefreshDevices();
            if (!mainWindow.Visible) mainWindow.Show();
            mainWindow.ShowInTaskbar = true;
            mainWindow.WindowState = FormWindowState.Normal;
            mainWindow.Activate();
            mainWindow.BringToFront();
        }

        private void ShowOptions()
        {
            string oldLanguage = Localization.CurrentLanguage;
            bool startupEnabled = StartupManager.IsEnabled();

            using (OptionsForm form = new OptionsForm(settings, startupEnabled))
            {
                DialogResult result = form.ShowDialog(mainWindow.Visible ? mainWindow : null);
                if (result == DialogResult.OK)
                {
                    bool trafficWasEnabled =
                        settings.ShowTrafficOverview ||
                        settings.ShowTaskbarTrafficMeter;
                    bool startupRequested = form.ResultStartWithWindows;
                    bool startupChanged = startupRequested != startupEnabled;

                    // Normal OffNet settings are committed first and are completely
                    // independent from Windows Task Scheduler. A startup-task failure
                    // must never block language, colors, graph settings, etc.
                    settings = form.ResultSettings.Clone();
                    Localization.SetLanguage(settings.Language);
                    ConfigurationStore.SaveSettings(settings);

                    ApplyTrafficSamplingSettings();
                    trafficGraph.UpdateSettings(settings);
                    taskbarTrafficForm.UpdateSettings(settings);

                    bool trafficIsEnabled =
                        settings.ShowTrafficOverview ||
                        settings.ShowTaskbarTrafficMeter;

                    if (!trafficWasEnabled && trafficIsEnabled)
                        trafficMonitor.ResetCounters();

                    UpdateTrafficOverviewVisibility();
                    UpdateTaskbarTrafficVisibility();
                    UpdateTrafficMonitoringState();
                    RecreateIcons();
                    ApplyLocalization();
                    mainWindow.RefreshDevices();

                    // Touch Task Scheduler only when the user actually changed the
                    // autostart checkbox. Merely changing language or colors must not
                    // re-register the startup task.
                    if (startupChanged)
                    {
                        try
                        {
                            StartupManager.SetEnabled(startupRequested);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                Localization.F("StartupChangeFailed", ex.Message),
                                Program.AppName,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    Localization.SetLanguage(oldLanguage);
                    ApplyLocalization();
                }
            }
        }

        private void UpdateTrafficOverviewVisibility()
        {
            bool visible =
                settings != null &&
                settings.ShowTrafficOverview &&
                !fullscreenActive;

            trafficHost.Visible = visible;
            trafficSeparator.Visible = visible;

            if (visible)
            {
                trafficMonitor.SetHistorySeconds(settings.TrafficHistorySeconds);
                trafficMonitor.SetSamplePoints(settings.TrafficSamplePoints);
                trafficGraph.UpdateSettings(settings);
            }
        }

        private void UpdateTaskbarTrafficVisibility()
        {
            bool visible =
                settings != null &&
                settings.ShowTaskbarTrafficMeter &&
                !fullscreenActive;

            taskbarTrafficForm.UpdateSettings(settings);
            taskbarTrafficForm.SetTrayState(currentState);
            taskbarTrafficForm.SetMeterVisible(visible);

            if (visible)
            {
                trafficMonitor.SetHistorySeconds(
                    settings.TrafficHistorySeconds);
                trafficMonitor.SetSamplePoints(
                    settings.TrafficSamplePoints);
            }
        }

        private void UpdateTrafficMonitoringState()
        {
            bool shouldRun =
                !fullscreenActive &&
                settings != null &&
                (settings.ShowTrafficOverview ||
                 settings.ShowTaskbarTrafficMeter);

            if (shouldRun)
            {
                ApplyTrafficSamplingSettings();

                if (!trafficTimer.Enabled)
                    trafficTimer.Start();
            }
            else
            {
                if (trafficTimer.Enabled)
                    trafficTimer.Stop();
            }
        }

        private void UpdateFullscreenState()
        {
            bool detected =
                FullscreenDetector.IsForegroundFullscreen(
                    mainWindow.Handle);

            if (detected == fullscreenActive)
                return;

            fullscreenActive = detected;

            if (fullscreenActive)
            {
                if (trayMenu.Visible)
                {
                    trayMenu.Close(
                        ToolStripDropDownCloseReason.AppFocusChange);
                }

                if (trafficTimer.Enabled)
                    trafficTimer.Stop();

                taskbarTrafficForm.SetMeterVisible(false);
                trafficHost.Visible = false;
                trafficSeparator.Visible = false;
            }
            else
            {
                // Do not turn a long fullscreen pause into a giant throughput
                // spike. Preserve the graph history but restart byte baselines.
                trafficMonitor.ResetCounters();
                UpdateTrafficOverviewVisibility();
                UpdateTaskbarTrafficVisibility();
                UpdateTrafficMonitoringState();

                if (settings.ShowTrafficOverview ||
                    settings.ShowTaskbarTrafficMeter)
                {
                    SampleTraffic();
                }
            }
        }

        private void ShowTrayMenu()
        {
            if (trayMenu == null || trayIcon == null)
                return;

            UpdateTrayState();
            UpdateTrafficOverviewVisibility();

            if (settings.ShowTrafficOverview)
                trafficGraph.Invalidate();

            // Close an already open instance first. AutoClose then handles every
            // click outside the popup exactly like a normal Windows context menu.
            if (trayMenu.Visible)
                trayMenu.Close(ToolStripDropDownCloseReason.AppClicked);

            try
            {
                NativeMethods.SetForegroundWindow(mainWindow.Handle);
            }
            catch
            {
            }

            trayMenu.Show(Cursor.Position);
        }

        private static int CalculateTrafficSampleIntervalMilliseconds(
            AppSettings appSettings)
        {
            int seconds =
                Math.Max(
                    10,
                    Math.Min(
                        600,
                        appSettings == null
                            ? 60
                            : appSettings.TrafficHistorySeconds));

            int points =
                Math.Max(
                    10,
                    Math.Min(
                        300,
                        appSettings == null
                            ? 75
                            : appSettings.TrafficSamplePoints));

            // Spread the requested number of measurements across the complete
            // rolling history window.
            double milliseconds =
                (seconds * 1000.0) /
                points;

            return Math.Max(
                30,
                Math.Min(
                    60000,
                    (int)Math.Round(milliseconds)));
        }

        private void ApplyTrafficSamplingSettings()
        {
            if (settings == null)
                return;

            trafficMonitor.SetHistorySeconds(
                settings.TrafficHistorySeconds);

            trafficMonitor.SetSamplePoints(
                settings.TrafficSamplePoints);

            if (trafficTimer != null)
            {
                trafficTimer.Interval =
                    CalculateTrafficSampleIntervalMilliseconds(
                        settings);
            }
        }

        private void SampleTraffic()
        {
            if (fullscreenActive)
                return;

            if (settings == null ||
                (!settings.ShowTrafficOverview &&
                 !settings.ShowTaskbarTrafficMeter))
            {
                return;
            }

            HashSet<string> ids = ManagedIds;

            // Red with at least one configured managed device means OffNet expects
            // its managed network path to be disabled. During these periods the
            // third curve records traffic on any OTHER active adapter.
            bool managedDisabled =
                ids.Count > 0 &&
                currentState == TrayState.Red;

            trafficMonitor.Sample(ids, managedDisabled);

            if (trayMenu.Visible && trafficHost.Visible)
                trafficGraph.Invalidate();

            if (settings.ShowTaskbarTrafficMeter)
                taskbarTrafficForm.RefreshTraffic();
        }

        private void CreateIcons()
        {
            Color active = Color.FromArgb(settings.ActiveColorArgb);
            Color disabled = Color.FromArgb(settings.DisabledColorArgb);
            Color offline = Color.FromArgb(settings.OfflineColorArgb);
            activeIcon = IconFactory.CreateCircle(active);
            disabledIcon = IconFactory.CreateCircle(disabled);
            offlineIconA = IconFactory.CreateCircle(offline);
            offlineIconB = IconFactory.CreateCircle(IconFactory.Lighter(offline));
        }

        private void RecreateIcons()
        {
            Icon oldActive = activeIcon;
            Icon oldDisabled = disabledIcon;
            Icon oldOfflineA = offlineIconA;
            Icon oldOfflineB = offlineIconB;
            CreateIcons();
            UpdateTrayState();
            if (oldActive != null) oldActive.Dispose();
            if (oldDisabled != null) oldDisabled.Dispose();
            if (oldOfflineA != null) oldOfflineA.Dispose();
            if (oldOfflineB != null) oldOfflineB.Dispose();
        }

        private void UpdateTrayState()
        {
            if (trayIcon == null) return;
            try
            {
                ManagedState state = DeviceManager.GetManagedState(ManagedIds);
                currentState = state.State;
                taskbarTrafficForm.SetTrayState(currentState);
                activateItem.Enabled = state.DisabledCount > 0 || state.MissingCount > 0;
                disableItem.Enabled = state.EnabledCount > 0;
                reconnectItem.Enabled = state.EnabledCount > 0;

                if (state.State == TrayState.Green)
                {
                    trayIcon.Icon = activeIcon;
                    trayIcon.Text = TrimTrayText(Localization.T("TrayStatusActive"));
                }
                else if (state.State == TrayState.Red)
                {
                    trayIcon.Icon = disabledIcon;
                    trayIcon.Text = TrimTrayText(Localization.T(state.ConfiguredCount == 0 ? "TrayStatusNoSelection" : "TrayStatusDisabled"));
                }
                else
                {
                    trayIcon.Icon = blinkPhase ? offlineIconA : offlineIconB;
                    trayIcon.Text = TrimTrayText(Localization.T("TrayStatusOffline"));
                }
            }
            catch
            {
                currentState = TrayState.Yellow;
                taskbarTrafficForm.SetTrayState(currentState);
                trayIcon.Icon = offlineIconA;
                trayIcon.Text = TrimTrayText(Localization.T("TrayStatusUnknown"));
            }
        }

        private void UpdateBlink()
        {
            if (currentState != TrayState.Yellow) return;
            blinkPhase = !blinkPhase;
            trayIcon.Icon = blinkPhase ? offlineIconA : offlineIconB;
        }

        private static string TrimTrayText(string text)
        {
            if (text.Length <= 63) return text;
            return text.Substring(0, 63);
        }

        private void ActivateManagedDevices()
        {
            HashSet<string> ids = ManagedIds;
            if (ids.Count == 0)
            {
                ShowBalloon("OffNet", Localization.T("NoManaged"), ToolTipIcon.Info);
                return;
            }

            List<string> errors = new List<string>();
            foreach (string id in ids)
            {
                try { DeviceManager.SetDeviceEnabled(id, true); }
                catch (Exception ex) { errors.Add(ex.Message); }
            }
            Thread.Sleep(250);
            trafficMonitor.ResetCounters();
            UpdateTrayState();
            mainWindow.SetStatusText(Localization.T("StatusActivate"));
            if (errors.Count == 0) ShowBalloon("OffNet — " + Localization.T("TrayActivate"), Localization.T("ActivateDone"), ToolTipIcon.Info);
            else ShowErrorSummary(errors);
        }

        private void DisableManagedDevices()
        {
            HashSet<string> ids = ManagedIds;
            if (ids.Count == 0)
            {
                ShowBalloon("OffNet", Localization.T("NoManaged"), ToolTipIcon.Info);
                return;
            }

            DialogResult answer = MessageBox.Show(Localization.T("DisableManagedConfirm"), Localization.T("DisableTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) return;

            List<string> errors = new List<string>();
            foreach (string id in ids)
            {
                try
                {
                    DeviceInfo current = DeviceManager.GetDeviceState(id);
                    if (current.IsPresent && !current.IsDisabled) DeviceManager.SetDeviceEnabled(id, false);
                }
                catch (Exception ex) { errors.Add(ex.Message); }
            }
            Thread.Sleep(250);
            trafficMonitor.ResetCounters();
            UpdateTrayState();
            mainWindow.SetStatusText(Localization.T("StatusDisable"));
            if (errors.Count == 0) ShowBalloon("OffNet — " + Localization.T("TrayDisable"), Localization.T("DisableDone"), ToolTipIcon.Info);
            else ShowErrorSummary(errors);
        }

        private void Reconnect()
        {
            mainWindow.SetStatusText(Localization.T("ReconnectStart"));
            ShowBalloon("OffNet — " + Localization.T("TrayReconnect"), Localization.T("ReconnectStarted"), ToolTipIcon.Info);
            ConnectivityManager.ReconnectAsync(delegate(string error)
            {
                if (mainWindow.IsDisposed) return;
                mainWindow.BeginInvoke(new Action(delegate
                {
                    if (String.IsNullOrEmpty(error))
                    {
                        mainWindow.SetStatusText(Localization.T("ReconnectDone"));
                        ShowBalloon("OffNet — " + Localization.T("TrayReconnect"), Localization.T("ReconnectCompleted"), ToolTipIcon.Info);
                    }
                    else
                    {
                        mainWindow.SetStatusText(Localization.T("ReconnectError"));
                        MessageBox.Show(error, Program.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    UpdateTrayState();
                }));
            });
        }

        private void ShowErrorSummary(List<string> errors)
        {
            MessageBox.Show(String.Join("\r\n\r\n", errors.ToArray()), Program.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowBalloon(string title, string text, ToolTipIcon icon)
        {
            trayIcon.BalloonTipTitle = title;
            trayIcon.BalloonTipText = text;
            trayIcon.BalloonTipIcon = icon;
            trayIcon.ShowBalloonTip(3000);
        }

        private void ExitOffNet()
        {
            exiting = true;
            statusTimer.Stop();
            blinkTimer.Stop();
            trafficTimer.Stop();
            fullscreenTimer.Stop();
            trayIcon.Visible = false;
            taskbarTrafficForm.SetMeterVisible(false);
            mainWindow.Close();
            ExitThread();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (statusTimer != null) statusTimer.Dispose();
                    if (blinkTimer != null) blinkTimer.Dispose();
                    if (trafficTimer != null) trafficTimer.Dispose();
                    if (fullscreenTimer != null) fullscreenTimer.Dispose();
                    if (trayIcon != null) trayIcon.Dispose();
                    if (trayMenu != null) trayMenu.Dispose();
                    if (mainWindow != null) mainWindow.Dispose();
                    if (taskbarTrafficForm != null) taskbarTrafficForm.Dispose();
                    if (activeIcon != null) activeIcon.Dispose();
                    if (disabledIcon != null) disabledIcon.Dispose();
                    if (offlineIconA != null) offlineIconA.Dispose();
                    if (offlineIconB != null) offlineIconB.Dispose();
                }
                catch { }
            }
            base.Dispose(disposing);
        }
    }
}
