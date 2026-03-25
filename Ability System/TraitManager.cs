using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class TraitNode
{
    public TraitObject Trait;
    public object Owner;

    public TraitNode(TraitObject trait, object owner)
    {
        Trait = trait;
        Owner = owner;

        if (trait.ProcStrategy != null)
        {
            trait.ProcStrategy.SetOwner(Owner);
        }

        if (trait.Effect != null)
        {
            trait.Effect.SetOwner(Owner);
        }
    }
}

[System.Serializable]
public class StaticTraitNode
{
    public StaticTraitObject Trait;
    public object Owner;

    public StaticTraitNode(StaticTraitObject trait, object owner)
    {
        Trait = trait;
        Owner = owner;

        //trait.ProcStrategy.SetOwner(owner);
        //trait.Effect.SetOwner(owner);
    }
}

[System.Serializable]
public class TraitManager
{
    public EntityStats m_stats;
    public List<StaticTraitNode> m_staticTraits = new List<StaticTraitNode>();
    public List<TraitNode> m_traits = new List<TraitNode>();

    private StaticTraitType m_containedTraits = StaticTraitType.NONE;

    public bool HasStaticTrait(StaticTraitType trait)
    {
        if(trait == StaticTraitType.NONE)
        {
            return m_containedTraits == StaticTraitType.NONE;
        }

        return (m_containedTraits & trait) == trait;
    }

    public void Initialize(EntityStats stats)
    {
        m_stats = stats;
        stats.OnStateUpdate += OnProcStateUpdate;
    }

    public void AddTrait(TraitObject obj)
    {
        if(obj is StaticTraitObject)
        {
            AddTrait(new StaticTraitNode(obj as StaticTraitObject, m_stats));   
        }
        else
        {
            AddTrait(new TraitNode(obj, m_stats));
        }
    }

    private void AddTrait(TraitNode trait)
    {
        if (!m_traits.Contains(trait))
        {
            m_traits.Add(trait);
        }

    }

    private void AddTrait(StaticTraitNode trait)
    {
        if(!m_staticTraits.Contains(trait))
        {
            m_containedTraits |= trait.Trait.TraitType;
            m_staticTraits.Add(trait);
        }

    }

    public void ClearTraits()
    {
        m_traits.Clear();
        m_staticTraits.Clear();
    }
    
    private void RemoveTrait(TraitNode trait)
    {

    }

    private void RemoveTrait(StaticTraitNode trait)
    {

    }

    private void OnProcStateUpdate(ProcStateUpdate update, object caller, object target = null, object data = null)
    {
        foreach (var trait in m_traits)
        {
            if (trait == null || trait.Trait == null)
                continue;

            TraitProcStrategy strategy = trait.Trait.ProcStrategy;

            if(ShouldTraitUpdate(update, strategy.ProcType))
            {
                strategy.UpdateStrategy(caller, target, data);
            }

            if(strategy.CheckStrategy_GoalAchieved())
            {
                trait.Trait.Effect.Invoke(caller, target, data);
            }
        }
    }

    private bool ShouldTraitUpdate(ProcStateUpdate update, ProcStateUpdate traitRequirement)
    {
        if((update & traitRequirement) != 0)
        {
            return true;
        }
        return false;
    }

#region Add/Remove Traits

    public void AddTrait(TraitObject Trait, object Owner = null)
    {
        if (Trait is StaticTraitObject)
        {
            m_staticTraits.Add(new StaticTraitNode((StaticTraitObject)Trait, Owner));
        }
        else
        {
            m_traits.Add(new TraitNode(Trait, Owner));
        }
    }

    public void RemoveTrait(TraitObject trait)
    {
        if(trait is StaticTraitObject)
        {
            var node = m_staticTraits.Find(p => p.Trait.Equals(trait));

            if (node != null)
            {
                m_staticTraits.Remove(node);
            }
        }
        else
        {
            var node = m_traits.Find(p => p.Trait.Equals(trait));

            if (node != null)
            {
                m_traits.Remove(node);
            }
        }
    }

    public void RemoveTrait_Name(TraitObject trait)
    {
        if (trait is StaticTraitObject)
        {
            var node = m_staticTraits.Find(p => p.Trait.name.Equals(trait.name));

            if (node != null)
            {
                m_staticTraits.Remove(node);
            }
        }
        else
        {
            var node = m_traits.Find(p => p.Trait.name.Equals(trait.name));

            if (node != null)
            {
                m_traits.Remove(node);
            }
        }
    }

#endregion


    public void Update(float time)
    {
        
    }
}
