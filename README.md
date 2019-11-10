# Thea 2 - BepInEx/Harmony Patches

Uses BepInEx and Harmony to make patches for Thea 2

Each modification is found in a different DLL file so you can choose which to use.

### Current Patches

+ /BepInEx/plugins/HarmonyFix.dll : Fix to allow Harmony to work with Thea 2

+ /BepInEx/plugins/StopHalloweenEvent.dll : Disables Halloween event (by running TemporalEventModeOff right after)

+ /BepInEx/plugins/ForceHalloweenEvent.dll : Forces Halloween event (by turning on halloween flag)

+ /BepInEx/plugins/AutosaveEveryTurn.dll : Add two new autosave slots, one for even turns and another for odd turns

+ /BepInEx/plugins/RemoveCharacterButton.dll : Remove Character button added to the Equip screen

+ /BepInEx/plugins/NoCardCloning.dll : After using a card the cost for the second use is bigger than possible

+ /BepInEx/patchers/NoCardCloningPrePatcher.dll : A fix to be able to use Harmony on the method for checking card cost


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
