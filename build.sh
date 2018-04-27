#!/bin/sh

rm -rf ./artifacts
mkdir ./artifacts
cd src/NetworkMonitor
dotnet publish -c Release -o ../../artifacts
