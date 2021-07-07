#!/bin/bash
wget --no-cache -q -O ./altv-server https://cdn.altv.mp/server/$1/x64_linux/altv-server

wget --no-cache -q -O ./modules/js-module/libjs-module.so https://cdn.altv.mp/js-module/$1/x64_linux/modules/js-module/libjs-module.so
wget --no-cache -q -O ./modules/libcsharp-module.so https://cdn.altv.mp/coreclr-module/$1/x64_linux/modules/libcsharp-module.so
wget --no-cache -q -O ./AltV.Net.Host.dll https://cdn.altv.mp/coreclr-module/$1/x64_linux/AltV.Net.Host.dll
wget --no-cache -q -O ./AltV.Net.Host.runtimeconfig.json https://cdn.altv.mp/coreclr-module/$1/x64_linux/AltV.Net.Host.runtimeconfig.json
wget --no-cache -q -O ./data/vehmodels.bin http://cdn.altv.mp/server/$1/x64_linux/data/vehmodels.bin
wget --no-cache -q -O ./data/vehmods.bin http://cdn.altv.mp/server/$1/x64_linux/data/vehmods.bin

if [ $1 != "release" ]
then
	wget --no-cache -q -O ./modules/js-module/libnode.so.83 https://cdn.altv.mp/js-module/$1/x64_linux/modules/js-module/libnode.so.83
	wget --no-cache -q -O ./data/clothes.bin http://cdn.altv.mp/server/$1/x64_linux/data/clothes.bin
else
	wget --no-cache -q -O ./modules/js-module/libnode.so.72 https://cdn.altv.mp/js-module/$1/x64_linux/modules/js-module/libnode.so.72
fi
