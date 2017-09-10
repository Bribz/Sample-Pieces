using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveDecision
{
    Forward = 0x0,
    Stay = 0x1,
    Backward = 0x1
}

public enum AttackDecision
{
    Parry = 0x0,
    Singular = 0x1,
    Combo = 0x2,
    ComboDefensive = 0x3
}

public class HardAI : AI
{
    private Vector3 facingDirection;
    private Vector3 moveDirection;
    private MoveDecision moveDecision;
    private AttackDecision atkDecision;
    private AttackRanges AttackRanges;
    [SerializeField]
    private float AttackInterval = .88f;
    private float AttackTimer = 0f;
    private float TargetDistance;
    private AIAttackCombo atkCombo;
    private Coroutine comboCoroutine;

    internal override void Awake()
    {
        base.Awake();

        AttackRanges = attackHandler.GetAttackRanges();
        atkDecision = AttackDecision.Singular;
        moveDecision = MoveDecision.Stay;
    }

    internal override void Update()
    {
        if (atkCombo == null ||(atkCombo != null && atkCombo.ComboDone()))
        {
            if(comboCoroutine != null)
            {
                StopCoroutine(comboCoroutine);
                comboCoroutine = null;
            }
            base.Update();
        }
        
    }

    internal override void Seek()
    {
        base.Seek();
    }

    internal override void Upkeep()
    {
        base.Upkeep();

        #region OLD_CODE
        /*
        if (fighterController.fighterStats.Health > fighterController.fighterStats.MaxHealth * .45f)
        {
            CPU_Strategy = CPU_Strategy.Chaotic;
        }
        else if (fighterController.fighterStats.Health > fighterController.fighterStats.MaxHealth * .25f)
        {
            CPU_Strategy = CPU_Strategy.Strategic;
        }
        else if (fighterController.fighterStats.Health > fighterController.fighterStats.MaxHealth * .1f)
        {
            CPU_Strategy = CPU_Strategy.Defensive;
        }
        else
        {
            CPU_Strategy = CPU_Strategy.Coward;
        }

        
        */
        #endregion

        if (!fighterController.dead)
        {
            facingDirection = (Target.transform.position - fighterController.transform.position).normalized;

            //Defensive Maneuvers
            if (fighterController.fighterStats.Health < 6)
            {
                moveDecision = MoveDecision.Forward;
            }
            else if (fighterController.fighterStats.Health < (Target.fighterStats.Health - 10))
            {
                moveDecision = MoveDecision.Backward;
            }
            //Aggressive Maneuvers
            else
            {
                moveDecision = MoveDecision.Forward;
            }
        }
        else
        {
            active = false;
        }
    }

