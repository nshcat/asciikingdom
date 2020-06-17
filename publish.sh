#!/bin/sh

cd Game
dotnet publish -r win10-x64 -c Release /p:PublishTrimmed=true
dotnet publish -r linux-x64 -c Release /p:PublishTrimmed=true
cd ..

mkdir ./Publish


cp -r ./Game/bin/Release/netcoreapp3.1/linux-x64/publish ./Publish/Linux
cp -r ./Game/bin/Release/netcoreapp3.1/win10-x64/publish ./Publish/Windows

cd ./Publish

cd ./Windows
zip -r ./../asciikingdom_windows.zip .
cd ..

cd ./Linux
tar -czvf ./../asciikingdom_linux.tar.gz .
cd ..
