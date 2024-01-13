using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;

namespace LethalArmors
{
    public class ArmorConfig
    {
        // Holds local players config
        public static IDictionary<string, string> hostConfig = new Dictionary<string, string>();

        public static void InitConfig()
        {
            // GENERAL OPTIONS
            LethalArmorsPlugin.Instance.BindConfig<int>(
                "General",
                "Regular Armor Health",
                25,
                "How much health each piece of regular armor should have"
            );

            LethalArmorsPlugin.Instance.BindConfig<int>(
                "General",
                "Regular Armor Cost",
                75,
                "How much each piece of regular armor should cost."
            );

            LethalArmorsPlugin.Instance.BindConfig<bool>(
                "General",
                "Shield Fall Damage",
                false,
                "Whether or not armor should absorb fall damage."
            );

            LethalArmorsPlugin.Instance.BindConfig<bool>(
                "General",
                "Super Armor Enabled",
                false,
                "Whether or not the player should be able to buy Super Armor plates. Each plate prevents one instance of instant kill mechanic."
            );

            LethalArmorsPlugin.Instance.BindConfig<int>(
                "General",
                "Super Armor Cost",
                100,
                "How much super armor should cost."
            );

            // DEBUG OPTIONS
            LethalArmorsPlugin.Instance.BindConfig<bool>(
                "Debug",
                "Start With Regular Plates",
                false,
                "Whether or not the player should start with regular armor plates."
            );

            LethalArmorsPlugin.Instance.BindConfig<int>(
                "Debug",
                "Regular Plate Start Count",
                4,
                "How many regular armor plates the player should start with."
            );

            LethalArmorsPlugin.Instance.BindConfig<bool>(
                "Debug",
                "Start With Super Armor Plates",
                false,
                "Whether or not the player should start with super armor plates."
            );

            LethalArmorsPlugin.Instance.BindConfig<int>(
                "Debug",
                "Super Armor Plate Start Count",
                1,
                "How many super armor plates the player should start with."
            );

        }

    }
}
