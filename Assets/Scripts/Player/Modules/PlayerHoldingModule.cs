using UnityEngine;

public class PlayerHoldingModule : PlayerModule
{
    public IPickupable currentlyHolding;

    [Header("Adjustable")]
    [SerializeField] private KeyCode throwKey = KeyCode.Q;
    [SerializeField] private  float pullingForce = 5f;
    [SerializeField] private  float minThrowingForce = 4f;
    [SerializeField] private  float maxThrowingForce = 20f;
    [SerializeField] private  float chargingPeriod = 2f;

    private PlayerInteractionSeeker _seeker;

    private void Awake()
    {
        _seeker = GetComponent<PlayerInteractionSeeker>();
        _seeker.OnPlayerInteract += PlayerPickObjectHandler;
    }


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

    private void PlayerPickObjectHandler(GameObject interactable)
    {
        if (!interactable.TryGetComponent<IPickupable>(out var pickupable)) return;

        currentlyHolding = pickupable;
        
        GetComponent<PlayerKatana>().SetKatanaUnavailable();
    }
}
