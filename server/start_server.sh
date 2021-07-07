#!/bin/bash

export extraFolders=""

for folder in resources-*
do
        export extraFolders="${extraFolders} --extra-res-folder ${folder}"
done

if [[ ! -z "$CDN_PATH" ]]
then
        ./altv-server --config config/server.cfg $extraFolders --host $ALT_HOST --port $ALT_PORT --justpack
        rm -r $CDN_PATH/*
        mv ./cdn_upload/* $CDN_PATH/
fi

./altv-server --config config/server.cfg --logfile logs/server.log $extraFolders