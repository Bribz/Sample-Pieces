using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base object for AI. You can implement this for other AI handling
/// </summary>
[RequireComponent(typeof(FighterController)), RequireComponent(typeof(AttackHandler))]
public class AI : IAI
{
    //Death Flag. Can be useful for things, but not necessarily needed. 
    //Let me know if this would be more useful public or whatever.

    //NOTE: THIS CAN TAKE A NULL PARAMETER. USE WITH CARE!!!
    internal delegate void DeathFlag(GameObject source);
    internal DeathFlag DeathEvent;
    //

    //Found Target Flag. Can be useful for things, but not necessarily needed. 
    //Let me know if this would be more useful public or whatever.
    internal delegate void FoundTargetFlag(GameObject target, bool newTarget);
    internal FoundTargetFlag FoundTargetEvent;
    
    [SerializeField]
    internal FighterController Target;
    internal FighterController fighterController;
    internal AttackHandler attackHandler;
    
    internal bool active = false;

    internal virtual void Awake()
    {
        fighterController = GetComponent<FighterController>();
        attackHandler = GetComponent<AttackHandler>();
    }

    public virtual void Init(FighterController enemy)
    {
        Target = enemy;
    }

    internal override void Upkeep()
    {
        
    }

    internal override void Seek()
    {
        if (!GameManager.instance.FightManager.FightStarted)
        {
            active = false;
        }
        else if (!Target)
        {
            active = false;
            return;
        }
        else
        {
            active = true;
        }
    }
   
    internal override void Move()
    {

    }

    internal override void Attack()
    {

    }
    
    internal virtual void Update()
    {
        //For now, assume tick speed is simply always maxxed out...
        //TODO: Change logic based on think speed.

        if (!active)
        {
            //Find a Target
            Seek();

            //Think
            Upkeep();
        }
        else
        {
            //Think
            Upkeep();

            //Determine Best Target
            Seek();

            if (Target && active)
            {
                if (GameManager.instance.FightManager.FightPaused == true) return;

                //Handle Movement
                Move();

                //Handle Attacks
                Attack();
            }
        }
    }
}
