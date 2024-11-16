using UnityEngine;

public class PlayerCameraController : PlayerModule
{
    #region State
    [Header("State")]
    public Vector2 viewAngles = Vector2.zero;
    public bool canLook = true;
    #endregion
    #region Settings
    [Header("Settings")]
    public Vector2 sensitivity = Vector2.one;
    #endregion
    #region Parameters
    [Header("Parameters")]
    [SerializeField] 
    [Range(-90f, 0f)] 
    float minPitch = -90f;
    [SerializeField] 
    [Range(0f, 90f)] 
    float maxPitch = 90f;
    [Header("Components")]
    [SerializeField] 
    Transform anchorPoint;
    #endregion


    public override void OnInit() 
    { 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        viewAngles = parent.usedCamera.viewAngles;
    }
    public override void OnLateUpdate(float deltaTime)
    {
        if (!canLook)
        {
            viewAngles = parent.usedCamera.viewAngles;
            return;
        }

        Vector2 input = GetInput();

        viewAngles.y += input.x;
        viewAngles.x = Mathf.Clamp(viewAngles.x - input.y, minPitch, maxPitch);

        parent.usedCamera.SetViewAngles(viewAngles);
        parent.usedCamera.SetPosition(anchorPoint.position);
    }

    Vector2 GetInput()
    {
        Vector2 input;
        input.x = Input.GetAxis("Mouse X");
        input.y = Input.GetAxis("Mouse Y");
        input.x *= sensitivity.x;
        input.y *= sensitivity.y;

        return input;
    }
}
