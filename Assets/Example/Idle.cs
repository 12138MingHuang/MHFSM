using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MHFSM;

public class Idle : FSMState
{
    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("Idle OnEnter");
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("Idle OnExit");
    } 
}
