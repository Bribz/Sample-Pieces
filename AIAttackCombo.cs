using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum MoveType
{
    Dodge,
    LoFierce,
    MidFierce,
    LoStrong,
    MidStrong,
    Parry,
    DodgeBack
}

public class AIAttackCombo
{
    private Queue<MoveType> ComboList;

    public AIAttackCombo()
    {
        ComboList = new Queue<MoveType>();
    }
	
    public bool ComboDone()
    {
        if (ComboList.Count > 0)
            return false;
        else
            return true;
    }

    public void PushMoveType(MoveType move)
    {
        ComboList.Enqueue(move);
    }

    public MoveType PopMoveType()
    {
        return ComboList.Dequeue();
    }

    public string GetComboList()
    {
        string ListDetails = "";
        foreach(var obj in ComboList)
        {
            ListDetails += "<" + Enum.GetName(typeof(MoveType), obj) + "> ";
        }
        return ListDetails;
    }
}
