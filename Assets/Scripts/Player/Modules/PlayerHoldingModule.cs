using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PlayerHoldingModule : PlayerModule
{
    public IPickupable currentlyHolding;

    [Header("Adjustable")]
    public KeyCode throwKey = KeyCode.Q;
    public float pullingForce = 5f;
    public float minThrowingForce = 4f;
    public float maxThrowingForce = 20f;
    public float chargingPeriod = 2f;


    float timePassed = 0f;
    public override void OnUpdate(float deltaTime)
    {
        if (currentlyHolding == null) return;

        if (Input.GetKey(throwKey)) {
            timePassed += deltaTime;
        }

        if (Input.GetKeyUp(throwKey)) {
            currentlyHolding.self.GetComponent<Rigidbody>().useGravity = true;
            currentlyHolding.Throw(parent.usedCamera.forward, Mathf.Lerp(minThrowingForce, maxThrowingForce, Mathf.Min(timePassed/chargingPeriod, 1f)));
            Drop();
            timePassed = 0f;
        }
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        if (currentlyHolding == null) return;

        Rigidbody rb = currentlyHolding.self.GetComponent<Rigidbody>();
        Vector3 targetPos = parent.usedCamera.position + parent.usedCamera.forward * currentlyHolding.holdingDistance;
        rb.AddForce(-rb.velocity * 0.9f);
        rb.velocity = (targetPos - currentlyHolding.self.position) * pullingForce;
    }

    public void Drop() {
        if (currentlyHolding == null) return;

        currentlyHolding.self.GetComponent<Rigidbody>().useGravity = true;
        currentlyHolding = null;
    }
}
