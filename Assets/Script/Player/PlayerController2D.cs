using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour {
    [Header("Movement Settings")]
    public float moveSpeed = 8f;

    [Header("Jump Settings")]
    public float jumpForce = 14f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("Wall Interaction")]
    public float wallSlideSpeed = 2f;
    public Vector2 wallJumpDirection = new Vector2(1f, 1.2f);
    public float wallJumpForce = 14f;

    // --- NEW SETTING ---
    [Tooltip("How long (in seconds) the player must wait before jumping again after a wall jump.")]
    public float wallJumpCooldown = 0.2f;
    private float wallJumpCooldownTimer;
    // -------------------

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashTime = 0.15f;
    public float dashCooldown = 1f;
    [HideInInspector] public float dashCooldownTimer;

    [Header("Mantle (Ledge Climb)")]
    public Transform ledgeCheck;
    public float ledgeCheckDist = 0.5f;
    public Vector2 mantleOffset = new Vector2(0.5f, 1f);
    public float mantleSpeed = 0.5f;

    [Header("Collision Checks (Raycast)")]
    public float rayLength = 0.5f;
    public float rayInset = 0.05f;

    public LayerMask groundLayer;
    public LayerMask hiddenLayerMask;

    [Header("Audio Settings")]
    public string jumpSound = "Jump";
    public string landSound = "Land";
    public string dashSound = "Dash";
    public string wallJumpSound = "WallJump";
    public string mantleSound = "Mantle";

    public AudioSource runningSource;

    [Header("Camera Effects")]
    public CameraSpring cameraSpring;
    public float dashShakeAmount = 15f;
    public float landShakeAmount = 5f;

    [Header("Landing Threshold")]
    public float minFallSpeedToLand = -10f;

    // --- Private Variables ---
    Rigidbody2D rb;
    Animator anim;
    Collider2D col;

    float moveInput;
    bool isGrounded;
    bool wasGrounded;
    bool isTouchingWall;
    bool isWallSticking;
    bool isDashing;
    bool isMantling;

    int wallSide;
    float coyoteCounter;
    float jumpBufferCounter;
    bool usedAirDash;

    float normalGravity;
    Vector3 originalScale;

    float lastYVelocity;

    [HideInInspector] public bool canMove = true;
    [HideInInspector] public float wallJumpLeft = 3;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();

        originalScale = transform.localScale;
        normalGravity = rb.gravityScale;
        groundLayer &= ~hiddenLayerMask;
    }

    void Update() {
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;

        // --- NEW: Manage Wall Jump Cooldown ---
        if (wallJumpCooldownTimer > 0) {
            wallJumpCooldownTimer -= Time.deltaTime;
        }
        // --------------------------------------

        if (!canMove) {
            moveInput = 0;
            anim.SetBool("isRunning", false);
            return;
        }

        if (isDashing || isMantling) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        CheckCollisions();
        CheckLedge();
        HandleTimers();
        HandleJump();
        HandleWallStick();
        HandleDash();
        HandleFlip();

        HandleRunAudio();

        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isRunning", Mathf.Abs(moveInput) > 0.1f);
        anim.SetBool("isWallSticking", isWallSticking);

        // Landing Logic
        if (isGrounded && !wasGrounded) {
            if (lastYVelocity < minFallSpeedToLand) {
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(landSound);
                if (CameraSpring.Instance != null) CameraSpring.Instance.Punch(Vector2.down, landShakeAmount);
            }
        }
        wasGrounded = isGrounded;

        if (isGrounded) {
            usedAirDash = false;
            wallJumpLeft = 3;
        }
    }

    void FixedUpdate() {
        lastYVelocity = rb.velocity.y;

        if (isDashing || isMantling) return;

        // Note: You might want to lock movement here too during wall jump cooldown
        // But for now, we just block the Jump input as requested.
        if (!isWallSticking)
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    void CheckCollisions() {
        // ... (Same as before) ...
        Bounds bounds = col.bounds;
        float xInset = rayInset;
        float yInset = rayInset;

        isGrounded = false;
        Vector2 feetLeft = new Vector2(bounds.min.x + xInset, bounds.min.y);
        Vector2 feetRight = new Vector2(bounds.max.x - xInset, bounds.min.y);

        for (int i = 0; i < 4; i++) {
            float t = i / 3f;
            Vector2 origin = Vector2.Lerp(feetLeft, feetRight, t);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, groundLayer);
            if (hit.collider != null) {
                isGrounded = true;
                break;
            }
        }

        isTouchingWall = false;
        float facingDir = Mathf.Sign(transform.localScale.x);
        Vector2 rayDir = (facingDir > 0) ? Vector2.right : Vector2.left;
        float xPos = (facingDir > 0) ? bounds.max.x : bounds.min.x;
        Vector2 faceTop = new Vector2(xPos, bounds.max.y - yInset);
        Vector2 faceBottom = new Vector2(xPos, bounds.min.y + yInset);

        for (int i = 0; i < 4; i++) {
            float t = i / 3f;
            Vector2 origin = Vector2.Lerp(faceTop, faceBottom, t);
            RaycastHit2D hit = Physics2D.Raycast(origin, rayDir, rayLength, groundLayer);
            if (hit.collider != null && !hit.collider.isTrigger) {
                isTouchingWall = true;
                wallSide = (int)facingDir;
                break;
            }
        }
    }

    void CheckLedge() {
        // ... (Same as before) ...
        if (isTouchingWall && !isGrounded && rb.velocity.y < 0) {
            float dir = Mathf.Sign(transform.localScale.x);
            bool isLedgeDetected = !Physics2D.Raycast(ledgeCheck.position, Vector2.right * dir, ledgeCheckDist, groundLayer);

            if (isLedgeDetected) {
                if (Input.GetAxisRaw("Vertical") > 0 || Input.GetButtonDown("Jump")) {
                    StartCoroutine(MantleRoutine());
                }
            }
        }
    }

    IEnumerator MantleRoutine() {
        // ... (Same as before) ...
        isMantling = true;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(mantleSound);

        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;

        float dir = Mathf.Sign(transform.localScale.x);
        Vector2 startPos = transform.position;
        Vector2 finalPos = startPos + new Vector2(mantleOffset.x * dir, mantleOffset.y);
        Vector2 highPos = new Vector2(startPos.x, finalPos.y);

        float halfDuration = mantleSpeed / 2f;
        float timer = 0;

        while (timer < halfDuration) {
            timer += Time.deltaTime;
            transform.position = Vector2.Lerp(startPos, highPos, timer / halfDuration);
            yield return null;
        }
        transform.position = highPos;

        timer = 0;
        while (timer < halfDuration) {
            timer += Time.deltaTime;
            transform.position = Vector2.Lerp(highPos, finalPos, timer / halfDuration);
            yield return null;
        }
        transform.position = finalPos;

        rb.gravityScale = normalGravity;
        rb.velocity = Vector2.zero;
        isMantling = false;
    }

    void HandleTimers() {
        if (isGrounded) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.deltaTime;

        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;
    }

    void HandleJump() {
        // --- NEW: If logic is cooling down, forbid jump ---
        if (wallJumpCooldownTimer > 0) return;
        // ------------------------------------------------

        if (jumpBufferCounter > 0 && isTouchingWall && !isGrounded && wallJumpLeft > 0) {
            float dir = -wallSide;
            rb.velocity = new Vector2(wallJumpDirection.x * dir * wallJumpForce, wallJumpDirection.y * wallJumpForce);
            anim.SetTrigger("jump");
            isWallSticking = false;
            jumpBufferCounter = 0;
            wallJumpLeft -= 1;

            // --- NEW: Start Cooldown ---
            wallJumpCooldownTimer = wallJumpCooldown;
            // ---------------------------

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(wallJumpSound);

            return;
        }

        if (jumpBufferCounter > 0 && coyoteCounter > 0) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            anim.SetTrigger("jump");
            jumpBufferCounter = 0;

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(jumpSound);
        }
    }

    void HandleWallStick() {
        if (isTouchingWall && !isGrounded && rb.velocity.y <= 0 && wallJumpLeft > 0) {
            isWallSticking = true;
            rb.velocity = new Vector2(0, -wallSlideSpeed);
        } else {
            isWallSticking = false;
        }
    }

    void HandleDash() {
        if (isDashing || dashCooldownTimer > 0) return;
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            if (!isGrounded && usedAirDash) return;
            if (!isGrounded) usedAirDash = true;

            dashCooldownTimer = dashCooldown;
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine() {
        // ... (Same as before) ...
        isDashing = true;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(dashSound);
        if (CameraSpring.Instance != null) CameraSpring.Instance.Punch(new Vector2(transform.localScale.x, 0), dashShakeAmount);

        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        float dir = moveInput == 0 ? Mathf.Sign(transform.localScale.x) : moveInput;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(dir * dashSpeed, 0f);
        anim.SetTrigger("dash");

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = normalGravity;
        rb.velocity = Vector2.zero;
        isDashing = false;

        if (enemyLayer != -1) Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
    }

    void HandleFlip() {
        if (moveInput == 0 || isWallSticking) return;
        float dir = Mathf.Sign(moveInput);
        transform.localScale = new Vector2(Mathf.Abs(originalScale.x) * dir, originalScale.y);
    }

    void HandleRunAudio() {
        if (runningSource == null) return;
        bool isMoving = Mathf.Abs(moveInput) > 0.1f;

        if (isMoving && isGrounded && !isWallSticking && !isDashing && !isMantling && canMove) {
            if (!runningSource.isPlaying) {
                runningSource.pitch = Random.Range(0.9f, 1.1f);
                runningSource.Play();
            }
        } else {
            if (runningSource.isPlaying) runningSource.Stop();
        }
    }

    public void ToggleHiddenLayer(bool isActive) {
        if (isActive) groundLayer |= hiddenLayerMask;
        else groundLayer &= ~hiddenLayerMask;
    }

    void OnDrawGizmos() {
        // ... (Same as before) ...
        if (col == null) col = GetComponent<Collider2D>();
        if (col == null) return;

        Bounds bounds = col.bounds;
        float xInset = rayInset;
        float yInset = rayInset;

        Gizmos.color = Color.red;
        Vector2 gLeft = new Vector2(bounds.min.x + xInset, bounds.min.y);
        Vector2 gRight = new Vector2(bounds.max.x - xInset, bounds.min.y);
        for (int i = 0; i < 4; i++) {
            Vector2 origin = Vector2.Lerp(gLeft, gRight, i / 3f);
            Gizmos.DrawLine(origin, origin + Vector2.down * rayLength);
        }

        float facingDir = Mathf.Sign(transform.localScale.x);
        float xPos = (facingDir > 0) ? bounds.max.x : bounds.min.x;
        Vector2 rayDir = (facingDir > 0) ? Vector2.right : Vector2.left;

        Gizmos.color = Color.yellow;
        Vector2 faceTop = new Vector2(xPos, bounds.max.y - yInset);
        Vector2 faceBottom = new Vector2(xPos, bounds.min.y + yInset);

        for (int i = 0; i < 4; i++) {
            Vector2 origin = Vector2.Lerp(faceTop, faceBottom, i / 3f);
            Gizmos.DrawLine(origin, origin + (Vector2)rayDir * rayLength);
        }

        if (ledgeCheck) {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + Vector3.right * facingDir * ledgeCheckDist);
        }
    }
}