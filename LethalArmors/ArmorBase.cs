using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalArmors.Armor
{

    // We may be able to expand this later. I'll jot some notes in a text file somewhere.
    public enum ArmorType
    {
        Regular,
        Super
    }

    public class ArmorPlate
    {
        public ArmorType plateType;
        private int Health;
        private bool _isBroken = false;

        public ArmorPlate(ArmorType armorType)
        {
            LethalArmorsPlugin.Log.LogInfo($"Creating new Armor Plate of type {armorType}");

            if (armorType == ArmorType.Regular)
            {
                Health = int.Parse(ArmorConfig.hostConfig["Regular Armor Health"]);
                plateType = ArmorType.Regular;
                LethalArmorsPlugin.Log.LogInfo($"Created new Regular Plate with {Health} health");
            }
            else if (armorType == ArmorType.Super)
            {
                Health = -1;
                plateType = ArmorType.Super;
                LethalArmorsPlugin.Log.LogInfo("Created new Super Plate");
            }
        }

        public int GetHealth()
        {
            return Health;
        }
        public void TakeDamage(int damage)
        {
            if (plateType == ArmorType.Super)
            {
                LethalArmorsPlugin.Log.LogWarning("Trying to damage a Super Plate. Something has gone wrong ...");
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

    public class PlayerArmor
    {
        public bool initialized = false;
        private Dictionary<ArmorType, List<ArmorPlate>> armorPlates = new Dictionary<ArmorType, List<ArmorPlate>>();

        public PlayerArmor()
        {
            LethalArmorsPlugin.Log.LogInfo($"Creating new PlayerArmor.");
            armorPlates[ArmorType.Regular] = new List<ArmorPlate>();
            armorPlates[ArmorType.Super] = new List<ArmorPlate>();
        }

        public void AddRegularArmorPlate()
        {
            ArmorPlate armorPlate = new ArmorPlate(ArmorType.Regular);
            armorPlates[ArmorType.Regular].Add(armorPlate);
            LethalArmorsPlugin.Log.LogInfo($"Added Regular Armor Plate.");
        }

        public void AddSuperArmorPlate()
        {
            ArmorPlate armorPlate = new ArmorPlate(ArmorType.Super);
            armorPlates[ArmorType.Super].Add(armorPlate);
            LethalArmorsPlugin.Log.LogInfo($"Added Super Armor Plate.");
        }

        public void BreakArmorPlate(ArmorType armorType)
        {
            if (armorPlates[armorType].Count > 0)
            {
                LethalArmorsPlugin.Log.LogInfo($"Removing armor plate of type {armorType}.");
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
            LethalArmorsPlugin.Log.LogInfo("Entered TakeDamage() in ArmorBase.cs[114]");

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

        // Called when a player connects to the server. This should check the debug values and add any initial armor plates
        // for the player accordingly.
        public void InitializeArmorsFromConfig()
        {

            // Check whether the player starts with regular armor plates
            LethalArmorsPlugin.Log.LogInfo("Checking whether player starts with regular plates.");
            if (bool.Parse(ArmorConfig.hostConfig["Start With Regular Plates"]))
            {
                LethalArmorsPlugin.Log.LogInfo($"Start With Regular Plates is '{ArmorConfig.hostConfig["Start With Regular Plates"]}'");
                // If so, add the number of plates specified in the config
                int regularPlateCount = int.Parse(ArmorConfig.hostConfig["Regular Plate Start Count"]);
                LethalArmorsPlugin.Log.LogInfo($"Adding {regularPlateCount} regular plates.");
                for (int i = 0; i < regularPlateCount; i++)
                {
                    AddRegularArmorPlate();
                }
            }

            // Check whether the player starts with super armor plates
            LethalArmorsPlugin.Log.LogInfo("Checking whether player starts with super plates.");
            if (bool.Parse(ArmorConfig.hostConfig["Start With Super Armor Plates"]))
            {
                LethalArmorsPlugin.Log.LogInfo($"Start With Super Armor Plates is '{ArmorConfig.hostConfig["Start With Super Armor Plates"]}'");

                // If so, add the number of plates specified in the config
                int superPlateCount = int.Parse(ArmorConfig.hostConfig["Super Armor Plate Start Count"]);
                LethalArmorsPlugin.Log.LogInfo($"Adding {superPlateCount} super plates.");
                for (int i = 0; i < superPlateCount; i++)
                {
                    AddSuperArmorPlate();
                }
            }


        }

    }
}
