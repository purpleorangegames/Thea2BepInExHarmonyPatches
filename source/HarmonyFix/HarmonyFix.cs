
using BepInEx;
using Harmony;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using CSharpCompiler;
using Thea2.Client;
using TheHoney;

namespace Thea2ScriptLoaderFix
{
    //Loading Harmony
    [BepInPlugin(GUID, Name, Version)]
    public class Thea2ScriptLoaderFix : BaseUnityPlugin
    {
        public const string GUID = "pog.thea2.bepinex.harmony.patches.harmonyfix";
        public const string Name = "Thea 2 BepInEx/Harmony Patches";
        public const string Version = "1.0";

        private void Start()
        {
            UnityEngine.Debug.Log(Name + " loaded (" + GUID + ")");

            var harmony = HarmonyInstance.Create(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
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
