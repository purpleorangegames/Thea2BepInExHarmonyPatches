
using BepInEx;
using Harmony;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CSharpCompiler;
using Thea2.General;
using Thea2.Common;
using Thea2.Client;
using Thea2.Server;
using TheHoney;
using TMPro;
using DBDef;

namespace Thea2ScriptLoaderFix
{
    //Loading Harmony
    [BepInPlugin(GUID, Name, Version)]
    public class Thea2ScriptLoaderFix : BaseUnityPlugin
    {
        public const string GUID = "pog.thea2.bepinex.harmony.patches";
        public const string Name = "Thea 2 BepInEx/Harmony Patches";
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



    //Add new button remove chraracter locatization in english
    [HarmonyPatch(typeof(Localization))]
    [HarmonyPatch("GuaranteeLibrary")]
    static class GuaranteeLibrary_Patch
    {
        static void Postfix(Localization __instance)
        {
            Dictionary<string, string> keys = Traverse.Create<Localization>().Field("keys").GetValue<Dictionary<string, string>>();
            
            if (!(keys is null) && !keys.ContainsKey("DES_TOOLTIP_REMOVE_CHARACTER"))
            {
                keys["DES_TOOLTIP_REMOVE_CHARACTER"] = "Remove Character";
                keys["DES_TOOLTIP_REMOVE_CHRACTER_DES"] = "Let you remove one or more characters.";
            }
        }
    }

    //Adds correct tooltip text to remove character button
    [HarmonyPatch(typeof(TopLayer))]
    [HarmonyPatch("ShowSimpleInfo")]
    static class ShowSimpleInfo_Patch
    {
        static void Prefix(TopLayer __instance, RolloverSimpleTooltip source)
        {
            if (source.name == "Remove Character" && source.title != "DES_TOOLTIP_REMOVE_CHARACTER")
            {
                source.title = "DES_TOOLTIP_REMOVE_CHARACTER";
                source.description = "DES_TOOLTIP_REMOVE_CHRACTER_DES";
            }
        }
    }
       
    //Clones recycle button on Equip for Remove Character event
    [HarmonyPatch(typeof(Equip))]
    [HarmonyPatch("OnStart")]
    static class Equip_CreateButtonForRemoveCharacter_Patch
    {
        static void Postfix(Equip __instance)
        {
            GameObject recycleClone = GameObjectUtils.Instantiate(__instance.recycle.gameObject, __instance.recycle.transform.parent);
            recycleClone.transform.localScale = new Vector3(2f, 2f, 1f);
            recycleClone.transform.SetPositionAndRotation((__instance.recycle.transform.position + new Vector3(80f, 20f, 0f)), __instance.recycle.transform.rotation);
            recycleClone.name = "Remove Character";
        }
    }

