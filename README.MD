# OffNet

OffNet is a lightweight Windows 10/11 tray application for enabling and disabling network devices directly at the Windows **PnP/driver level**.

Project page: **https://github.com/zeittresor/OffNet**

## Features

- Small native-style C# / WinForms application; no Electron or browser runtime.
- Uses Windows SetupAPI / Configuration Manager APIs for network-device control.
- Persistent device disable through `CM_Disable_DevNode(..., CM_DISABLE_PERSIST)`.
- Tray status indicator:
  - **Green by default:** at least one managed device is enabled and Windows reports Internet access.
  - **Red by default:** managed devices are disabled at PnP/driver level.
  - **Blinking yellow by default:** a managed device is enabled, but Windows reports no Internet access.
- Tray actions: **Activate**, **Disable**, and **Reconnect**.
- Reconnect runs DHCP release/renew for IPv4 and IPv6 without showing a console window.
- Per-device **Tray control** selection so physical adapters can be managed without disabling every WAN miniport or virtual adapter.
- Options dialog accessible directly from the tray menu.
- UI languages: **English (default)**, **German**, and **French**.
- Configurable colors for all three tray states.
- Project link available from the Options dialog.
- No telemetry and no cloud dependency.
- MIT licensed.

## Why PnP-level disabling?

OffNet does not merely disconnect a network profile or disable a logical interface. The `Disable` action disables the selected Windows PnP device node itself. This is comparable to disabling a device in Device Manager and remains disabled across reboot when the Windows API accepts `CM_DISABLE_PERSIST` for the device.

Software running with ordinary user privileges cannot simply re-enable such a device through normal network-interface APIs. Software running with administrator or SYSTEM privileges can still re-enable Windows PnP devices; OffNet is not intended to override Windows administrator authority.

## Tray menu

A left click on the tray circle opens a deliberately compact menu:

- Activate
- Disable
- Reconnect
- Open OffNet...
- Options...
- Exit

The project link is intentionally kept inside **Options** rather than the tray menu.
Windows Device Manager remains accessible from the main OffNet window.

## Options

The **Options** dialog allows you to change:

- Language: English / German / French
- Active + Internet circle color
- Disabled circle color
- Active but no Internet circle color
- Open the OffNet project page on GitHub

The no-Internet state blinks between the chosen color and a lighter version of the same color.

Settings are stored in:

```text
%LOCALAPPDATA%\OffNet\settings.ini
```

Managed device instance IDs are stored separately in:

```text
%LOCALAPPDATA%\OffNet\managed_devices.txt
```

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
﻿OffNet 1.0.1
============

Projekt
-------
OffNet ist eine kleine Windows-10/11-Tray-App zum Aktivieren und Deaktivieren
von Netzwerkgeräten direkt auf PnP-/Treiberebene.

Projektverweis:
https://github.com/zeittresor/OffNet

Lizenz:
MIT (siehe LICENSE)

Warum C# / WinForms?
--------------------
Die fertige EXE verwendet nur das klassische Windows/.NET-Framework und
Windows-APIs. Es gibt keine PowerShell-Laufzeit, keinen Browser-Unterbau,
keinen Electron-Prozess und keine externen DLLs im Programmpaket.

BUILD
-----
Einmal ausführen:

    Build_OffNet.cmd

Danach entsteht:

    OffNet.exe

Alternativ:

    Start_OffNet.cmd

Wenn die EXE noch fehlt, wird sie zuerst gebaut und anschließend gestartet.

Der Build sucht csc.exe bevorzugt hier:

    %WINDIR%\Microsoft.NET\Framework64\v4.0.30319\
    %WINDIR%\Microsoft.NET\Framework\v4.0.30319\

Die fertige OffNet.exe ist anschließend unabhängig vom Build-Script startbar.

ADMINISTRATORRECHTE
-------------------
OffNet.exe besitzt ein Windows-Manifest mit:

    requestedExecutionLevel = requireAdministrator

Beim Start erscheint daher die normale Windows-UAC-Abfrage.

TRAY-STATUS
-----------
GRÜNER KREIS
    Mindestens ein von OffNet verwaltetes Netzwerkgerät ist auf
    PnP-/Treiberebene aktiviert UND Windows meldet Internetzugang.

ROTER KREIS
    Kein verwaltetes Netzwerkgerät ist aktiv bzw. die verwalteten Geräte
    sind auf PnP-/Treiberebene deaktiviert.

