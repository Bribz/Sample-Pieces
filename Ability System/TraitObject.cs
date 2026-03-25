using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, CreateAssetMenu(menuName = "Abilities/Trait/Trait")]
public class TraitObject : ScriptableObject
{
    public Texture2D Image;
    public string Name = "";
    public string Description = "";

    public TraitProcStrategy ProcStrategy = null;
    public TraitEffect Effect = null;
}
