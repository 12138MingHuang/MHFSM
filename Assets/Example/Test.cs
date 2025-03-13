using MHFSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private FSMController controller;

    private void Start()
    {
        controller = GetComponent<FSMController>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            controller.SetBool("IsWalk", true);
        }
        
        if(Input.GetKeyDown(KeyCode.E))
        {
            controller.SetBool("IsWalk", false);
        }
    }
}
