using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MHFSM;

public class Skill_1_Attack : FSMState
{
    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("Skill_1_Attack OnEnter");
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("Skill_1_Attack OnExit");
    } 
}
