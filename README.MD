OffNet 1.0.1
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
