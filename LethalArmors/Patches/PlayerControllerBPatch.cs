using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalArmors.Patches
{
    /* We need to patch PlayerControllerB.DamagePlayer() to override the damage values and ensure the armor gets hit first.
    
        The basic idea is to chunk up the damage that the player _would_ be taking (damageNumber, as provided to DamagePlayer()) and
        first subtract that value from the pool of armor. For each 25 damage dealt, the armor should drop by 1.

        One potential issue that I could see here is that we would need to track each armor piece as an entity to accurately track partial 
        damages under 25. This leaves us with two main options:
        
        1. Ignore weaker damage than 25 (we could call this "resistance to weak hits")
        2. Simply "break" an armor piece on damage of any kind.

        To be fair here, I'm not sure many damage sources in the whole game do less than 25 damage so it may not even be an issue.
    */
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        

    }
}
