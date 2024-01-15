using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

// External Refs from Lethal Company Directory DLLs.
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

namespace LethalArmors
{
    [BepInPlugin(modGUID, modName, modVersion)]
    internal class LethalArmorsPlugin : BaseUnityPlugin
    {
        // Mod metadate, GUID must be unique
        private const String modGUID = "kinou.LethalArmors";
        private const String modName = "Lethal Armors";
        private const String modVersion = "1.0.0";
        public static AssetBundle armorBundle;

        // Logger and Instance information
        internal static ManualLogSource Log;
        public static LethalArmorsPlugin Instance { get; private set; }

        // Harmony instance for this particular mod
        private readonly Harmony harmony = new Harmony(modGUID);

        private void Awake()
        {

            Log = Logger;
            Log.LogDebug("Called Awake() in Plugin.cs");

            armorBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lethalarmors"));
            Log.LogInfo("Loaded AssetBundle.");

            NetcodePatcher();

            // Verify whether the instance exists, create a new one if not.
            if (Instance == null)
            {
                Instance = this;
            }

            try
            {
                harmony.PatchAll();
                Log.LogInfo("Lethal Armors loaded.");
            }
            catch (Exception e)
            {
                Log.LogError("Failed to load Lethal Armors");
                Log.LogError(e);
            }

            Log.LogInfo("Attemtping to load config.");
            ArmorConfig.InitConfig();
            Log.LogInfo("Config loaded.");
        }

        private static void NetcodePatcher()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }


        // Config Stuff
        // Will replace this with DataContracts once I actually figure out how to use them.
        public void BindConfig<T>(string section, string key, T defaultValue, string description = "")
        {
            Config.Bind(
                section, 
                key, 
                defaultValue,
                description
            );
        }

        public IDictionary<string, string> GetAllConfigEntries()
        {
            IDictionary<string, string> localConfig = Config.GetConfigEntries().ToDictionary(
                entry => entry.Definition.Key,
                entry => entry.GetSerializedValue()
            );
        
            return localConfig;
        }

    }
}
