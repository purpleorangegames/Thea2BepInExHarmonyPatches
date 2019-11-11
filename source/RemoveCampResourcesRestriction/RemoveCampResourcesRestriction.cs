
using BepInEx;
using Harmony;
using System.Reflection;
using Thea2.Server;
using Thea2.Common;
using System.Collections.Generic;
using TheHoney;

namespace Thea2ScriptLoaderFix
{
    //Loading Harmony
    [BepInPlugin(GUID, Name, Version)]
    public class Thea2ScriptLoaderFix : BaseUnityPlugin
    {
        public const string GUID = "pog.thea2.bepinex.harmony.patches.removecampresourcerestrictions";
        public const string Name = "Remove Camp Resources Restrictions";
        public const string Version = "1.0";

        private void Start()
        {
            UnityEngine.Debug.Log(Name + " loaded (" + GUID + ")");

            var harmony = HarmonyInstance.Create(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    //Patch to suppress Halloween and probably other Events
    [HarmonyPatch(typeof(Thea2.Server.Group))]
    [HarmonyPatch("UpdateGatheringRangeArea")]
    static class Group_UpdateGatheringRangeArea_Patch
    {
        static bool Prefix(Thea2.Server.Group __instance)
        {
            if (!__instance.settlement && !__instance.camping)
            {             
                if (__instance.gatheringRangeArea != null)
                {
                    __instance.gatheringRangeArea.Clear();
                }
                else
                {
                    __instance.gatheringRangeArea = new List<Vector3i>();
                }
                return false;
            }
            if (!__instance.settlement && (__instance.characters == null || __instance.characters.Count < 1))
            {
                if (__instance.gatheringRangeArea != null)
                {
                    __instance.gatheringRangeArea.Clear();
                }
                else
                {
                    __instance.gatheringRangeArea = new List<Vector3i>();
                }
                return false;
            }
            SPlayer player = GameInstance.Get().GetPlayer(__instance.ownerID);
            if (__instance.camping && player != null && player.attributes.Contains(DBDef.TAG.CAMP_GATHERING_RANGE))
            {
                __instance.gatheringRange = (FInt)2;
            }
            __instance.gatheringRangeArea = HexNeighbors.GetRange(__instance.Position, __instance.gatheringRange.ToInt());
            List<Thea2.Server.Group> playerGroups = GameInstance.Get().GetPlayerGroups();

            return false;
        }
    }

}
