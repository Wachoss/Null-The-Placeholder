using UnityEngine;

// Canales de color del Chroma Bus (Sector 1).
public enum ChromaChannel { None, Cyan, Magenta, Yellow }

/// <summary>
/// Controlador 2D de Null: The Placeholder.
/// - Carrera con aceleración / deceleración (inercia) distinta en suelo y aire.
/// - Salto de altura variable + coyote time + jump buffer.
/// - Wall Slide y Wall Jump (desbloqueo del Sector 0).
/// - Chromatic Dash horizontal: mientras dura, Null es inmune a obstáculos
///   y barreras de su mismo canal de color (Sector 1).
/// Requiere: Rigidbody2D (Gravity Scale ~4, Freeze Rotation Z, Interpolate),
/// BoxCollider2D y un Animator. Lee los controles con NullInput (Input System nuevo o clásico).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class NullController2D : MonoBehaviour
{
    [Header("Carrera (inercia)")]
    public float maxSpeed = 8f;
    public float groundAcceleration = 80f;
    public float groundDeceleration = 70f;
    public float airAcceleration = 45f;
    public float airDeceleration = 25f;

    [Header("Salto")]
    public float jumpVelocity = 15f;
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.45f; // soltar el botón corta el salto
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.12f;
    public float maxFallSpeed = 20f;

    [Header("Wall Slide / Wall Jump")]
    public bool wallJumpUnlocked = true;
    public float wallSlideSpeed = 3f;
    public Vector2 wallJumpVelocity = new Vector2(9f, 14f);
    public float wallJumpControlLock = 0.15f; // tiempo sin control horizontal tras el wall jump

    [Header("Chromatic Dash")]
    public bool dashUnlocked = true;
    public float dashSpeed = 22f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.25f;
    public ChromaChannel currentChannel = ChromaChannel.Cyan;

    [Header("Detección")]
    public LayerMask groundMask;
    public float probeDistance = 0.06f;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    // Estado público (lo leen hazards, barreras, enemigos y la cámara)
    public bool IsDashing { get; private set; }
    public bool IsGrounded { get; private set; }
    public int FacingDir { get; private set; } = 1;
    public bool InputLocked { get; set; }

    Rigidbody2D rb;
    BoxCollider2D box;
    float inputX;
    float lastGroundedTime = -10f;
    float lastJumpPressedTime = -10f;
    float wallJumpLockUntil;
    float dashEndTime;
    float nextDashTime;
    bool dashAvailable = true;  // se recarga al tocar suelo o pared
    int wallDir;                // -1 pared izquierda, 1 derecha, 0 ninguna
    float defaultGravity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        defaultGravity = rb.gravityScale;
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (InputLocked) { inputX = 0; return; }

        inputX = NullInput.Horizontal();
        if (NullInput.JumpDown()) lastJumpPressedTime = Time.time;
        if (NullInput.JumpUp() && rb.linearVelocity.y > 0f && !IsDashing)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        // Cambio de canal de color (1/2/3 directo) para el Chromatic Dash
        if (NullInput.Channel1()) currentChannel = ChromaChannel.Cyan;
        if (NullInput.Channel2()) currentChannel = ChromaChannel.Magenta;
        if (NullInput.Channel3()) currentChannel = ChromaChannel.Yellow;
        // Q / E: canal anterior / siguiente (más cómodo con WASD)
        if (NullInput.ChannelNext()) currentChannel = CycleChannel(currentChannel, 1);
        if (NullInput.ChannelPrev()) currentChannel = CycleChannel(currentChannel, -1);

        if (dashUnlocked && dashAvailable && Time.time >= nextDashTime &&
            NullInput.DashDown())
            StartDash();

        UpdateVisuals();
    }

    void FixedUpdate()
    {
        CheckSurroundings();

        if (IsDashing)
        {
            if (Time.time >= dashEndTime) EndDash();
            else { rb.linearVelocity = new Vector2(FacingDir * dashSpeed, 0f); return; }
        }

        // --- Movimiento horizontal con inercia ---
        if (Time.time >= wallJumpLockUntil)
        {
            float target = inputX * maxSpeed;
            bool accelerating = Mathf.Abs(inputX) > 0.01f;
            float rate = IsGrounded
                ? (accelerating ? groundAcceleration : groundDeceleration)
                : (accelerating ? airAcceleration : airDeceleration);
            float vx = Mathf.MoveTowards(rb.linearVelocity.x, target, rate * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
            if (accelerating) FacingDir = inputX > 0 ? 1 : -1;
        }

        // --- Salto / Wall Jump (con buffer y coyote time) ---
        bool jumpBuffered = Time.time - lastJumpPressedTime <= jumpBufferTime;
        if (jumpBuffered)
        {
            if (Time.time - lastGroundedTime <= coyoteTime)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
                lastJumpPressedTime = lastGroundedTime = -10f;
            }
            else if (wallJumpUnlocked && wallDir != 0)
            {
                rb.linearVelocity = new Vector2(-wallDir * wallJumpVelocity.x, wallJumpVelocity.y);
                FacingDir = -wallDir;
                wallJumpLockUntil = Time.time + wallJumpControlLock;
                lastJumpPressedTime = -10f;
            }
        }

        // --- Wall Slide ---
        bool pushingIntoWall = wallDir != 0 && Mathf.Sign(inputX) == wallDir && Mathf.Abs(inputX) > 0.01f;
        if (wallJumpUnlocked && !IsGrounded && pushingIntoWall && rb.linearVelocity.y < -wallSlideSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);

        if (rb.linearVelocity.y < -maxFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
    }

    void CheckSurroundings()
    {
        Bounds b = box.bounds;
        Vector2 size = new Vector2(b.size.x * 0.9f, b.size.y * 0.9f);

        IsGrounded = Physics2D.BoxCast(b.center, size, 0f, Vector2.down, probeDistance + b.extents.y * 0.1f, groundMask);
        if (IsGrounded) { lastGroundedTime = Time.time; dashAvailable = true; }

        bool left = Physics2D.BoxCast(b.center, size, 0f, Vector2.left, probeDistance + b.extents.x * 0.1f, groundMask);
        bool right = Physics2D.BoxCast(b.center, size, 0f, Vector2.right, probeDistance + b.extents.x * 0.1f, groundMask);
        wallDir = IsGrounded ? 0 : (right ? 1 : left ? -1 : 0);
        if (wallDir != 0 && wallJumpUnlocked) dashAvailable = true;
    }

    void StartDash()
    {
        IsDashing = true;
        dashAvailable = false;
        dashEndTime = Time.time + dashDuration;
        nextDashTime = dashEndTime + dashCooldown;
        if (Mathf.Abs(inputX) > 0.01f) FacingDir = inputX > 0 ? 1 : -1;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(FacingDir * dashSpeed, 0f);
        ChromaBarrier.SetPassThrough(box, currentChannel, true);
        if (animator) animator.SetTrigger("Dash");
    }

    void EndDash()
    {
        IsDashing = false;
        rb.gravityScale = defaultGravity;
        rb.linearVelocity = new Vector2(FacingDir * maxSpeed, 0f);
        ChromaBarrier.SetPassThrough(box, currentChannel, false);
    }

    /// <summary>¿Es inmune ahora mismo a un obstáculo de este color?</summary>
    public bool IsImmuneTo(ChromaChannel hazardChannel)
    {
        return IsDashing && hazardChannel != ChromaChannel.None && hazardChannel == currentChannel;
    }

    /// <summary>Lo llama NullRespawn al morir: corta todo el estado.</summary>
    public void ResetState()
    {
        if (IsDashing) EndDash();
        rb.linearVelocity = Vector2.zero;
        dashAvailable = true;
        lastJumpPressedTime = lastGroundedTime = -10f;
        wallJumpLockUntil = 0f;
    }

    void UpdateVisuals()
    {
        if (spriteRenderer) spriteRenderer.flipX = FacingDir < 0;
        if (!animator) return;
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x) / maxSpeed);
        animator.SetFloat("VerticalSpeed", rb.linearVelocity.y);
        animator.SetBool("Grounded", IsGrounded);
        animator.SetBool("WallSlide", wallDir != 0 && !IsGrounded);
        animator.SetBool("Dashing", IsDashing);
    }

    // Cian -> Magenta -> Amarillo -> Cian ...
    static ChromaChannel CycleChannel(ChromaChannel c, int dir)
    {
        int i = c == ChromaChannel.None ? 0 : (int)c - 1;   // 0..2
        i = (i + dir + 3) % 3;
        return (ChromaChannel)(i + 1);
    }

    public static Color ToColor(ChromaChannel c)
    {
        switch (c)
        {
            case ChromaChannel.Cyan: return new Color(0f, 1f, 1f);
            case ChromaChannel.Magenta: return new Color(1f, 0f, 1f);
            case ChromaChannel.Yellow: return new Color(1f, 1f, 0f);
            default: return Color.white;
        }
    }
}
