
# OffNet

OffNet is a lightweight Windows 10/11 tray application for enabling and disabling network devices directly at the Windows **PnP/driver level**.

Project page: **https://github.com/zeittresor/OffNet**

## Features

- Small C# / WinForms application; no Electron or browser runtime.
- Uses Windows SetupAPI / Configuration Manager APIs for network-device control.
- Persistent device disable through `CM_Disable_DevNode(..., CM_DISABLE_PERSIST)`.
- Tray status indicator:
  - **Green by default:** at least one managed device is enabled and Windows reports Internet access.
  - **Red by default:** managed devices are disabled at PnP/driver level.
  - **Blinking yellow by default:** a managed device is enabled, but Windows reports no Internet access.
- Tray actions: **Activate**, **Disable**, and **Reconnect**.
- Reconnect runs DHCP release/renew for IPv4 and IPv6 without showing a console window.
- Per-device **Tray control** selection so physical adapters can be managed without disabling every WAN miniport or virtual adapter.
- Optional rolling traffic graph directly inside the tray menu.
- Optional permanent, click-through throughput meter visually embedded in the Windows taskbar.
- UI languages: **English (default)**, **German**, and **French**.
- Configurable tray-state colors and traffic-graph colors.
- Optional automatic startup at Windows sign-in.
- Project link available from the Options dialog.
- No telemetry and no cloud dependency.
- MIT licensed.

## Why PnP-level disabling?

OffNet does not merely disconnect a network profile or disable a logical interface. The `Disable` action disables the selected Windows PnP device node itself. This is comparable to disabling a device in Device Manager and remains disabled across reboot when the Windows API accepts `CM_DISABLE_PERSIST` for the device.

Software running with ordinary user privileges cannot simply re-enable such a device through normal network-interface APIs. Software running with administrator or SYSTEM privileges can still re-enable Windows PnP devices; OffNet is not intended to override Windows administrator authority.

## Tray menu

A **left or right click** on the tray circle opens the same deliberately compact context menu:

- Activate
- Disable
- Reconnect
- Open OffNet...
- Options...
- Exit

If the optional traffic overview is enabled, its graph is shown above these actions.

The context menu uses normal Windows popup behavior: clicking anywhere outside it closes the menu immediately.

The project link is intentionally kept inside **Options** rather than the tray menu. Windows Device Manager remains accessible from the main OffNet window.

## Optional traffic overview

The tray menu can display a continuously updated rolling traffic graph.

Default history length: **1 minute**

The history length is configurable in **Options** with a slider from **10 seconds to 10 minutes** in **10-second steps**.

Three independently colored curves are available:

- **Download** — receive throughput on OffNet-managed network adapters, in Mbit/s.
- **Upload** — send throughput on OffNet-managed network adapters, in Mbit/s.
- **Offline activity** — combined receive + send throughput on other active adapters, but only during periods in which all OffNet-managed adapters are disabled.

Download and upload share the same graph and can overlap. Their separate colors make them distinguishable.

The third curve is intentionally shown only for disabled periods. This makes it possible to notice traffic through another adapter even though the adapters controlled by OffNet are supposed to be offline.

Traffic sampling is derived from the selected history window and measurement-point count. The default is **1 minute / 75 points**, which is approximately one measurement every **0.8 seconds**.

No external server is contacted for the traffic graph. OffNet reads local Windows network-interface byte counters.

## Options

The **Options** dialog allows you to change:

- Language: English / German / French
- Active + Internet tray-circle color
- Disabled tray-circle color
- Active but no Internet tray-circle color
- Show or hide the traffic overview in the tray menu
- Show or hide the permanent taskbar throughput meter
- Traffic history window (10 seconds to 10 minutes)
- Measurement points (10 to 300)
- Download curve color
- Upload curve color
- Offline-activity curve color
- Start OffNet automatically when you sign in to Windows
- Open the OffNet project page on GitHub

The no-Internet tray state blinks between the chosen color and a lighter version of the same color.

Settings are stored in:

```text
%LOCALAPPDATA%\OffNet\settings.ini
```

Managed device instance IDs are stored separately in:

```text
%LOCALAPPDATA%\OffNet\managed_devices.txt
```

## Automatic startup

Because OffNet needs elevated privileges for PnP/driver-level device control, the automatic-start option does **not** use the ordinary Startup folder or `HKCU\...\Run` registry value. Instead, OffNet registers a per-user Windows Task Scheduler task with an **At log on** trigger and **highest privileges**. Disabling the checkbox removes that task.

The task points to the current `OffNet.exe`. OffNet only changes the scheduled task when the automatic-start checkbox itself is changed. If the application is moved to another folder, disable and re-enable the automatic-start option once so the task is registered with the new executable path.

## Build

OffNet is intentionally kept simple and runtime-light. On a Windows machine, run:

```text
Build_OffNet.cmd
```

The script looks for the classic .NET Framework C# compiler (`csc.exe`) and creates:

```text
OffNet.exe
```

You can also run:

```text
Start_OffNet.cmd
```

If the EXE does not exist yet, the script builds it first and then starts it.

The generated EXE includes a manifest that requests administrator privileges because PnP device state changes require elevation.

## Reconnect behavior

Reconnect executes the equivalent of:

```text
ipconfig /release *
ipconfig /release6 *
ipconfig /renew *
ipconfig /renew6 *
```

Statically configured addresses are not converted to DHCP addresses by OffNet.

## Remote-session warning

Disabling the physical network adapter currently carrying an RDP, VPN, TeamViewer, AnyDesk, or other remote session can disconnect that session immediately. OffNet asks for confirmation before disabling managed devices.

