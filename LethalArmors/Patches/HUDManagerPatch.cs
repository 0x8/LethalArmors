using LethalArmors;
using LethalArmors.Config;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalArmors.Patches
{
    [HarmonyPatch]
    internal class HUDManagerPatch
    {
        private static GameObject armorBar;
        private static TextMeshProUGUI armorBarText;
        private static float armorBarTime;
    }
}
