using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;

using LethalArmors;
using LethalArmors.Armor;

/* 
    
    [The Basic Idea]:
        
    Chunk up the damage that the player _would_ be taking (damageNumber, as provided to DamagePlayer()) and
    first subtract that value from the pool of armor. For each 25 damage dealt, the armor should drop by 1.

    One potential issue that I could see here is that we would need to track each armor piece as an entity to accurately track partial 
    damages under 25. This leaves us with two main options:
    
    1. Ignore weaker damage than 25 (we could call this "resistance to weak hits")
    2. Simply "break" an armor piece on damage of any kind.

    To be fair here, I'm not sure many damage sources in the whole game do less than 25 damage so it may not even be an issue. 
    I'm looking into the AI instances to see if we ever take less than 30 damage from an enemy attack. If so I'll figure out a plan then.

    Because the DamagePlayer method also handles fall damage, we'll need to account for that as well. In the case of fall damage, we can
    ignore the armor and simply let the damage proceed. This may also be an opportunity to add another configurable option to allow armor
    pieces to soak fall damage.

    In the case of the NPCs that instantly kill the player, we can override the PlayerControllerB.KillPlayer method to first check for
    super armor, and evaluate based on that. If the player has super armor, we can simply ignore the kill command and instead set the player's
    super armor count to count--. If the player has no super armor, we can proceed with the kill command as normal.
    
*/

namespace LethalArmors.Patches
{

    [HarmonyPatch]
    internal class DamagePlayer_Patch 
    {

        // In order to modify the damage of an attack to first consider the shields, we need to patch the PlayerControllerB.DamagePlayer method.
        // In this case, we want to use a PREFIX patch and inherit all of the variables from the original method call in order to fully override the method.
        // This is because we want to modify the damage dealt to the player before the original method is called.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControllerB), "DamagePlayer")]
        public static bool DamagePlayerWithArmor(PlayerControllerB __instance, ref int damageNumber, ref bool fallDamage, ref CauseOfDeath causeOfDeath)
        {

            LethalArmorsPlugin.Log.LogInfo("Entered DamagePlayerWithArmor()");

            // Pull the damage number value into a local variable so we can modify it without messing with the original value.
            int damage = damageNumber;

            PlayerArmor playersArmor = LethalArmors.LethalArmorNetworkHandler.Instance.playerArmor;
            if(playersArmor == null)
            {
                // No armor, No damage reduction. Just return.
                LethalArmorsPlugin.Log.LogWarning($"No armor found for player with SteamId: {__instance.playerSteamId} . Did config syncing fail?");
                return true;
            }
            LethalArmorsPlugin.Log.LogInfo("Found player armor for instance.");
            LethalArmorsPlugin.Log.LogInfo($"Player has {playersArmor.GetRegularPlateCount()} regular plates and {playersArmor.GetSuperPlateCount()} super plates.");

            if (fallDamage)
            {
                LethalArmorsPlugin.Log.LogInfo("Determined the damage source is fall damage.");

                // Check if shielding fall damage is enabled in the config
                if (!bool.Parse(ArmorConfig.hostConfig["Shield Fall Damage"]))
                {
                    // If shielding fall damage is disabled, we can just return here and let the original method handle it.
                    LethalArmorsPlugin.Log.LogInfo("Shielding fall damage is disabled. Skipping damage reduction.");
                    return true;
                }

                LethalArmorsPlugin.Log.LogInfo("Shielding fall damage is enabled. Reducing damage by armor.");
            }

            LethalArmorsPlugin.Log.LogInfo("Calculating damage reduction from armor.");
            damage = playersArmor.TakeDamage(damage);

            // Evaluate if there is remaining damage, otherwise set to 0
            // NOTE: There's probably a much more elegant way to do this.
            // NOTE: Should I just skip the function if the remaining damage is 0? Will that cause issues?
            damageNumber = damage >= 0 ? damage : 0;
            return true;

        }

    }

    [HarmonyPatch]
    internal class KillPlayer_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
        public static bool KillPlayerWithArmor(PlayerControllerB __instance)
        {

            LethalArmorsPlugin.Log.LogInfo("Entered KillPlayerWithArmor()");

            if (!bool.Parse(ArmorConfig.hostConfig["Super Armor Enabled"]))
            {
                // If super armor is disabled, we can just return here and let the original method handle it.
                LethalArmorsPlugin.Log.LogInfo("Super Armor is disabled. Skipping super armor check.");
                return true;
            }

            PlayerArmor playersArmor = LethalArmors.LethalArmorNetworkHandler.Instance.playerArmor;
            if (playersArmor == null)
            {
                // No armor, No damage reduction. Just return.
                LethalArmorsPlugin.Log.LogWarning($"No armor found for player with SteamId: {__instance.playerSteamId} . Did config syncing fail?");
                return true;
            }

            if (playersArmor.GetSuperPlateCount() <= 0) {
                
                // If the player has no super armor, we can just return here and let the original method handle it.
                LethalArmorsPlugin.Log.LogInfo("Player has no super armor. Resuming kill command.");
                return true;
            }

            // If the player has super armor, we decrement it
            LethalArmorsPlugin.Log.LogInfo("Player has super armor. Skipping kill command.");
            playersArmor.BreakArmorPlate(ArmorType.Super);

            // returning false will skip the original method call
            return false;

        }
    }
    [HarmonyPatch]
    internal class connectClientToPlayerObject_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        public static void InitializeLocalPlayer(PlayerControllerB __instance)
        {
            LethalArmorsPlugin.Log.LogInfo("Entered InitializeLocalPlayer()");

            ulong playerSteamId = __instance.playerSteamId;

            // Initialize the player's armor values based on the config
            // When we add the RPCs to sync the config, we'll actually run the instantiation from said configs there.
            // As such, there isn't really a need to do anything here so...
            // TODO: Revisit this when I un-fuck the rest of the code base
        }
    }

}
