# Project Zomboid Save Manager (Linux)

A lightweight save manager for Project Zomboid written in C# using Avalonia UI.
This tool allows you to create, restore, rename and manage backups of your Project Zomboid worlds easily.

This project was developed as a personal utility program, and I also received some help from my pet ChatGPT during the development process.

The goal is to provide a simple, open-source and cross-platform save management tool for Project Zomboid players.

## About this fork
This project is a modified Linux build of PZSaveManager.

The original PZSaveManager project was developed by Wirmaple73 and targets Windows systems.

Original project:
https://github.com/Wirmaple73/PZSaveManager

This repository provides a Linux-adapted version of the application and is not affiliated with the original author.

## Personal Notes and Info;
Thanks to my somewhat pragmatic (and occasionally sarcastic) personality, a few features that I personally consider unnecessary have been disabled in this version. These include auto-save, the update checker, save sounds, global hotkeys, thumbnail generation, and similar extras.

Some of my friends who love RGB (you know, the ones who spend their time in games like Fortnite, Valorant, and League of Legends) practically have an orgasm when they see bright, colorful lights everywhere. As for me, every time I see RGB, I feel like I'm about to have an epileptic seizure. I prefer functionality and realism over visual glitter.

I’m slightly embarrassed to admit this, but scenes like Reznov in CoD: BO1, Arthur’s ending in RDR2, Ghost and Roach in MW2, and Soap’s death in MW3 hit me so hard that I sometimes replay them… and end up hiding under my blanket and crying.

If there is enough interest, these features can easily be re-implemented. The codebase is open and structured so that anyone can add them back or extend the UI. Contributions and pull requests are welcome.

## Features

- List all local Project Zomboid worlds
- Automatically detect available backups
- Create new backups
- Restore backups
- Rename backups
- Delete backups
- View backup metadata
- Open backup folder directly
- Backup search/filter
- Backup size display
- Progress indicator during backup creation
- Log panel showing program activity


**Note**
This project was something I started enthusiastically, but later I realized that a similar tool had already been created: ZomboidVault.
If you are looking for a more polished UI or a more feature-complete solution, you may want to check out ZomboidVault instead.
This repository remains as a minimal Linux-adapted build of the original PZSaveManager.
😢

## Requirements

To build this project you need:

- .NET SDK 8.0 or newer
- Linux (tested on Arch Linux)
- Git

Check your .NET installation:

```bash
dotnet --version
```


## Clone the repository

```bash
git clone https://github.com/yourusername/pz-save-manager.git
cd pz-save-manager
```


## Build the project

First restore dependencies:

```bash
dotnet restore
```

Then compile:

```bash
dotnet build
```


## Create a Release Build

To produce a release binary:

```bash
dotnet publish -c Release -r linux-x64
```

The compiled program will appear inside:

```bash
bin/Release/net8.0/linux-x64/publish/
```

The executable file is:

```bash
PZSaveManager
```


## Run the program

Navigate to the publish directory:

```bash
cd bin/Release/net8.0/linux-x64/publish/
```

Make the file executable:

```bash
chmod +x PZSaveManager
```

Run it:

```bash
./PZSaveManager
```


## Optional: Install the program system-wide

You can move the executable to a common binary directory:

```bash
sudo cp PZSaveManager /usr/local/bin/
```

Then run it from anywhere:

```bash
PZSaveManager
```


## Create a desktop launcher

To add the program to your Linux application menu create this file:

```
~/.local/share/applications/pz-save-manager.desktop
```

Example content:

```ini
[Desktop Entry]
Name=PZ Save Manager
Exec=/usr/local/bin/PZSaveManager
Icon=folder
Type=Application
Categories=Game;
Terminal=false
```

After saving, refresh the desktop database:

```bash
update-desktop-database ~/.local/share/applications
```

Now the program should appear in your application launcher.


## Backup location

By default backups are stored in:

```
~/Zomboid/Backups
```

This can be changed in the configuration file:

```
PZSaveManager.dll.config
```


## Contributing

Pull requests are welcome.

Possible improvements:

- better UI polish
- backup thumbnails
- backup integrity check
- automatic backup scheduling
- AppImage / Flatpak packaging

## Note

Project Zomboid is developed by The Indie Stone.

This program is not affiliated with, endorsed by, or supported by The Indie Stone.
