using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AI interface for game. Don't use this as a means of AI construction
/// </summary>
public abstract class IAI : MonoBehaviour
{
    //Code Stubs
    internal abstract void Upkeep();
    internal abstract void Seek();
    internal abstract void Move();
    internal abstract void Attack();
}
