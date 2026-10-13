@echo off
rem Removes SaitekBridge from MSFS 2024's exe.xml and re-enables the Logitech plugin entry.
cd /d "%~dp0"
SaitekBridge.exe --uninstall
pause
