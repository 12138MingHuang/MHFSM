using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MHFSM;

public class AttackBase : FSMState
{
    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("AttackBase OnEnter");
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("AttackBase OnExit");
    } 
}
