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
        public static IDictionary<string, string> hostConfig = new Dictionary<string, string>();
        public static void InitConfig()
        {
            LethalArmorsPlugin.Instance.BindConfig<int>(
                "General",
                "Armor Value",
                25,
                "How much health each piece of armor should have"
            );

            LethalArmorsPlugin.Instance.BindConfig<int>(
                "General",
                "Armor Piece Cost",
                75,
                "How much each piece of armor should cost."
            );


        }
    }
}