    internal override void Move()
    {
        
        if (fighterController.running)
            return;

        base.Move();

        TargetDistance = Vector3.Distance(Target.transform.position, fighterController.transform.position);

        if (moveDecision == MoveDecision.Forward)
        {
            if (TargetDistance > AttackRanges.LongestRangeDist)
            {
                if (fighterController.crouched)
                {
                    fighterController.crouched = false;
                }
                moveDirection = facingDirection;

                if (TargetDistance > AttackRanges.LongestRangeDist + 1f)
                {
                    if (fighterController.canDodge == false)
                    {
                        fighterController.previousDirection = new Vector2(moveDirection.x, 0).normalized;
                        fighterController.DoDodge(true);
                    }
                }
            }
            else
            {
                fighterController.running = false;
                moveDirection = Vector3.zero;

                if (TargetDistance < 0.9)
                {
                    if (fighterController.crouched)
                    {
                        fighterController.crouched = false;
                    }
                    moveDirection = -facingDirection;
                }
            }
        }
        else if (moveDecision == MoveDecision.Backward)
        {
            if (fighterController.crouched)
            {
                fighterController.crouched = false;
            }

            moveDirection = -facingDirection;
            if (TargetDistance < AttackRanges.LongestRangeDist - .3f)
            {
                if (fighterController.canDodge == false)
                {
                    fighterController.previousDirection = new Vector2(moveDirection.x, 0).normalized;
                    fighterController.DoDodge(true);
                }
            }
            else if(TargetDistance > AttackRanges.LongestRangeDist+1.2f)
            {
                moveDirection = facingDirection;
            }
            else
            {
                moveDirection = Vector3.zero;
            }
            
            fighterController.running = false;
            
        }

        #region OLD_CODE
        /*
        if (CPU_Strategy != CPU_Strategy.Chaotic && CPU_Strategy != CPU_Strategy.Coward)
        {
            if (TargetDistance > 3)
            {
                if (fighterController.crouched)
                {
                    fighterController.crouched = false;
                }
                moveDirection = facingDirection;

                if (TargetDistance > 4)
                {
                    if (fighterController.canDodge == false)
                    {
                        fighterController.previousDirection = new Vector2(-facingDirection.x, 0).normalized;
                        fighterController.DoDodge(true);
                    }
                }
            }
            else
            {
                if (TargetDistance < 1.8)
                {
                    fighterController.running = false;
                    moveDirection = Vector3.zero;

                    if (Target.crouched)
                    {
                        fighterController.crouched = true;
                    }
                }

                else if (TargetDistance < 1.1)
                {
                    if (fighterController.crouched)
                    {
                        fighterController.crouched = false;
                    }
                    moveDirection = -facingDirection;
                }
            }
        }
        else if (CPU_Strategy == CPU_Strategy.Chaotic)
        {
            if (TargetDistance > 2)
            {
                if (fighterController.crouched)
                {
                    fighterController.crouched = false;
                }
                moveDirection = facingDirection;

                if (TargetDistance > 3)
                {
                    if (fighterController.canDodge == false)
                    {
                        fighterController.previousDirection = new Vector2(moveDirection.x, 0).normalized;
                        fighterController.DoDodge(true);
                    }
                }
            }
            else
            {
                fighterController.running = false;
                moveDirection = Vector3.zero;

                if (TargetDistance < 0.9)
                {
                    if (fighterController.crouched)
                    {
                        fighterController.crouched = false;
                    }
                    moveDirection = -facingDirection;
                }
            }
        }
        else if (CPU_Strategy == CPU_Strategy.Coward)
        {
            moveDirection = Vector3.zero;

            if (TargetDistance < 3)
            {
                if (fighterController.crouched)
                {
                    fighterController.crouched = false;
                }
                moveDirection = -facingDirection;
            }
        }
        
        */
        #endregion

        fighterController.InputDirection = moveDirection;
    }

    internal override void Attack()
    {
        base.Attack();

        if (fighterController.CC != CrowdControl.None)
        {
            return;
        }

        if (fighterController.dead)
        {
            return;
        }

        if (AttackTimer <= 0f)
        {
            HandleAttackLogic();
        }
        else if (AttackTimer > 0f && TargetDistance < 2f && fighterController.canDodge)
        { 
            fighterController.previousDirection = new Vector2(-facingDirection.x, 0).normalized;
            fighterController.DoDodge(true);
            AttackTimer -= Time.deltaTime;
        }
        else
        {
            AttackTimer -= Time.deltaTime;
        }
        
    }

