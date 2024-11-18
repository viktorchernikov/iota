using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDoor : ConditionalDoor
{
    protected override void OnAllConditionEmitted()
    {
        Debug.Log("TestDoor: all condition emitted");
    }
}
