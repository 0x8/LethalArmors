using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using LethalArmors;

// We'll try to patch the HUDManager to display the armor pieces to each player.
// TODO: Implement me!

namespace LethalArmors.Patches
{
    //[HarmonyPatch]
    internal class HUDManagerPatch
    {
        private static GameObject armorBar;
        private static TextMeshProUGUI armorBarText;
        private static float armorBarTime;
    }
}
