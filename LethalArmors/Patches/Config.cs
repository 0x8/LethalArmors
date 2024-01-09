using BepInEx.Configuration;
using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalArmors.Config
{
    internal class ArmorConfig
    {
        public static ConfigEntry<int> armorValue;
        public static ConfigEntry<int> armorCost;
        public static ConfigEntry<bool> shieldFalls;
        public static ConfigEntry<bool> superArmor;
        public static ConfigEntry<int> superArmorCost;

        public ArmorConfig(ConfigFile cfg)
        {
            armorValue = cfg.Bind(
                "General",
                "Armor Value",
                25,
                "How much health each piece of armor should have"
            );

            armorCost = cfg.Bind(
                "General",
                "Armor Piece Cost",
                75,
                "How much each piece of armor should cost."
            );

            shieldFalls = cfg.Bind(
                "General",
                "Shield Fall Damage",
                false,
                "Whether or not armor should absorb fall damage."
            );

            superArmor = cfg.Bind(
                "General",
                "Super Armor",
                false,
                "Whether or not the player should have a super armor mechanic."
            );

            superArmorCost = cfg.Bind(
                "General",
                "Super Armor Cost",
                100,
                "How much super armor should cost."
            );

        }
    }
}