## License

OffNet is released under the **MIT License**. See [LICENSE](LICENSE).


## Runtime options reliability (1.2.1)

- Language, tray colors, traffic history and graph options are applied independently from Windows autostart.
- The Task Scheduler entry is modified only when the **Start OffNet automatically when I sign in to Windows** checkbox actually changes.
- A Windows autostart error therefore no longer prevents a runtime language change or any other OffNet option from being saved.
- Scheduled-task registration now uses the running executable's resolved path and no unnecessary working-directory field.


## Permanent taskbar throughput meter

OffNet can optionally show a lightweight, click-through throughput meter directly over the Windows taskbar. It shares the same rolling history and colors as the tray-menu graph and displays Download, Upload, and Offline activity in Mbit/s. The graph scale follows the highest visible value in the selected rolling time window and can move both upward and downward as old peaks leave the history.

The meter background is transparent, so the current Windows taskbar appearance remains visible underneath it.


## Taskbar meter readability and Z-order

Version 1.3.1 improves the optional permanent Windows-taskbar throughput meter:

- OffNet explicitly reapplies the Win32 topmost Z-order while the meter is visible using `SetWindowPos(..., HWND_TOPMOST, ... SWP_NOACTIVATE ...)`.
- The meter remains click-through and does not take keyboard focus.
- Z-order and position are refreshed periodically while the meter is enabled, preventing the taskbar compositor from visually covering it after focus changes.
- Current Download, Upload and Offline-activity values are arranged horizontally instead of being squeezed into three vertical rows.
- The taskbar meter font is selectable in **Options** from the fonts installed on Windows.
- Default meter font: **Segoe UI Semibold**.
- If a configured font is unavailable, OffNet falls back safely to Segoe UI Semibold / Segoe UI.
- The Options dialog is resizable and scrollable so future options remain accessible on smaller displays.


## Configurable traffic measurement points

The traffic options include a **Measurement points** slider from **10 to 300 points**. The default is **75**.

OffNet distributes the requested measurements across the complete selected rolling history:

```text
sample interval = history duration / measurement points
```

Examples:

- 1 minute / 75 points ≈ 0.8 seconds per measurement
- 1 minute / 300 points = 0.2 seconds per measurement
- 10 seconds / 300 points ≈ 0.033 seconds per measurement
- 10 minutes / 75 points = 8 seconds per measurement
- 10 minutes / 10 points = 60 seconds per measurement

This keeps memory usage bounded and lets the user choose the desired graph resolution without storing an unnecessarily large history.

## Statistics themes

The tray-menu statistics graph supports:

- Light
- Dark
- Sepia
- Ocean
- Matrix
- Hellfire
- Purple
- Aurora

The selected theme styles the graph background, grid, borders and supporting text. The separately configured Download, Upload and Offline-activity curve colors are preserved.

The permanent Windows-taskbar meter deliberately stays transparent so the real taskbar color, accent and transparency remain visible underneath it.

## Single tray status indicator

Version 1.3.3 removes the additional small OffNet status circle from the permanent taskbar throughput meter.

The normal OffNet notification-area icon remains the single status indicator:

- active + Internet
- device/driver disabled
- active but no Internet

The permanent throughput meter now contains only the traffic values, rolling graph and dynamic Mbit/s scale.


## Dynamic graph scaling

The throughput Y-axis no longer remains tied to an old peak for the entire history window. OffNet recalculates the graph scale from recent traffic at a configurable interval.

Default: **5 seconds**

Options range: **1–30 seconds**

The same adaptive scaling is used by both the tray-menu graph and the permanent taskbar meter. This makes low current traffic such as 0.90 Mbit/s visibly occupy the graph after an older high-throughput burst has passed the scale-refresh interval.

## Full-screen behavior

When the foreground application covers its monitor in true full-screen mode, OffNet temporarily:

- hides the permanent taskbar throughput meter;
- pauses traffic sampling for both graph views;
- closes an open traffic/tray popup if necessary.

The normal device-control state remains intact. When full-screen mode ends, OffNet re-establishes its byte-counter baselines and resumes monitoring without turning the full-screen pause into an artificial traffic spike.

## Application themes

The complete OffNet GUI can use:

- Muffin
- Light
- Dark
- Sepia
- Ocean
- Matrix
- Hellfire
- Purple
- Aurora

**Muffin** represents OffNet's original neutral/light appearance.

Theme palettes explicitly define foreground, secondary text, inputs, borders, selection colors and status colors to maintain readable contrast. Statistics themes remain independently selectable.

## Taskbar meter typography

The permanent taskbar meter now:

- displays current rates with **two decimal places**, for example `1.23 Mbit`;
- supports a configurable font;
- provides one **Text shadow** switch directly below the font option;
- applies the shadow consistently to all three traffic value labels and the dynamic scale label, or to none of them.

Text shadow is disabled by default.


## Windows executable metadata

OffNet 1.5.1 explicitly embeds Windows version information in the generated executable instead of relying on compiler defaults.

The **Details** tab of `OffNet.exe` includes:

- File description: `OffNet - Network Device & Traffic Controller`
- File version: `1.5.1.0`
- Product name: `OffNet`
- Product version: `1.5.1`
- Company: `zeittresor`
- Copyright / MIT license reference
- Project/source reference: `https://github.com/zeittresor/OffNet`

The project/source URL is also embedded in the assembly description/trademark metadata so it remains associated with the compiled executable.

## Window icon

The icon embedded in `OffNet.exe` is now also reused by the normal OffNet main window and the Options dialog. The application therefore has consistent branding in Explorer, the executable, and WinForms title bars.
