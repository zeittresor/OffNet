# Changelog

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
