using System;
using UnityEngine;

public abstract class PlayerMotor : PlayerModule
{
    public PlayerMoveType moveType => _moveType;

    [SerializeField] PlayerMoveType _moveType;


    public override void OnFixedUpdate(float deltaTime) { }
}
