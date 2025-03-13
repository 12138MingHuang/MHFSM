using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MHFSM;

public class NormalAttack : FSMState
{
    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("NormalAttack OnEnter");
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("NormalAttack OnExit");
    } 
}
