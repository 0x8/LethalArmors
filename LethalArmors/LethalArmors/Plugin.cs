using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalArmors
{
    [BepInPlugin(modGUID, modName, modVersion)]
    internal class LethalArmorsPlugin : BaseUnityPlugin
    {
        // Mod metadate, GUID must be unique
        private const String modGUID = "kinou.LethalArmors";
        private const String modName = "Lethal Armors";
        private const String modVersion = "1.0.0";

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
            
            harmony.PatchAll(typeof(LethalArmorsPlugin));

            Log = Logger;
            Log.LogInfo("Lethal Armors loaded.");

        }

        // Handle the configuration file
        public void BindConfig<t>(string section, string key, t defaultValue, string description = "")
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