GELB BLINKENDER KREIS
    Mindestens ein verwaltetes Netzwerkgerät ist aktiviert, Windows meldet
    aber keinen Internetzugang.

Für die Internetbewertung fragt OffNet Windows' Network List Manager (NLM)
ab. Es pingt nicht ständig selbst irgendeinen externen Server an.

TRAY-MENÜ
---------
Linksklick auf den Kreis:

    Activate
    Disable
    Reconnect

Zusätzlich:

    OffNet öffnen...
    Windows Geräte-Manager
    Beenden

Activate
--------
Aktiviert die in der Haupt-GUI als "Tray verwaltet" markierten Geräte über
die Windows Configuration Manager API.

Disable
-------
Deaktiviert die verwalteten Geräte direkt über:

    CM_Disable_DevNode(..., CM_DISABLE_PERSIST)

Damit wird nicht nur eine Netzwerkverbindung getrennt. Der PnP-Geräteknoten
selbst wird deaktiviert, entsprechend der Geräte-Manager-/Treiberebene.

CM_DISABLE_PERSIST sorgt dafür, dass der Disable-Zustand über einen
Windows-Neustart erhalten bleibt.

Eine Anwendung mit normalen Benutzerrechten kann das Gerät dadurch nicht
einfach über gewöhnliche Netzwerk-APIs wieder einschalten. Software mit
Administrator- oder SYSTEM-Rechten kann PnP-Geräte grundsätzlich wieder
aktivieren.

Reconnect
---------
Reconnect führt ohne sichtbares Konsolenfenster nacheinander aus:

    ipconfig /release *
    ipconfig /release6 *
    ipconfig /renew *
    ipconfig /renew6 *

Statisch konfigurierte IP-Adressen werden dadurch nicht in DHCP-Adressen
umgewandelt.

HAUPT-GUI
---------
Die Haupt-GUI listet die Windows-Geräteklasse "Netzwerkadapter" auf mit:

    - Tray verwaltet
    - Status
    - Typ (Hardware / Virtuell-System)
    - Gerätename
    - Hersteller
    - Geräteinstanz-ID

Die Checkbox "Tray verwaltet" ist wichtig: Dadurch kann OffNet z.B.
Ethernet und WLAN steuern, ohne automatisch WAN-Miniports, Hyper-V,
VPN-Adapter oder andere virtuelle Netzwerkgeräte abzuschalten.

Beim ersten Start versucht OffNet automatisch echte Hardwareadapter
(PCI/USB/PCMCIA/SD/Bluetooth) als Tray-Geräte vorzuwählen. Die Auswahl kann
jederzeit geändert werden.

KONFIGURATION
-------------
OffNet speichert ausschließlich die ausgewählten Geräteinstanz-IDs in:

    %LOCALAPPDATA%\OffNet\managed_devices.txt

Keine Telemetrie.
Keine Cloud-Anbindung.
Keine externen Abhängigkeiten im Programmpaket.

HINWEIS ZU REMOTE-SITZUNGEN
---------------------------
Wenn du den gerade verwendeten LAN-/WLAN-Adapter deaktivierst, kann eine
RDP-, AnyDesk-, TeamViewer-, VPN- oder andere Remote-Verbindung sofort
abbrechen. Deshalb fragt OffNet vor "Disable" noch einmal nach.

DATEIEN
-------
OffNet.cs
    Vollständiger C#-Quellcode.

OffNet.manifest
    UAC-/Windows-Manifest.

OffNet.ico
    Programmsymbol.

Build_OffNet.cmd
    Compiler-Script.

Start_OffNet.cmd
    Build-falls-nötig + Start.

LICENSE
    MIT-Lizenz.


ÄNDERUNGEN 1.0.1
----------------
- Layoutfehler behoben: Die Geräteliste liegt nicht mehr unter dem Kopfbereich.
- Spaltenüberschriften und alle gefundenen Geräte sind vollständig sichtbar.
- Physische Hardwareadapter werden ausdrücklich vor virtuellen/System-Adaptern sortiert.
- "Tray verwaltet" wurde zur eindeutigeren Bezeichnung "Tray steuern".
- Erklärung ergänzt:
    Haken = dauerhaft vom Tray verwaltet
    Zeile markieren = einmalige Aktion über die Buttons unten
- Buttons heißen nun "Markierte aktivieren/deaktivieren".
