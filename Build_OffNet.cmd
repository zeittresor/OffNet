@echo off
setlocal EnableExtensions
cd /d "%~dp0"

title OffNet 1.0.1 - Build

echo ============================================================
echo   OffNet 1.0.1
echo   C# / WinForms - Windows 10/11 Build
echo   https://github.com/zeittresor
echo ============================================================
echo.

set "CSC="

if exist "%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" (
    set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
)

if not defined CSC if exist "%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe" (
    set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)

if not defined CSC (
    for /f "delims=" %%I in ('where csc.exe 2^>nul') do (
        if not defined CSC set "CSC=%%I"
    )
)

if not defined CSC (
    echo [ERROR] csc.exe wurde nicht gefunden.
    echo.
    echo OffNet verwendet bewusst keinen mitgelieferten Runtime-Ballast.
    echo Installiere/aktiviere bei Bedarf .NET Framework 4.x Developer Tools
    echo oder baue die Quelle mit einem vorhandenen Visual Studio / Build Tools.
    echo.
    pause
    exit /b 1
)

echo [COMPILER] "%CSC%"
echo [SOURCE]   OffNet.cs
echo [OUTPUT]   OffNet.exe
echo.

if exist "OffNet.exe" del /q "OffNet.exe" >nul 2>&1

"%CSC%" ^
 /nologo ^
 /target:winexe ^
 /optimize+ ^
 /platform:anycpu ^
 /win32icon:"OffNet.ico" ^
 /win32manifest:"OffNet.manifest" ^
 /reference:System.dll ^
 /reference:System.Drawing.dll ^
 /reference:System.Windows.Forms.dll ^
 /out:"OffNet.exe" ^
 "OffNet.cs"

if errorlevel 1 (
    echo.
    echo [ERROR] Build fehlgeschlagen.
    pause
    exit /b 1
)

echo.
echo [OK] OffNet.exe erfolgreich erstellt.
for %%F in ("OffNet.exe") do echo [SIZE] %%~zF Bytes
echo.
echo Beim Start fordert die EXE per Manifest Administratorrechte an.
echo.

exit /b 0
