﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

using HarmonyLib;
using UnityEngine;

namespace LethalArmors
{
    [HarmonyPatch]
    internal class LethalArmorNetworkManager
    {
        public static GameObject armorNetworkObject;
        public static LethalArmorNetworkHandler armorInstance;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        public static void Init()
        {
            LethalArmorsPlugin.Log.LogDebug("Entered LethalArmorNetworkManager.Init()");
            
            // Check whether the network object is already instantiated
            if(armorNetworkObject != null)
                return;

            LethalArmorsPlugin.Log.LogDebug("armorNetworkObject is null, instantiating...");
            armorNetworkObject =  (GameObject)LethalArmorsPlugin.armorBundle.LoadAsset("LethalArmorHandler");
            armorNetworkObject.AddComponent<LethalArmorNetworkHandler>();
            NetworkManager.Singleton.AddNetworkPrefab(armorNetworkObject);
            
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        static void SpawnNetworkHnadler()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                var networkHandlerHost = UnityEngine.Object.Instantiate(armorNetworkObject, Vector3.zero, Quaternion.identity);
                networkHandlerHost.GetComponent<NetworkObject>().Spawn();
                armorInstance = networkHandlerHost.GetComponent<LethalArmorNetworkHandler>();
                LethalArmorsPlugin.Log.LogInfo("Inialized the LethalArmorNetworkHandler.");
            }
        }
    }
}
