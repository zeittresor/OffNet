/*
 * OffNet 1.1.0
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
        internal const string Version = "1.1.1";
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

        internal AppSettings Clone()
        {
            AppSettings copy = new AppSettings();
            copy.Language = Language;
            copy.ActiveColorArgb = ActiveColorArgb;
            copy.DisabledColorArgb = DisabledColorArgb;
            copy.OfflineColorArgb = OfflineColorArgb;
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
                {"TrayColors", "Tray circle colors"},
                {"ActiveInternetColor", "Active + Internet"},
                {"DisabledColor", "Device/driver disabled"},
                {"OfflineColor", "Active, no Internet"},
                {"ChooseColor", "Choose..."},
                {"RestoreDefaults", "Restore defaults"},
                {"OpenProject", "Open project page"},
                {"OptionsHint", "The offline color blinks between the selected color and a lighter shade."},
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
                {"TrayColors", "Farben der Tray-Kreise"},
                {"ActiveInternetColor", "Aktiv + Internet"},
                {"DisabledColor", "Gerät/Treiber deaktiviert"},
                {"OfflineColor", "Aktiv, kein Internet"},
                {"ChooseColor", "Auswählen..."},
                {"RestoreDefaults", "Standard wiederherstellen"},
                {"OpenProject", "Projektseite öffnen"},
                {"OptionsHint", "Die Offline-Farbe blinkt zwischen der gewählten Farbe und einem helleren Farbton."},
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
                {"TrayColors", "Couleurs des cercles du tray"},
                {"ActiveInternetColor", "Actif + Internet"},
                {"DisabledColor", "Périphérique/pilote désactivé"},
                {"OfflineColor", "Actif, sans Internet"},
                {"ChooseColor", "Choisir..."},
                {"RestoreDefaults", "Valeurs par défaut"},
                {"OpenProject", "Ouvrir la page du projet"},
                {"OptionsHint", "La couleur hors ligne clignote entre la couleur choisie et une teinte plus claire."},
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
                    if (key == "language") settings.Language = value;
                    else if (key == "active_color" && Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out colorValue)) settings.ActiveColorArgb = colorValue;
                    else if (key == "disabled_color" && Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out colorValue)) settings.DisabledColorArgb = colorValue;
                    else if (key == "offline_color" && Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out colorValue)) settings.OfflineColorArgb = colorValue;
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

    internal sealed class OptionsForm : Form
    {
        private readonly ComboBox languageBox;
        private readonly Label languageLabel;
        private readonly GroupBox colorsGroup;
        private readonly Label activeLabel;
        private readonly Label disabledLabel;
        private readonly Label offlineLabel;
        private readonly Label hintLabel;
        private Button activeButton;
        private Button disabledButton;
        private Button offlineButton;
        private readonly Button defaultsButton;
        private readonly Button projectButton;
        private readonly Button okButton;
        private readonly Button cancelButton;
        private readonly ColorDialog colorDialog;

        private Color activeColor;
        private Color disabledColor;
        private Color offlineColor;
        private bool applyingLanguage;

        internal AppSettings ResultSettings { get; private set; }

        internal OptionsForm(AppSettings current)
        {
            ResultSettings = current.Clone();
            activeColor = Color.FromArgb(current.ActiveColorArgb);
            disabledColor = Color.FromArgb(current.DisabledColorArgb);
            offlineColor = Color.FromArgb(current.OfflineColorArgb);

            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Width = 535;
            Height = 410;
            Font = new Font("Segoe UI", 9.0f);

            languageLabel = new Label();
            languageLabel.AutoSize = true;
            languageLabel.Location = new Point(18, 22);
            Controls.Add(languageLabel);

            languageBox = new ComboBox();
            languageBox.DropDownStyle = ComboBoxStyle.DropDownList;
            languageBox.Location = new Point(165, 18);
            languageBox.Width = 320;
            Controls.Add(languageBox);

            colorsGroup = new GroupBox();
            colorsGroup.Location = new Point(16, 62);
            colorsGroup.Size = new Size(474, 190);
            Controls.Add(colorsGroup);

            activeLabel = AddColorRow(colorsGroup, 28, out activeButton);
            disabledLabel = AddColorRow(colorsGroup, 72, out disabledButton);
            offlineLabel = AddColorRow(colorsGroup, 116, out offlineButton);

            hintLabel = new Label();
            hintLabel.AutoSize = false;
            hintLabel.Location = new Point(16, 153);
            hintLabel.Size = new Size(440, 30);
            colorsGroup.Controls.Add(hintLabel);

            activeButton.Click += delegate { activeColor = PickColor(activeColor); UpdateColorButtons(); };
            disabledButton.Click += delegate { disabledColor = PickColor(disabledColor); UpdateColorButtons(); };
            offlineButton.Click += delegate { offlineColor = PickColor(offlineColor); UpdateColorButtons(); };

            defaultsButton = new Button();
            defaultsButton.Location = new Point(16, 266);
            defaultsButton.Size = new Size(160, 32);
            defaultsButton.Click += delegate
            {
                activeColor = Color.LimeGreen;
                disabledColor = Color.Red;
                offlineColor = Color.Gold;
                UpdateColorButtons();
            };
            Controls.Add(defaultsButton);

            projectButton = new Button();
            projectButton.Location = new Point(184, 266);
            projectButton.Size = new Size(180, 32);
            projectButton.Click += delegate { ShellHelper.OpenProjectPage(); };
            Controls.Add(projectButton);

            okButton = new Button();
            okButton.Location = new Point(300, 320);
            okButton.Size = new Size(90, 32);
            okButton.DialogResult = DialogResult.OK;
            okButton.Click += delegate { CommitSettings(); };
            Controls.Add(okButton);

            cancelButton = new Button();
            cancelButton.Location = new Point(400, 320);
            cancelButton.Size = new Size(90, 32);
            cancelButton.DialogResult = DialogResult.Cancel;
            Controls.Add(cancelButton);

            AcceptButton = okButton;
            CancelButton = cancelButton;
            colorDialog = new ColorDialog();
            colorDialog.FullOpen = true;

            FillLanguages(current.Language);
            languageBox.SelectedIndexChanged += delegate
            {
                if (applyingLanguage) return;
                LanguageChoice choice = languageBox.SelectedItem as LanguageChoice;
                if (choice == null) return;
                Localization.SetLanguage(choice.Code);
                ApplyLocalization();
                FillLanguages(choice.Code);
            };

            ApplyLocalization();
            UpdateColorButtons();
        }

        private Label AddColorRow(Control parent, int y, out Button button)
        {
            Label label = new Label();
            label.AutoSize = false;
            label.Location = new Point(14, y + 6);
            label.Size = new Size(235, 24);
            parent.Controls.Add(label);

            button = new Button();
            button.Location = new Point(270, y);
            button.Size = new Size(180, 30);
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
                    if (item != null && item.Code == selectCode) { languageBox.SelectedIndex = i; break; }
                }
                if (languageBox.SelectedIndex < 0) languageBox.SelectedIndex = 0;
            }
            finally { applyingLanguage = false; }
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
        }

        private static void SetColorButton(Button button, Color color)
        {
            button.BackColor = color;
            int brightness = color.R + color.G + color.B;
            button.ForeColor = brightness < 360 ? Color.White : Color.Black;
            button.Text = String.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }

        private void ApplyLocalization()
        {
            Text = Localization.T("OptionsTitle");
            languageLabel.Text = Localization.T("Language");
            colorsGroup.Text = Localization.T("TrayColors");
            activeLabel.Text = Localization.T("ActiveInternetColor");
            disabledLabel.Text = Localization.T("DisabledColor");
            offlineLabel.Text = Localization.T("OfflineColor");
            hintLabel.Text = Localization.T("OptionsHint");
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
        private bool loading;

        internal event EventHandler ManagedDevicesChanged;
        internal event EventHandler ReconnectRequested;

        internal MainWindow()
        {
            Text = "OffNet " + Program.Version;
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
                    if (!device.IsPresent) row.DefaultCellStyle.ForeColor = Color.Gray;
                    else if (device.IsDisabled) row.DefaultCellStyle.ForeColor = Color.Firebrick;
                    else row.DefaultCellStyle.ForeColor = Color.DarkGreen;
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

        private AppSettings settings;
        private Icon activeIcon;
        private Icon disabledIcon;
        private Icon offlineIconA;
        private Icon offlineIconB;
        private TrayState currentState;
        private bool blinkPhase;
        private bool exiting;

        internal OffNetApplicationContext(AppSettings initialSettings)
        {
            settings = initialSettings.Clone();
            Localization.SetLanguage(settings.Language);
            CreateIcons();

            trayMenu = new ContextMenuStrip();
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

            trayIcon = new NotifyIcon();
            trayIcon.Visible = true;
            trayIcon.Icon = offlineIconA;
            trayIcon.Text = "OffNet";
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.MouseClick += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    UpdateTrayState();
                    trayMenu.Show(Cursor.Position);
                }
            };
            trayIcon.DoubleClick += delegate { ShowMainWindow(); };

            bool firstRun = !ConfigurationStore.HasManagedDeviceConfiguration;
            mainWindow = new MainWindow();
            mainWindow.ManagedDevicesChanged += delegate { UpdateTrayState(); };
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
            UpdateTrayState();
        }

        private void ShowMainWindow()
        {
            mainWindow.ApplyLocalization();
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
            using (OptionsForm form = new OptionsForm(settings))
            {
                DialogResult result = form.ShowDialog(mainWindow.Visible ? mainWindow : null);
                if (result == DialogResult.OK)
                {
                    settings = form.ResultSettings.Clone();
                    Localization.SetLanguage(settings.Language);
                    ConfigurationStore.SaveSettings(settings);
                    RecreateIcons();
                    ApplyLocalization();
                    mainWindow.RefreshDevices();
                }
                else
                {
                    Localization.SetLanguage(oldLanguage);
                    ApplyLocalization();
                }
            }
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
            trayIcon.Visible = false;
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
                    if (trayIcon != null) trayIcon.Dispose();
                    if (trayMenu != null) trayMenu.Dispose();
                    if (mainWindow != null) mainWindow.Dispose();
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
