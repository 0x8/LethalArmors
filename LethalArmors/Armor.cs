using Microsoft.VisualBasic;
using System;
using System.Collections;
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

        /////////////
        /// RPC Stuff

        [ServerRpc(RequireOwnership = false)]
        public void PlayerConnect_ServerRpc()
        {
            // Does this run when a player connects?
            LethalArmorsPlugin.Log.LogDebug("PlayerConnect_ServerRpc called");
            IDictionary<string, string> playerConfigs = LethalArmorsPlugin.Instance.GetAllConfigEntries();
            string serializedConfig = JsonConvert.SerializeObject(playerConfigs);
            SendConfigsClientRpc(serializedConfig);
        }

        [ClientRpc]
        public void SendConfigsClientRpc(string serializedConfig)
        {
            IDictionary<string, string> playerConfigs = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> entry in playerConfigs)
            {
                ArmorConfig.hostConfig[entry.Key] = entry.Value;
                LethalArmorsPlugin.Log.LogInfo($"Added {entry.Key}: {entry.Value} to hostConfig");
            }

            // Is this object Initialized Already?
            if (!Initialized)
            {
                Initialized = true;
                LethalArmorNetworkManager.armorInstance = this;
                playerArmor = new PlayerArmor();
                playerArmor.InitializeArmorsFromConfig();
            }
        }


    }
}
