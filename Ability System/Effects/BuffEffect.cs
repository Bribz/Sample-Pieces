using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, CreateAssetMenu(menuName = "Abilities/Trait/Effect/Buff")]
public class BuffEffect : TraitEffect
{
    public List<BuffData> Buffs;

    public override void Invoke(object caller, object target, object data = null)
    {
        if(ON_COOLDOWN())
        {
            return;
        }

        EntityStats stats = ((EntityStats)caller);
        WeaponManager wepManager = stats.transform.GetComponent<WeaponManager>();

        foreach (var buff in Buffs)
        {
            if (buff is WeaponBuffData)
            {
                WeaponBuffData buffData = ScriptableObject.Instantiate(buff) as WeaponBuffData;
                
                wepManager.AddBuffData(buffData);
            }
            else if (buff is StatBuffData)
            {
                StatBuffData buffData = ScriptableObject.Instantiate(buff) as StatBuffData;
                stats.AddBuffData(buffData);
            }
        }
    }
}
