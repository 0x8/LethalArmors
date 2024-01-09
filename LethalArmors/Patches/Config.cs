using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TerminalApi;
using Unity.Collections;
using Unity.Netcode;

namespace LethalArmors.Config
{
    [DataContract]
    public class ArmorConfig : SyncedInstance<ArmorConfig>
    {
        [DataMember] public ConfigEntry<int> armorValue;
        [DataMember] public ConfigEntry<int> armorCost;
        [DataMember] public ConfigEntry<bool> shieldFalls;
        [DataMember] public ConfigEntry<bool> superArmor;
        [DataMember] public ConfigEntry<int> superArmorCost;

        // DEBUG options for quick testing mostly
        [DataMember] public ConfigEntry<bool> startWithRegularPlates;
        [DataMember] public ConfigEntry<int> regularPlateStartCount;
        [DataMember] public ConfigEntry<bool> startWithSuperArmorPlates;
        [DataMember] public ConfigEntry<int> superArmorPlateStartCount;

        public ArmorConfig(ConfigFile cfg)
        {
            InitInstance(this);

            // GENERAL OPTIONS
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
                "Whether or not the player should be able to buy Super Armor plates. Each plate prevents one instance of instant kill mechanic."
            );

            superArmorCost = cfg.Bind(
                "General",
                "Super Armor Cost",
                100,
                "How much super armor should cost."
            );

            // DEBUG OPTIONS
            startWithRegularPlates = cfg.Bind(
                "Debug",
                "Start With Regular Plates",
                false,
                "Whether or not the player should start with regular armor plates."
            );

            regularPlateStartCount = cfg.Bind(
                "Debug",
                "Regular Plate Start Count",
                4,
                "How many regular armor plates the player should start with."
            );

            startWithSuperArmorPlates = cfg.Bind(
                "Debug",
                "Start With Super Armor Plates",
                false,
                "Whether or not the player should start with super armor plates."
            );

            superArmorPlateStartCount = cfg.Bind(
                "Debug",
                "Super Armor Plate Start Count",
                1,
                "How many super armor plates the player should start with."
            );

        }

        public static void RequestSync()
        {
            if (!IsClient) return;

            using FastBufferWriter stream = new(INT_SIZE, Allocator.Temp);
            SendMessage("LethalArmors_OnRequestConfigSync", 0uL, stream);
        }

        public static void OnRequestSync(ulong clientId, FastBufferReader _)
        {
            if (!IsHost) return;

            LethalArmorsPlugin.Log.LogInfo($"Config sync request received from client: {clientId}");

            byte[] array = SerializeToBytes(Instance);
            int value = array.Length;

            using FastBufferWriter stream = new(value + INT_SIZE, Allocator.Temp);

            try
            {
                stream.WriteValueSafe(in value, default);
                stream.WriteBytesSafe(array);

                SendMessage("LethalArmors_OnReceiveConfigSync", clientId, stream);
            }
            catch (Exception e)
            {
                LethalArmorsPlugin.Log.LogInfo($"Error occurred syncing config with client: {clientId}\n{e}");
            }
        }

        public static void OnReceiveSync(ulong _, FastBufferReader reader)
        {
            if (!reader.TryBeginRead(INT_SIZE))
            {
                LethalArmorsPlugin.Log.LogError("Config sync error: Could not begin reading buffer.");
                return;
            }

            reader.ReadValueSafe(out int val, default);
            if (!reader.TryBeginRead(val))
            {
                LethalArmorsPlugin.Log.LogError("Config sync error: Host could not sync.");
                return;
            }

            byte[] data = new byte[val];
            reader.ReadBytesSafe(ref data, val);

            SyncInstance(data);

            LethalArmorsPlugin.Log.LogInfo("Successfully synced config with host.");
        }


        // Sync on Player Connect
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "JoinLobby")]
        public static void InitializeLocalPlayer()
        {
            if (IsHost)
            {
                MessageManager.RegisterNamedMessageHandler("LethalArmors_OnRequestConfigSync", OnRequestSync);
                Synced = true;

                return;
            }

            Synced = false;
            MessageManager.RegisterNamedMessageHandler("LethalArmors_OnReceiveConfigSync", OnReceiveSync);
            RequestSync();
        }

        // Revert on Player Disconnect
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        public static void PlayerLeave()
        {
            ArmorConfig.RevertSync();
        }

    }
}
