using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class Player : MonoBehaviour, IInteractor
{
    [Header("OnStart")]
    public float initialScale = 1f;
    #region State
    public bool isAlive { get; private set; } = true;
    public bool duringCinematic { get; private set; } = false;
    public bool isHiding { get; private set; }
    public HidingSpot hidingSpot { get; private set; }
    public float currentScale { get; private set; } = 1;
    public PlayerDimensions currentDimensions { get; private set; } = new PlayerDimensions() { height = 2f, radius = 0.4f };
    /// <summary>
    /// Player's body forward vector.
    /// </summary>
    public Vector3 bodyForward
    {
        get
        {
            Vector3 direction = Vector3.forward;
            float yaw = usedCamera.GetViewAngles().y;
            direction = Quaternion.Euler(Vector3.up * yaw) * direction;
            return direction;
        }
    }
    /// <summary>
    /// Player's body right vector.
    /// </summary>
    public Vector3 bodyRight
    {
        get
        {
            Vector3 direction = Vector3.right;
            float yaw = usedCamera.GetViewAngles().y;
            direction = Quaternion.Euler(Vector3.up * yaw) * direction;
            return direction;
        }
    }
    /// <summary>
    /// Current environment of the player
    /// </summary>
    // public EnvironmentType environment { get; set; }
    /// <summary>
    /// Fired when player's environment changed
    /// </summary>
    // public event EventHandler<EnvironmentChangeEventArgs> onEnvironmentChange;
    /// <summary>
    /// Currently used type of the movement
    /// </summary>
    public PlayerMoveType moveType { get; private set; } = PlayerMoveType.Ground;
    public float hidingTime = 1.25f;
    public float unhidingTime = 1.25f;
    public static Player local { get; private set; }
    #endregion
    #region Components
    public PlayerCamera usedCamera { get; private set; }
    private PlayerCinematicController usedCinematicController;
    /// <summary>
    /// A reference to the rigidbody component used by the player.
    /// </summary>
    public Rigidbody usedRigidbody { get; private set; }
    public CapsuleCollider usedCollider { get; private set; }
    public AudioSource ambientSource;
    #endregion
    #region Modules
    /// <summary>
    /// References to all modules attached to this player.
    /// </summary>
    public List<PlayerModule> modules = new List<PlayerModule>();
    #endregion
    #region Points
    public Transform cameraAnchor => _cameraAnchor;
    [Header("Points")]
    [SerializeField] Transform _cameraAnchor;

    [SerializeField] private Transform _spawnPoint;
    #endregion
    #region Constants
    public readonly PlayerDimensions defaultDimensions = new PlayerDimensions() { height = 2f, radius = 0.4f};
    #endregion

    [SerializeField] private ScreenFade screenFade;
    public event Action<InteractionContext> onInteract;
    public event Action<float, float, PlayerDimensions, PlayerDimensions> onScaleChange;

    public void PrepareToDie(Vector3 focusPoint)
    {
        duringCinematic = true;
        usedRigidbody.isKinematic = true;
        isAlive = false;
        StartCoroutine(PrepareToDieCo(focusPoint));
    }


    private IEnumerator PrepareToDieCo(Vector3 focusPoint)
    {
        yield return usedCinematicController.AdjustCameraPositionAndViewAngles(usedCamera.transform.position, focusPoint, 3, 180);
        yield return null;
    }
    
    public void Die()
    {
        screenFade.FadeOut();
        StartCoroutine(DieCo());
        
    }

    private IEnumerator DieCo()
    {
        yield return new WaitForSeconds(1);
        yield return null;
        
        screenFade.FadeIn();
        
        Teleport(_spawnPoint);
        isAlive = true;
        usedRigidbody.isKinematic = false;
        duringCinematic = false;
    }
    public void Teleport(Transform transform)
    {
        Teleport(transform.position);
    }
    public void Teleport(Vector3 position)
    {
        Debug.Log($"Teleported from: {transform.position}");
        usedRigidbody.position = position;
        transform.position = position;
        Debug.Log($"Teleported to: {transform.position}");
    }
    public void HideInSpot(HidingSpot spot)
    {
        StartCoroutine(HideInSpotCo(spot));
    }
    IEnumerator HideInSpotCo(HidingSpot spot)
    {
        Vector3 destinationPos = spot.hidePoint.position;
        Quaternion destinationRotation = spot.hidePoint.rotation;

        duringCinematic = true;
        usedRigidbody.isKinematic = true;

        yield return usedCinematicController.AdjustCameraPositionAndViewAngles(destinationPos, destinationRotation.eulerAngles, 3, 180);
        yield return null;
        Teleport(destinationPos - cameraAnchor.localPosition);

        duringCinematic = false;
        isHiding = true;
        hidingSpot = spot;
        yield return null;
    }

    public Transform self { get => transform; }

    public void UnhideFromSpot()
    {
        StartCoroutine(UnhideFromSpotCo());
    }
    IEnumerator UnhideFromSpotCo()
    {
        Vector3 destinationPos = hidingSpot.exitPoint.position;
        Quaternion destinationRotation = hidingSpot.exitPoint.rotation;

        duringCinematic = true;

        yield return usedCinematicController.AdjustCameraPositionAndViewAngles(destinationPos + cameraAnchor.localPosition, destinationRotation.eulerAngles, 3, 180);
        yield return null;
        Teleport(destinationPos);

        duringCinematic = false;
        isHiding = false;
        hidingSpot = null;
        usedRigidbody.isKinematic = false;
        yield return null;
    }
    public void SetDuringCinematic(bool isDuringCinematic)
    {
        if (isDuringCinematic)
        {
            usedRigidbody.isKinematic = true;
            usedRigidbody.velocity = Vector3.zero;

            GetModule<PlayerGroundMotor>().canMove = false;
            GetModule<PlayerCameraController>().canLook = false;
        }
        else
        {
            usedRigidbody.isKinematic = false;

            GetModule<PlayerGroundMotor>().canMove = true;
            GetModule<PlayerCameraController>().canLook = true;
        }
    }
    public void SetScale(float newScale)
    {
        float oldScale = currentScale;
        PlayerDimensions oldDimensions = currentDimensions;

        currentScale = newScale;
        PlayerDimensions newDimensions = new PlayerDimensions();
        newDimensions.height = defaultDimensions.height * currentScale;
        newDimensions.radius = defaultDimensions.radius * currentScale;
        currentDimensions = newDimensions;

        usedCollider.height = newDimensions.height;
        usedCollider.radius = newDimensions.radius;
        usedCollider.transform.localPosition = Vector3.up * newDimensions.height * 0.5f;
        cameraAnchor.transform.localPosition = (Vector3.up * 0.95f) * newDimensions.height;

        onScaleChange?.Invoke(oldScale, newScale, oldDimensions, newDimensions);
    }
    /// <summary>
    /// Updates current environment
    /// </summary>
    /*
    public void SetEnvironment(object initializer, EnvironmentType newEnv)
    {
        if (environment != newEnv)
        {
            EnvironmentType oldEnv = environment;
            environment = newEnv;

            var args = new EnvironmentChangeEventArgs();
            args.previous = oldEnv;
            args.current = newEnv;

            SetMoveType(newEnv == EnvironmentType.Air ? PlayerMoveType.Ground : PlayerMoveType.Water);
            onEnvironmentChange?.Invoke(this, args);
        }
    }
    */
    /// <summary>
    /// Updates current movement type.
    /// </summary>
    public void SetMoveType(PlayerMoveType newMoveType)
    {
        if (moveType == newMoveType)
            return;

        moveType = newMoveType;
        // When player is moving as an spectator
        if (moveType == PlayerMoveType.Spectator)
        {
            throw new NotImplementedException("Spectator movement was not implemented yet!");
        }
        // But when player is on the ground:
        else
        {
            Vector3 position = usedRigidbody.position;
            // Rigidbody should be disabled.
            usedRigidbody.Sleep();
            usedRigidbody.isKinematic = false;
            usedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            usedRigidbody.WakeUp();
            // Character controller should be enabled.
            transform.position = position;
        }
    }

    bool IInteractor.CanInteract()
    {
        return isAlive;
    }
    void IInteractor.OnInteract(InteractionContext context)
    {
        Debug.LogWarning("OnInteract was not implemented!");
    }

    #region Unity Messages
    void Awake()
    {
        InitSingleton();
        InitComponents();
        InitModules();
    }
    private void Start()
    {
        SetScale(initialScale);
        //SetEnvironment(this, EnvironmentType.Air);
    }
    void Update()
    {
        UpdateModules();
    }
    void FixedUpdate()
    {
        FixedUpdateModules();
    }
    void LateUpdate()
    {
        LateUpdateModules();
    }
    #endregion

    #region Initializing
    /// <summary>
    /// This is a method that helps with initializing fields with component references.
    /// </summary>
    void InitComponents()
    {
        usedRigidbody = GetComponent<Rigidbody>();
        usedCamera = GetComponentInChildren<PlayerCamera>();
        usedCollider = GetComponentInChildren<CapsuleCollider>();
        usedCinematicController = GetComponent<PlayerCinematicController>();

        usedCamera.Unparent();
    }
    /// <summary>
    /// This is a method that helps with initializing each module attached to the player.
    /// </summary>
    void InitModules()
    {
        foreach(var module in GetComponentsInChildren<PlayerModule>())
        {
            // Initializing each module attached to the player.
            if (module.Init(this))
            {
                modules.Add(module);
            }
            else
            {
                Debug.LogError("Failed to initialize player modules in initial initialization. What's wrong?", module);
                return;
            }
        }
    }
    void InitSingleton()
    {
        if (local == null)
        {
            local = this;
        }
        else if (local != this)
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Modules helpers
    /// <summary>
    /// Returns a desired module by given type.
    /// </summary>
    /// <typeparam name="T">The type of a desired module.</typeparam>
    /// <exception cref="NullReferenceException">Will throw an error if a module was not found.</exception>
    /// <returns>Desired module.</returns>
    public T GetModule<T>() where T : PlayerModule
    {
        
        return (T)modules.First(x => x is T);
    }
    /// <summary>
    /// Tries finding a desired module by given type.
    /// </summary>
    /// <typeparam name="T">The type of a desired module.</typeparam>
    /// <param name="module">Where result should be given.</param>
    /// <returns>Desired module.</returns>
    public bool TryGetModule<T>(out T module) where T : PlayerModule
    {
        T m = (T)modules.FirstOrDefault(x => x is T);
        if (m == null || m == default(T))
        {
            module = null;
            return false;
        }
        module = m;
        return true;
    }
    #endregion

    #region Modules messages
    /// <summary>
    /// Fires OnUpdate message for each module of this player.
    /// </summary>
    void UpdateModules()
    {
        modules.ForEach(x => x.OnUpdate(Time.deltaTime));
    }
    /// <summary>
    /// Fires OnFixedUpdate message for each module of this player.
    /// </summary>
    void FixedUpdateModules()
    {
        modules.ForEach(x => x.OnFixedUpdate(Time.fixedDeltaTime));
    }
    /// <summary>
    /// Fires OnLateUpdate message for each module of this player.
    /// </summary>
    void LateUpdateModules()
    {
        modules.ForEach(x => x.OnLateUpdate(Time.deltaTime));
    }
    #endregion
}
