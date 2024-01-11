using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

using System.Transactions;
using BepInEx.Logging;


namespace LethalArmors
{
    // We may be able to expand this later. I'll jot some notes in a text file somewhere.
    public enum ArmorType
    {
        Regular,
        Super
    }

    internal class ArmorPlate
    {
        private ArmorType plateType;
        private int? Health;
        private bool _isBroken = false;

        public ArmorPlate(ArmorType armorType)
        { 
            LethalArmorsPlugin.Log.LogInfo($"Creating new Armor Plate of type {armorType}");

            if (armorType == ArmorType.Regular)
            {
                Health = LethalArmorsPlugin.Config.armorValue;
                plateType = ArmorType.Regular;
                LethalArmorsPlugin.Log.LogInfo($"Created new Regular Plate with {Health} health");
            }
            else if (armorType == ArmorType.Super)
            {
                Health = null;
                plateType = ArmorType.Super;
                LethalArmorsPlugin.Log.LogInfo("Created new Super Plate");
            }
        }

        public int? GetHealth()
        {
            return Health;
        }
        public void TakeDamage(int damage)
        {
            if (plateType == ArmorType.Super)
            {
                LethalArmorsPlugin.Log.LogWarning("Trying to damage a Super Plate. Something has gone wrong");
                return;
            }

            Health = Health - damage;
            if (Health <= 0)
            {
                _isBroken = true;
            }
        }

        public bool IsBroken()
        {
            return _isBroken;
        }
    }


    internal class PlayerArmor
    {
        private ulong playerSteamId;
        public bool initialized = false;
        private Dictionary<ArmorType, List<ArmorPlate>> armorPlates = new Dictionary<ArmorType, List<ArmorPlate>>();

        public PlayerArmor(ulong steamID)
        {
            LethalArmorsPlugin.Log.LogInfo($"Creating new PlayerArmor for player with SteamId: {steamID}");

            playerSteamId = steamID;
            armorPlates[ArmorType.Regular] = new List<ArmorPlate>();
            armorPlates[ArmorType.Super] = new List<ArmorPlate>();
        }

        public ulong GetPlayerSteamId()
        {
            return playerSteamId;
        }

        public void SetPlayerSteamId(ulong steamId)
        {
            playerSteamId = steamId;
        }

        public void AddRegularArmorPlate()
        {
            ArmorPlate armorPlate = new ArmorPlate(ArmorType.Regular);
            armorPlates[ArmorType.Regular].Add(armorPlate);
            LethalArmorsPlugin.Log.LogInfo($"Added Regular Armor Plate for player with SteamId: {playerSteamId}.");
        }

        public void AddSuperArmorPlate()
        {
            ArmorPlate armorPlate = new ArmorPlate(ArmorType.Super);
            armorPlates[ArmorType.Super].Add(armorPlate);
            LethalArmorsPlugin.Log.LogInfo($"Added Super Armor Plate for player with SteamId: {playerSteamId}.");
        }

        public void BreakArmorPlate(ArmorType armorType)
        {
            if (armorPlates[armorType].Count > 0)
            {
                LethalArmorsPlugin.Log.LogDebug($"Removing armor plate of type {armorType} from SteamId: {playerSteamId}");
                armorPlates[armorType].RemoveAt(0);
            }

        }

        public int GetRegularPlateCount()
        {
            return armorPlates[ArmorType.Regular].Count;
        }

        public int GetSuperPlateCount()
        {
            return armorPlates[ArmorType.Super].Count;
        }

        public int TakeDamage(int damage)
        {

            // Iterate through all of the armor plates and take damage until the damage is gone or the armor is gone.
            // For potential future plateType expansion, we'll iterate through the types starting with regular when calculating damage mitigation.
            int playerRegularPlateCount = GetRegularPlateCount();
            while ((damage > 0) && (playerRegularPlateCount > 0))
            {
                int newDamage;

                // Check the first plate in the list and deal damage to it. Check if it's broken, and if so, remove it from the list.
                // This should repeat until all plates are broken or the damage is gone.
                ArmorPlate firstArmorPlate = armorPlates[ArmorType.Regular].First();
                newDamage = damage - (int)firstArmorPlate.GetHealth();
                firstArmorPlate.TakeDamage(damage);

                // If damage breaks the plate, remove it from the list and decrement the player's plate count.
                if (firstArmorPlate.IsBroken())
                {
                    BreakArmorPlate(ArmorType.Regular);
                    playerRegularPlateCount--;
                }

                // Update the damage value to the remaining damage
                damage = newDamage;
            }
            
            // If there is still damage remaining, we return it to process against health.
            // If the plates blocked all damage and did not break, we return 0.
            return damage > 0 ? damage : 0;
        }
    }

