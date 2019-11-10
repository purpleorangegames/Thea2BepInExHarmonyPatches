
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
        public const string GUID = "pog.thea2.bepinex.harmony.patches.nocardcloning";
        public const string Name = "No card cloning";
        public const string Version = "1.0";

        private void Start()
        {
            UnityEngine.Debug.Log(Name + " loaded (" + GUID + ")");

            var harmony = HarmonyInstance.Create(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    
    [HarmonyPatch]
    public static class attempt_patch_1
    {
        static System.Reflection.MethodBase TargetMethod()
        {
            var methods = typeof(NetCard).GetMethods();
            foreach (var method in methods)
            {
                if (method.Name == "GetCastingCost")
                {
                    return method;
                }
            }
            throw new System.Exception("Tried to patch [whatever you're doing] but couldn't find it.");
        }

        static void Prefix(NetCard __instance)
        {
            if (__instance.UseCount > 0)
                __instance.UseCount = 10;
        }
    }
}
