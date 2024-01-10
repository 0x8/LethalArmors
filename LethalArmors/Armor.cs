using System;
using System.Collections.Generic;
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
            if (armorType == ArmorType.Regular)
            {
                Health = Config.Instance.armorValue;
                LethalArmorsPlugin.Log.LogInfo($"Created new Regular Plate with {Health} health");
            }
            else if (armorType == ArmorType.Super)
            {
                Health = null;
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
        ulong playerSteamId;
        Dictionary<ArmorType, List<ArmorPlate>> armorPlates = new Dictionary<ArmorType, List<ArmorPlate>>();

        public ulong GetPlayerSteamId()
        {
            return playerSteamId;
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
            // TODO: Initialize the playerArmorsList for all players
            PlayerConnect_ServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlayerConnect_ServerRpc()
        {
            return;
        }

        internal static PlayerArmor GetPlayerArmors(ulong playerSteamId)
        {
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
    }
}
