
using BepInEx;
using Harmony;
using System.Reflection;
using Thea2.Common;

namespace Thea2ScriptLoaderFix
{
    //Loading Harmony
    [BepInPlugin(GUID, Name, Version)]
    public class Thea2ScriptLoaderFix : BaseUnityPlugin
    {
        public const string GUID = "pog.thea2.bepinex.harmony.patches.forcehalloweenevent";
        public const string Name = "Force Halloween Event";
        public const string Version = "1.0";

        private void Start()
        {
            UnityEngine.Debug.Log(Name + " loaded (" + GUID + ")");

            var harmony = HarmonyInstance.Create(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }


    //Set Halloween flag regardless of date
    [HarmonyPatch(typeof(NOGameDifficultySettings))]
    [HarmonyPatch("UpdateForNewGame")]
    static class NOGameDifficultySettings_SetFlag_Patch
    {
        static void Postfix(NOGameDifficultySettings __instance)
        {
            __instance.hallowin = true;
        }
    }

    //How to force event on and off for future refence in case it is removed in a future patch
    /*
    private void TemporalEventMode()
    {
        if (NOGameDifficultySettings.current.hallowin)
        {
            this.temporalEventMode = true;
            Debug.LogWarning("[TEMPORAL EVENTS] Halloween mode");
            Resource instanceFromDB = Globals.GetInstanceFromDB<Resource>(RES.VEGGIES);
            if (instanceFromDB != null)
            {
                instanceFromDB.terrainModel[0] = "VeggiesHalloween";
                instanceFromDB.descriptionInfo.iconName = "VeggiesHalloween";
            }
            Resource instanceFromDB2 = Globals.GetInstanceFromDB<Resource>(RES.FRUITS);
            if (instanceFromDB2 != null)
            {
                instanceFromDB2.terrainModel[0] = "FruitsHalloween";
                instanceFromDB2.descriptionInfo.iconName = "FruitsHalloween";
            }
            AdventureLibrary.UpdateForced(new List<string>
                {
                    "Haloween DLC"
                }, new List<string>
                {
                    "Death"
                }, null);
        }
    }

    private void TemporalEventModeOff()
    {
        if (this.temporalEventMode)
        {
            this.temporalEventMode = false;
            Debug.LogWarning("[TEMPORAL EVENTS] Halloween mode OFF");
            Resource instanceFromDB = Globals.GetInstanceFromDB<Resource>(RES.VEGGIES);
            if (instanceFromDB != null)
            {
                instanceFromDB.terrainModel[0] = "Veggies";
                instanceFromDB.descriptionInfo.iconName = "Veggies";
            }
            Resource instanceFromDB2 = Globals.GetInstanceFromDB<Resource>(RES.FRUITS);
            if (instanceFromDB2 != null)
            {
                instanceFromDB2.terrainModel[0] = "Fruits";
                instanceFromDB2.descriptionInfo.iconName = "Fruits";
            }
            AdventureLibrary.UpdateForced(null, null, new List<string>
                {
                    "Haloween DLC",
                    "Death"
                });
        }
    }
    */

}
