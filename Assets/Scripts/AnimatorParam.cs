using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorParam
{
    //overloaded method that is used for triggers
    public static void SetParams(Animator animator, string parameter,
                                 AnimatorParams.AnimParamType parameterType)
    {
        if (parameterType == AnimatorParams.AnimParamType.Trigger) animator.SetTrigger(parameter);
    }

    //overloaded method that is used for booleans
    public static void SetParams(Animator animator, string parameter, bool bValue,
                                 AnimatorParams.AnimParamType parameterType)
    {
        if (parameterType == AnimatorParams.AnimParamType.Bool) animator.SetBool(parameter, bValue);
    }

    //overloaded method that is used for floats
    public static void SetParams(Animator animator, string parameter, float fValue,
                                 AnimatorParams.AnimParamType parameterType)
    {
        if (parameterType == AnimatorParams.AnimParamType.Float) animator.SetFloat(parameter, fValue);
    }

    //overloaded method that is used for integers
    public static void SetParams(Animator animator, string parameter, int iValue,
                                 AnimatorParams.AnimParamType parameterType)
    {
        if (parameterType == AnimatorParams.AnimParamType.Int) animator.SetInteger(parameter, iValue);
    }

    //overloaded method that accepts all values
    public static void SetParams(Animator animator, string parameter, bool bValue, float fValue, int iValue,
                                 AnimatorParams.AnimParamType parameterType)
    {
        switch (parameterType)
        {
            case AnimatorParams.AnimParamType.Trigger:
                animator.SetTrigger(parameter);
                break;
            case AnimatorParams.AnimParamType.Bool:
                animator.SetBool(parameter, bValue);
                break;
            case AnimatorParams.AnimParamType.Float:
                animator.SetFloat(parameter, fValue);
                break;
            case AnimatorParams.AnimParamType.Int:
                animator.SetInteger(parameter, iValue);
                break;
        }
    }
    
    public static bool HasParameter(Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}