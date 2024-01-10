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

namespace LethalArmors
{
    [DataContract]
    public class Config : SyncedInstance<Config>
    {
        [DataMember] public int armorValue { get; private set;}
        [DataMember] public int armorCost { get; private set; } 
        [DataMember] public bool shieldFalls { get; private set; }
        [DataMember] public bool superArmor { get; private set; }
        [DataMember] public int superArmorCost { get; private set; }

        // DEBUG options for quick testing mostly
        [DataMember] public bool startWithRegularPlates { get; private set; }
        [DataMember] public int regularPlateStartCount { get; private set; }
        [DataMember] public bool startWithSuperArmorPlates { get; private set; }
        [DataMember] public int superArmorPlateStartCount { get; private set; }

        public Config(ConfigFile cfg)
        {
            InitInstance(this);

            // GENERAL OPTIONS
            armorValue = cfg.Bind(
                "General",
                "Armor Value",
                25,
                "How much health each piece of armor should have"
            ).Value;

            armorCost = cfg.Bind(
                "General",
                "Armor Piece Cost",
                75,
                "How much each piece of armor should cost."
            ).Value;

            shieldFalls = cfg.Bind(
                "General",
                "Shield Fall Damage",
                false,
                "Whether or not armor should absorb fall damage."
            ).Value;

            superArmor = cfg.Bind(
                "General",
                "Super Armor",
                false,
                "Whether or not the player should be able to buy Super Armor plates. Each plate prevents one instance of instant kill mechanic."
            ).Value;

            superArmorCost = cfg.Bind(
                "General",
                "Super Armor Cost",
                100,
                "How much super armor should cost."
            ).Value;

            // DEBUG OPTIONS
            startWithRegularPlates = cfg.Bind(
                "Debug",
                "Start With Regular Plates",
                false,
                "Whether or not the player should start with regular armor plates."
            ).Value;

            regularPlateStartCount = cfg.Bind(
                "Debug",
                "Regular Plate Start Count",
                4,
                "How many regular armor plates the player should start with."
            ).Value;

            startWithSuperArmorPlates = cfg.Bind(
                "Debug",
                "Start With Super Armor Plates",
                false,
                "Whether or not the player should start with super armor plates."
            ).Value;

            superArmorPlateStartCount = cfg.Bind(
                "Debug",
                "Super Armor Plate Start Count",
                1,
                "How many super armor plates the player should start with."
            ).Value;

        }

        public static void RequestSync()
        {
            if (!IsClient) return;

            using FastBufferWriter stream = new(IntSize, Allocator.Temp);
            SendMessage("LethalArmors_OnRequestConfigSync", 0uL, stream);
        }

        public static void OnRequestSync(ulong clientId, FastBufferReader _)
        {
            if (!IsHost) return;

            LethalArmorsPlugin.Log.LogInfo($"Config sync request received from client: {clientId}");

            byte[] array = SerializeToBytes(Instance);
            int value = array.Length;

            using FastBufferWriter stream = new(value + IntSize, Allocator.Temp);

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
            if (!reader.TryBeginRead(IntSize))
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
            Config.RevertSync();
        }

    }
}
