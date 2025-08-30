using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region General

    #region Variables

    //ESSENTIALS
    private PlayerManager pM;
    private Rigidbody2D rb;
    private BoxCollider2D col;

    public MovementStates currentMovementState;

    [Header("Visuals")]

    [Header("Essentials")]
    public SpriteRenderer graphic;
    private Animator anim;

    [Header("Animations")]
    public PlayerAnimations idleAnimation;
    public PlayerAnimations runAnimation;
    public PlayerAnimations jumpAnimation;
    public PlayerAnimations fallAnimation;
    public PlayerAnimations landingAnimation;
    public PlayerAnimations dashAnimation;
    public PlayerAnimations wallSlideAnimation;
    public PlayerAnimations wallJumpAnimation;
    public PlayerAnimations wallPressAnimation;
    public PlayerAnimations runTransitionAnimation;
    public PlayerAnimations landingRunTransitionAnimation;
    public PlayerAnimations turnTransitionAnimation;

    [Header("Animation variables")]
    public float landingCooldownMultiplier;
    public float minLandingTime;
    private float landingTimer;
    private bool isFalling;
    public float maxFallTime;
    private float fallTime;
    private bool isJumping;
    private bool isTryingToRun;
    private bool inAir;
    private bool isIdling;
    private bool lockTurn;
    public float absoluteTurnLockCooldown;
    private float absoluteTurnLockTimer;

    [Header("Effects")]
    public TrailRenderer trail;

    [Header("Movement")]

    [Header("Horizontal movement")] //acceleration - max speed - deceleration - quick turn
    public float direction;
    public float acceleration;
    public float deceleration;
    public float maxSpeed;
    [SerializeField] private float currentSpeed;
    public float minMovementSpeed;
    public float airTurnDeceleration;

    [Header("Vertical movement")] //coyote time - jump - bonus air time - fast fall
    public float jumpForce;
    public float maxFallSpeed;
    public float initialGravity;
    public float jumpCutGravity;
    public float fallGravity;
    public float hangTimeVelocityThreshold;
    public float hangTimeGravity;
    public bool isGrounded;
    public float groundCheckDistance;
    public LayerMask groundLayer;

    [Header("Movement bonuses")]
    public float coyoteTime;
    private float coyoteTimeCounter;
    public float jumpBufferTime;
    private float jumpBufferCounter;

    [Header("Bash mechanic")] //detect target - get direction input - launch - bonus air control grace
    public bool bashEnabled;
    public float bashDetectionRangeX;
    public float bashDetectionRangeY;
    public float launchVelocity;
    public float bashLockTime;
    private float bashLockTimer;
    public float bashGravity;
    public float bashTime;
    private float bashTimer;
    public LayerMask bashTargetLayer;
    private Vector2 bashDir;
    public float exitBashGravity;
    public float bashInputBuffer;
    private float bashInputBufferCounter;

    [Header("Wall jump")]
    public bool wallJumpEnabled;
    public bool isPressedToAWall;
    public float wallSlideGravity;
    public int wallSlideDirection;
    public int wallSlideSetDirection;
    public float minWallSlideVelocity;
    private Vector2 wallJumpDirection;
    public float wallJumpLockTime;
    private float wallJumpLockTimer;
    public float wallJumpHorizontalForce;
    public float wallJumpVerticalForce;
    public float wallJumpBuffer;
    private float wallJumpBufferCounter;
    public float wallJumpCoyoteTime;
    private float wallJumpCoyoteTimeCounter;
    public float maxWallSlideSpeed;

    #endregion

    #region BuiltInEngineVoids

    void Awake()
    {
        currentSpeed = 0;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        pM = GetComponent<PlayerManager>();
        anim = graphic.GetComponent<Animator>();
        trail.emitting = false;
    }

    // Update is called once per frame
    void Update()
    {
        StateMachineHandling();
        FunctionalityHandling();
        GraphicHandling();
    }

    #endregion

    #region StandaloneChecks

    void FunctionalityHandling()
    {
        RaycastHit2D groundCheck = Physics2D.Raycast(new Vector2(col.bounds.min.x + 0.3f, col.bounds.min.y - groundCheckDistance), Vector2.right, col.bounds.max.x - col.bounds.min.x - 0.55f, groundLayer); //checking in a horizontal line right bellow player's feet
        isGrounded = groundCheck.collider != null;

        #if UNITY_EDITOR
        Debug.DrawLine(new Vector2(col.bounds.min.x, col.bounds.min.y - groundCheckDistance), new Vector2(col.bounds.max.x, col.bounds.min.y - groundCheckDistance), Color.red);
        #endif

        RaycastHit2D wallCheck = new RaycastHit2D();

        switch (direction)
        {
            case 1:

                wallSlideDirection = 1;
                wallCheck = Physics2D.Raycast(new Vector2(col.bounds.max.x + 0.6f, col.bounds.max.y - 0.3f), Vector2.down, col.bounds.max.y - col.bounds.min.y - 0.6f, groundLayer);

                #if UNITY_EDITOR
                Debug.DrawLine(new Vector2(col.bounds.max.x, col.bounds.max.y - 0.3f), new Vector2(col.bounds.max.x, col.bounds.min.y + 0.3f), Color.red);
                #endif

                isPressedToAWall = wallCheck.collider != null;

                if (isPressedToAWall)
                {
                    wallSlideSetDirection = wallSlideDirection;
                }

                break;

            case -1:

                wallSlideDirection = -1;
                wallCheck = Physics2D.Raycast(new Vector2(col.bounds.min.x - 0.6f, col.bounds.max.y - 0.3f), Vector2.down, col.bounds.max.y - col.bounds.min.y - 0.6f, groundLayer);

                #if UNITY_EDITOR
                Debug.DrawLine(new Vector2(col.bounds.min.x, col.bounds.max.y - 0.3f), new Vector2(col.bounds.min.x, col.bounds.min.y + 0.3f), Color.red);
                #endif

                isPressedToAWall = wallCheck.collider != null;

                if (isPressedToAWall)
                {
                    wallSlideSetDirection = wallSlideDirection;
                }

                break;
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            isTryingToRun = true;
        }
        else
        {
            isTryingToRun = false;
        }
    }

    #endregion

    #endregion

    #region Visuals

    void GraphicHandling()
    {
        AnimationHandling();
        transform.localScale = new Vector2(direction, transform.localScale.y);
    }

    void AnimationHandling()
    {
        if (currentMovementState == MovementStates.Dash || currentMovementState == MovementStates.ExitDash)
        {
            anim.Play(dashAnimation.ToString());
            return;
        }

        if (isGrounded)
        {
            isJumping = false;
            isFalling = false;

            landingTimer -= Time.deltaTime;

            if (transform.localScale.x != direction)
            {
                anim.Play(turnTransitionAnimation.ToString());
                absoluteTurnLockTimer = absoluteTurnLockCooldown;
                lockTurn = true;
                return;
            }
            else if (lockTurn == true && (absoluteTurnLockTimer > 0 || currentMovementState == MovementStates.Walk))
            {
                absoluteTurnLockTimer -= Time.deltaTime;
                return;
            }
            else
            {
                lockTurn = false;

                if (wallSlideDirection == direction && isPressedToAWall && isTryingToRun)
                {
                    anim.Play(wallPressAnimation.ToString());
                    return;
                }
                else
                {
                    if (currentMovementState == MovementStates.Walk && isTryingToRun)
                    {
                        if (inAir)
                        {
                            anim.Play(landingRunTransitionAnimation.ToString());
                            inAir = false;
                        }
                        else if (isIdling)
                        {
                            anim.Play(runTransitionAnimation.ToString());
                            isIdling = false;
                        }

                        return;
                    }
                    else if (!isTryingToRun)
                    {
                        if (inAir)
                        {
                            fallTime = Mathf.Clamp(fallTime, 0, maxFallTime);
                            landingTimer = (fallTime * landingCooldownMultiplier) + minLandingTime;
                            anim.Play(landingAnimation.ToString());
                            fallTime = 0;
                            inAir = false;
                        }
                        else if (landingTimer < 0)
                        {
                            anim.Play(idleAnimation.ToString());
                        }

                        isIdling = true;
                        return;
                    }
                }

                fallTime = 0;
            }
        }
        else
        {
            lockTurn = false;
            inAir = true;

            switch (currentMovementState)
            {
                case MovementStates.Jump: // JUMP

                    if (!isJumping)
                    {
                        anim.Play(jumpAnimation.ToString());
                        isJumping = true;
                        isFalling = false;
                    }

                    break;

                case MovementStates.Fall: // FALL

                    if (!isFalling)
                    {
                        anim.Play(fallAnimation.ToString());
                        isFalling = true;
                        isJumping = false;
                    }

                    fallTime += Time.deltaTime;

                    break;

                case MovementStates.WallSlide: // WALL SLIDE

                    anim.Play(wallSlideAnimation.ToString());
                    isJumping = false;
                    isFalling = false;

                    break;

                case MovementStates.WallJump: // WALL JUMP

                    if (!isJumping)
                    {
                        anim.Play(wallJumpAnimation.ToString());
                        isJumping = true;
                        isFalling = false;
                    }

                    break;
            }
        }
    }

    #endregion

    #region StateMachine

    #region StateMachineHandling

    void StateMachineHandling()
    {
        switch (currentMovementState)
        {
            case MovementStates.Idle: // IDLE

                WhileIdling();
                CheckForMovement();
                CheckForAbilities();

                break;

            case MovementStates.Walk: // WALK / RUN

                WhileWalking();
                CheckForMovement();
                CheckForAbilities();

                break;

            case MovementStates.Jump: // JUMP

                CheckForHorizontalMovement();
                WhileJumping();
                CheckForAbilities();

                break;

            case MovementStates.Fall: // FALL

                Fall();
                WhileFalling();
                CheckForHorizontalMovement();
                CheckForAbilities();

                break;

            case MovementStates.Dash: // DASH / BASH

                CheckForDashChaining();
                CheckingForDashExits();

                break;

            case MovementStates.ExitDash: // EXIT DASH / EXIT BASH

                ExitingDash();
                CheckForHorizontalMovement();
                CheckForAbilities();

                break;

            case MovementStates.WallSlide: // WALL SLIDE

                WhileWallSliding();
                CheckForHorizontalMovement();
                CheckForAbilities();

                break;

            case MovementStates.WallJump: // WALL JUMP

                wallJumpLockTimer -= Time.deltaTime;

                WhileJumping();

                if (wallJumpLockTimer < 0)
                {
                    CheckForHorizontalMovement();
                }

                CheckForAbilities();

                break;
        }
    }

    #endregion

    #region MainMovementChecks

    #region MovementChecks

    void CheckForMovement()
    {
        CheckForHorizontalMovement();
        CheckForVerticalMovement();
    }

    void CheckForHorizontalMovement()
    {
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) //right movement
        {
            direction = 1;

            if (wallJumpEnabled)
            {
                CheckForWallJump();
            }

            Movement();
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) //left movement
        {
            direction = -1;

            if (wallJumpEnabled)
            {
                CheckForWallJump();
            }

            Movement();
        }
        else if (currentMovementState != MovementStates.WallSlide)
        {
            Decelerate(direction, deceleration); //slow down
        }
    }

    void CheckForWallJump()
    {
        if (wallSlideDirection == direction && isPressedToAWall && currentMovementState != MovementStates.WallSlide)
        {
            rb.linearVelocityY = minWallSlideVelocity;
            rb.linearVelocityX = 0;
            currentMovementState = MovementStates.WallSlide;
        }
    }

    void CheckForVerticalMovement()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && currentMovementState != MovementStates.Jump) //jump
        {
            Jump();
        }

        if (rb.linearVelocityY < -0.1 && currentMovementState != MovementStates.Dash && currentMovementState != MovementStates.WallSlide) //fall
        {
            SetStateToFall();
        }
        else if (currentMovementState != MovementStates.Jump)
        {
            rb.gravityScale = initialGravity;
        }
    }

    void CheckForAbilities()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            bashInputBufferCounter = bashInputBuffer;
        }

        if (bashInputBufferCounter > 0 && bashEnabled)
        {
            bashInputBufferCounter -= Time.deltaTime;
            SearchForBashTargets();
        }

        if (Input.GetKeyDown(KeyCode.Space) && (!isGrounded || (isGrounded && currentMovementState == MovementStates.WallSlide)))
        {
            wallJumpBufferCounter = wallJumpBuffer;
        }

        wallJumpCoyoteTimeCounter -= Time.deltaTime;
        wallJumpBufferCounter -= Time.deltaTime;

        if (wallJumpBufferCounter > 0 && (currentMovementState == MovementStates.WallSlide || wallJumpCoyoteTimeCounter > 0) && currentMovementState != MovementStates.WallJump && (!isGrounded || (isGrounded && currentMovementState == MovementStates.WallSlide)))
        {
            wallJumpCoyoteTimeCounter = 0;
            wallJumpBufferCounter = 0;
            WallJump();
        }
    }

    #endregion

    #region Movement

    void Movement()
    {
        currentSpeed += acceleration * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed); // Can't go above max

        // Handling direction change
        if (rb.linearVelocityX * direction < 0) // Moving opposite to desired direction
        {
            if (isGrounded)
            {
                currentSpeed = 0;
            }
            else
            {
                Decelerate(-direction, airTurnDeceleration);
                return;
            }
        }

        rb.linearVelocityX = currentSpeed * direction;

        if (Mathf.Abs(rb.linearVelocityX) < minMovementSpeed && currentSpeed > 0)
        {
            rb.linearVelocityX = minMovementSpeed * direction; //minimal movement value when the button is pressed
        }
    }

    void Decelerate(float dir, float dec)
    {
        currentSpeed -= dec * Time.deltaTime;

        if (rb.linearVelocityX == 0 || (rb.linearVelocityX <= 0 && dir > 0) || (rb.linearVelocityX >= 0 && dir < 0))
        {
            currentSpeed = 0; //makes you stop from going in the opposite direction
        }

        rb.linearVelocityX = currentSpeed * dir;
    }

    #endregion

    #endregion

    #region States

    #region Idle

    void WhileIdling()
    {
        coyoteTimeCounter = coyoteTime;

        if (currentSpeed != 0)
        {
            currentMovementState = MovementStates.Walk;
        }
    }

    #endregion

    #region Walk

    void WhileWalking()
    {
        coyoteTimeCounter = coyoteTime;

        if (rb.linearVelocityX == 0)
        {
            currentMovementState = MovementStates.Idle;
        }
    }

    #endregion

    #region Jump

    void Jump()
    {
        if (isGrounded || coyoteTimeCounter > 0)
        {
            wallJumpBufferCounter = 0;
            wallJumpCoyoteTimeCounter = 0;
            rb.gravityScale = initialGravity;
            rb.linearVelocityY = jumpForce;
            jumpBufferCounter = 0;
            currentMovementState = MovementStates.Jump;
            Invoke("JumpCorrection", 0.1f);
        }
    }

    void JumpCorrection()
    {
        currentMovementState = MovementStates.Jump;
    }

    void WhileJumping()
    {
        if (Mathf.Abs(rb.linearVelocityY) <= hangTimeVelocityThreshold)
        {
            rb.gravityScale = hangTimeGravity; //bonus air time
            SetStateToFall();
        }

        coyoteTimeCounter = 0;

        if (rb.linearVelocityY < -0.01) //fall
        {
            SetStateToFall();
        }
        else if (!Input.GetKey(KeyCode.Space))
        {
            rb.gravityScale = jumpCutGravity; //cuts the jump when the button is released
        }
    }

    #endregion

    #region Fall

    void SetStateToFall()
    {
        currentMovementState = MovementStates.Fall;
    }

    void Fall()
    {
        rb.gravityScale = fallGravity; //fast fall
        rb.linearVelocityY = Mathf.Clamp(rb.linearVelocityY, -maxFallSpeed, 0); //can't fall faster than max
        currentMovementState = MovementStates.Fall;
    }

    void WhileFalling()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime; //can jump even if the button was pressed slightly before hitting the ground
        }

        coyoteTimeCounter -= Time.deltaTime; //can jump for a short period after already falling off a ledge

        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && currentMovementState != MovementStates.Jump) //jump
        {
            Jump();
        }

        if (isGrounded)
        {
            currentMovementState = MovementStates.Idle;
        }
    }

    #endregion

    #region Bash

    void SearchForBashTargets() 
    {
        Vector2 boxSize = new Vector2(col.size.x * bashDetectionRangeX, col.size.y * bashDetectionRangeY); 
        Collider2D[] targets = Physics2D.OverlapBoxAll(transform.position, boxSize, transform.eulerAngles.z, bashTargetLayer);
        float closestDistance = Mathf.Infinity;
        Transform selectedTarget = null;

        foreach (Collider2D target in targets) 
        {
            float distance = Vector2.Distance(transform.position, target.transform.position);

            if (distance >= closestDistance) continue;

            selectedTarget = target.transform;
            closestDistance = distance;
        }

        if (selectedTarget != null)
        {
            Bash(selectedTarget);
        }
    }

    void Bash(Transform target)
    {
        transform.position = target.position;
        isGrounded = false;
        currentMovementState = MovementStates.Dash;
        bashInputBufferCounter = 0;
        trail.emitting = true;
        coyoteTimeCounter = 0;
        jumpBufferCounter = 0;
        rb.gravityScale = bashGravity;
        transform.position = target.position;
        rb.linearVelocity = Vector2.zero;
        bashLockTimer = bashLockTime;
        bashTimer = bashTime;
        bashDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (bashDir.x == 0 && bashDir.y == 0)
        {
            bashDir.y = 1;
        }
    }

    void CheckForDashChaining()
    {
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift))
        {
            bashInputBufferCounter = bashInputBuffer;
        }

        if (bashInputBufferCounter > 0)
        {
            bashInputBufferCounter -= Time.deltaTime;
            SearchForBashTargets();
        }
    }

    void CheckingForDashExits()
    {
        if (isGrounded && bashDir.y < 0)
        {
            rb.linearVelocityY = 0;
            currentMovementState = MovementStates.ExitDash;
            return;
        }

        if (bashTimer <= 0)
        {
            // Only apply upward velocity if we're in the air
            if (!isGrounded && rb.linearVelocityY > 0)
            {
                if (rb.linearVelocityX == 0)
                {
                    rb.linearVelocityY = maxSpeed * 1.5f;
                }
                else
                {
                    rb.linearVelocityY = maxSpeed;
                }
            }

            rb.gravityScale = jumpCutGravity;
            currentMovementState = MovementStates.ExitDash;
        }
        else
        {
            rb.linearVelocity = launchVelocity * bashDir;
            bashTimer -= Time.deltaTime;
        }

        if (bashLockTimer <= 0)
        {
            CheckForDashCancelling();
            CheckForAbilities();

            if ((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) && rb.linearVelocityY > 0)
            {
                rb.linearVelocityY = maxSpeed;
                currentMovementState = MovementStates.ExitDash;
            }
        }
        else
        {
            bashLockTimer -= Time.deltaTime;
        }
    }

    void CheckForDashCancelling()
    {
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) //right movement
        {
            direction = 1;
        }

        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) //left movement
        {
            direction = -1;
        }

        int bashX = Mathf.Abs(bashDir.x) < 0.1f ? 0 : (int)Mathf.Sign(bashDir.x);

        if (direction != bashX && bashX != 0)
        {
            rb.linearVelocityY = maxSpeed;
            currentMovementState = MovementStates.ExitDash;
        }
    }

    void ExitingDash()
    {
        rb.gravityScale = exitBashGravity;

        if (rb.linearVelocityY <= hangTimeVelocityThreshold)
        {
            rb.gravityScale = hangTimeGravity; //bonus air time
            SetStateToFall();
        }

        trail.emitting = false;
    }

    #endregion

    #region WallJump

    void WhileWallSliding()
    {
        wallJumpCoyoteTimeCounter = wallJumpCoyoteTime;
        rb.gravityScale = wallSlideGravity;
        rb.linearVelocityY = Mathf.Clamp(rb.linearVelocityY, -maxWallSlideSpeed, 0); //can't slide faster than max

        if (!isPressedToAWall || wallSlideDirection != direction)
        {
            SetStateToFall();
        }
    }

    void WallJump()
    {
        switch (wallSlideSetDirection)
        {
            case 1:

                wallJumpDirection = new Vector2(-1, 1);

                break;

            case -1:

                wallJumpDirection = new Vector2(1, 1);

                break;
        }

        currentMovementState = MovementStates.WallJump;
        transform.position = new Vector2(transform.position.x + (0.35f * wallJumpDirection.x), transform.position.y);
        wallJumpLockTimer = wallJumpLockTime;
        rb.linearVelocityY = wallJumpVerticalForce;
        rb.linearVelocityX = wallJumpDirection.x * wallJumpHorizontalForce;
        rb.gravityScale = initialGravity;
        wallJumpBufferCounter = 0;
        jumpBufferCounter = 0;
        coyoteTimeCounter = 0;
    }

    #endregion

    #endregion

    #endregion
}

#region CustomEnums

public enum MovementStates
{
    Idle, Walk, Jump, Fall, Dash, ExitDash, WallSlide, WallJump
}

public enum PlayerAnimations
{
    staticIdleSketch, idleSketch, 
    testRunSketch, runSketch, 
    jumpStartSketch, jumpEndSketch, 
    fallStartSketch, fallEndSketch, 
    landingSketch, runTransitionSketch,
    turnTransitionSketch, landingRunTransitionSketch
}

#endregion