using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Unity.Netcode;
using UnityEngine.UIElements;

using BepInEx.Logging;
using Newtonsoft.Json;

using LethalArmors.Armor;

namespace LethalArmors
{
    
    internal class LethalArmorNetworkHandler : NetworkBehaviour
    {
        public static LethalArmorNetworkHandler Instance { get; private set; }

        // Local to player, not network synced
        public PlayerArmor playerArmor;
        public bool Initialized = false;

        public void Start()
        {
            LethalArmorsPlugin.Log.LogInfo("Entered LC_ARMOR.Start()");
            PlayerConnect_ServerRpc();
        }

        //////////////////
        /// Instance Stuff

        public PlayerArmor GetPlayerArmors()
        {
            return playerArmor;
        }

        public override void OnNetworkSpawn()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            Instance = this;
            base.OnNetworkSpawn();
        }

        /////////////
        /// RPC Stuff

        [ServerRpc(RequireOwnership = false)]
        public void PlayerConnect_ServerRpc()
        {

            LethalArmorsPlugin.Log.LogDebug("PlayerConnect_ServerRpc called in Armor.cs");
            IDictionary<string, string> playerConfigs = LethalArmorsPlugin.Instance.GetAllConfigEntries();
            string serializedConfig = JsonConvert.SerializeObject(playerConfigs);
            
            // Debug logs for configs
            foreach(KeyValuePair<string, string> entry in playerConfigs)
            {
                LethalArmorsPlugin.Log.LogDebug($"Added {entry.Key}: {entry.Value} to playerConfigs.");
            }

            SendConfigsClientRpc(serializedConfig);
        }

        [ClientRpc]
        public void SendConfigsClientRpc(string serializedConfig)
        {
            LethalArmorsPlugin.Log.LogDebug("Called SendConfigsClientRpc() in Armor.cs.");

            IDictionary<string, string> playerConfigs = JsonConvert.DeserializeObject<IDictionary<string, string>>(serializedConfig);
            foreach (KeyValuePair<string, string> entry in playerConfigs)
            {
                ArmorConfig.hostConfig[entry.Key] = entry.Value;
                LethalArmorsPlugin.Log.LogInfo($"Added {entry.Key}: {entry.Value} to hostConfig.");
            }

            // Is this object Initialized Already?
            if (!Initialized)
            {
                LethalArmorsPlugin.Log.LogInfo("playerArmor is not initialized, initializing...");

                Initialized = true;
                LethalArmorNetworkManager.armorInstance = this;
                playerArmor = new PlayerArmor();
                playerArmor.InitializeArmorsFromConfig();
            }
        }


    }
}
