using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, CreateAssetMenu(menuName = "Abilities/Trait/Effect")]
public class TraitEffect : ScriptableObject
{
    public float Cooldown = 0;
    protected float m_lastInvokeTime = 0f;
    protected object m_owner;

    public void SetOwner(object owner)
    {
        m_owner = owner;
        m_lastInvokeTime = Time.time;
    }

    public virtual void Invoke(object caller, object target, object data = null)
    {
        
    }

    
    public bool ON_COOLDOWN(bool setInvokeTime = true)
    {
        if (Time.time - m_lastInvokeTime > Cooldown)
        {
            if(setInvokeTime)
            {
                m_lastInvokeTime = Time.time;
            }
            return false;
        }
        return true;
    }
}
