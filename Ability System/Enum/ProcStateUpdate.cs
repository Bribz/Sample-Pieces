using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Individual flags for each of these states if we want one specifically. Used for masking. 
/// </summary>
[System.Flags]
public enum ProcStateUpdate
{
    None            = 1 << 0,                                   //Unused. 
    Jump            = 1 << 1,                                   //Owner began a jump

    DealtDamage     = 1 << 2,                                   //Owner of attack dealt damage

    HitObject       = 1 << 3,                                   //Owner hit an Object in the scene
    HitEnemy        = 1 << 4 | HitObject,                       //Owner hit an enemy with a Killable component
    HitVital        = 1 << 5 | HitEnemy,                        //Owner hit a Vital part
    HitTagged       = 1 << 6 | HitEnemy,                        //Owner hit a part that was previously tagged
    HitTaggedVital  = HitTagged | HitVital,                     //Owner hit a Vital part that was previously tagged

    Killed          = 1 << 7,                                   //Owner of attack killed an entity
    
    ReloadStart     = 1 << 8,                                   //Owner began reloading
    ReloadEnd       = 1 << 9,                                   //Owner finished reloading
       
    PoppedTag       = 1 << 0xA,                                 //Owner popped a tag
    TagApplied      = 1 << 0xB,                                 //Owner applied a non-damage-type tag

    DodgeStart      = 1 << 0xC,                                 //Owner began a dodge
    DodgeEnd        = 1 << 0xD,                                 //Owner finished a dodge
    
    Breakable       = 1 << 0xE,                                 //Owner of attack broke a breakable part
    Dismember       = 1 << 0xF,                                 //Owner of attack broke a dismember part
    
    HealthLost      = 1 << 0x10,                                //Owner took damage
    HealthGained    = 1 << 0x11,                                //Owner gained health from healing

    CombatEntered   = 1 << 0x12,                                //Owner has made an attack recently.
    CombatExited    = 1 << 0x13,                                //Time since last attack has expired

    HitWhileInvulnerable = 1 << 0x14,                           //Damageable part was hit while it was invulnerable
    HitWhileInvincible = 1 << 0x15 | HitWhileInvulnerable,      //Damageable part was hit while it was invincible

    ShieldBroken = 1 << 0x16,                                   //Object shielding was broken
    ShieldApplied = 1 << 0x17,                                  //Object gained a new shield

    LowHealthState = 1 << 0x18,                                 //Entity health dropped below LowHealth percentage

    //Time Based
    SemiRegular     = 1 << 0xFFFC,                              //Proc occurs at an interval loop. Usually every other or every 4th update. Use this when possible.
    FixedUpdate     = 1 << 0xFFFD,                              //Proc occurs during every FixedUpdate loop
    Update          = 1 << 0xFFFE,                              //Proc occurs during every Update loop

    Everything      = 0xFFFF
}
