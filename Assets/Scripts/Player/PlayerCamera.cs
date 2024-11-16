using UnityEngine;
using System;


public class PlayerCamera : MonoBehaviour
{
    public Vector3 position
    {
        get => transform.position;
        set => transform.position = value;
    }
    public Vector3 viewAngles
    {
        get => transform.eulerAngles;
        set => transform.eulerAngles = value;
    }
    public Vector3 forward => transform.forward;
    public Vector3 right => transform.right;
    public Ray forwardRay => new Ray(GetPosition(), forward);
    public Ray rightRay => new Ray(GetPosition(), right);


    public void Unparent()
    {
        transform.SetParent(null);
    }
    public Vector3 GetPosition() => position;
    public void SetPosition(Vector3 newPos) => position = newPos;
    public Vector3 GetViewAngles() => viewAngles;
    public void SetViewAngles(Vector3 newAngles) => viewAngles = newAngles;
}