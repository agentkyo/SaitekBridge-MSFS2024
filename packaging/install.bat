@echo off
rem Registers SaitekBridge in MSFS 2024's exe.xml so it starts together with the simulator.
cd /d "%~dp0"
SaitekBridge.exe --install
pause
