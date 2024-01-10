using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TerminalApi;
using Unity.Collections;
using Unity.Netcode;

namespace LethalArmors
{
    [DataContract]
    public class Config
    {
        // Config Entry refs
        public ConfigEntry<int> armorValueConfig;
        public ConfigEntry<int> armorCostConfig;
        public ConfigEntry<bool> shieldFallsConfig;
        public ConfigEntry<bool> superArmorConfig;
        public ConfigEntry<int> superArmorCostConfig;
        public ConfigEntry<bool> startWithRegularPlatesConfig;
        public ConfigEntry<int> regularPlateStartCountConfig;
        public ConfigEntry<bool> startWithSuperArmorPlatesConfig;
        public ConfigEntry<int> superArmorPlateStartCountConfig;

        [DataMember] public int armorValue { get; private set;}
        [DataMember] public int armorCost { get; private set; } 
        [DataMember] public bool shieldFalls { get; private set; }
        [DataMember] public bool superArmor { get; private set; }
        [DataMember] public int superArmorCost { get; private set; }

        // DEBUG options for quick testing mostly
        [DataMember] public bool startWithRegularPlates { get; private set; }
        [DataMember] public int regularPlateStartCount { get; private set; }
        [DataMember] public bool startWithSuperArmorPlates { get; private set; }
        [DataMember] public int superArmorPlateStartCount { get; private set; }

        public Config(ConfigFile cfg)
        {
            // GENERAL OPTIONS
            armorValueConfig = cfg.Bind(
                "General",
                "Armor Value",
                25,
                "How much health each piece of armor should have"
            );

            armorCostConfig = cfg.Bind(
                "General",
                "Armor Piece Cost",
                75,
                "How much each piece of armor should cost."
            );

            shieldFallsConfig = cfg.Bind(
                "General",
                "Shield Fall Damage",
                false,
                "Whether or not armor should absorb fall damage."
            );

            superArmorConfig = cfg.Bind(
                "General",
                "Super Armor",
                false,
                "Whether or not the player should be able to buy Super Armor plates. Each plate prevents one instance of instant kill mechanic."
            );

            superArmorCostConfig = cfg.Bind(
                "General",
                "Super Armor Cost",
                100,
                "How much super armor should cost."
            );

            // DEBUG OPTIONS
            startWithRegularPlatesConfig = cfg.Bind(
                "Debug",
                "Start With Regular Plates",
                false,
                "Whether or not the player should start with regular armor plates."
            );

            regularPlateStartCountConfig = cfg.Bind(
                "Debug",
                "Regular Plate Start Count",
                4,
                "How many regular armor plates the player should start with."
            );

            startWithSuperArmorPlatesConfig = cfg.Bind(
                "Debug",
                "Start With Super Armor Plates",
                false,
                "Whether or not the player should start with super armor plates."
            );

            superArmorPlateStartCountConfig = cfg.Bind(
                "Debug",
                "Super Armor Plate Start Count",
                1,
                "How many super armor plates the player should start with."
            );

            // Assign values from ConfigEntries
            armorValue = armorValueConfig.Value;
            armorCost = armorCostConfig.Value;
            shieldFalls = shieldFallsConfig.Value;
            superArmor = superArmorConfig.Value;
            superArmorCost = superArmorCostConfig.Value;
            startWithRegularPlates = startWithRegularPlatesConfig.Value;
            regularPlateStartCount = regularPlateStartCountConfig.Value;
            startWithSuperArmorPlates = startWithSuperArmorPlatesConfig.Value;
            superArmorPlateStartCount = superArmorPlateStartCountConfig.Value;

        }

    }
}