    internal void HandleAttackLogic()
    {
        if (Target.CC == CrowdControl.Attacking && fighterController.CC == CrowdControl.None)
        {
            atkDecision = AttackDecision.Parry;
        }
        else if(Target.CC == CrowdControl.Stun)
        {
            atkDecision = AttackDecision.Combo;
        }
        else if(moveDecision == MoveDecision.Backward)
        {
            if (TargetDistance > AttackRanges.LongestRangeDist)
            {
                atkDecision = AttackDecision.ComboDefensive;
            }
            else
            atkDecision = AttackDecision.Singular;
        }
        else
        {
            if (TargetDistance < AttackRanges.LongestRangeDist)
            {
                if (Target.crouched)
                {
                    atkDecision = AttackDecision.Singular;
                }
                else
                {
                    atkDecision = AttackDecision.Combo;
                }
            }
            else
                atkDecision = AttackDecision.Combo;
        }

        switch (atkDecision)
        {
            case AttackDecision.Parry:
                {
                    attackHandler.Attack(nAttackType.Parry, -1);
                    AttackTimer = AttackInterval;
                    break;
                }
            case AttackDecision.Singular:
                {
                    AttackRanges.GetBestAttackType(Target.crouched?-1:0, TargetDistance);
                    AttackTimer = AttackInterval;
                    break;
                }
            case AttackDecision.Combo:
            case AttackDecision.ComboDefensive:
                {
                    if (atkCombo != null)
                    {
                        if (atkCombo.ComboDone())
                        {
                            CreateCombo();
                        }
                    }
                    else
                    {
                        CreateCombo();
                    }
                    comboCoroutine = StartCoroutine(ExecuteCombo());
                    break;
                }
        }

        
        #region OLD_CODE
        /*
        if (Target.CC == CrowdControl.Attacking && fighterController.CC == CrowdControl.None)
        {
            attackHandler.Attack(nAttackType.Parry, -1);
        }
        else
        {
            if (CPU_Strategy == CPU_Strategy.Chaotic)
            {
                if (Target.crouched)
                {
                    attackHandler.Attack(nAttackType.Strong, -1);
                    AttackTimer = AttackInterval;
                }
                else
                {
                    attackHandler.Attack(nAttackType.Fierce, 0);
                    AttackTimer = AttackInterval;
                }
            }
            else if (CPU_Strategy == CPU_Strategy.Strategic)
            {
                if (TargetDistance < .5)
                {
                    if (fighterController.crouched)
                    {
                        attackHandler.Attack(nAttackType.Parry, -1);
                        AttackTimer = AttackInterval;
                    }
                    else
                    {
                        attackHandler.Attack(nAttackType.Parry, 0);
                        AttackTimer = AttackInterval;
                    }
                }
                else
                {
                    if (fighterController.crouched)
                    {
                        attackHandler.Attack(nAttackType.Strong, -1);
                        AttackTimer = AttackInterval;
                    }
                    else
                    {
                        attackHandler.Attack(nAttackType.Fierce, 0);
                        AttackTimer = AttackInterval;
                    }
                }
            }
            else if (CPU_Strategy == CPU_Strategy.Defensive)
            {
                if (TargetDistance < .85f)
                {
                    if (fighterController.crouched)
                    {
                        attackHandler.Attack(nAttackType.Parry, -1);
                        AttackTimer = AttackInterval;
                    }
                    else
                    {
                        attackHandler.Attack((nAttackType)Random.Range((int)0, 2), 0);
                        AttackTimer = AttackInterval;
                    }
                }
                else
                {
                    if (fighterController.crouched)
                    {
                        attackHandler.Attack((nAttackType)Random.Range((int)0, 2), -1);
                        AttackTimer = AttackInterval;
                    }
                    else
                    {
                        attackHandler.Attack((nAttackType)Random.Range((int)0, 2), 0);
                        AttackTimer = AttackInterval;
                    }
                }
            }
            else if (CPU_Strategy == CPU_Strategy.Coward)
            {
                attackHandler.Attack(nAttackType.Parry, 0);
                AttackTimer = AttackInterval;
            }
        }
        */
        #endregion
    }

    internal void CreateCombo()
    {
        atkCombo = new AIAttackCombo();
        //bool parryIncluded = false;
        bool dodgeIncluded = false;
        int iteration = 3;
        if (Target.CC == CrowdControl.Stun)
        {
            iteration = 4;
        }
        int startingIt = iteration;

        if(atkDecision == AttackDecision.ComboDefensive)
        {
            atkCombo.PushMoveType(MoveType.Dodge);
            atkCombo.PushMoveType(MoveType.Parry);
            atkCombo.PushMoveType(MoveType.MidFierce);
            atkCombo.PushMoveType(MoveType.DodgeBack);
            return;
        }

        for(int i = iteration; i > 0; i--)
        {
            if(TargetDistance > 1.5f && !dodgeIncluded)
            {
                atkCombo.PushMoveType(MoveType.Dodge);
                dodgeIncluded = true;
            }
            else
            {
                if(i == startingIt)
                {
                    if (startingIt == 3 )
                    {
                        if(TargetDistance > AttackRanges.LongestRangeDist && !dodgeIncluded)
                        {
                            atkCombo.PushMoveType(MoveType.Dodge);
                            dodgeIncluded = true;
                        }
                        else
                        {
                            atkCombo.PushMoveType(MoveType.Parry);
                        }
                    }
                    else
                    {
                        atkCombo.PushMoveType((MoveType)(1 + Random.Range((int)0, 2)));
                    }
                }
                else
                {
                    atkCombo.PushMoveType((MoveType)(2 + Random.Range((int)0, 2)));
                }
            }
        }

        atkCombo.PushMoveType(MoveType.DodgeBack);
        Debug.Log("AI Created Combo: " + atkCombo.GetComboList());
    }

