/*
 * OffNet 1.0.0
 * Lightweight Windows 10/11 network-device tray controller.
 *
 * Project reference:
 * https://github.com/zeittresor
 *
 * SPDX-License-Identifier: MIT
 *
 * Copyright (c) 2026 zeittresor
 *
 * MIT License - see LICENSE included with this source package.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        internal const string Version = "1.0.1";

        [STAThread]
        private static void Main()
        {
            bool createdNew;

            using (Mutex mutex = new Mutex(true, @"Local\OffNet_zeittresor", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show(
                        "OffNet läuft bereits im Windows-Tray.",
                        AppName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                if (!AdminHelper.IsAdministrator())
                {
                    MessageBox.Show(
                        "OffNet benötigt Administratorrechte, weil Netzwerkgeräte direkt " +
                        "auf PnP-/Treiberebene aktiviert und deaktiviert werden.\r\n\r\n" +
                        "Bitte starte OffNet erneut und bestätige die UAC-Abfrage.",
                        AppName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                using (OffNetApplicationContext context = new OffNetApplicationContext())
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

    internal sealed class DeviceInfo
    {
        internal string Name;
        internal string Manufacturer;
        internal string InstanceId;
        internal string Kind;
        internal bool IsPresent;
        internal bool IsDisabled;
        internal uint ProblemCode;

        internal string StateText
        {
            get
            {
                if (!IsPresent)
                    return "Nicht vorhanden";

                if (IsDisabled)
                    return "Deaktiviert";

                if (ProblemCode != 0)
                    return "Aktiv / Problem " + ProblemCode.ToString();

                return "Aktiviert";
            }
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
                if (EnabledCount <= 0)
                    return TrayState.Red;

                if (InternetConnected)
                    return TrayState.Green;

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

        internal static bool HasConfiguration
        {
            get { return File.Exists(ManagedDevicesFile); }
        }

        internal static HashSet<string> LoadManagedIds()
        {
            HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                if (!File.Exists(ManagedDevicesFile))
                    return result;

                string[] lines = File.ReadAllLines(ManagedDevicesFile, Encoding.UTF8);

                foreach (string raw in lines)
                {
                    string line = raw == null ? String.Empty : raw.Trim();

                    if (line.Length == 0)
                        continue;

                    if (line.StartsWith("#", StringComparison.Ordinal))
                        continue;

                    result.Add(line);
                }
            }
            catch
            {
                // A damaged config must never prevent OffNet from starting.
            }

            return result;
        }

        internal static void SaveManagedIds(IEnumerable<string> ids)
        {
            Directory.CreateDirectory(ConfigDirectory);

            List<string> lines = new List<string>();
            lines.Add("# OffNet managed PnP network-device instance IDs");
            lines.Add("# https://github.com/zeittresor");

            HashSet<string> unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string id in ids)
            {
                if (String.IsNullOrWhiteSpace(id))
                    continue;

                if (unique.Add(id.Trim()))
                    lines.Add(id.Trim());
            }

            File.WriteAllLines(ManagedDevicesFile, lines.ToArray(), new UTF8Encoding(false));
        }
    }

    internal static class NativeMethods
    {
        internal static readonly Guid GUID_DEVCLASS_NET =
            new Guid("4D36E972-E325-11CE-BFC1-08002BE10318");

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
        internal static extern IntPtr SetupDiGetClassDevs(
            ref Guid ClassGuid,
            IntPtr Enumerator,
            IntPtr hwndParent,
            uint Flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiEnumDeviceInfo(
            IntPtr DeviceInfoSet,
            uint MemberIndex,
            ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiGetDeviceInstanceId(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            StringBuilder DeviceInstanceId,
            int DeviceInstanceIdSize,
            out int RequiredSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            uint Property,
            out uint PropertyRegDataType,
            byte[] PropertyBuffer,
            uint PropertyBufferSize,
            out uint RequiredSize);

        [DllImport("setupapi.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
        internal static extern uint CM_Locate_DevNode(
            out uint pdnDevInst,
            string pDeviceID,
            uint ulFlags);

        [DllImport("cfgmgr32.dll")]
        internal static extern uint CM_Get_DevNode_Status(
            out uint pulStatus,
            out uint pulProblemNumber,
            uint dnDevInst,
            uint ulFlags);

        [DllImport("cfgmgr32.dll")]
        internal static extern uint CM_Disable_DevNode(
            uint dnDevInst,
            uint ulFlags);

        [DllImport("cfgmgr32.dll")]
        internal static extern uint CM_Enable_DevNode(
            uint dnDevInst,
            uint ulFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("wininet.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool InternetGetConnectedState(
            out int lpdwConnection,
            int dwReserved);
    }

    internal static class DeviceManager
    {
        internal static List<DeviceInfo> EnumerateNetworkDevices(bool includeNonPresent)
        {
            List<DeviceInfo> devices = new List<DeviceInfo>();

            Guid classGuid = NativeMethods.GUID_DEVCLASS_NET;
            uint flags = includeNonPresent ? 0U : NativeMethods.DIGCF_PRESENT;

            IntPtr set = NativeMethods.SetupDiGetClassDevs(
                ref classGuid,
                IntPtr.Zero,
                IntPtr.Zero,
                flags);

            if (set == NativeMethods.INVALID_HANDLE_VALUE)
                throw new InvalidOperationException(
                    "SetupDiGetClassDevs ist fehlgeschlagen. Win32-Fehler: " +
                    Marshal.GetLastWin32Error().ToString());

            try
            {
                uint index = 0;

                while (true)
                {
                    NativeMethods.SP_DEVINFO_DATA data =
                        new NativeMethods.SP_DEVINFO_DATA();

                    data.cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SP_DEVINFO_DATA));

                    if (!NativeMethods.SetupDiEnumDeviceInfo(set, index, ref data))
                    {
                        int error = Marshal.GetLastWin32Error();

                        if (error == 259) // ERROR_NO_MORE_ITEMS
                            break;

                        throw new InvalidOperationException(
                            "SetupDiEnumDeviceInfo ist fehlgeschlagen. Win32-Fehler: " +
                            error.ToString());
                    }

                    DeviceInfo device = new DeviceInfo();

                    device.InstanceId = GetInstanceId(set, ref data);
                    device.Name = GetStringProperty(
                        set,
                        ref data,
                        NativeMethods.SPDRP_FRIENDLYNAME);

                    if (String.IsNullOrWhiteSpace(device.Name))
                    {
                        device.Name = GetStringProperty(
                            set,
                            ref data,
                            NativeMethods.SPDRP_DEVICEDESC);
                    }

                    if (String.IsNullOrWhiteSpace(device.Name))
                        device.Name = device.InstanceId;

                    device.Manufacturer = GetStringProperty(
                        set,
                        ref data,
                        NativeMethods.SPDRP_MFG);

                    device.Kind = LooksLikePhysicalHardware(device.InstanceId)
                        ? "Hardware"
                        : "Virtuell/System";

                    uint status;
                    uint problem;
                    uint cr = NativeMethods.CM_Get_DevNode_Status(
                        out status,
                        out problem,
                        data.DevInst,
                        0);

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
                int aRank = String.Equals(a.Kind, "Hardware", StringComparison.OrdinalIgnoreCase) ? 0 : 1;
                int bRank = String.Equals(b.Kind, "Hardware", StringComparison.OrdinalIgnoreCase) ? 0 : 1;

                int kind = aRank.CompareTo(bRank);

                if (kind != 0)
                    return kind;

                return String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });

            return devices;
        }

        private static string GetInstanceId(
            IntPtr set,
            ref NativeMethods.SP_DEVINFO_DATA data)
        {
            int required;

            NativeMethods.SetupDiGetDeviceInstanceId(
                set,
                ref data,
                null,
                0,
                out required);

            if (required <= 0)
                return String.Empty;

            StringBuilder builder = new StringBuilder(required + 1);

            if (!NativeMethods.SetupDiGetDeviceInstanceId(
                set,
                ref data,
                builder,
                builder.Capacity,
                out required))
            {
                return String.Empty;
            }

            return builder.ToString();
        }

        private static string GetStringProperty(
            IntPtr set,
            ref NativeMethods.SP_DEVINFO_DATA data,
            uint property)
        {
            uint regType;
            uint required;

            byte[] buffer = new byte[2048];

            bool ok = NativeMethods.SetupDiGetDeviceRegistryProperty(
                set,
                ref data,
                property,
                out regType,
                buffer,
                (uint)buffer.Length,
                out required);

            if (!ok || required == 0)
                return String.Empty;

            int length = (int)Math.Min((uint)buffer.Length, required);

            if (length <= 0)
                return String.Empty;

            string value = Encoding.Unicode.GetString(buffer, 0, length);
            return value.TrimEnd('\0').Trim();
        }

        internal static bool LooksLikePhysicalHardware(string instanceId)
        {
            if (String.IsNullOrWhiteSpace(instanceId))
                return false;

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
            result.Kind = LooksLikePhysicalHardware(instanceId)
                ? "Hardware"
                : "Virtuell/System";

            uint devInst;
            uint cr = NativeMethods.CM_Locate_DevNode(out devInst, instanceId, 0);

            if (cr != NativeMethods.CR_SUCCESS)
            {
                result.IsPresent = false;
                return result;
            }

            uint status;
            uint problem;

            cr = NativeMethods.CM_Get_DevNode_Status(
                out status,
                out problem,
                devInst,
                0);

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
            {
                throw new InvalidOperationException(
                    "Gerät nicht gefunden (CONFIGRET=" + cr.ToString() + "):\r\n" +
                    instanceId);
            }

            if (enable)
            {
                cr = NativeMethods.CM_Enable_DevNode(devInst, 0);
            }
            else
            {
                cr = NativeMethods.CM_Disable_DevNode(
                    devInst,
                    NativeMethods.CM_DISABLE_PERSIST);
            }

            if (cr != NativeMethods.CR_SUCCESS)
            {
                string operation = enable ? "Aktivieren" : "Deaktivieren";

                throw new InvalidOperationException(
                    operation + " fehlgeschlagen (CONFIGRET=" +
                    cr.ToString() + "):\r\n" + instanceId);
            }
        }

        internal static ManagedState GetManagedState(IEnumerable<string> instanceIds)
        {
            ManagedState state = new ManagedState();

            foreach (string id in instanceIds)
            {
                if (String.IsNullOrWhiteSpace(id))
                    continue;

                state.ConfiguredCount++;

                DeviceInfo device = GetDeviceState(id);

                if (!device.IsPresent)
                {
                    state.MissingCount++;
                    continue;
                }

                if (device.IsDisabled)
                    state.DisabledCount++;
                else
                    state.EnabledCount++;
            }

            if (state.EnabledCount > 0)
                state.InternetConnected = ConnectivityManager.IsConnectedToInternet();

            return state;
        }
    }

    internal static class ConnectivityManager
    {
        private static readonly Guid NetworkListManagerClsid =
            new Guid("DCB00C01-570F-4A9B-8D69-199FDBA5723B");

        internal static bool IsConnectedToInternet()
        {
            object networkListManager = null;

            try
            {
                Type type = Type.GetTypeFromCLSID(NetworkListManagerClsid, true);
                networkListManager = Activator.CreateInstance(type);

                object result = networkListManager.GetType().InvokeMember(
                    "IsConnectedToInternet",
                    BindingFlags.GetProperty,
                    null,
                    networkListManager,
                    null);

                return Convert.ToBoolean(result);
            }
            catch
            {
                int flags;
                return NativeMethods.InternetGetConnectedState(out flags, 0);
            }
            finally
            {
                if (networkListManager != null &&
                    Marshal.IsComObject(networkListManager))
                {
                    try
                    {
                        Marshal.FinalReleaseComObject(networkListManager);
                    }
                    catch
                    {
                    }
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
                catch (Exception ex)
                {
                    error = ex.Message;
                }

                if (completed != null)
                {
                    completed(error);
                }
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
                if (process == null)
                    throw new InvalidOperationException("ipconfig.exe konnte nicht gestartet werden.");

                process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0 && !String.IsNullOrWhiteSpace(stderr))
                {
                    throw new InvalidOperationException(stderr.Trim());
                }
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
                graphics.SmoothingMode =
                    System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                graphics.Clear(Color.Transparent);
                graphics.FillEllipse(shadow, 5, 6, 23, 23);
                graphics.FillEllipse(brush, 3, 3, 25, 25);
                graphics.DrawEllipse(border, 3, 3, 25, 25);

                IntPtr handle = bitmap.GetHicon();

                try
                {
                    using (Icon temp = Icon.FromHandle(handle))
                    {
                        return (Icon)temp.Clone();
                    }
                }
                finally
                {
                    NativeMethods.DestroyIcon(handle);
                }
            }
        }
    }

    internal sealed class MainWindow : Form
    {
        private readonly DataGridView grid;
        private readonly CheckBox showNonPresent;
        private readonly Label statusLabel;
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
            header.Height = 112;

            Label title = new Label();
            title.Text = "OffNet – Netzwerkgeräte auf PnP-/Treiberebene";
            title.Font = new Font("Segoe UI Semibold", 15.0f);
            title.AutoSize = true;
            title.Location = new Point(14, 10);
            header.Controls.Add(title);

            Label description = new Label();
            description.Text =
                "Haken bei „Tray steuern“ = dauerhaft vom Tray verwaltet. Zeile markieren = einmalige Aktion über die Buttons unten.";
            description.ForeColor = Color.DimGray;
            description.AutoSize = true;
            description.Location = new Point(17, 44);
            header.Controls.Add(description);

            Label legend = new Label();
            legend.Text =
                "Tray:  Grün = Treiber aktiv + Internet   |   Rot = Treiber deaktiviert   |   Gelb blinkend = aktiv, aber kein Internet";
            legend.AutoSize = true;
            legend.Location = new Point(17, 70);
            header.Controls.Add(legend);

            showNonPresent = new CheckBox();
            showNonPresent.Text = "Nicht vorhandene Geräte anzeigen";
            showNonPresent.AutoSize = true;
            showNonPresent.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            showNonPresent.Location = new Point(825, 18);
            showNonPresent.CheckedChanged += delegate { RefreshDevices(); };
            header.Controls.Add(showNonPresent);

            Button refresh = new Button();
            refresh.Text = "Neu laden";
            refresh.Width = 120;
            refresh.Height = 30;
            refresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            refresh.Location = new Point(955, 56);
            refresh.Click += delegate { RefreshDevices(); };
            header.Controls.Add(refresh);

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

            DataGridViewCheckBoxColumn managedColumn =
                new DataGridViewCheckBoxColumn();
            managedColumn.HeaderText = "Tray steuern";
            managedColumn.Width = 95;
            managedColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            grid.Columns.Add(managedColumn);

            DataGridViewTextBoxColumn stateColumn =
                new DataGridViewTextBoxColumn();
            stateColumn.HeaderText = "Status";
            stateColumn.Width = 145;
            stateColumn.ReadOnly = true;
            grid.Columns.Add(stateColumn);

            DataGridViewTextBoxColumn kindColumn =
                new DataGridViewTextBoxColumn();
            kindColumn.HeaderText = "Typ";
            kindColumn.Width = 110;
            kindColumn.ReadOnly = true;
            grid.Columns.Add(kindColumn);

            DataGridViewTextBoxColumn nameColumn =
                new DataGridViewTextBoxColumn();
            nameColumn.HeaderText = "Gerät";
            nameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            nameColumn.FillWeight = 35;
            nameColumn.ReadOnly = true;
            grid.Columns.Add(nameColumn);

            DataGridViewTextBoxColumn manufacturerColumn =
                new DataGridViewTextBoxColumn();
            manufacturerColumn.HeaderText = "Hersteller";
            manufacturerColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            manufacturerColumn.FillWeight = 22;
            manufacturerColumn.ReadOnly = true;
            grid.Columns.Add(manufacturerColumn);

            DataGridViewTextBoxColumn idColumn =
                new DataGridViewTextBoxColumn();
            idColumn.HeaderText = "Geräteinstanz-ID";
            idColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            idColumn.FillWeight = 43;
            idColumn.ReadOnly = true;
            grid.Columns.Add(idColumn);

            grid.CurrentCellDirtyStateChanged += delegate
            {
                if (grid.IsCurrentCellDirty)
                {
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };

            grid.CellValueChanged += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (loading)
                    return;

                if (e.RowIndex >= 0 && e.ColumnIndex == 0)
                {
                    SaveManagedSelection();

                    EventHandler handler = ManagedDevicesChanged;

                    if (handler != null)
                        handler(this, EventArgs.Empty);
                }
            };

            Panel bottom = new Panel();
            bottom.Dock = DockStyle.Fill;
            bottom.Height = 94;

            Button activate = new Button();
            activate.Text = "Markierte aktivieren";
            activate.Width = 175;
            activate.Height = 32;
            activate.Location = new Point(12, 10);
            activate.Click += delegate
            {
                SetSelectedDevices(true);
            };
            bottom.Controls.Add(activate);

            Button disable = new Button();
            disable.Text = "Markierte deaktivieren";
            disable.Width = 185;
            disable.Height = 32;
            disable.Location = new Point(195, 10);
            disable.Click += delegate
            {
                SetSelectedDevices(false);
            };
            bottom.Controls.Add(disable);

            Button reconnect = new Button();
            reconnect.Text = "Reconnect";
            reconnect.Width = 120;
            reconnect.Height = 32;
            reconnect.Location = new Point(388, 10);
            reconnect.Click += delegate
            {
                EventHandler handler = ReconnectRequested;

                if (handler != null)
                    handler(this, EventArgs.Empty);
            };
            bottom.Controls.Add(reconnect);

            Button deviceManager = new Button();
            deviceManager.Text = "Geräte-Manager";
            deviceManager.Width = 130;
            deviceManager.Height = 32;
            deviceManager.Location = new Point(516, 10);
            deviceManager.Click += delegate
            {
                Process.Start("devmgmt.msc");
            };
            bottom.Controls.Add(deviceManager);

            statusLabel = new Label();
            statusLabel.AutoEllipsis = true;
            statusLabel.Anchor =
                AnchorStyles.Left |
                AnchorStyles.Right |
                AnchorStyles.Bottom;
            statusLabel.Location = new Point(14, 57);
            statusLabel.Size = new Size(1060, 22);
            statusLabel.Text = "Bereit.";
            bottom.Controls.Add(statusLabel);

            // Fixed three-row layout. This prevents the Fill-docked grid from
            // sliding underneath the header (which previously hid the column
            // headers and the first device rows, including physical adapters).
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
        }

        internal void RefreshDevices()
        {
            try
            {
                loading = true;
                statusLabel.Text = "Netzwerkgeräte werden eingelesen ...";
                Application.DoEvents();

                List<DeviceInfo> devices =
                    DeviceManager.EnumerateNetworkDevices(showNonPresent.Checked);

                HashSet<string> managed = ConfigurationStore.LoadManagedIds();

                if (!ConfigurationStore.HasConfiguration)
                {
                    foreach (DeviceInfo device in devices)
                    {
                        if (device.IsPresent &&
                            DeviceManager.LooksLikePhysicalHardware(device.InstanceId))
                        {
                            managed.Add(device.InstanceId);
                        }
                    }

                    if (managed.Count == 0)
                    {
                        foreach (DeviceInfo device in devices)
                        {
                            if (device.IsPresent)
                                managed.Add(device.InstanceId);
                        }
                    }

                    ConfigurationStore.SaveManagedIds(managed);
                }

                grid.Rows.Clear();

                foreach (DeviceInfo device in devices)
                {
                    bool isManaged = managed.Contains(device.InstanceId);

                    int rowIndex = grid.Rows.Add(
                        isManaged,
                        device.StateText,
                        device.Kind,
                        device.Name,
                        device.Manufacturer,
                        device.InstanceId);

                    DataGridViewRow row = grid.Rows[rowIndex];
                    row.Tag = device.InstanceId;

                    if (!device.IsPresent)
                        row.DefaultCellStyle.ForeColor = Color.Gray;
                    else if (device.IsDisabled)
                        row.DefaultCellStyle.ForeColor = Color.Firebrick;
                    else
                        row.DefaultCellStyle.ForeColor = Color.DarkGreen;
                }

                statusLabel.Text =
                    devices.Count.ToString() +
                    " Netzwerkgerät(e) gefunden. Vom Tray verwaltet: " +
                    managed.Count.ToString() + ".";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    Program.AppName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                statusLabel.Text = "Fehler beim Einlesen.";
            }
            finally
            {
                loading = false;
            }
        }

        internal void SetStatusText(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(SetStatusText), text);
                return;
            }

            statusLabel.Text = text;
        }

        private void SaveManagedSelection()
        {
            List<string> ids = new List<string>();

            foreach (DataGridViewRow row in grid.Rows)
            {
                bool selected = false;

                if (row.Cells[0].Value != null)
                    selected = Convert.ToBoolean(row.Cells[0].Value);

                if (selected && row.Tag != null)
                    ids.Add(row.Tag.ToString());
            }

            ConfigurationStore.SaveManagedIds(ids);
            statusLabel.Text =
                "Tray-Auswahl gespeichert: " +
                ids.Count.ToString() + " Gerät(e).";
        }

        private List<string> GetSelectedDeviceIds()
        {
            List<string> ids = new List<string>();
            HashSet<string> unique =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (DataGridViewRow row in grid.SelectedRows)
            {
                if (row.Tag == null)
                    continue;

                string id = row.Tag.ToString();

                if (unique.Add(id))
                    ids.Add(id);
            }

            return ids;
        }

        private void SetSelectedDevices(bool enable)
        {
            List<string> ids = GetSelectedDeviceIds();

            if (ids.Count == 0)
            {
                MessageBox.Show(
                    "Bitte mindestens ein Netzwerkgerät auswählen.",
                    Program.AppName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!enable)
            {
                DialogResult answer = MessageBox.Show(
                    "Die ausgewählten Netzwerkgeräte wirklich persistent auf " +
                    "PnP-/Treiberebene deaktivieren?\r\n\r\n" +
                    "Eine laufende Netzwerk- oder Remote-Verbindung kann sofort abbrechen.",
                    "OffNet – Disable",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (answer != DialogResult.Yes)
                    return;
            }

            List<string> errors = new List<string>();

            foreach (string id in ids)
            {
                try
                {
                    DeviceManager.SetDeviceEnabled(id, enable);
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }

            Thread.Sleep(250);
            RefreshDevices();

            EventHandler changed = ManagedDevicesChanged;

            if (changed != null)
                changed(this, EventArgs.Empty);

            if (errors.Count > 0)
            {
                MessageBox.Show(
                    String.Join("\r\n\r\n", errors.ToArray()),
                    Program.AppName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }

    internal sealed class OffNetApplicationContext :
        ApplicationContext, IDisposable
    {
        private readonly NotifyIcon trayIcon;
        private readonly ContextMenuStrip trayMenu;
        private readonly ToolStripMenuItem activateItem;
        private readonly ToolStripMenuItem disableItem;
        private readonly ToolStripMenuItem reconnectItem;
        private readonly MainWindow mainWindow;

        private readonly System.Windows.Forms.Timer statusTimer;
        private readonly System.Windows.Forms.Timer blinkTimer;

        private readonly Icon greenIcon;
        private readonly Icon redIcon;
        private readonly Icon yellowIconA;
        private readonly Icon yellowIconB;

        private TrayState currentState;
        private bool blinkPhase;
        private bool exiting;

        internal OffNetApplicationContext()
        {
            greenIcon = IconFactory.CreateCircle(Color.LimeGreen);
            redIcon = IconFactory.CreateCircle(Color.Red);
            yellowIconA = IconFactory.CreateCircle(Color.Gold);
            yellowIconB = IconFactory.CreateCircle(Color.Khaki);

            trayMenu = new ContextMenuStrip();

            activateItem = new ToolStripMenuItem("Activate");
            activateItem.Click += delegate { ActivateManagedDevices(); };
            trayMenu.Items.Add(activateItem);

            disableItem = new ToolStripMenuItem("Disable");
            disableItem.Click += delegate { DisableManagedDevices(); };
            trayMenu.Items.Add(disableItem);

            reconnectItem = new ToolStripMenuItem("Reconnect");
            reconnectItem.Click += delegate { Reconnect(); };
            trayMenu.Items.Add(reconnectItem);

            trayMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem openItem =
                new ToolStripMenuItem("OffNet öffnen...");
            openItem.Click += delegate { ShowMainWindow(); };
            trayMenu.Items.Add(openItem);

            ToolStripMenuItem deviceManagerItem =
                new ToolStripMenuItem("Windows Geräte-Manager");
            deviceManagerItem.Click += delegate
            {
                Process.Start("devmgmt.msc");
            };
            trayMenu.Items.Add(deviceManagerItem);

            trayMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem exitItem =
                new ToolStripMenuItem("Beenden");
            exitItem.Click += delegate { ExitOffNet(); };
            trayMenu.Items.Add(exitItem);

            trayIcon = new NotifyIcon();
            trayIcon.Visible = true;
            trayIcon.Icon = yellowIconA;
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

            trayIcon.DoubleClick += delegate
            {
                ShowMainWindow();
            };

            bool firstRun = !ConfigurationStore.HasConfiguration;

            mainWindow = new MainWindow();
            mainWindow.ManagedDevicesChanged += delegate
            {
                UpdateTrayState();
            };
            mainWindow.ReconnectRequested += delegate
            {
                Reconnect();
            };

            mainWindow.FormClosing += delegate(object sender, FormClosingEventArgs e)
            {
                if (!exiting)
                {
                    e.Cancel = true;
                    mainWindow.Hide();
                    mainWindow.ShowInTaskbar = false;
                }
            };

            // Create the WinForms handle even when later starts begin hidden.
            // This also makes cross-thread BeginInvoke safe for Reconnect completion.
            IntPtr mainWindowHandle = mainWindow.Handle;

            mainWindow.RefreshDevices();
            UpdateTrayState();

            if (firstRun)
            {
                mainWindow.Show();
                mainWindow.Activate();

                ShowBalloon(
                    "OffNet",
                    "Wähle einmal aus, welche Netzwerkgeräte vom Tray verwaltet werden sollen.",
                    ToolTipIcon.Info);
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

        private HashSet<string> ManagedIds
        {
            get { return ConfigurationStore.LoadManagedIds(); }
        }

        private void ShowMainWindow()
        {
            mainWindow.RefreshDevices();

            if (!mainWindow.Visible)
                mainWindow.Show();

            mainWindow.ShowInTaskbar = true;
            mainWindow.WindowState = FormWindowState.Normal;
            mainWindow.Activate();
            mainWindow.BringToFront();
        }

        private void UpdateTrayState()
        {
            try
            {
                HashSet<string> ids = ManagedIds;
                ManagedState state = DeviceManager.GetManagedState(ids);

                currentState = state.State;

                activateItem.Enabled =
                    state.DisabledCount > 0 || state.MissingCount > 0;
                disableItem.Enabled = state.EnabledCount > 0;
                reconnectItem.Enabled = state.EnabledCount > 0;

                if (state.State == TrayState.Green)
                {
                    trayIcon.Icon = greenIcon;
                    trayIcon.Text = TrimTrayText(
                        "OffNet - Gerät/Treiber aktiv + Internet");
                }
                else if (state.State == TrayState.Red)
                {
                    trayIcon.Icon = redIcon;

                    if (state.ConfiguredCount == 0)
                    {
                        trayIcon.Text = TrimTrayText(
                            "OffNet - keine Tray-Geräte gewählt");
                    }
                    else
                    {
                        trayIcon.Text = TrimTrayText(
                            "OffNet - Gerät/Treiber deaktiviert");
                    }
                }
                else
                {
                    trayIcon.Icon = blinkPhase ? yellowIconA : yellowIconB;
                    trayIcon.Text = TrimTrayText(
                        "OffNet - aktiv, aber kein Internet");
                }
            }
            catch
            {
                currentState = TrayState.Yellow;
                trayIcon.Icon = yellowIconA;
                trayIcon.Text = "OffNet - Status unbekannt";
            }
        }

        private void UpdateBlink()
        {
            if (currentState != TrayState.Yellow)
                return;

            blinkPhase = !blinkPhase;
            trayIcon.Icon = blinkPhase ? yellowIconA : yellowIconB;
        }

        private static string TrimTrayText(string text)
        {
            // Older .NET Framework versions are conservative about NotifyIcon.Text length.
            if (text.Length <= 63)
                return text;

            return text.Substring(0, 63);
        }

        private void ActivateManagedDevices()
        {
            HashSet<string> ids = ManagedIds;

            if (ids.Count == 0)
            {
                ShowBalloon(
                    "OffNet",
                    "Keine Netzwerkgeräte sind als „Tray verwaltet“ markiert.",
                    ToolTipIcon.Info);
                return;
            }

            List<string> errors = new List<string>();

            foreach (string id in ids)
            {
                try
                {
                    DeviceManager.SetDeviceEnabled(id, true);
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }

            Thread.Sleep(250);
            UpdateTrayState();
            mainWindow.SetStatusText("Tray-Aktion: Activate ausgeführt.");

            if (errors.Count == 0)
            {
                ShowBalloon(
                    "OffNet – Activate",
                    "Verwaltete Netzwerkgeräte wurden aktiviert.",
                    ToolTipIcon.Info);
            }
            else
            {
                ShowErrorSummary(errors);
            }
        }

        private void DisableManagedDevices()
        {
            HashSet<string> ids = ManagedIds;

            if (ids.Count == 0)
            {
                ShowBalloon(
                    "OffNet",
                    "Keine Netzwerkgeräte sind als „Tray verwaltet“ markiert.",
                    ToolTipIcon.Info);
                return;
            }

            DialogResult answer = MessageBox.Show(
                "Die vom Tray verwalteten Netzwerkgeräte wirklich persistent " +
                "auf PnP-/Treiberebene deaktivieren?\r\n\r\n" +
                "LAN, WLAN, VPN oder eine Remote-Sitzung können dabei sofort getrennt werden.",
                "OffNet – Disable",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (answer != DialogResult.Yes)
                return;

            List<string> errors = new List<string>();

            foreach (string id in ids)
            {
                try
                {
                    DeviceInfo current = DeviceManager.GetDeviceState(id);

                    if (current.IsPresent && !current.IsDisabled)
                        DeviceManager.SetDeviceEnabled(id, false);
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }

            Thread.Sleep(250);
            UpdateTrayState();
            mainWindow.SetStatusText("Tray-Aktion: Disable persistent ausgeführt.");

            if (errors.Count == 0)
            {
                ShowBalloon(
                    "OffNet – Disable",
                    "Verwaltete Netzwerkgeräte wurden persistent deaktiviert.",
                    ToolTipIcon.Info);
            }
            else
            {
                ShowErrorSummary(errors);
            }
        }

        private void Reconnect()
        {
            mainWindow.SetStatusText(
                "Reconnect läuft: DHCP Release/Renew für IPv4 und IPv6 ...");

            ShowBalloon(
                "OffNet – Reconnect",
                "DHCP Release/Renew wurde gestartet.",
                ToolTipIcon.Info);

            ConnectivityManager.ReconnectAsync(delegate(string error)
            {
                if (mainWindow.IsDisposed)
                    return;

                mainWindow.BeginInvoke(new Action(delegate
                {
                    if (String.IsNullOrEmpty(error))
                    {
                        mainWindow.SetStatusText(
                            "Reconnect abgeschlossen: DHCP Release/Renew wurde ausgeführt.");

                        ShowBalloon(
                            "OffNet – Reconnect",
                            "Reconnect abgeschlossen.",
                            ToolTipIcon.Info);
                    }
                    else
                    {
                        mainWindow.SetStatusText(
                            "Reconnect wurde mit einem Fehler beendet.");

                        MessageBox.Show(
                            error,
                            Program.AppName,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }

                    UpdateTrayState();
                }));
            });
        }

        private void ShowErrorSummary(List<string> errors)
        {
            MessageBox.Show(
                String.Join("\r\n\r\n", errors.ToArray()),
                Program.AppName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void ShowBalloon(
            string title,
            string text,
            ToolTipIcon icon)
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
                    if (statusTimer != null)
                        statusTimer.Dispose();

                    if (blinkTimer != null)
                        blinkTimer.Dispose();

                    if (trayIcon != null)
                        trayIcon.Dispose();

                    if (trayMenu != null)
                        trayMenu.Dispose();

                    if (mainWindow != null)
                        mainWindow.Dispose();

                    if (greenIcon != null)
                        greenIcon.Dispose();

                    if (redIcon != null)
                        redIcon.Dispose();

                    if (yellowIconA != null)
                        yellowIconA.Dispose();

                    if (yellowIconB != null)
                        yellowIconB.Dispose();
                }
                catch
                {
                }
            }

            base.Dispose(disposing);
        }
    }
}
