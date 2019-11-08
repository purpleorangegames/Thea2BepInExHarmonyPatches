# Thea 2 - BepInEx/Harmony Patches

Uses BepInEx and Harmony to make patches for Thea 2

### Current Patches

+ ScriptBundleLoader.ScriptBundle constructor as to skip assemblies with no location (allows Harmony to be imported)

+ FSMCoreGame.TemporalEventMode() to disable Halloween event (by running TemporalEventModeOff right after)

+ SMTM_PlayerTurn.Activated() to add two new autosave code, one for even turns and another for odd turns


### Default files:

/BepInEx/core/0Harmony.dll

/BepInEx/core/BepInEx.dll

/BepInEx/core/BepInEx.Harmony.dll

/BepInEx/core/BepInEx.Harmony.xml

/BepInEx/core/BepInEx.Preloader.dll

/BepInEx/core/Mono.Cecil.dll

/BepInEx/config/BepInEx.cfg

/BepInEx/LogOutput.log

/winhttp.dll

/doorstop_config.ini


### File with patches for Thea 2:

/BepInEx/plugins/Thea2Patch.dll




# Installation

Unzip the files in the Thea 2 game folder.

### Steam default location:

C:\Program Files (x86)\Steam\steamapps\common\Thea 2 The Shattering


It may be a bit different for you.



# Use

After copying the files just start the game normally.



# Credits

[BepInEx](https://github.com/BepInEx/BepInEx) 5.0.0.122

This isn't the latest version because it is not working with Harmony correctly

From BepInEx:

NeighTools/UnityDoorstop

pardeike/Harmony

0x0ade/MonoMod

jbevain/cecil


# License

MIT