    internal IEnumerator ExecuteCombo()
    {
        while(!atkCombo.ComboDone() && fighterController.CC != CrowdControl.HitStun && fighterController.CC != CrowdControl.Stun && GameManager.instance.FightManager.FightStarted)
        {
            if(GameManager.instance.FightManager.FightPaused)
            {
                yield return new WaitWhile(() => GameManager.instance.FightManager.FightPaused == true);
            }

            if(fighterController.fighterStats.Health <=0)
            {
                atkCombo = null;
                yield break;
            }

            //Select Move
            MoveType currentMove;

            currentMove = atkCombo.PopMoveType();

            if (currentMove == MoveType.Dodge)
            {
                if (Target.CC != CrowdControl.Attacking)
                {
                    while (TargetDistance > AttackRanges.LongestRangeDist+.15f)
                    {
                        moveDirection = facingDirection;
                        fighterController.previousDirection = new Vector2(moveDirection.x, 0).normalized;
                        fighterController.DoDodge(true);
                        yield return new WaitForSeconds(.4f);
                        TargetDistance = Vector3.Distance(Target.transform.position, fighterController.transform.position);
                    }
                    moveDirection = Vector3.zero;
                    fighterController.previousDirection = Vector2.zero;
                }
                else
                {
                    atkCombo = null;
                    yield break;
                }
            }
            else if (currentMove == MoveType.LoFierce)
            {
                TargetDistance = Vector3.Distance(Target.transform.position, fighterController.transform.position);
                if (TargetDistance > AttackRanges.LongestRangeDist)
                {
                    atkCombo = null;
                    yield break;
                }
                else if (Target.CC == CrowdControl.Attacking)
                {
                    attackHandler.Attack(nAttackType.Parry, Random.Range(0f, 1f));
                    yield return new WaitForSeconds(AttackInterval);
                }
                else
                {
                    fighterController.InputDirection = new Vector3(0, -1, 0).normalized;
                    yield return new WaitForSeconds(.5f);
                    fighterController.crouched = true;
                    attackHandler.Attack(nAttackType.Fierce, -1);
                    yield return new WaitForSeconds(AttackInterval);
                }
            }
            else if (currentMove == MoveType.MidFierce)
            {
                TargetDistance = Vector3.Distance(Target.transform.position, fighterController.transform.position);
                if (TargetDistance > AttackRanges.LongestRangeDist)
                {
                    atkCombo = null;
                    yield break;
                }
                else if (Target.CC == CrowdControl.Attacking)
                {
                    attackHandler.Attack(nAttackType.Parry, Random.Range(0f, 1f));
                    yield return new WaitForSeconds(AttackInterval);
                }
                else
                {
                    attackHandler.Attack(nAttackType.Fierce, Random.Range(0f, 1f));
                    yield return new WaitForSeconds(AttackInterval);
                }
            }
            else if (currentMove == MoveType.LoStrong)
            {
                TargetDistance = Vector3.Distance(Target.transform.position, fighterController.transform.position);
                if (TargetDistance > AttackRanges.LongestRangeDist)
                {
                    atkCombo = null;
                    yield break;
                }
                else if(Target.CC == CrowdControl.Attacking)
                {
                    moveDirection = -facingDirection;
                    fighterController.previousDirection = new Vector2(moveDirection.x, 0).normalized;
                    fighterController.DoDodge(true);
                    atkCombo = null;
                    yield break;
                }
                fighterController.InputDirection = new Vector3(0, -1, 0).normalized;
                yield return new WaitForSeconds(.5f);
                fighterController.crouched = true;
                attackHandler.Attack(nAttackType.Strong, -1);
                yield return new WaitForSeconds(AttackInterval);     
            }
            else if (currentMove == MoveType.MidStrong)
            {
                TargetDistance = Vector3.Distance(Target.transform.position, fighterController.transform.position);
                if (TargetDistance > AttackRanges.LongestRangeDist)
                {
                    atkCombo = null;
                    yield break;
                }
                else if (Target.CC == CrowdControl.Attacking)
                {
                    moveDirection = -facingDirection;
                    fighterController.previousDirection = new Vector2(moveDirection.x, 0).normalized;
                    fighterController.DoDodge(true);
                    atkCombo = null;
                    yield break;
                }
                attackHandler.Attack(nAttackType.Strong, Random.Range(0f, 1f));
                yield return new WaitForSeconds(AttackInterval);        
            }
            else if (currentMove == MoveType.Parry)
            {
                if(Target.CC == CrowdControl.Attacking || atkDecision == AttackDecision.ComboDefensive)
                {
                    attackHandler.Attack(nAttackType.Parry, Random.Range(0f, 1f));
                    yield return new WaitForSeconds(AttackInterval);
                }     
            }
            else if (currentMove == MoveType.DodgeBack)
            {
                moveDirection = -facingDirection;
                fighterController.previousDirection = new Vector2(moveDirection.x, 0).normalized;
                fighterController.DoDodge(true);
                yield return new WaitForSeconds(.8f);
            }
            yield return null;
        }
        atkCombo = null;
    }
}
