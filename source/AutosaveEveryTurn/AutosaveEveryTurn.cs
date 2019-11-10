
using BepInEx;
using Harmony;
using System;
using System.Reflection;
using Thea2.General;
using Thea2.Common;
using Thea2.Client;
using Thea2.Server;

namespace Thea2ScriptLoaderFix
{
    //Loading Harmony
    [BepInPlugin(GUID, Name, Version)]
    public class Thea2ScriptLoaderFix : BaseUnityPlugin
    {
        public const string GUID = "pog.thea2.bepinex.harmony.patches.autosaveeveryturn";
        public const string Name = "Autosave Every Turn";
        public const string Version = "1.0";

        private void Start()
        {
            UnityEngine.Debug.Log(Name + " loaded (" + GUID + ")");

            var harmony = HarmonyInstance.Create(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    //Patch to autosave everyturn into two different save files
    [HarmonyPatch(typeof(SMTM_PlayerTurn))]
    [HarmonyPatch("Activated")]
    static class SMTM_PlayerTurn_AutosaveEveryTurn_Patch
    {
        static void Postfix(SMTM_PlayerTurn __instance)
        {
            bool skipFirstTurnEvent = Traverse.Create(__instance).Field("skipFirstTurnEvent").GetValue<bool>();
            int tunrIndex = Traverse.Create(__instance).Field("stateMachine").GetValue<SMTM>().tunrIndex;

            if (!skipFirstTurnEvent && tunrIndex % 2 == 0)
            {
                Exception ex = SaveManager.Get().SaveGame(GameInstance.Get().gameUniqueID, Localization.Get("UI_AUTOSAVE", true, null) + " Even");
                if (ex != null)
                {
                    PopupGeneral.OpenPopup(null, "UI_ERROR", ex.ToString(), "UI_OK", null, null, null, null, null);
                }
            }
            else
            {
                Exception ex2 = SaveManager.Get().SaveGame(GameInstance.Get().gameUniqueID, Localization.Get("UI_AUTOSAVE", true, null) + " Odd");
                if (ex2 != null)
                {
                    PopupGeneral.OpenPopup(null, "UI_ERROR", ex2.ToString(), "UI_OK", null, null, null, null, null);
                }
            }
        }
    }

}
