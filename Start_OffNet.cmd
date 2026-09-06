@echo off
setlocal EnableExtensions
cd /d "%~dp0"

if not exist "OffNet.exe" (
    echo [INFO] OffNet.exe is not present yet - starting build.
    call "%~dp0Build_OffNet.cmd"
    if errorlevel 1 exit /b 1
)

start "" "%~dp0OffNet.exe"
exit /b 0
