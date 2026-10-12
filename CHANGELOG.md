# Changelog

## 1.6.2

- Added an optional safety confirmation for PnP network-device Activate/Disable actions.
- Confirmation is enabled by default and can be disabled in **Options** for immediate switching without another popup.
- The confirmation setting applies to both the main OffNet window and tray Activate/Disable actions.
- Added **Dynamic Y-axis scaling** on/off control.
- Added a manually configurable **Fixed Y-axis maximum (Mbit/s)**.
- In fixed-scale mode, values above the configured maximum remain numerically correct but their graph curve is clipped at the top edge.
- Fixed-scale mode applies to both the tray-menu graph and permanent taskbar meter.
- Dynamic scale refresh and per-stream `Affects scale` controls are disabled in Options while fixed-scale mode is selected.
- Preserved all OffNet 1.6.1 stream visibility, scale-influence, taskbar width, text-shadow, disk activity, full-screen and theme features.

## 1.6.1

- Added independent **Show** and **Affects scale** controls for Download, Upload, Offline network activity and Disk activity.
- Disk activity remains visible by default but no longer affects automatic graph scaling by default.
- Added a configurable permanent taskbar-meter width slider (**260–720 px**, default **420 px**).
- Taskbar width is still clamped to the actual free area before the Windows notification section.
- Fixed the taskbar text-shadow option so one common shadow color is applied consistently to every visible stream value and the scale label.
- Added a configurable taskbar shadow color; default is black.
- Preserved all OffNet 1.6.0 network/device, disk-monitoring, theme, full-screen and traffic-history features.

## 1.6.0

- Added a fourth telemetry stream: **Disk activity** in Mbit/s.
- Disk activity represents combined local physical-disk read + write throughput.
- Added independent visibility check boxes for Download, Upload, Offline network activity and Disk activity.
- Enabling all four check boxes displays all available streams.
- Added a configurable disk-activity curve/value color.
- The tray-menu graph and permanent taskbar meter both honor the same stream-visibility selection.
- Automatic Y-axis scaling now considers only streams that are currently visible.
- Hidden disk telemetry is not queried, reducing unnecessary work when the disk stream is disabled.
- Disk telemetry uses language-independent Windows `IOCTL_DISK_PERFORMANCE` counters rather than WMI, PowerShell or localized Performance Counter names.
- The taskbar meter dynamically distributes its value cells across the selected streams and widens when four streams are visible.
- The tray-menu graph uses one or two legend rows depending on the selected stream count.
- Preserved the 1.5.2 fine-grained adaptive scale, full-screen pausing, themes, taskbar font/shadow settings and PnP device controls.

## 1.5.2

- Refined the adaptive traffic graph Y-axis scaling.
- Replaced the coarse 1 / 1.5 / 2 / 3 / 5 / 7.5 / 10 scale ladder with finer decimal subdivisions.
- A recent peak around 99 Mbit/s now produces roughly a 110 Mbit/s ceiling instead of 150 Mbit/s.
- Keeps the existing ~10% visual headroom and configurable scale-refresh interval.
- Applies to both the tray-menu graph and the permanent taskbar meter through the shared `TrafficScaleState`.

## 1.5.1

- Added explicit Windows executable version metadata.
- `OffNet.exe` now reports file version **1.5.1.0** and product version **1.5.1** instead of `0.0.0.0`.
- Added descriptive file/product metadata for Windows Explorer's **Details** tab.
- Embedded `zeittresor` as the company/project owner.
- Embedded the MIT license/copyright reference.
- Embedded the project/source reference `https://github.com/zeittresor/OffNet` in assembly metadata.
- Added `AssemblyInfo.cs` to keep executable metadata separate and maintainable.
- The OffNet main window and Options dialog now display the same icon embedded in `OffNet.exe`.
- Preserved all OffNet 1.5.0 traffic, full-screen, theme, tray and device-control features.

## 1.5.0

- Added dynamic throughput-scale recalculation, defaulting to **5 seconds**.
- Added a **1–30 second** scale-update slider in Traffic options.
- Both the tray-menu graph and permanent taskbar meter now use recent traffic for adaptive Y-axis scaling, so low current throughput remains visually meaningful after old high peaks.
- Added true foreground full-screen detection.
- While a game, browser or other application occupies its monitor in full-screen mode, OffNet hides the taskbar meter and pauses traffic sampling for both graph views.
- Traffic monitoring resumes automatically after full-screen mode ends, with byte counters re-baselined to avoid false spikes.
- Added application-wide themes: **Muffin, Light, Dark, Sepia, Ocean, Matrix, Hellfire, Purple, Aurora**.
- **Muffin** represents the original neutral/light OffNet appearance.
- Added contrast-aware theme colors for backgrounds, controls, inputs, grids, menus, selections and status text.
- Added a custom themed tray-menu renderer for readable selections in dark/custom themes.
- Increased the Options dialog's usable vertical size while retaining automatic scrollbars and a 50-pixel top/bottom working-area safety margin.
- Permanent taskbar rates now use **two decimal places** (`1.23 Mbit`).
- Added an all-or-none **Text shadow in taskbar meter** option directly below the font selector; default is **off**.
- Preserved 1-minute / 75-point traffic defaults, 10-second to 10-minute history slider, 10–300 measurement points, taskbar font selection and single status icon.

## 1.4.0

