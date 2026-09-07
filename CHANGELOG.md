# Changelog

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
