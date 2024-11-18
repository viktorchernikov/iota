using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCondition2 : Condition
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) EmitCondition();
    }
}
