using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AnimationHandler))]
[RequireComponent(typeof(NetworkHandler))]
public class AttackHandler : MonoBehaviour
{
    #region Declaration Station

    private FighterController controller;
    private Transform HitboxTrans;
    //private MeshRenderer HitboxRenderer;
    private BoxCollider HitboxCollider;
    private AnimationHandler aniHandler;
    private NetworkHandler netHandler;

    private FighterStanceData attackData;
    private HitboxData Hitbox;
    public bool attacking;
    #endregion

    public void Init(int charID, FighterStance stance)
    {
        //Load details from database
        attackData = GameManager.instance.Database.LoadAttackData(charID==5?4:charID, stance);

        controller = GetComponent<FighterController>();
        aniHandler = GetComponent<AnimationHandler>();
        HitboxTrans = transform.FindChild("Hitbox");
        Hitbox = HitboxTrans.GetComponent<HitboxData>();
        //HitboxRenderer = Hitbox.GetComponent<MeshRenderer>();
        HitboxCollider = HitboxTrans.GetComponent<BoxCollider>();

        attacking = false;
    }

    public AttackRanges GetAttackRanges()
    {
        return new AttackRanges(attackData);
    }

    public void Attack(nAttackType type, float stanceVal, bool animate = true)
    {
        if(!attacking)
        {
            //Debug.Log("Attack! Type<" + System.Enum.GetName(typeof(AttackType), type) + ">");
            
            attacking = true;
            if (type != nAttackType.Special)
            {
                StartCoroutine(AtkDuration(type, stanceVal, animate));
            }
            else
            {
                UseSpecial(controller.fighterStats.CharacterID);
            }
        }
    }

    public void StopAttack()
    {
        StopAllCoroutines();
        Hitbox.StopAttack();
        attacking = false;
    }

    private void UseSpecial(int CharID)
    {
        //LEGACY CODE
    }

    private IEnumerator AtkDuration(nAttackType type, float stanceVal, bool animate = true)
    {
        if(animate)
        {
            aniHandler.TriggerAnimation(type, stanceVal);
            aniHandler.TriggerWeaponTrail(true);
        }
        GameManager.instance.SoundManager.PlaySound(SoundEffectType.AttackSFX, 107);

        AttackData relevantAttack = attackData.GetAttackData(type, stanceVal);
        controller.SetCC(CrowdControl.Attacking, relevantAttack.durationOfAttack);
        Hitbox.SetHitboxData(relevantAttack.Damage, relevantAttack.hitstunDuration, relevantAttack.hitboxLocalTransform, relevantAttack.hitboxScale, controller.facingDirection);
        Hitbox.isParry = (type == nAttackType.Parry);
        controller.Parrying = (type == nAttackType.Parry);
        
        float timeSinceStart = 0f;
        bool startedLinger = false;
        while(timeSinceStart < relevantAttack.durationOfAttack)
        {
            if(!startedLinger && timeSinceStart > relevantAttack.hitboxStartTime)
            {
                Hitbox.StartHitboxLinger(relevantAttack.hitboxLingerDuration);
                startedLinger = true;
            }

            timeSinceStart += Time.deltaTime;
            yield return null;
        }

        attacking = false;
        controller.Parrying = false;
        aniHandler.TriggerWeaponTrail(false);
        yield return null;
    }
}
