# Last Stand: Pixel Zombie Survival

A standalone, asset-free, top-down zombie survival game built with C# and MonoGame DesktopGL. It targets .NET 8 and creates its sprites and bitmap text at runtime, so no content pipeline or external game assets are required. The player character stays upright while the pistol or purchased rifle sprite rotates toward the mouse.

The arena is a small compound with five buildings and scattered cover. Their positions shift every new game, as do the four zombie entrances and their repairable plank barricades. The three Mystery Boxes also spawn at random reachable locations. Structures block player and zombie movement and stop bullets; zombies navigate around them, creating lanes and chokepoints instead of an open field. Restarting after game over generates a new layout.

Combat includes generated sound effects for firing, reloading, hits, kills, damage, pickups, purchases, and new waves, alongside animated muzzle flashes and weapon recoil, flying shell casings, impact and defeat particles, animated zombies with hit feedback and health bars, and brief screen feedback when firing or taking damage. Gunshots have distinct pistol, rifle, shotgun, sniper, grenade launcher, flamethrower, and heavy-weapon profiles, with randomized variations so repeated sounds are less repetitive. Sounds are synthesized at runtime, so no external audio files are required, and can be toggled in Settings. Zombies have a 14% chance to drop a floating power-up when killed. Collect drops by walking over them; uncollected drops disappear after 22 seconds.

## Requirements

- .NET 8 SDK (the project can also be built with a newer SDK that supports `net8.0`)
- Windows x64 for the portable Windows publish command below
- Internet access on the first build to restore MonoGame from NuGet

## Run from source

From the folder containing `ZombieSurvival.csproj`:

```powershell
dotnet restore
dotnet run
```

## Publish a portable Windows executable

The self-contained publish includes the .NET runtime. `PublishSingleFile` bundles managed application files into one executable; native MonoGame/OpenAL dependencies may be emitted alongside it and should remain in the publish folder.

```powershell
dotnet publish .\ZombieSurvival.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Run `.\bin\Release\net8.0\win-x64\publish\ZombieSurvival.exe`. Distribute the complete `publish` folder.

## Controls and gameplay

- **Start menu:** Choose Easy, Normal, or Hard, open Settings, or Exit
- - **Settings:** Toggle sound effects, screen shake, and borderless fullscreen (does not change your display resolution)
- **WASD:** Move
- **Mouse:** Aim; left-click to fire
- **R:** Reload the current weapon
- **Q:** Switch between weapons you own
- **E:** Use a Mystery Box
- **Enter** or **R:** Restart after game over
- **Esc:** Pause/resume during a game; return from Settings in menus
- **Exit:** Quit from the start or pause menu

Rounds begin with six zombies on Normal and add five zombies each round. Easy starts with fewer, weaker, slower zombies; Hard starts with more, tougher, faster zombies and quicker spawns. Zombies gain health, speed, and contact damage as rounds advance, and spawn more quickly, with speed and spawn rate capped at later rounds. Clear a wave to start the five-second intermission. Earn 10 points per hit and 50 per kill. The active Mystery Box costs 950 points and grants a random weapon; its rare grenade launcher and flamethrower have an 8% combined chance. Duplicate box weapons replenish their ammunition. Fire Sale reduces the cost to 10 points and activates all three box locations. Find the randomized weapon upgrade station and press E to upgrade your current gun up to three times; each upgrade increases damage, magazine and reserve capacity, and firing speed, and refills its ammo. Upgrades cost 1,500, 2,500, and 4,000 points. Press Q to cycle through collected weapons; each weapon keeps its own ammunition and upgrade level.

The arsenal includes a pistol, heavy pistol, rifle, shotgun, SMG, and sniper rifle. Shotguns fire a spread of pellets, sniper rounds pierce multiple zombies, the grenade launcher detonates for area damage, and the rare flamethrower fires short-range bursts of flame.

Power-ups include Max Ammo, Double Points, Insta-Kill, Nuke, Carpenter, Fire Sale, and Death Machine. Double Points, Insta-Kill, and Fire Sale last 30 seconds; Death Machine lasts up to 20 seconds or 300 shots. Carpenter restores the four window defenses, which lose condition when zombies reach them.
