# RaidBossChecker: "I work while you sleep"

[![GitHub release](https://img.shields.io/github/v/release/ibkucher/raidbosschecker?label=version&color=important&style=for-the-badge)](https://github.com/ibkucher/RaidBossChecker/releases/)
[![Github all releases](https://img.shields.io/github/downloads/ibkucher/raidbosschecker/total?style=for-the-badge&color=yellow)](https://github.com/ibkucher/RaidBossChecker/releases/)
[![VirusTotal](https://img.shields.io/badge/VirusTotal-done-success?style=for-the-badge)](https://www.virustotal.com/gui/file/8c42aded0037d6983ce33ee13d3a02acc43f1e2bc4e4ecba4d26454569d6ce78/detection)
[![Made with C# WPF](https://img.shields.io/badge/made%20with-c%23%20WPF-blue?style=for-the-badge)](https://github.com/ibkucher/RaidBossChecker/)\
[![Download](https://img.shields.io/badge/Download%20/%20СКАЧАТЬ-Raid%20Boss%20Checker%20.%20exe-blueviolet?style=for-the-badge&logo=download)](https://github.com/ibkucher/RaidBossChecker/releases/download/v0.1.1.4/RaidBossChecker.exe)

## Description
RaidBossChecker for Asterios.tm will let you know when a key raid boss is killed and notify by a sound alert.
- Choosing a specific boss raid that is required for the quest
- View the minimum and maximum raid boss respawn time 
- The sound will play until you wake up and stop it
- Change the sound and its volume
- Quick target chest (copies when you click on the chest icon)
- Copying information about the boss raid, when you click on it in the list
- You can select to show time in yours or Moscow zone
- There is a list of key and epic raid bosses

### Demo
<img src="http://g.recordit.co/D3BfyGzSAw.gif" width="450"/>

### Details
- Made with C# WPF
- Requires .NET Framework 4.7.2
- Optimized and multithreaded
- The program is fully bilingual (Russian and English versions, depending on the installed system language)
- The list of servers is always up to date because downloads from asterios.tm

## Getting Started
### Dependencies
- Visual Studio 2015 or higher
- .NET Desktop Development Components in Visual Studio
- .NET Framework 4.5 or higher

### Installing
1. Clone the repository. You can use the direct link in your browser: [RaidBossChecker-master.zip](https://github.com/ibkucher/RaidBossChecker/archive/master.zip)
2. Unzip the downloaded archive
3. In the folder open **RaidBossChecker.sln**
4. Open Package Manager Console. To do this, in the top menu: **Tools** -> **NuGet Package Manager** -> **Package Manager Console**
5. *(Optional) Reinstall the build packages. Enter into the console:* ```Update-Package -reinstall```
6. Unblock files. Enter into the console: ```Get-ChildItem *. * -Recurse | Unblock-File```
7. In Visual Studio, change **Debug** to **Release** in the top menu
8. In Solution Explorer (this is the default menu on the right), right-click on **"RaidBossChecker"**, then select **"Build"**
9. Ready file is in the folder **/RaidBossChecker/bin/Release/RaidBossChecker.exe**

## License
© Bohdan Kucher. Distributed under the [GNU AGPLv3 License](LICENSE.md). This software carries no warranty of any kind.
