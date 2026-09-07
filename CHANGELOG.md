# Changelog

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
