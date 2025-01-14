using System;
using UnityEngine;

public sealed class PlayerGroundMotor : PlayerMotor
{
    [Header("State")]
    public bool canMove = true;
    public bool grounded;
    public Vector3 groundNormal = Vector3.up;
    [Header("Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float groundDrag;
    public float crouchScaleY;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public AnimationCurve speedScalePropoprtion;
    bool crouching;
    bool readyToJump;

    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float standScaleY;

    [Header("Stamina")]
    public bool StaminaWorkInSky = true;
    public float maxStamina = 100f;
    public float staminaRegenRate = 10f;
    public float sprintStaminaUsage = 20f;
    public float minSprintStamina = 20f;
    public float Whenlessthenstoprun = 5f;
    public float currentStamina;

    [Header("Keybinds")]
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    [SerializeField] public float groundCheckDistance = 0.3f;
    [SerializeField] public float groundCheckOffset = 0.01f;
    [SerializeField] public LayerMask whatIsGround;

    public float speedofplayul = 0.1f;
    public bool audiocanplay = true;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;
    AudioSource audioSource;

    [Header("Footstep Sounds")]
    public AudioClip[] defaultSounds;
    public AudioClip[] metalSounds;
    public AudioClip[] woodSounds;
    public AudioClip landingSound;
    public float walkFootstepInterval;
    public float sprintFootstepInterval;
    public float VolumeOfFootstep = 0.7f;
    private float footstepTimer;

    float lastLandingTime = 0;

    public override void OnInit()
    {
        base.OnInit();

        rb = parent.usedRigidbody;
        rb.freezeRotation = true;
        readyToJump = true;
        crouching = false;
        standScaleY = transform.localScale.y;
        currentStamina = maxStamina;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0.5f;
        audioSource.volume = VolumeOfFootstep;
    }

    public override void OnFixedUpdate(float deltaTime)
    {

        base.OnFixedUpdate(deltaTime);

        if (parent.moveType != moveType)
            return;

        FixedUpdateTakenFromMovent();
    }


    public override void OnUpdate(float deltaTime)
    {
        
        UpdateTakenFromMovent();
    }



    #region movent
    private void UpdateTakenFromMovent()
    {
        MyInput();
        SpeedControl();
        RegenerateStamina();

        if (grounded)
        {
            rb.drag = groundDrag;

            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f && (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f))
            {
                PlayRandomFootstepSound();
                footstepTimer = moveSpeed == sprintSpeed ? sprintFootstepInterval : walkFootstepInterval;
            }
        }
        else
        {
            rb.drag = 0;
        }
    }

    private void FixedUpdateTakenFromMovent()
    {
        IsGronded();
        MovePlayer();
        CounterScaleGravity();
    }

    private void IsGronded() 
    {
        Vector3 origin = transform.position;
        origin += Vector3.up * groundCheckOffset;

        RaycastHit hit;
        bool newGrounded = Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance * parent.currentScale, whatIsGround);
        groundNormal = newGrounded ? hit.normal : Vector3.up;
        if (!grounded && newGrounded)
        {
            if (Time.time - lastLandingTime > 0.3f)
            {
                lastLandingTime = Time.time;
                audioSource.PlayOneShot(landingSound);
            }
        }
        if (hit.collider)
        {
            IPickupable pickupable = hit.collider.GetComponent<IPickupable>();
            if (pickupable != null)
            {
                if (parent.GetModule<PlayerHoldingModule>().currentlyHolding == pickupable)
                {
                    parent.GetModule<PlayerHoldingModule>().Drop();

                }
            }
        }

        grounded = newGrounded;
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        bool sprinting = Input.GetKey(sprintKey) && currentStamina > 0 && grounded;
        if (StaminaWorkInSky)
        {
            sprinting = Input.GetKey(sprintKey) && currentStamina > 0;
        }
        else
        {
            sprinting = Input.GetKey(sprintKey) && currentStamina > 0 && grounded;
        }

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey) && grounded)
        {
            Crouch();
        }

        // Sprawd�, czy gracz si� porusza
        bool isMoving = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;

        if (!isMoving)
        {
            sprinting = false;
        }

        if (currentStamina < minSprintStamina && !crouching)
        {
            moveSpeed = walkSpeed;
        }
        else if (crouching)
        {
            moveSpeed = walkSpeed * crouchSpeed;
        }
        else
        {
            moveSpeed = sprinting ? sprintSpeed : walkSpeed;

            if (sprinting)
            {
                if (currentStamina > 0)
                {
                    currentStamina -= sprintStaminaUsage * Time.deltaTime;
                }
                else
                {
                    currentStamina = 0;
                    sprinting = false;
                }
            }
            else
            {
                if (currentStamina < maxStamina)
                {
                    currentStamina += staminaRegenRate * Time.deltaTime;
                    if (currentStamina > maxStamina)
                        currentStamina = maxStamina;
                }
            }
        }

        if (currentStamina < Whenlessthenstoprun && currentStamina > 0f && grounded && !crouching) 
        {
            moveSpeed = walkSpeed;
        }
    }

    private void MovePlayer()
    {
        if (!canMove)
            return;

        moveDirection = parent.bodyForward * verticalInput + parent.bodyRight * horizontalInput;

        if (grounded)
        {
            rb.AddForce(-Physics.gravity, ForceMode.Acceleration);
            rb.AddForce(Vector3.ProjectOnPlane(moveDirection.normalized, groundNormal) * moveSpeed * speedScalePropoprtion.Evaluate(parent.currentScale) * 10f, ForceMode.Force);
        }
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * speedScalePropoprtion.Evaluate(parent.currentScale) * 10f * airMultiplier, ForceMode.Force);
    }
    private void CounterScaleGravity()
    {
        if (!grounded)
            rb.AddForce(Physics.gravity * (parent.currentScale - 1f), ForceMode.Acceleration);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > moveSpeed * parent.currentScale)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed * parent.currentScale;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce * parent.currentScale, ForceMode.Impulse);

        parent.GetModule<PlayerHoldingModule>().Drop();
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void Crouch()
    {
        var origin = Player.local.transform.position;
        origin.y += standScaleY;

        RaycastHit hitInfo;
        bool wasHit = Physics.Raycast(origin, Player.local.transform.up, out hitInfo, Player.local.currentDimensions.height);
        
        if (crouching && wasHit && hitInfo.collider.tag != "Water") return;
        
        crouching = !crouching;

        if (crouching) transform.localScale = new Vector3(transform.localScale.x, crouchScaleY, transform.localScale.z);
        else transform.localScale = new Vector3(transform.localScale.x, standScaleY, transform.localScale.z);

        rb.AddForce(Vector3.down * 5, ForceMode.Impulse);
    }

    private void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina)
                currentStamina = maxStamina;
        }
    }

    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    private void PlayRandomFootstepSound()
    {
        /*
        AudioClip[] footstepSoundsArray;


        if (parent.floorMaterial == FloorMaterial.Metal)
        {
            footstepSoundsArray = metalSounds;
        }
        else if (parent.floorMaterial == FloorMaterial.Wood)
        {
            footstepSoundsArray = woodSounds;
        }
        else
        {
            footstepSoundsArray  = defaultSounds;
        }

        if (footstepSoundsArray != null && footstepSoundsArray.Length > 0 && audiocanplay)
        {
            int randomIndex = UnityEngine.Random.Range(0, footstepSoundsArray.Length);
            audioSource.PlayOneShot(footstepSoundsArray[randomIndex]);
        }
        */
    }
    #endregion
}