- Changed the default traffic history to **1 minute**.
- Changed the default measurement resolution to **75 points**.
- Replaced the old minute field with a slider from **10 seconds to 10 minutes** in **10-second steps**.
- Reduced the measurement-point range to **10–300 points**.
- Sampling frequency is calculated from the selected history window and measurement-point count.
- Added statistics themes: **Light, Dark, Sepia, Ocean, Matrix, Hellfire, Purple, Aurora**.
- Themes style graph background/grid/borders/supporting text without overriding the three configurable traffic-curve colors.
- Preserved the transparent permanent taskbar meter, selectable taskbar font, Z-order fix, scrollable Options dialog, and single OffNet status icon.

## 1.3.2

- Added a **Measurement points** slider to Traffic options.
- Configurable range: **10–1000 points**; default: **600**.
- The requested points are distributed across the complete selected rolling history rather than simply truncating one-second samples.
- Traffic sampling interval is now calculated dynamically from history duration / measurement-point count.
- The traffic history is capped to the selected number of points as well as the selected time window.
- Sparse configurations such as 60 minutes / 10 points are handled correctly instead of being discarded as stale counter intervals.
- Preserved the 1.3.1 taskbar Z-order fix, selectable taskbar font, scrollable Options dialog, and all three traffic streams.

## 1.3.1

- Fixed the permanent taskbar throughput meter being visually pushed behind the Windows taskbar after focus changes.
- Added explicit `SetWindowPos(... HWND_TOPMOST ... SWP_NOACTIVATE ...)` Z-order maintenance.
- Kept the taskbar meter click-through and non-activating.
- Increased taskbar-meter readability with a horizontal current-value layout.
- Added a selectable taskbar-meter font in **Options**.
- Default taskbar-meter font is **Segoe UI Semibold**.
- Added safe font fallback if a configured font is unavailable.
- Made the Options dialog resizable and automatically scrollable when its content no longer fits.
- Preserved the transparent taskbar background, three traffic streams, configurable traffic colors/history, runtime languages, and the 1.2.1 startup-setting isolation fix.

## 1.3.0

- Added an optional permanent network-throughput meter for the Windows taskbar.
- Added **Show permanent throughput meter on the Windows taskbar** to **Options**.
- The taskbar meter uses the same configurable 1–60 minute rolling history as the tray-menu graph (10 minutes by default).
- Displays current **Download**, **Upload**, and **Offline activity** values in Mbit/s.
- Draws all three traffic curves in one compact graph using the configured traffic colors.
- The graph Y scale automatically moves upward or downward according to the highest visible throughput in the selected rolling history window.
- Offline activity remains a separate third stream and is only plotted for periods in which all OffNet-managed adapters are disabled.
- The taskbar meter background is transparent so the real Windows taskbar theme/accent/transparency remains visible.
- The meter is click-through, does not take keyboard focus, and does not block taskbar mouse interaction.
- Added a compact current-state circle to the taskbar meter.
- Traffic sampling now remains active when either the tray-menu graph or taskbar meter is enabled; sampling remains dormant when both are disabled.
- Preserved the 1.2.1 runtime-language/autostart decoupling fix.

## 1.2.1

- Fixed runtime language/options changes being coupled to Windows autostart registration.
- Windows Task Scheduler is now touched only when the autostart checkbox actually changes.
- Normal options are saved and applied before any optional autostart operation.
- Made scheduled-task executable path handling more robust and removed the unnecessary working-directory field.
- Ensured the Options OK button commits values before the dialog closes.


## 1.2.0

- Added an optional rolling network-throughput overview directly inside the tray context menu.
- Default graph history is 10 minutes and can be configured from 1 to 60 minutes in **Options**.
- Added overlapping **Download** and **Upload** curves in Mbit/s.
- Added a third **Offline activity** curve. It is visible only for periods in which all OffNet-managed adapters are disabled and reports combined RX + TX traffic on other active adapters.
- Added configurable colors for all three traffic curves.
- Traffic sampling continues while the tray menu is closed when the overview option is enabled, preserving the rolling history.
- Traffic sampling is disabled entirely when the overview option is off to keep background overhead minimal.
- Left and right tray-icon clicks now use the same popup path.
- Explicitly enabled normal context-menu AutoClose behavior so clicking outside the tray popup closes it immediately.
- Kept the tray action list compact; the graph is optional and appears above the existing actions.
- Added English, German and French localization for the new traffic options and graph labels.

## 1.1.2

- Added an **Options** checkbox to start OffNet automatically at Windows sign-in.
- Autostart uses Windows Task Scheduler with a per-user **At log on** trigger and **highest privileges**, avoiding the limitations of ordinary startup entries for elevated applications.
- Disabling the checkbox removes the scheduled task.
- Saving Options while autostart is enabled refreshes the task to the current `OffNet.exe` path.
- Added English, German and French localization for the new option and related errors.

## 1.1.1

- Simplified the tray context menu.
- Removed the direct GitHub project entry from the tray menu.
- Removed the direct Windows Device Manager entry from the tray menu.
- The GitHub project link remains available in **Options**.
- Windows Device Manager remains available from the main OffNet window.
- Kept the tray focused on core actions: Activate, Disable, Reconnect, Open OffNet, Options, Exit.

## 1.1.0

- English is now the default UI language.
- Added runtime language selection: English, German and French.
- Added a tray **Options** item and Options dialog.
- Added configurable tray colors for active/Internet, disabled and active/no-Internet states.
- Added a direct link to https://github.com/zeittresor/OffNet in the tray menu and Options dialog.
- Added English GitHub-ready `README.md`.
- Preserved lightweight C# / WinForms architecture, native PnP device control and MIT licensing.
- Kept the 1.0.1 device-list layout fix and hardware-first sorting.
