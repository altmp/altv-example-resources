@echo off

setlocal EnableDelayedExpansion
set launcherCommand=altv-server.exe --config "config/server.cfg" --logfile "logs/server.log"
for /d %%i in (resources-*) do (
    set "launcherCommand=!launcherCommand! --extra-res-folder %%i"
)
%launcherCommand%