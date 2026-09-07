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

Default history length: **10 minutes**

The history length is configurable in **Options** from **1 to 60 minutes**.

Three independently colored curves are available:

- **Download** — receive throughput on OffNet-managed network adapters, in Mbit/s.
- **Upload** — send throughput on OffNet-managed network adapters, in Mbit/s.
- **Offline activity** — combined receive + send throughput on other active adapters, but only during periods in which all OffNet-managed adapters are disabled.

Download and upload share the same graph and can overlap. Their separate colors make them distinguishable.

The third curve is intentionally shown only for disabled periods. This makes it possible to notice traffic through another adapter even though the adapters controlled by OffNet are supposed to be offline.

Traffic is sampled once per second while the overview option is enabled. The default 10-minute history therefore keeps only about 600 samples in memory.

No external server is contacted for the traffic graph. OffNet reads local Windows network-interface byte counters.

## Options

The **Options** dialog allows you to change:

- Language: English / German / French
- Active + Internet tray-circle color
- Disabled tray-circle color
- Active but no Internet tray-circle color
- Show or hide the traffic overview in the tray menu
- Traffic history length
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

The task points to the current `OffNet.exe`. Saving Options while automatic startup is enabled refreshes the path automatically if the application was moved.

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
