using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, CreateAssetMenu(menuName = "Abilities/Trait/Strategy/Movement")]
public class MovementStrategy : EventStrategy
{
    private Rigidbody m_entityRigidBody;
    private float MovementMagnitude = 0f;

    public bool Check_IsMoving = false;


    public override void UpdateStrategy(object caller, object target = null, object data = null)
    {
        m_eventInvoked = true;

        EntityStats stats = (EntityStats)caller;
        m_entityRigidBody = stats.GetComponent<Rigidbody>();
        if(m_entityRigidBody != null)
        {
            MovementMagnitude = m_entityRigidBody.velocity.magnitude;
        }
    }

    public override bool CheckStrategy_GoalAchieved()
    {
        if (m_eventInvoked)
        {
            m_eventInvoked = false;
            return Check_IsMoving == (MovementMagnitude > 0.05f);
        }
        return false;
    }
}
