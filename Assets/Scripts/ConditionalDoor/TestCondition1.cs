using System;
using UnityEngine;

[Serializable]
public class TestCondition1 : Condition
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) EmitCondition();
    }
}
    
