using System;
using System.Collections;
using System.Collections.Generic;
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

// Internal Refs from Lethal Company Directory DLLs.
using LethalArmors.Config;

namespace LethalArmors
{
    [BepInPlugin(modGUID, modName, modVersion)]
    internal class LethalArmorsPlugin : BaseUnityPlugin
    {
        // Mod metadate, GUID must be unique
        private const String modGUID = "kinou.LethalArmors";
        private const String modName = "Lethal Armors";
        private const String modVersion = "1.0.0";
        public static new ArmorConfig LethalArmorConfig { get; internal set; }
        public static AssetBundle armorBundle;  // FIXME: Do we even need this?

        // Logger and Instance information
        internal static ManualLogSource Log;
        public static LethalArmorsPlugin Instance { get; private set; }

        // Harmony instance for this particular mod
        private readonly Harmony harmony = new Harmony(modGUID);

        private void Awake()
        {
            // Verify whether the instance exists, create a new one if not.
            if (Instance == null)
            {
                Instance = this;
            }

            LethalArmorConfig = new(base.Config); 
            harmony.PatchAll(typeof(LethalArmorsPlugin));

            Log = Logger;
            Log.LogInfo("Lethal Armors loaded.");

            

        }

        // Handle the configuration file
        public void BindConfig<T>(string section, string key, T defaultValue, string description = "")
        {
            Config.Bind(
                section,
                key,
                defaultValue,
                description
            );
        }

        // Get the key and value for each config entry
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
