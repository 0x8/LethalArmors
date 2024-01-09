using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LethalArmors.Config;


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

    // We need to patch the class for PlayerControllerB itself in order to track the shield values for each
    [HarmonyPatch]
    internal class PlayerControllerBPatch
    {


    }

    [HarmonyPatch]
    internal class DamagePlayerPatch 
    {

        // In order to modify the damage of an attack to first consider the shields, we need to patch the PlayerControllerB.DamagePlayer method.
        // In this case, we want to use a PREFIX patch and inherit all of the variables from the original method call in order to fully override the method.
        // This is because we want to modify the damage dealt to the player before the original method is called.
        [HarmonyPatch(typeof(PlayerControllerB), "DamagePlayer")]
        public static void DamagePlayerWithArmor(PlayerControllerB __instance, ref int damageNumber, ref bool fallDamage)
        {
            if (fallDamage)
            {
                // Check if shielding fall damage is enabled in the config
                if (LethalArmors.Config.ArmorConfig.shieldFalls.Value)
                {
                    // TODO: Implement shield fall damage logic
                } else
                {
                    // TODO: Implement normal fall damage logic
                }

                // TODO: FIXME: How do we exit here to ensure that the original method is not called?
            }

            // TODO: Implement armor damage logic

        }

    }

    [HarmonyPatch]
    internal class KillPlayerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
        public static void KillPlayerWithArmor(PlayerControllerB __instance, ref int damageNumber, ref bool fallDamage)
        {
            // TODO: Verify superarmor is enabled, short circuit if not. Implement SuperArmor logic
        }
    }
}
