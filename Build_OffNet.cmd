@echo off
setlocal EnableExtensions
cd /d "%~dp0"

title OffNet 1.3.3 - Build

echo ============================================================
echo   OffNet 1.3.3
echo   C# / WinForms - Windows 10/11 Build
echo   https://github.com/zeittresor/OffNet
echo ============================================================
echo.

set "CSC="
if exist "%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not defined CSC if exist "%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not defined CSC for /f "delims=" %%I in ('where csc.exe 2^>nul') do if not defined CSC set "CSC=%%I"

if not defined CSC (
    echo [ERROR] csc.exe was not found.
    echo.
    echo OffNet intentionally ships without a bundled runtime.
    echo Enable/install .NET Framework 4.x developer tools if needed,
    echo or compile the source with Visual Studio / Build Tools.
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
 /reference:Microsoft.CSharp.dll ^
 /out:"OffNet.exe" ^
 "OffNet.cs"

if errorlevel 1 (
    echo.
    echo [ERROR] Build failed.
    pause
    exit /b 1
)

echo.
echo [OK] OffNet.exe built successfully.
for %%F in ("OffNet.exe") do echo [SIZE] %%~zF bytes
echo.
echo The EXE requests administrator privileges through its manifest.
echo.
exit /b 0
