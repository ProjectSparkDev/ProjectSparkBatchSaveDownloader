# Project Spark Batch Save Downloader 
a program to connect to xbox live and download your saves of the game of project spark

[![.NET Core Desktop](https://github.com/ProjectSparkDev/ProjectSparkBatchSaveDownloader/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/ProjectSparkDev/ProjectSparkBatchSaveDownloader/actions/workflows/dotnet-desktop.yml)

***

Files of Project Spark game are intercompatible between Xbox and PC versions. you can Download your saves from Xbox live title storage and copy them to your PC to use localy.

### requirements:
* Windows 10 or 11 (not tested on 11 but should work)
* Offical Xbox app from [Microsoft Store](https://www.microsoft.com/store/productId/9MV0B5HZVK9Z)
* A Microsoft Account with Project Spark logged in into xbox app

### known issues:
* kinect captures directly taken from xbox wont work in PC but if they are converted into brains or assemblies, they work
* sometimes, app gets confiused when there are multiple saved credentials with the same name. a guide on how to get the app working is being written while a fix for this issue is found.

## How to use:
1. Download the latest release from [github](https://github.com/ProjectSparkDev/ProjectSparkBatchSaveDownloader/releases)
2. install the xbox app from [Microsoft Store](https://www.microsoft.com/store/productId/9MV0B5HZVK9Z)
3. open the xbox app at least once and make sure you are logged in
4. Run the app and select a empty output folder
5. click download and wait for it to finish
6. copy all of the files into `%localappdata%\Packages\Microsoft.Dakota_8wekyb3d8bbwe\LocalState\Dakota`
7. Enjoy Sparking ✨✨

for Issues, bugs and questions Please use the [issues](https://github.com/ProjectSparkDev/ProjectSparkBatchSaveDownloader/issues) page or join me on discord at [Sparkdev](https://discord.gg/zGGpFp8fSm) server

***
based on [billynothingelse/xbcsmgr](https://github.com/billynothingelse/xbcsmgr)
with help of the OG [@tuxuser](https://github.com/tuxuser) and [ArcadeGamer1929](https://github.com/ArcadeGamer1929)
