#!/bin/bash
if [[ ! -z "$CDN_PATH" ]]
then
	./altv-server --config "config/server.cfg" --host $ALT_HOST --port $ALT_PORT --justpack
	rm -r $CDN_PATH/*
	mv ./cdn_upload/* $CDN_PATH/
fi

export CSHARP_MODULE_DISABLE_COLLECTIBLE=true
./altv-server --config "config/server.cfg" --logfile "logs/server.log"