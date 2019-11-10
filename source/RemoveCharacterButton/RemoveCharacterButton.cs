
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
        public const string GUID = "pog.thea2.bepinex.harmony.patches.removecharacterbutton";
        public const string Name = "Remove Character Button";
        public const string Version = "1.0";

        private void Start()
        {
            UnityEngine.Debug.Log(Name + " loaded (" + GUID + ")");

            var harmony = HarmonyInstance.Create(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    //Add new locatization for remove chraracter button in english
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
            if (source!=null && source.name == "Remove Character" && source.title != "DES_TOOLTIP_REMOVE_CHARACTER")
            {
                source.title = "DES_TOOLTIP_REMOVE_CHARACTER";
                source.description = "DES_TOOLTIP_REMOVE_CHRACTER_DES";
            }
        }
    }
       
    //Clone recycle button on Equip for Remove Character event
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
                AdvEvent[] advEventList = Array.FindAll<AdvEvent>(advModule.events, (AdvEvent o) => o.name == "Generic Remove Character");
                AdvEvent advEvent = null;
                if (advEventList.Length>0)
                {
                    advEvent = advEventList[0];
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

    //Goes back to HUD (World view), clone remove character event, simplifies it and then runs it
    [HarmonyPatch(typeof(Toggle))]
    [HarmonyPatch("OnPointerClick")]
    static class Equip_OnPointerClickButtonForRemoveCharacter_Patch
    {
        static bool Prefix(Equip __instance)
        {
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

}
