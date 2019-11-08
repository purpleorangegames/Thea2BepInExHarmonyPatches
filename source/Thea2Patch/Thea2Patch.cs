
using BepInEx;
using Harmony;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using CSharpCompiler;
using Thea2.General;
using Thea2.Common;
using Thea2.Client;
using Thea2.Server;
using TheHoney;

namespace Thea2ScriptLoaderFix
{
    //Loading Harmony
    [BepInPlugin(GUID, Name, Version)]
    public class Thea2ScriptLoaderFix : BaseUnityPlugin
    {
        public const string GUID = "pog.thea2.scriptloader.fix";
        public const string Name = "Thea 2 ScriptLoader Fix";
        public const string Version = "0.1";

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


    //Patch to change the constructor of ScriptBundleLoader.ScriptBundle to be able to load assembly without location
    [HarmonyPatch]
    static class FSMCoreGame_OnEnter_Patch
    {
        static System.Reflection.MethodBase TargetMethod()
        {
            return typeof(ScriptBundleLoader.ScriptBundle).GetConstructor(new Type[] { typeof(ScriptBundleLoader), typeof(IEnumerable<string>) });
        }
        
        static bool Prefix(FSMCoreGame __instance, ScriptBundleLoader manager, IEnumerable<string> filePaths)
        {
            UnityEngine.Debug.Log("ScriptBundleLoader.ScriptBundle Patched true");

            AccessTools.Field(typeof(ScriptBundleLoader.ScriptBundle), "fileSystemWatchers").SetValue(__instance, new List<FileSystemWatcher>() );
            AccessTools.Field(typeof(ScriptBundleLoader.ScriptBundle), "instances").SetValue(__instance, new List<object>() );

            List<string> fullPaths = new List<string>();
            foreach (string value in filePaths)
            {
                fullPaths.Add(Path.GetFullPath(value));
            }

            AccessTools.Field(typeof(ScriptBundleLoader.ScriptBundle), "filePaths").SetValue(__instance, fullPaths);
            AccessTools.Field(typeof(ScriptBundleLoader.ScriptBundle), "manager").SetValue(__instance, manager);

            AppDomain currentDomain = AppDomain.CurrentDomain;
            Assembly[] assemblies = currentDomain.GetAssemblies();

            List<string> list = new List<string>();
            
            for (int i=0;i<assemblies.Count();i++)
            {
                string result;
                try
                {
                    result = assemblies[i].Location;
                }
                catch
                {
                    result = null;
                }                
                if (result != null)
                    list.Add(result);
            }

            Debug.Log("domains list size " + list.Count);
            
            AccessTools.Field(typeof(ScriptBundleLoader.ScriptBundle), "assemblyReferences").SetValue(__instance, list.ToArray());
            
            if (HoneyDebug.Get().loadingPerformance)
            {
                manager.logWriter.WriteLine("loading " + Environment.NewLine + string.Join(Environment.NewLine, filePaths.ToArray<string>()));
            }

            AccessTools.Method(typeof(ScriptBundleLoader.ScriptBundle), "CompileFiles").Invoke(__instance, null);
            AccessTools.Method(typeof(ScriptBundleLoader.ScriptBundle), "CreateFileWatchers").Invoke(__instance, null);
            AccessTools.Method(typeof(ScriptBundleLoader.ScriptBundle), "CreateNewInstances").Invoke(__instance, null);

            return false;
        }
    }

}
