using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager Object handled by the Game Manager.
/// </summary>
public class IManager
{
    /// <summary>
    /// Called by GameManager singleton on initialize. Used for preemptive setup by manager objects.
    /// </summary>
    /// <returns>Error Code for manager load</returns>
    public virtual ManagerInitializeErrorCode Initialize()
    {
        return ManagerInitializeErrorCode.NONE;
    }

    #region Update Functions
    /// <summary>
    /// Update loop called by Game Manager
    /// </summary>
    /// <param name="delta"></param>
    public virtual void Update(float delta)
    {

    }

    /// <summary>
    /// Fixed update loop called by Game Manager
    /// </summary>
    /// <param name="fixedDelta"></param>
    public virtual void FixedUpdate(float fixedDelta)
    {

    }

    public virtual void OnDestroy()
    {

    }

    #endregion
}


public enum ManagerInitializeErrorCode : byte
{
    NONE = 0x0,
    ASSET_LOAD_FAILURE = 0x1,

    UNKNOWN = 0xFF
}
