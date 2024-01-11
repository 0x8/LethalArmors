using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

// External Refs
using HarmonyLib;
using UnityEngine;

// FIXME: We may not need this at all? I'll try to figure it out.
namespace LethalArmors
{
    [HarmonyPatch]
    internal class LethalArmorNetworkManager
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        public static void Init()
        {
            //DEBUG
            LethalArmorsPlugin.Log.LogInfo("Entered LethalArmorNetworkManager.Init()");
            LethalArmors.LC_ARMOR PlayerArmors = new();
            PlayerArmors.Start();
        }

    }
}