    // Roughly copying what Stoneman does in LethalProgressions, just tracking armor plating instead of progression.
    internal class LC_ARMOR : NetworkBehaviour
    {

        // Local Player Armor Values
        // This maps the player's Steam ID to their armor values.
        public static Dictionary<ulong, PlayerArmor> playerArmorsList;

        // Initialize the playerArmorsList
        public void Start()
        {
            LethalArmorsPlugin.Log.LogInfo("Entered LC_ARMOR.Start()");

            playerArmorsList = new();
            PlayerConnect_ServerRpc();
        }

        public void PlayerConnect_ServerRpc()
        {
            // Does this run when a player connects?
            LethalArmorsPlugin.Log.LogInfo("PlayerConnect_ServerRpc called");
            return;
        }

        internal static PlayerArmor GetPlayerArmors(ulong playerSteamId)
        {
            LethalArmorsPlugin.Log.LogInfo($"Getting armor for player with SteamId: {playerSteamId}");

            PlayerArmor playersArmor = playerArmorsList[playerSteamId];
            if (playersArmor.GetPlayerSteamId == null)
            {
                LethalArmorsPlugin.Log.LogError($"Failed to find an armor entry for player with SteamId: {playerSteamId}!");
                return null;
            } else
            {
                return playersArmor;
            }
        }

        // Called when a player connects to the server. This should check the debug values and add any initial armor plates
        // for the player accordingly.
        internal static void InitializeArmorsFromConfig(ulong playerSteamId)
        {
            // I can probably refactor how the playerArmorsList is initialized, but I'm just gonna get it working for now.
            if (playerArmorsList == null)
            {
                LethalArmorsPlugin.Log.LogInfo("Initializing playerArmorsList");
                playerArmorsList = new Dictionary<ulong, PlayerArmor>();
                playerArmorsList[playerSteamId] = new PlayerArmor(playerSteamId);
            }

            if (LethalArmorsPlugin.Config.startWithRegularPlates)
            {   
                LethalArmorsPlugin.Log.LogInfo($"Adding {LethalArmorsPlugin.Config.regularPlateStartCount} regular armor plates for player with SteamId: {playerSteamId}");
                for (int i = 0; i < LethalArmorsPlugin.Config.regularPlateStartCount; i++)
                {
                    GetPlayerArmors(playerSteamId).AddRegularArmorPlate();
                }
                
                // Verify the values after adding the plates so we know if something done goofed.
                LethalArmorsPlugin.Log.LogInfo($"Expected number of regular armor plates for [{playerSteamId} is [{LethalArmorsPlugin.Config.regularPlateStartCount}]");
                LethalArmorsPlugin.Log.LogInfo($"Actual number of regular armor plates for [{playerSteamId} is [{GetPlayerArmors(playerSteamId).GetRegularPlateCount()}]");
            }

            if (LethalArmorsPlugin.Config.startWithSuperArmorPlates)
            {
                LethalArmorsPlugin.Log.LogInfo($"Adding {LethalArmorsPlugin.Config.superArmorPlateStartCount} super armor plates for player with SteamId: {playerSteamId}");
                for (int i = 0; i < LethalArmorsPlugin.Config.superArmorPlateStartCount; i++)
                {
                    GetPlayerArmors(playerSteamId).AddSuperArmorPlate();
                }

                // Verify the values after adding the plates so we know if something done goofed.
                LethalArmorsPlugin.Log.LogInfo($"Expected number of super armor plates for [{playerSteamId} is [{LethalArmorsPlugin.Config.superArmorPlateStartCount}]");
                LethalArmorsPlugin.Log.LogInfo($"Actual number of super armor plates for [{playerSteamId} is [{GetPlayerArmors(playerSteamId).GetSuperPlateCount()}]");
            }
        }
    }
}
