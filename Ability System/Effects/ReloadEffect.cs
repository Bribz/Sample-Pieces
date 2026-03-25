using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, CreateAssetMenu(menuName = "Abilities/Trait/Effect/Reload")]
public class ReloadEffect : TraitEffect
{
    public override void Invoke(object caller, object target, object data = null)
    {
        if(ON_COOLDOWN())
        {
            return;
        }

        EntityStats stats = ((EntityStats)caller);
        WeaponManager wepManager = stats.transform.GetComponent<WeaponManager>();

        foreach (var wep in wepManager.WeaponBehaviors)
        {
            if (!wep.IsReloading)
            {
                wep.Reload(true);
            }
        }
    }
}
