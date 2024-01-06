using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

// External Refs
using HarmonyLib;
using UnityEngine;

namespace LethalArmors
{
    [HarmonyPatch]
    internal class LethalArmorNetworkManager
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        public static void Init()
        {
            if (armorNetworkObject != null)
                return;

            armorNetworkObject = (GameObject)LethalArmorsPlugin.armorBundle.LoadAsset("LA_ArmorHandler");
            armorNetworkObject.AddComponent<LC_ARMOR>();

        }

        public static GameObject armorNetworkObject;
    }
}
