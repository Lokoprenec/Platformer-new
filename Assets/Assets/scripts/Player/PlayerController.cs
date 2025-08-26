using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //ESSENTIALS
    private PlayerManager pM;
    private Rigidbody2D rb;
    private BoxCollider2D col;

    public PlayerStates currentState;

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
    public float landingCooldown;
    private float landingTimer;

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
        //STATE MACHINE
        switch (currentState)
        {
            case PlayerStates.Idle: // IDLE

                CheckForMovement();
                CheckForAbilities();

                coyoteTimeCounter = coyoteTime;

                if (currentState == PlayerStates.Idle)
                {
                    anim.Play(idleAnimation.ToString());
                }

                break;

            case PlayerStates.Walk: // WALK / RUN

                CheckForMovement();
                CheckForAbilities();

                coyoteTimeCounter = coyoteTime;

                if (currentState == PlayerStates.Walk)
                {
                    if (rb.linearVelocityX == 0)
                    {
                        currentState = PlayerStates.Idle;
                    }

                    if (currentState == PlayerStates.Walk)
                    {
                        anim.Play(runAnimation.ToString());
                    }
                }

                break;

            case PlayerStates.Jump: // JUMP

                CheckForHorizontalMovement();
                WhileJumping();
                CheckForAbilities();

                break;

            case PlayerStates.Fall: // FALL

                CheckForHorizontalMovement();
                CheckForAbilities();

                if (currentState == PlayerStates.Fall)
                {
                    Fall();
                    WhileFalling();
                }

                break;

            case PlayerStates.Landing: // LANDING

                coyoteTimeCounter = coyoteTime;

                landingTimer -= Time.deltaTime;

                if (landingTimer <= 0 && currentState == PlayerStates.Landing)
                {
                    currentState = PlayerStates.Idle;
                    landingTimer = landingCooldown;
                }
                else if (currentState == PlayerStates.Landing)
                {
                    anim.Play(landingAnimation.ToString());
                }

                CheckForMovement();
                CheckForAbilities();

                break;

            case PlayerStates.Dash: // DASH / BASH

                CheckForDashChaining();

                if (isGrounded && bashDir.y < 0)
                {
                    rb.linearVelocityY = 0;
                    currentState = PlayerStates.ExitDash;
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
                    currentState = PlayerStates.ExitDash;
                }
                else
                {
                    rb.linearVelocity = launchVelocity * bashDir;
                    bashTimer -= Time.deltaTime;
                }

                if (bashLockTimer <= 0)
                {
                    CheckForHorizontalMovement();
                    CheckForAbilities();

                    if ((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) && rb.linearVelocityY > 0)
                    {
                        rb.linearVelocityY = maxSpeed;
                        currentState = PlayerStates.ExitDash;
                    }
                }
                else
                {
                    bashLockTimer -= Time.deltaTime;
                }

                break;

            case PlayerStates.ExitDash: // EXIT DASH / EXIT BASH

                CheckForHorizontalMovement();
                CheckForAbilities();

                rb.gravityScale = exitBashGravity;

                if (rb.linearVelocityY <= hangTimeVelocityThreshold)
                {
                    rb.gravityScale = hangTimeGravity; //bonus air time
                    SetStateToFall();
                }

                trail.emitting = false;

                break;

            case PlayerStates.WallSlide: // WALL SLIDE

                wallJumpCoyoteTimeCounter = wallJumpCoyoteTime;
                CheckForHorizontalMovement();
                CheckForAbilities();

                if (currentState == PlayerStates.WallSlide)
                {
                    rb.gravityScale = wallSlideGravity;

                    rb.linearVelocityY = Mathf.Clamp(rb.linearVelocityY, -maxWallSlideSpeed, 0); //can't slide faster than max

                    anim.Play(wallSlideAnimation.ToString());
                }

                break;

            case PlayerStates.WallJump: // WALL JUMP

                wallJumpLockTimer -= Time.deltaTime;
                CheckForAbilities();
                WhileJumping();

                if (wallJumpLockTimer < 0)
                {
                    CheckForHorizontalMovement();
                }

                break;
        }

        RaycastHit2D groundCheck = Physics2D.Raycast(new Vector2(col.bounds.min.x + 0.3f, col.bounds.min.y - groundCheckDistance), Vector2.right, col.bounds.max.x - col.bounds.min.x - 0.55f, groundLayer); //checking in a horizontal line right bellow player's feet
        isGrounded = groundCheck.collider != null;
        Debug.DrawLine(new Vector2(col.bounds.min.x, col.bounds.min.y - groundCheckDistance), new Vector2(col.bounds.max.x, col.bounds.min.y - groundCheckDistance), Color.red);

        RaycastHit2D wallCheck = new RaycastHit2D();

        switch (direction)
        {
            case 1:

                wallSlideDirection = 1;
                wallCheck = Physics2D.Raycast(new Vector2(col.bounds.max.x + 0.6f, col.bounds.max.y - 0.3f), Vector2.down, col.bounds.max.y - col.bounds.min.y - 0.6f, groundLayer);
                Debug.DrawLine(new Vector2(col.bounds.max.x, col.bounds.max.y - 0.3f), new Vector2(col.bounds.max.x, col.bounds.min.y + 0.3f), Color.red);
                isPressedToAWall = wallCheck.collider != null;

                if (isPressedToAWall)
                {
                    wallSlideSetDirection = wallSlideDirection;
                }

                break;

            case -1:

                wallSlideDirection = -1;
                wallCheck = Physics2D.Raycast(new Vector2(col.bounds.min.x - 0.6f, col.bounds.max.y - 0.3f), Vector2.down, col.bounds.max.y - col.bounds.min.y - 0.6f, groundLayer);
                Debug.DrawLine(new Vector2(col.bounds.min.x, col.bounds.max.y - 0.3f), new Vector2(col.bounds.min.x, col.bounds.min.y + 0.3f), Color.red);
                isPressedToAWall = wallCheck.collider != null;

                if (isPressedToAWall)
                {
                    wallSlideSetDirection = wallSlideDirection;
                }

                break;
        }

        transform.localScale = new Vector2(direction, transform.localScale.y);
    }

    #region Checks

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

            if (wallSlideDirection == direction && isPressedToAWall && currentState != PlayerStates.WallSlide)
            {
                rb.linearVelocityY = minWallSlideVelocity;
                rb.linearVelocityX = 0;
                currentState = PlayerStates.WallSlide;
            }

            Movement();
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) //left movement
        {
            direction = -1;

            if (wallSlideDirection == direction && isPressedToAWall && currentState != PlayerStates.WallSlide)
            {
                rb.linearVelocityY = minWallSlideVelocity;
                rb.linearVelocityX = 0;
                currentState = PlayerStates.WallSlide;
            }

            Movement();
        }
        else if (currentState != PlayerStates.Dash && currentState != PlayerStates.WallSlide)
        {
            Decelerate(direction, deceleration); //slow down
        }

        if (currentState == PlayerStates.WallSlide && (!isPressedToAWall || wallSlideDirection != direction))
        {
            SetStateToFall();
        }
    }

    void CheckForVerticalMovement()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && currentState != PlayerStates.Jump) //jump
        {
            Jump();
        }

        if (rb.linearVelocityY < -0.1 && currentState != PlayerStates.Dash && currentState != PlayerStates.WallSlide) //fall
        {
            SetStateToFall();
        }
        else if (currentState != PlayerStates.Jump)
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

        if (Input.GetKeyDown(KeyCode.Space) && (!isGrounded || (isGrounded && currentState == PlayerStates.WallSlide)))
        {
            wallJumpBufferCounter = wallJumpBuffer;
        }

        wallJumpCoyoteTimeCounter -= Time.deltaTime;
        wallJumpBufferCounter -= Time.deltaTime;

        if (wallJumpBufferCounter > 0 && (currentState == PlayerStates.WallSlide || wallJumpCoyoteTimeCounter > 0) && currentState != PlayerStates.WallJump && (!isGrounded || (isGrounded && currentState == PlayerStates.WallSlide)))
        {
            wallJumpCoyoteTimeCounter = 0;
            wallJumpBufferCounter = 0;
            WallJump();
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

    #endregion

    #region Movement

    void Movement()
    {
        currentSpeed += acceleration * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed); // Can't go above max
        int bashX = Mathf.Abs(bashDir.x) < 0.1f ? 0 : (int)Mathf.Sign(bashDir.x);

        if (currentState != PlayerStates.Dash)
        {
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
        else if (direction != bashX && bashX != 0)
        {
            rb.linearVelocityY = maxSpeed;
            currentState = PlayerStates.ExitDash;
        }

        if (currentState == PlayerStates.Idle || currentState == PlayerStates.Landing)
        {
            currentState = PlayerStates.Walk;
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
            currentState = PlayerStates.Jump;
            anim.Play(jumpAnimation.ToString());
            Invoke("JumpCorrection", 0.1f);
        }
    }

    void JumpCorrection()
    {
        currentState = PlayerStates.Jump;
        anim.Play(jumpAnimation.ToString());
    }

    void WhileJumping()
    {
        if (Mathf.Abs(rb.linearVelocityY) <= hangTimeVelocityThreshold)
        {
            rb.gravityScale = hangTimeGravity; //bonus air time
            SetStateToFall();
        }

        landingTimer = landingCooldown;
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
        anim.Play(fallAnimation.ToString());
        currentState = PlayerStates.Fall;
    }

    void Fall()
    {
        rb.gravityScale = fallGravity; //fast fall
        rb.linearVelocityY = Mathf.Clamp(rb.linearVelocityY, -maxFallSpeed, 0); //can't fall faster than max
        currentState = PlayerStates.Fall;
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

        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && currentState != PlayerStates.Jump) //jump
        {
            Jump();
        }

        if (isGrounded)
        {
            currentState = PlayerStates.Landing;
            landingTimer = landingCooldown;
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
        currentState = PlayerStates.Dash;
        bashInputBufferCounter = 0;
        anim.Play(dashAnimation.ToString());
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

    #endregion

    #region WallJump

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

        currentState = PlayerStates.WallJump;
        transform.position = new Vector2(transform.position.x + (0.35f * wallJumpDirection.x), transform.position.y);
        wallJumpLockTimer = wallJumpLockTime;
        rb.linearVelocityY = wallJumpVerticalForce;
        rb.linearVelocityX = wallJumpDirection.x * wallJumpHorizontalForce;
        anim.Play(wallJumpAnimation.ToString());
        rb.gravityScale = initialGravity;
        wallJumpBufferCounter = 0;
        jumpBufferCounter = 0;
        coyoteTimeCounter = 0;
    }

    #endregion
}

public enum PlayerStates
{
    Idle, Walk, Jump, Fall, Landing, Dash, ExitDash, WallSlide, WallJump
}

public enum PlayerAnimations
{
    staticIdleSketch, idleSketch, 
    testRunSketch, runSketch, 
    jumpStartSketch, jumpEndSketch, 
    fallStartSketch, fallEndSketch, 
    landingSketch
}
