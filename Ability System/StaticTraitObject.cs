using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, CreateAssetMenu(menuName = "Abilities/Trait/StaticTrait")]
public class StaticTraitObject : TraitObject
{
    public StaticTraitType TraitType = StaticTraitType.NONE;

}

[System.Serializable, System.Flags]
public enum StaticTraitType
{
    NONE                            = 0,
    UseHealthAsEnergy               = 1,
    ReflectProjectilesOnDodge       = 1 << 2,
    DealExtraDamageWhileShielded    = 1 << 3,
    FirstBullet_ExtraBreakDamage    = 1 << 4,
    FirstBullet_ExtraShieldDamage   = 1 << 5,
    FirstBullet_ExtraVitalDamage    = 1 << 6,
    UseHealthAsStamina              = 1 << 7,

    EVERYTHING                      = 0xFF
}
