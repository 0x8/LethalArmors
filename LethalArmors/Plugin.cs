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

namespace LethalArmors
{
    [BepInPlugin(modGUID, modName, modVersion)]
    internal class LethalArmorsPlugin : BaseUnityPlugin
    {
        // Mod metadate, GUID must be unique
        private const String modGUID = "kinou.LethalArmors";
        private const String modName = "Lethal Armors";
        private const String modVersion = "1.0.0";
        public static new Config Config { get; private set; }

        // Logger and Instance information
        internal static ManualLogSource Log;
        public static LethalArmorsPlugin Instance { get; private set; }

        // Harmony instance for this particular mod
        private readonly Harmony harmony = new Harmony(modGUID);

        private void Awake()
        {
            Log = Logger;

            // Verify whether the instance exists, create a new one if not.
            if (Instance == null)
            {
                Instance = this;
            }

            Log.LogInfo("Passed Instance Check, trying to generate config...");

            Config = new(base.Config);

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
        }

    }
}
