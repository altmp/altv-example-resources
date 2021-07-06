@echo off

set CSHARP_MODULE_DISABLE_COLLECTIBLE=true
altv-server.exe --config "config/server.cfg" --logfile "logs/server.log"