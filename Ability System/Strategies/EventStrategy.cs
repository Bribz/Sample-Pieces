using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, CreateAssetMenu(menuName = "Abilities/Trait/Strategy/Event")]
public class EventStrategy : TraitProcStrategy
{
    protected bool m_eventInvoked = false;

    public override void UpdateStrategy(object caller, object target = null, object data = null)
    {
        m_eventInvoked = true;
    }

    public override bool CheckStrategy_GoalAchieved()
    {
        if(m_eventInvoked)
        {
            m_eventInvoked = false;
            return true;
        }
        return false;
    }
}