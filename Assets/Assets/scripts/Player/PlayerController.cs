using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    #region General

    #region Variables

    //ESSENTIALS
    private PlayerManager pM;
    private Rigidbody2D rb;
    private BoxCollider2D col;

    public KeybindManager keybindManager;

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
    public PlayerAnimations upBashAnimation;
    public PlayerAnimations upBashToFallTransitionAnimation;
    public PlayerAnimations sideBashAnimation;
    public PlayerAnimations sideBashFlipAnimation;
    public PlayerAnimations sideBashToFallTransitionAnimation;
    public PlayerAnimations diagonalUpBashAnimation;
    public PlayerAnimations diagonalDownBashAnimation;
    public PlayerAnimations wallSlideStartAnimation;
    public PlayerAnimations wallSlideEndAnimation;
    public PlayerAnimations wallJumpAnimation;
    public PlayerAnimations wallPressAnimation;
    public PlayerAnimations runTransitionAnimation;
    public PlayerAnimations landingRunTransitionAnimation;
    public PlayerAnimations turnTransitionAnimation;
    public PlayerAnimations knockbackedAnimation;

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
    private bool isBashing;
    private string bashType;
    public float bonusBashLockTime;
    public float wallSlideAnimationSwitchVelocityThreshold;

    [Header("Effects")]
    public TrailRenderer trail;

    [Header("Movement")]

    public MovementStates currentMovementState;

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
    public bool isAbsolutelySafelyGrounded;
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
    public float baseLaunchVelocity;
    public float launchVelocityDivider;
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
    public float bashDirectionChangeGrace;

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
    public float wallJumpBuffer;
    private float wallJumpBufferCounter;
    public float wallJumpCoyoteTime;
    private float wallJumpCoyoteTimeCounter;
    public float maxWallSlideSpeed;
    public float wallJumpHorizontalForce;
    public float wallJumpVerticalForce;
    public float wallLeapHorizontalForce;
    public float wallLeapVerticalForce;
    public float wallFlipHorizontalForce;
    public float wallFlipVerticalForce;

    [Header("Knockback stun")]
    public float knockbackedHorizontalForce;
    public float knockbackedVerticalForce;
    public float knockbackedCooldown;
    public float knockbackedTimer;
    public float knockbackedStun;
    public float knockbackedStunTimer;
    public int knockbackedXDir;

    [Header("Attack pushback")]
    public float attackPushbackForce;
    public float attackPushbackCooldown;
    private float attackPushbackTimer;
    private float attackPushbackDirection;

    [Header("Combat")]

    public CombatStates currentCombatState;
    public AttackTypes currentAttackType;
    public float attackInputBuffer;
    private float attackInputBufferTimer;
    private bool attackCancel;
    private float prevDirection;

    [Header("Melee")]
    public PlayerSlashManager meleeWeapon;

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
        attackPushbackTimer = attackPushbackCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        MovementStateMachineHandling();
        CombatStateMachineHandling();
        FunctionalityHandling();
        GraphicHandling();
    }

    #endregion

    #region StandaloneChecks

    void FunctionalityHandling()
    {
        RaycastHit2D groundCheck = Physics2D.Raycast(new Vector2(col.bounds.min.x + 0.3f, col.bounds.min.y - groundCheckDistance), Vector2.right, col.bounds.max.x - col.bounds.min.x - 0.55f, groundLayer); //checking in a horizontal line right bellow player's feet
        isGrounded = groundCheck.collider != null;

        if (isGrounded)
        {
            // How far to the sides to check
            float sideOffset = 1f;  // distance to the left/right of the player's center
            float extraCheckDistance = 0.1f;  // how far below to check for ground

            // Base position at foot level
            float footY = col.bounds.min.y;
            float checkDistance = groundCheckDistance + extraCheckDistance;

            // Left and right start positions
            Vector2 leftStart = new Vector2(col.bounds.center.x - sideOffset, footY);
            Vector2 rightStart = new Vector2(col.bounds.center.x + sideOffset, footY);

            // Cast rays straight down from both sides
            RaycastHit2D leftHit = Physics2D.Raycast(leftStart, Vector2.down, checkDistance, groundLayer);
            RaycastHit2D rightHit = Physics2D.Raycast(rightStart, Vector2.down, checkDistance, groundLayer);

            // Require both sides to have ground
            isAbsolutelySafelyGrounded = leftHit.collider != null && rightHit.collider != null;

            #if UNITY_EDITOR
            Debug.DrawRay(leftStart, Vector2.down * checkDistance, Color.red);
            Debug.DrawRay(rightStart, Vector2.down * checkDistance, Color.red);
            #endif
        }

        #if UNITY_EDITOR
        Debug.DrawLine(new Vector2(col.bounds.min.x, col.bounds.min.y - groundCheckDistance), new Vector2(col.bounds.max.x, col.bounds.min.y - groundCheckDistance), Color.red);
        #endif

        RaycastHit2D wallCheck = new RaycastHit2D();

        switch (direction)
        {
            case 1:

                wallSlideDirection = 1;
                wallCheck = Physics2D.Raycast(new Vector2(col.bounds.max.x + 0.6f, col.bounds.max.y - 0.3f), Vector2.down, col.bounds.max.y - col.bounds.min.y - 0.9f, groundLayer);

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
                wallCheck = Physics2D.Raycast(new Vector2(col.bounds.min.x - 0.6f, col.bounds.max.y - 0.3f), Vector2.down, col.bounds.max.y - col.bounds.min.y - 0.9f, groundLayer);

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

        if (Input.GetKey(keybindManager.Left) || Input.GetKey(keybindManager.Right))
        {
            isTryingToRun = true;
        }
        else
        {
            isTryingToRun = false;
        }

        if (prevDirection != direction && currentCombatState == CombatStates.Attack)
        {
            attackCancel = true;
        }

        prevDirection = direction;
    }

    private int GetAxis(KeyCode negative, KeyCode positive)
    {
        if (Input.GetKey(negative)) return -1;
        if (Input.GetKey(positive)) return 1;
        return 0;
    }

    #endregion

    #endregion

    #region Visuals

    void GraphicHandling()
    {
        AnimationHandling();

        if (currentMovementState == MovementStates.Dash && bashDir.x != 0)
        {
            if (bashDir.y != 0)
            {
                Vector2 fixedBashDir = bashDir / launchVelocityDivider;
                transform.localScale = new Vector2(fixedBashDir.x, transform.localScale.y);
            }
            else
            {
                transform.localScale = new Vector2(bashDir.x, transform.localScale.y);
            }
        }
        else
        {
            transform.localScale = new Vector2(direction, transform.localScale.y);
        }
    }

    void AnimationHandling()
    {
        if (currentMovementState == MovementStates.Knockbacked)
        {
            PlayAnimation(knockbackedAnimation.ToString());
        }
        else if (currentMovementState == MovementStates.Dash || currentMovementState == MovementStates.ExitDash)
        {
            DetermineBashType();
            isBashing = true;
            isJumping = false;
            isFalling = false;
            fallTime = 0;
            return;
        }

        if (isGrounded)
        {
            HandleGroundedAnimations();
        }
        else
        {
            HandleAirbornAnimations();
        }
    }

    void DetermineBashType()
    {
        if (bashDir.x != 0)
        {
            if (currentMovementState == MovementStates.ExitDash || bashLockTimer < -bonusBashLockTime)
            {
                PlayAnimation(sideBashFlipAnimation.ToString());
            }
            else
            {
                Vector2 fixedBashDir = bashDir / launchVelocityDivider;
                int yDir = Mathf.RoundToInt(fixedBashDir.y);

                switch (yDir)
                {
                    case 1:

                        PlayAnimation(diagonalUpBashAnimation.ToString());

                        break;

                    case 0:

                        PlayAnimation(sideBashAnimation.ToString());

                        break;

                    case -1:

                        PlayAnimation(diagonalDownBashAnimation.ToString());

                        break;
                }
            }

            bashType = "sideBash";
        }
        else
        {
            PlayAnimation(upBashAnimation.ToString());
            bashType = "upBash";
        }
    }

    void HandleGroundedAnimations()
    {
        isBashing = false;
        isJumping = false;
        isFalling = false;

        landingTimer -= Time.deltaTime;

        if (transform.localScale.x != direction)
        {
            PlayAnimation(turnTransitionAnimation.ToString());
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
                PlayAnimation(wallPressAnimation.ToString());
                return;
            }
            else
            {
                if (currentMovementState == MovementStates.Walk && isTryingToRun)
                {
                    if (inAir)
                    {
                        PlayAnimation(landingRunTransitionAnimation.ToString());
                        inAir = false;
                    }
                    else if (isIdling)
                    {
                        PlayAnimation(runTransitionAnimation.ToString());
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
                        PlayAnimation(landingAnimation.ToString());
                        fallTime = 0;
                        inAir = false;
                    }
                    else if (landingTimer < 0)
                    {
                        PlayAnimation(idleAnimation.ToString());
                    }

                    isIdling = true;
                    return;
                }
            }

            fallTime = 0;
        }
    }

    void HandleAirbornAnimations()
    {
        lockTurn = false;
        inAir = true;

        switch (currentMovementState)
        {
            case MovementStates.Jump: // JUMP

                if (!isJumping)
                {
                    PlayAnimation(jumpAnimation.ToString());
                    isJumping = true;
                    isFalling = false;
                }

                break;

            case MovementStates.Fall: // FALL

                if (!isFalling)
                {
                    if (isBashing)
                    {
                        if (bashType == "sideBash")
                        {
                            PlayAnimation(sideBashToFallTransitionAnimation.ToString());
                        }
                        else if (bashType == "upBash")
                        {
                            PlayAnimation(upBashToFallTransitionAnimation.ToString());
                        }

                        isBashing = false;
                        isFalling = true;
                        isJumping = false;
                        return;
                    }

                    PlayAnimation(fallAnimation.ToString());
                    isFalling = true;
                    isJumping = false;
                }

                fallTime += Time.deltaTime;

                break;

            case MovementStates.WallSlide: // WALL SLIDE

                if (rb.linearVelocityY < wallSlideAnimationSwitchVelocityThreshold)
                {
                    PlayAnimation(wallSlideEndAnimation.ToString());
                }
                else
                {
                    PlayAnimation(wallSlideStartAnimation.ToString());
                }

                fallTime = 0;
                isJumping = false;
                isFalling = false;

                break;

            case MovementStates.WallJump: // WALL JUMP

                if (!isJumping)
                {
                    PlayAnimation(wallJumpAnimation.ToString());
                    fallTime = 0;
                    isJumping = true;
                    isFalling = false;
                }

                break;
        }

        isBashing = false;
    }

    void PlayAnimation(string animName)
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(animName))
            anim.Play(animName);
    }

    #endregion

    #region MovementStateMachine

    #region MovementStateMachineHandling

    void MovementStateMachineHandling()
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

            case MovementStates.Knockbacked: // KNOCKBACKED

                WhileKnockbacked();

                break;

            case MovementStates.AttackPushback: // ATTACK PUSHBACK

                WhilePushedBack();

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
        if (Input.GetKey(keybindManager.Right)) //right movement
        {
            direction = 1;

            if (wallJumpEnabled)
            {
                CheckForWallJump();
            }

            Movement();
        }
        else if (Input.GetKey(keybindManager.Left)) //left movement
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
        if (Input.GetKeyDown(keybindManager.Jump))
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
        if (Input.GetKeyDown(keybindManager.Bash))
        {
            bashInputBufferCounter = bashInputBuffer;
        }

        if (bashInputBufferCounter > 0 && bashEnabled)
        {
            bashInputBufferCounter -= Time.deltaTime;
            SearchForBashTargets();
        }

        if (Input.GetKeyDown(keybindManager.Jump) && (!isGrounded || (isGrounded && currentMovementState == MovementStates.WallSlide)))
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

    #region MovementStates

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
        else if (!Input.GetKey(keybindManager.Jump))
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
        if (Input.GetKeyDown(keybindManager.Jump))
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
        // Core setup (same as before)
        transform.position = target.position;
        isGrounded = false;
        currentMovementState = MovementStates.Dash;
        bashInputBufferCounter = 0;
        trail.emitting = true;
        coyoteTimeCounter = 0;
        jumpBufferCounter = 0;
        rb.gravityScale = bashGravity;
        rb.linearVelocity = Vector2.zero;
        bashLockTimer = bashLockTime;
        bashTimer = bashTime;

        SetBashDirection();

        Invoke("SetBashDirection", bashDirectionChangeGrace);
    }

    void SetBashDirection()
    {
        // Use helper
        int x = GetAxis(keybindManager.Left, keybindManager.Right);
        int y = GetAxis(keybindManager.Down, keybindManager.Up);

        bashDir = new Vector2(x, y);

        if (bashDir == Vector2.zero)
        {
            bashDir = Vector2.up;
        }
        else if (bashDir.x != 0 && bashDir.y != 0)
        {
            bashDir *= launchVelocityDivider;
        }
    }

    void CheckForDashChaining()
    {
        if (Input.GetKeyDown(keybindManager.Bash))
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
            rb.linearVelocity = baseLaunchVelocity * bashDir;
            bashTimer -= Time.deltaTime;
        }

        if (bashLockTimer <= 0)
        {
            CheckForDashCancelling();
            CheckForAbilities();

            if ((Input.GetKey(keybindManager.Down)) && rb.linearVelocityY > 0)
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
        if (Input.GetKey(keybindManager.Right)) //right movement
        {
            direction = 1;
        }

        else if (Input.GetKey(keybindManager.Left)) //left movement
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
        rb.linearVelocityX = 0;

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

        transform.position = new Vector2(transform.position.x + (0.35f * wallJumpDirection.x), transform.position.y);
        wallJumpLockTimer = wallJumpLockTime;
        currentMovementState = MovementStates.WallJump;
        rb.linearVelocityY = wallJumpVerticalForce;
        rb.linearVelocityX = wallJumpDirection.x * wallJumpHorizontalForce;
        rb.gravityScale = initialGravity;
        wallJumpBufferCounter = 0;
        jumpBufferCounter = 0;
        coyoteTimeCounter = 0;

        Invoke("CheckForJumpForm", 0.1f);
    }

    void CheckForJumpForm()
    {
        switch (wallJumpDirection.x)
        {
            case 1:

                if (Input.GetKey(keybindManager.Right))
                {
                    LeapFromWall();
                }

                break;

            case -1:

                if (Input.GetKey(keybindManager.Left))
                {
                    LeapFromWall();
                }

                break;
        }
    }

    void LeapFromWall()
    {
        wallJumpLockTimer = wallJumpLockTime;

        StartCoroutine(JumpToLeapVelocityTransition(
            wallJumpDirection.x * wallLeapHorizontalForce,
            wallLeapVerticalForce,
            0.4f // transition duration in seconds
        ));
    }

    IEnumerator JumpToLeapVelocityTransition(float targetXVelocity, float targetYVelocity, float duration)
    {
        float elapsed = 0f;
        Vector2 startVelocity = rb.linearVelocity;
        Vector2 targetVelocity = new Vector2(targetXVelocity, targetYVelocity);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Ease-out cubic: starts fast, slows down as it approaches target
            t = 1f - Mathf.Pow(1f - t, 3);

            rb.linearVelocity = Vector2.Lerp(startVelocity, targetVelocity, t);
            yield return null;
        }

        rb.linearVelocity = targetVelocity; // snap to final value
    }

    #endregion

    #region Knockbacked

    void WhileKnockbacked()
    {
        direction = -knockbackedXDir;
        knockbackedTimer -= Time.deltaTime;
        knockbackedStunTimer -= Time.deltaTime;

        if (knockbackedTimer > 0)
        {
            rb.linearVelocity = new Vector2(knockbackedHorizontalForce * knockbackedXDir, knockbackedVerticalForce);
        }
        else
        {
            rb.linearVelocityX = 3 * knockbackedXDir;
            rb.linearVelocityY = -3;
        }

        if (knockbackedStunTimer < 0)
        {
            SetStateToFall();
        }
    }

    #endregion

    #region AttackPushback

    void WhilePushedBack()
    {
        attackPushbackTimer -= Time.deltaTime;
        rb.linearVelocityX = attackPushbackForce * attackPushbackDirection;

        if (attackPushbackTimer < 0)
        {
            attackPushbackTimer = attackPushbackCooldown;
            currentMovementState = MovementStates.Idle;
        }
    }

    #endregion

    #endregion

    #endregion

    #region CombatStateMachine

    #region CombatStateMachinehandling

    void CombatStateMachineHandling()
    {
        switch (currentCombatState)
        {
            case CombatStates.Neutral: // NEUTRAL

                meleeWeapon.slashCooldownTimer -= Time.deltaTime;

                CheckForAttack();

                break;

            case CombatStates.Attack: // ATTACK

                switch (currentAttackType)
                {
                    case AttackTypes.Melee:

                        if (meleeWeapon.slashCooldownTimer < 0)
                        {
                            attackInputBufferTimer = 0;
                            WhileSlashing();
                        }
                        else
                        {
                            currentCombatState = CombatStates.Neutral;
                        }

                        break;
                }

                break;
        }
    }

    #endregion

    #region CombatChecks

    void CheckForAttack()
    {
        if (Input.GetKeyDown(keybindManager.Attack))
        {
            attackInputBufferTimer = attackInputBuffer;
        }

        if (attackInputBufferTimer > 0)
        {
            meleeWeapon.slashTimer = meleeWeapon.slashDuration;
            currentCombatState = CombatStates.Attack;
        }
    }

    #endregion

    #region CombatStates

    #region Slash

    void WhileSlashing()
    {
        meleeWeapon.slashGraphic.SetActive(true);
        meleeWeapon.slashTimer -= Time.deltaTime;

        for (int i = meleeWeapon.hitObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = meleeWeapon.hitObjects[i];
            EnemyManager objManager = obj.GetComponent<EnemyManager>();
            objManager.Knockback(meleeWeapon.slashKnockback, direction);
            objManager.health -= meleeWeapon.damage;
            currentMovementState = MovementStates.AttackPushback;
            attackPushbackDirection = -direction;
            meleeWeapon.ignoredObjects.Add(obj);
            meleeWeapon.hitObjects.RemoveAt(i);
        }

        if (meleeWeapon.slashTimer < 0 || attackCancel)
        {
            attackCancel = false;
            meleeWeapon.slashGraphic.SetActive(false);
            meleeWeapon.slashCooldownTimer = meleeWeapon.slashCooldown;
            meleeWeapon.ignoredObjects.Clear();
            meleeWeapon.hitObjects.Clear();
            currentCombatState = CombatStates.Neutral;
        }
    }

    #endregion

    #endregion

    #endregion
}

#region CustomEnums

public enum MovementStates
{
    Idle, Walk, Jump, Fall, Dash, ExitDash, WallSlide, WallJump, Knockbacked, AttackPushback
}

public enum CombatStates
{
    Neutral, Attack
}

public enum AttackTypes
{
    Melee
}

public enum PlayerAnimations
{
    // SKETCH ANIMATIONS
    staticIdleSketch, idleSketch, 
    testRunSketch, runSketch, 
    jumpStartSketch, jumpEndSketch, 
    fallStartSketch, fallEndSketch, 
    landingSketch, runTransitionSketch,
    turnTransitionSketch, landingRunTransitionSketch,
    wallPressSketch, upBashSketch,
    upBashToFallTransitionSketch, sideBashSketch,
    sideBashToFallTransitionSketch, sideBashFlipSketch,
    diagonalUpBashSketch, diagonalDownBashSketch,
    wallSlideStartSketch, wallSlideEndSketch,
    wallLeapSketch
}

#endregion