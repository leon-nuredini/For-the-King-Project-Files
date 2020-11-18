using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorParams
{
    // Animator parameter types
    public enum AnimParamType
    {
        Trigger,
        Bool,
        Float,
        Int
    }

    public static AnimParamType parameterTypeTrigger = AnimParamType.Trigger;
    public static AnimParamType parameterTypeBool = AnimParamType.Bool;
    public static AnimParamType parameterTypeFloat = AnimParamType.Float;
    public static AnimParamType parameterTypeInt = AnimParamType.Int;

    // Soldier animator parameters
    public static readonly string Idle = "idle";
    public static readonly string Walk = "walk";
    public static readonly string Attack = "attack";
    public static readonly string Die = "die";
    public static readonly string Hit = "hit";
    
    // Start game button param
    public static readonly string Start = "start";
    
    //Clip names
    public static readonly string ClipAttack = "attack";
    public static readonly string ClipDie = "die";

}