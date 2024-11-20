using System;
using UnityEngine;

public class PlayerInteractionSeeker : PlayerModule
{
    #region Events
    public event Action<IInteractable, IInteractable> onHoveredChange;
    public event Action<GameObject> onHovered; 
    #endregion
    #region State
    public IInteractable hoveredObject { get; private set; }
    #endregion
    #region Parameters
    public float maxSeekDistance = 3f;
    #endregion

    GameObject currentHoveredGameObject = null;
    IInteractable _hoveredObject;
    RaycastHit[] hitObjects = new RaycastHit[8];


    public override void OnFixedUpdate(float deltaTime)
    {
        base.OnFixedUpdate(deltaTime);

        IInteractable currentHover = null;
        int count = Physics.RaycastNonAlloc(parent.usedCamera.forwardRay, hitObjects, maxSeekDistance * parent.currentScale);

        if (count != 0 && IsGrounded())
        {
            Array.Sort(hitObjects, (a, b) => (b.distance.CompareTo(a.distance)));

            for (int i = 0; i < count; ++i)
            {
                var obj = hitObjects[i].transform.gameObject;
                IInteractable interObj;
                if (obj.TryGetComponent(out interObj))
                {
                    currentHover = interObj;
                    Debug.Log(obj.tag);
                    currentHoveredGameObject = obj;
                    break;
                }
            }
        }


        if (currentHover == hoveredObject)
            return;

        var oldHover = hoveredObject;
        hoveredObject = currentHover;
        Debug.Log(hoveredObject);
        onHoveredChange?.Invoke(oldHover, currentHover);
    }
    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);

        if (hoveredObject == null)
            return;
        
        if (hoveredObject.CanInteract(parent) && GetInput() && IsGrounded())
        {
            hoveredObject.OnInteract(parent);
            onHovered.Invoke(currentHoveredGameObject);
        }
    }

    bool GetInput()
    {
        return Input.GetKeyDown(KeyCode.E);
    }
    bool IsGrounded() => parent.GetModule<PlayerGroundMotor>().grounded;
}