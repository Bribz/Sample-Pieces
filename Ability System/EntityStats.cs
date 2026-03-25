using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class EntityStats : MonoBehaviour
{
    public delegate void ProcStateDel(ProcStateUpdate update, object caller, object target = null, object data = null);
    public event ProcStateDel OnStateUpdate;

    public delegate void DeathEvent();
    public event DeathEvent OnDeathEvent;

    [Header("Entity Stats")]
    public float MaxHealth = 100f;
    public float Health = 100f;
    public float HealthRegen = 0f;

    [Range(0f, 1f)]
    public float LowHealthPercentage = .33f;

    public bool IsAlive { get { return Health > 0f; } }

    [Space()]
    public float MaxStamina = 100f;
    public float Stamina = 100f;
    public float StaminaRegen = 0f;

    public float HealthCostForStaminaMultiplier = 1f;
    public float HealthCostForEnergyMultiplier = 1f;

    [Header("Movement")]
    public EntitySquad CurrentSquad;

    public float MoveSpeed = 1f;
    public int MaxJumps = 1;
    public bool Grounded = true;

    [Header("Buffs")]
    public List<StatBuffData> CurrentBuffs = new List<StatBuffData>();
    public List<ArmorBuffData> ArmorBuffs = new List<ArmorBuffData>();
    public ArmorBuffData ArmorBuffTotal = new ArmorBuffData();

    [Header("Traits")]
    public TraitManager m_traitManager = new TraitManager();
    public TraitObject[] Debug_Traits;


    [Header("Combat State")]
    public CombatState CombatState = CombatState.None;

    private float m_InCombatTimer = 0f;
    [Tooltip("How long after the time resetting to wait before putting the player out of combat")]
    public float CombatTimer = 6f;
    public bool InCombat { get { return m_InCombatTimer > 0; } }

    public List<Damageable> DamageableComponents;

    public bool HasShielding 
    { 
        get 
        { 
            if (m_mainDamageable == null) 
                return false; 
            return m_mainDamageable.HasShielding; 
        } 
    }
    private Damageable m_mainDamageable;

    //Managers
    [HideInInspector] public AbilityManager m_abilityManager;
    [HideInInspector] public InventoryManager m_inventoryManager;

    protected virtual void Awake()
    {
        m_traitManager.Initialize(this);

        m_abilityManager = GetComponent<AbilityManager>();
        m_inventoryManager = GetComponent<InventoryManager>();

        m_mainDamageable = GetComponent<Damageable>();

        if (DamageableComponents == null)
        {
            DamageableComponents = new List<Damageable>();
        }

        foreach(var trait in Debug_Traits)
        {
            m_traitManager.AddTrait(trait);
        }
    }

    #region Squads
    public void EnterSquad(EntitySquad squad)
    {
        CurrentSquad = squad;
    }

    public void ExitSquad()
    {
        CurrentSquad = null;
    }
    #endregion

    public void DamageHealth(float amount)
    {
        if (!IsAlive)
        {
            return;
        }

        if(amount > 0)
        {
            InvokeStateUpdate(ProcStateUpdate.HealthLost, this, null, amount);
        }

        if(LowHealthPercentage > 0 && Health/MaxHealth > LowHealthPercentage && ((Health - amount) /MaxHealth) <= LowHealthPercentage)
        {
            InvokeStateUpdate(ProcStateUpdate.LowHealthState, this, null, LowHealthPercentage);
        }

        Health -= amount;
        Health = Mathf.Clamp(Health, 0f, MaxHealth);

        if(Health <= 0)
        {
            OnDeath();
        }
    }

    public void HealHealth(float amount)
    {
        if(Health <= 0f)
        {
            //Reviving
            foreach(var comp in DamageableComponents)
            {
                if(comp is Killable)
                {
                    ((Killable)comp).Revive();
                }
            }
        }

        if (amount > 0f)
        {
            InvokeStateUpdate(ProcStateUpdate.HealthGained, this, null, amount);
        }

        Health += amount;
        Health = Mathf.Clamp(Health, 0f, MaxHealth);
    }

    /// <summary>
    /// Check the current health to see if the parameter amount is less than the remaining Health of the entity.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public  bool CheckHealth(float amount)
    {
        if(Health > amount)
        {
            return true;
        }

        MessageSystem.QueueMsg(new InsufficientStatNotificationMessage(QueuedMessageType.UI_SINGLE, InsufficientStatNotificationMessage.UIStat.Health, this));

        return false;
    }

    public bool CheckStamina(float amount)
    {
        bool retVal = false;
        if(m_traitManager.HasStaticTrait(StaticTraitType.UseHealthAsStamina))
        {
            float excess = (Stamina - amount) * -1;
            
            retVal = (excess <= 0 || Health > (excess * HealthCostForStaminaMultiplier));

            if(!retVal)
            {
                MessageSystem.QueueMsg(new InsufficientStatNotificationMessage(QueuedMessageType.UI_SINGLE, InsufficientStatNotificationMessage.UIStat.Stamina, this));
            }

            return retVal;
        }

        retVal = (Stamina >= amount);

        if (!retVal)
        {
            MessageSystem.QueueMsg(new InsufficientStatNotificationMessage(QueuedMessageType.UI_SINGLE, InsufficientStatNotificationMessage.UIStat.Stamina, this));
        }

        return retVal;
    }

    public void DrainStamina(float amount)
    {
        float excess = (Stamina - amount) * -1;

        Stamina -= amount;
        Stamina = Mathf.Clamp(Stamina, 0f, MaxStamina);
        
        if(excess > 0 && m_traitManager.HasStaticTrait(StaticTraitType.UseHealthAsEnergy))
        {
            DamageHealth(excess * HealthCostForStaminaMultiplier);
        }
    }

    public void RecoverStamina(float amount)
    {
        Stamina += amount;
        Stamina = Mathf.Clamp(Stamina, 0f, MaxStamina);
    }

    #region Buff Management

    public void AddArmorBuff(ArmorBuffData buff)
    {
        if(buff == null)
        {
            return;
        }

        var obj = ArmorBuffs.FirstOrDefault(p => p.Slot == buff.Slot);
        if (obj != null)
        {
            ArmorBuffs.Remove(obj);
        }
        ArmorBuffs.Add(buff);
        RecalculateArmorBuffs();
    }

    public void RemoveArmorBuff(ArmorBuffData buff)
    {
        if(buff == null)
        {
            return;
        }

        var obj = ArmorBuffs.FirstOrDefault(p => p.Slot == buff.Slot);
        if (obj != null) 
        {
            ArmorBuffs.Remove(obj);
        }
        RecalculateArmorBuffs();
    }

    public void AddBuffData(StatBuffData buff)
    {
        if (!CurrentBuffs.Exists(p=>p.RelevantStatBonus == buff.RelevantStatBonus && p.Amount == buff.Amount))
        {
            CurrentBuffs.Add(buff);
            ApplyBuffStat(buff.RelevantStatBonus, buff.Amount);
        }
    }

    public void RemoveBuffData(StatBuffData buff)
    {
        if(CurrentBuffs.Exists(p=>p.RelevantStatBonus.Equals(buff.RelevantStatBonus) && p.Amount.Equals(buff.Amount)))
        {
            CurrentBuffs.Remove(buff);
            ApplyBuffStat(buff.RelevantStatBonus, -buff.Amount);
        }
    }

    public void RecalculateArmorBuffs()
    {
        ArmorBuffTotal.MaxHealth = 0f;
        ArmorBuffTotal.HealthRegen = 0f;
        ArmorBuffTotal.MaxStamina = 0f;
        ArmorBuffTotal.StaminaRegen = 0f;
        ArmorBuffTotal.MoveSpeed = 0f;

        foreach (var buff in ArmorBuffs)
        {
            ArmorBuffTotal.MaxHealth += buff.MaxHealth;
            ArmorBuffTotal.HealthRegen += buff.HealthRegen;
            ArmorBuffTotal.MaxStamina += buff.MaxStamina;
            ArmorBuffTotal.StaminaRegen += buff.StaminaRegen;
            ArmorBuffTotal.MoveSpeed += buff.MoveSpeed;
        }

        ArmorBuffTotal.MoveSpeed /= ArmorBuffs.Count;
    }

    public virtual void ApplyBuffStat(StatType type, float amount)
    {
        switch (type)
        {
            case StatType.HealthRegen:
                HealthRegen += amount;
                break;
            case StatType.StaminaGain:
                StaminaRegen += amount;
                break;
            case StatType.MoveSpeed:
                MoveSpeed += amount;
                break;
            case StatType.DodgeCoolDown:
                //TODO: If Dodging cooldown time is a thing
                break;

            default:
                break;
        }
    }

    public void ResetCombatTimer()
    {
        if(!InCombat)
        {
            InvokeStateUpdate(ProcStateUpdate.CombatEntered, this, null, null);
        }

        m_InCombatTimer = CombatTimer;
    }

    /// <summary>
    /// Handle the current InCombat state of the entity
    /// </summary>
    protected virtual void HandleInCombatState()
    {
        if (m_InCombatTimer > 0f)
        {
            m_InCombatTimer -= Time.deltaTime;

            if (m_InCombatTimer <= 0f)
            {
                m_InCombatTimer = 0f;
                InvokeStateUpdate(ProcStateUpdate.CombatExited, this, null, null);
            }
        }
    }

    protected virtual void HandleBuffDurations()
    {
        for(int i = CurrentBuffs.Count-1; i >=0; i--)
        {
            if(CurrentBuffs[i].Duration > 0)
            {
                CurrentBuffs[i].Timer += Time.deltaTime;
                if (CurrentBuffs[i].Timer > CurrentBuffs[i].Duration)
                {
                    RemoveBuffData(CurrentBuffs[i]);
                }
            }
        }
    }

    #endregion

    public void InvokeStateUpdate(ProcStateUpdate update, object caller, object target = null, object data = null)
    {
        OnStateUpdate?.Invoke(update, caller, target, data);
    }

    private float m_timeDelta = 0;
    protected void ProcStateUpdate_Interval()
    {
        InvokeStateUpdate(ProcStateUpdate.Update, this, this.transform);
        m_timeDelta += Time.deltaTime;
        if(m_timeDelta > 2f)
        {
            m_timeDelta -= 2f;
            InvokeStateUpdate(ProcStateUpdate.SemiRegular, this, this.transform);
        }
    }



    public float GetStatValue(StatType type)
    {
        switch (type)
        {
            //case StatType.DodgeCoolDown:
            //    return 
            case StatType.MoveSpeed:
                return MoveSpeed;
            case StatType.HealthRegen:
                return HealthRegen;
            case StatType.StaminaGain:
                return StaminaRegen;
            case StatType.MaxJump:
                return MaxJumps;
            default:
                return 0f;
        }
    }

    public void AddDamageableComponent(Damageable damageable)
    {
        DamageableComponents.Add(damageable);
    }
	
    /// <summary>
    /// Called by Killable class on death
    /// </summary>
    public virtual void OnDeath()
    {
        if (OnDeathEvent != null)
        {
            OnDeathEvent.Invoke();
        }

        if(m_abilityManager != null)
        {
            m_abilityManager.OnDeath();
        }
    }

#region Update_Loops
    public virtual void Update()
    {
        HandleBuffDurations();

        HandleInCombatState();

        ProcStateUpdate_Interval();
    }

    public virtual void FixedUpdate()
    {
        InvokeStateUpdate(ProcStateUpdate.FixedUpdate, this, this.transform);
    }
#endregion
	
}