
using BepInEx;
using Harmony;
using System.Reflection;
using Thea2.Client;

namespace Thea2ScriptLoaderFix
{
    //Loading Harmony
    [BepInPlugin(GUID, Name, Version)]
    public class Thea2ScriptLoaderFix : BaseUnityPlugin
    {
        public const string GUID = "pog.thea2.bepinex.harmony.patches.stophalloweenevent";
        public const string Name = "Stop Halloween Event";
        public const string Version = "1.0";

        private void Start()
        {
            UnityEngine.Debug.Log(Name + " loaded (" + GUID + ")");

            var harmony = HarmonyInstance.Create(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    //Patch to suppress Halloween and probably other Events
    [HarmonyPatch(typeof(FSMCoreGame))]
    [HarmonyPatch("TemporalEventMode")]
    static class FSMCoreGame_TemporalEventMode_Patch
    {
        static void Postfix(FSMCoreGame __instance)
        {
            UnityEngine.Debug.Log("TemporalEventMode will be force to be turned off.");
            AccessTools.Method(typeof(FSMCoreGame), "TemporalEventModeOff").Invoke(__instance, null);
        }
    }

}