    //Removes the temporary Remove Character event
    [HarmonyPatch(typeof(AdventureManager))]
    [HarmonyPatch("OnDestroy")]
    static class RemoveTemporaryEvent_RemoveCharacter_Patch
    {
        static void Prefix(AdventureManager __instance)
        {
            //Event was finished, checks if the temporary event was created and removes it to avoid it from being used by the game
            if (AdventureManager.eventOwners.ContainsKey(__instance))
            {
                AdvModule advModule = Array.Find<AdvModule>(AdventureLibrary.modules, (AdvModule o) => o.name == "Slavyan");
                AdvEvent advEvent = Array.FindAll<AdvEvent>(advModule.events, (AdvEvent o) => o.name == "Generic Remove Character")[0];
                if (advEvent!=null)
                { 
                    List<AdvEvent> list = new List<AdvEvent>(advModule.events);
                    if (advModule.events.Contains(advEvent))
                    {
                        list.Remove(advEvent);
                        advModule.events = list.ToArray();
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(Toggle))]
    [HarmonyPatch("OnPointerClick")]
    static class Equip_OnPointerClickButtonForRemoveCharacter_Patch
    {
        static bool Prefix(Equip __instance)
        {
            UnityEngine.Debug.Log("Equip_OnPointerClickButtonForRemoveCharacter_Patch --------------------- " + __instance.name);
            UnityEngine.Debug.Log("Equip_OnPointerClickButtonForRemoveCharacter_Patch --------------------- " + __instance.ToString());

            if (__instance.name== "Remove Character")
            {
                Dictionary<UIManager.Screen, List<ScreenBase>> openScreensByEnum = Traverse.Create(UIManager.Get()).Field("openScreensByEnum").GetValue<Dictionary<UIManager.Screen, List<ScreenBase>>>();

                //Closes Equip and Navbar to return to HUD
                if (UIManager.Get() != null && openScreensByEnum != null)
                {
                    foreach (KeyValuePair<UIManager.Screen, List<ScreenBase>> keyValuePair in openScreensByEnum)
                    {
                        if (keyValuePair.Value != null && keyValuePair.Value.Count >= 1)
                        {
                            List<ScreenBase> list = new List<ScreenBase>(keyValuePair.Value);
                            foreach (ScreenBase screenBase in list)
                            {
                                if (!(screenBase is TopLayer) && screenBase.name != "HUD(Clone)")
                                {
                                    UIManager.Close(screenBase);
                                }
                            }
                        }
                    }
                }

                //Clones "Giving up people" to simplify it and allow character to be removed without gaining items/rep with the slavyans
                AdvModule advModule = Array.Find<AdvModule>(AdventureLibrary.modules, (AdvModule o) => o.name == "Slavyan");
                AdvEvent[] advEventList = Array.FindAll<AdvEvent>(advModule.events, (AdvEvent o) => o.name == "Generic Remove Character");
                AdvEvent advEvent;
                if (advEventList.GetLength(0) == 0)
                {
                    advEvent = Array.FindAll<AdvEvent>(advModule.events, (AdvEvent o) => o.name == "Giving up people")[0].Clone();
                    advEvent.module = advModule;
                    advEvent.name = "Generic Remove Character";
                    Array.Resize<AdvEvent>(ref advModule.events, advModule.events.Length + 1);
                    advModule.events[advModule.events.Length - 1] = advEvent;
                    //advEvent.uniqueID = advModule.GetNextEventIndex(); //Might not be necessary to add ID since it will be removed later
                    UnityEngine.Debug.Log("Equip_ButtonClick_Patch --------------------- making new event " + advEvent.name);
                }
                else
                {
                    //In case the temporary event already exists, but I'm removing them after the event ends
                    advEvent = advEventList[0];
                }

                //Modifies the event to make it more generic and simple
                AdvNode[] nodes = advEvent.nodes;
                foreach (AdvNode node in nodes)
                {
                    if (node is NodeAdventure)
                    {
                        (node as NodeAdventure).story = "You approach your group to decide if anyone should leave.";
                    }
                    List<AdvOutput> outputs = node.outputs;
                    if (outputs != null)
                    {
                        foreach (AdvOutput output in outputs)
                        {
                            if (output.name == "Wish them well and leave.")
                            {
                                output.name = "Send someone away.";
                            }
                            if (output.name == "Actually, ask them to stay with you!")
                            {
                                output.name = "Nevermind.";
                            }
                            if (output.name == "Confirm")
                            {
                                output.targetID = 5;
                            }
                            if (output.name == "Cancel")
                            {
                                output.targetID = 5;
                            }
                        }
                    }
                }

                //Run the new temporary event
                ClientGroupData selectedGroup = GroupSelectionManager.Get().GetSelectedGroup();
                Thea2.Server.Group group = null;
                if (selectedGroup != null)
                {
                    group = EntityManager.Get<Thea2.Server.Group>(selectedGroup.GetID(), true);
                }
                AdventureManager adventureManager = AdventureManager.TriggerEvent(group.ownerID, advEvent, group, null, -1, true);

                return false;
            }

            return true;
        }
    }

    /*
    //Change the Recycle button on Equip screen into the Remove Character event
    [HarmonyPatch(typeof(Equip))]
    [HarmonyPatch("ButtonClick")]
    static class Equip_ButtonClick_Patch
    {
        static bool Prefix(Equip __instance, Selectable s)
        {
            Toggle recycle = Traverse.Create(__instance).Field("recycle").GetValue<Toggle>();
            //Recycle button was clicked in the Equip screen
            if (s == recycle)
            {
                Dictionary<UIManager.Screen, List<ScreenBase>> openScreensByEnum = Traverse.Create(UIManager.Get()).Field("openScreensByEnum").GetValue<Dictionary<UIManager.Screen, List<ScreenBase>>>();

                //Closes Equip and Navbar to return to HUD
                if (UIManager.Get() != null && openScreensByEnum != null)
                {
                    foreach (KeyValuePair<UIManager.Screen, List<ScreenBase>> keyValuePair in openScreensByEnum)
                    {
                        if (keyValuePair.Value != null && keyValuePair.Value.Count >= 1)
                        {
                            List<ScreenBase> list = new List<ScreenBase>(keyValuePair.Value);
                            foreach (ScreenBase screenBase in list)
                            {
                                if (!(screenBase is TopLayer) && screenBase.name != "HUD(Clone)")
                                {
                                    UIManager.Close(screenBase);
                                }
                            }
                        }
                    }
                }

                //Clones "Giving up people" to simplify it and allow character to be removed without gaining items/rep with the slavyans
                AdvModule advModule = Array.Find<AdvModule>(AdventureLibrary.modules, (AdvModule o) => o.name == "Slavyan");
                AdvEvent[] advEventList = Array.FindAll<AdvEvent>(advModule.events, (AdvEvent o) => o.name == "Generic Remove Character");
                AdvEvent advEvent;
                if (advEventList.GetLength(0) == 0)
                {
                    advEvent = Array.FindAll<AdvEvent>(advModule.events, (AdvEvent o) => o.name == "Giving up people")[0].Clone();
                    advEvent.module = advModule;
                    advEvent.name = "Generic Remove Character";
                    Array.Resize<AdvEvent>(ref advModule.events, advModule.events.Length + 1);
                    advModule.events[advModule.events.Length - 1] = advEvent;
                    //advEvent.uniqueID = advModule.GetNextEventIndex(); //Might not be necessary to add ID since it will be removed later
                    UnityEngine.Debug.Log("Equip_ButtonClick_Patch --------------------- making new event " + advEvent.name);
                }
                else
                {
                    //In case the temporary event already exists, but I'm removing them after the event ends
                    advEvent = advEventList[0];
                }

                //Modifies the event to make it more generic and simple
                AdvNode[] nodes = advEvent.nodes;
                foreach (AdvNode node in nodes)
                {
                    if (node is NodeAdventure)
                    { 
                        (node as NodeAdventure).story = "You approach your group to decide if anyone should leave.";
                    }
                    List<AdvOutput> outputs = node.outputs;
                    if (outputs != null)
                    {
                        foreach (AdvOutput output in outputs)
                        {
                            if (output.name == "Wish them well and leave.")
                            {
                                output.name = "Send someone away.";
                            }
                            if (output.name == "Actually, ask them to stay with you!")
                            {
                                output.name = "Nevermind.";
                            }
                            if (output.name == "Confirm")
                            {
                                output.targetID = 5;
                            }
                            if (output.name == "Cancel")
                            {
                                output.targetID = 5;
                            }
                        }
                    }
                }

                //Run the new temporary event
                ClientGroupData selectedGroup = GroupSelectionManager.Get().GetSelectedGroup();
                Thea2.Server.Group group = null;
                if (selectedGroup != null)
                {
                    group = EntityManager.Get<Thea2.Server.Group>(selectedGroup.GetID(), true);
                }
                AdventureManager adventureManager = AdventureManager.TriggerEvent(group.ownerID, advEvent, group, null, -1, true);

                return false;
            }

            return true;
        }
    }
    */


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
