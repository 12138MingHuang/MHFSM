using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MHFSM;

public class Walk : FSMState
{
    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("Walk OnEnter");
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("Walk OnExit");
    } 
}
