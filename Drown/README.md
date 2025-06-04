# Drown

## Overview

Play with or against one another in a race to open the Arena shelters. Attack creatures to earn points. Points buy revives, weapons, and ultimately... your escape. Disable spear hits to work together and pool your points. 

Creatures will flood you every 20 seconds. Will you control the battlefield just long enough to escape, or drown in its wake?

## Requirements
You must own Rain World on Steam, and have [Rain Meadow](https://steamcommunity.com/sharedfiles/filedetails/?id=3388224007) installed.

## Network Tuning
The default value for the max number of creatures alive in Remix for Drown is 10. This is a conservative number so users running potato PCs and weak internet don't explode. You and your friends / lobby will have different hardware / internet specs so adjust as necessary. 

If the code detects less than 10 creatures alive, it will call SpawnCreatures. This does not guarantee a maximum upper limit of 10 creatures, only a prevention in future SpawnCreature calls if the current count is < 10.

## Creature Cleanup
By default, on every third wave, dead, non-Slugcat creatures will be removed from the game to clear up the battlefield. Adjust the frequency of this cleanup in Remix. 

## Building Your Own Arena Game Mode
To mod your own arena mode for Rain Meadow's engine, check out the Arena API docs written [on my GitHub repo](https://github.com/6fears7/Arena-Online). You can view the base class on [RainMeadow's GitHub](https://github.com/henpemaz/Rain-Meadow/blob/main/Arena/ArenaOnlineGameModes/BaseGameMode.cs)

## Reporting Issues
Most of your problems are likely to come from interactions from other mods in the Rain Meadow game engine. Before opening an issue, disable most of your mods and try again. Open a GitHub issue [on my GitHub repo](https://github.com/6fears7/Arena-Online), attach your logs.

## Credits
May all the best parts of this mod go to the Lord Jesus Christ and all the worst parts be opened as a GitHub issue.

Thanks to the following playtesters for your feedback and support:
1. ax13
2. ChirpingWolf
3. Essam
4. FlareInferno
5. InfamousDog
6. Greewhick
