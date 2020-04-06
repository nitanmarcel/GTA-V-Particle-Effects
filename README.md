# GTA-V-Particle-Effects

This is a modding tool for developers to find and play particles effects. Press F9 to open the effects menu.

All the particles data are pulled over from <a href="https://github.com/DurtyFree/gta-v-data-dumps">this github repo.</a> and the mod user can recieve real-time updates when the data from the repo changes.

## Adding add/change particles effects:

- Edit the generated json file in "Grand Theft Auto V\scripts" following the file's example.

- If you know any particles that are not in the list just follow the link in the data source repository.


## How to update the database:

- If there are any updates you'll be notified in the game at next launch and all you have to do is open the menu (F9) and press enter.


## Source Code:

https://github.com/nitanmarcel/GTA-V-Particle-Effects


## Credits:

- @DurtyFree - The data I'm using is pulled from his repository: https://github.com/DurtyFree/gta-v-data-dumps

- github/LfxB - The menu base I'm using it's from his github repository: https://github.com/LfxB/SimpleUI


## Requirements:

ScriptHookVDotNet - https://github.com/crosire/scripthookvdotnet/releases


## Installation:

Copy PTFX.dll to the scripts folder in the game's directory.

## Changelog:

0.1:
- Initial Release

0.2:
- Update to SHVDN v3
- Change player visibility when the menu is open and play PTFX at player's position

0.3:

- Update ptfx database to a new up-to date one.
- Add a new menu.
- Add controls to change the particle size (right/left arrows).
- Try to play the particle looped if it failed as non looped particles.
- Code rewritten from scratch.