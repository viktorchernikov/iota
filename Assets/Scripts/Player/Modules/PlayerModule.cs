using UnityEngine;

public abstract class PlayerModule : MonoBehaviour
{
    /// <summary>
    /// A reference to the parent player of this module.
    /// </summary>
    public Player parent { get; private set; }
    /// <summary>
    /// Tells if this module was initialized or not.
    /// </summary>
    public bool initialized { get; private set; } = false;


    /// <summary>
    /// This method initializes this module, a reference to the parent player must be passed as an parameter.
    /// If module was already initialized, then a module can't be initialized again and initialization will fail.
    /// </summary>
    /// <param name="newParent">A reference to the new parent player.</param>
    /// <returns>Tells if this modules was successfully initialized or not.</returns>
    public bool Init(Player newParent)
    {
        if (initialized || newParent == null)
        {
            return false;
        }
        initialized = true;
        parent = newParent;
        OnInit();
        return true;
    }

    /// <summary>
    /// This message is fired when after initialization of the module.
    /// </summary>
    /// <remarks>
    /// Do not call this directly!
    /// </remarks>
    public virtual void OnInit() { }
    /// <summary>
    /// This message is fired every Update of the parent player.
    /// </summary>
    /// <remarks>
    /// Do not call this directly!
    /// </remarks>
    public virtual void OnUpdate(float deltaTime) { }
    /// <summary>
    /// This message is fired every FixedUpdate of the parent player.
    /// </summary>
    /// <remarks>
    /// Do not call this directly!
    /// </remarks>
    public virtual void OnFixedUpdate(float deltaTime) { }
    /// <summary>
    /// This message is fired every LateUpdate of the parent player.
    /// </summary>
    /// <remarks>
    /// Do not call this directly!
    /// </remarks>
    public virtual void OnLateUpdate(float deltaTime) { }
}