using UnityEngine;

/// <summary>
/// 角色控制器：状态机管理移动、跳跃、锚点、受击、死亡
/// 8 个状态：Idle / Moving / Jumping / Falling / AnchorFire / BeingPulled / Hit / Death
/// 动画系统：Trigger 控制 Throw/Restore/Jump，代码提供 speed / isGround / verticalVelocity 参数
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    // ===== 状态机 =====
    public enum PlayerState
    {
        Idle, Moving, Jumping, Falling, AnchorFire, BeingPulled, Hit, Death
    }

    [HideInInspector] public PlayerState currentState = PlayerState.Idle;
    private float stateTimer;

    // ===== 引用 =====
    [Header("References")]
    public Planet targetPlanet;
    public GameObject anchorPrefab;
    public Animator anim;
    public SpriteRenderer sprite;

    [Header("Config")]
    [SerializeField] private ControlConfig config;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    [Header("Anchor")]
    public float anchorFireDuration = 0.3f;

    [Header("Hit")]
    public float hitDuration = 0.5f;

    [Header("死亡检测")]
    public string damageLayerName = "Damage";

    // ===== 动画参数配置（必须与Animator中的参数名完全一致） =====
    [Header("动画参数配置（必须与Animator参数名一致）")]
    [Tooltip("Animator中speed参数的名称")]
    public string speedParam = "speed";
    [Tooltip("Animator中isGround参数的名称")]
    public string isGroundParam = "isGround";
    [Tooltip("Animator中verticalVelocity参数的名称")]
    public string verticalVelocityParam = "verticalVelocity";
    [Tooltip("Animator中Throw触发器的名称")]
    public string throwTrigger = "Throw";
    [Tooltip("Animator中Restore触发器的名称")]
    public string restoreTrigger = "Restore";
    [Tooltip("Animator中Jump触发器的名称")]
    public string jumpTrigger = "Jump";

    [Header("朝向设置")]
    public bool autoFlip = true;
    [Tooltip("true=翻转SpriteRenderer.flipX, false=翻转Transform.localScale.x (推荐，可翻转整个动画)")]
    public bool useSpriteFlip = false;
    [Tooltip("当使用SpriteFlip时，是否反向（true:右面朝右时flipX=false）")]
    public bool flipXWhenFacingRight = false;
    [Tooltip("角色默认是否朝左（原始scale.x为负数）")]
    public bool defaultFacingLeft = true;  // 【新增】明确标记默认朝向

    // ===== 内部 =====
    private Rigidbody2D rb;
    private Anchor currentAnchor;
    private bool isAnchorOut;      // 锚点是否在外面
    private bool isGrounded;       // 是否着地
    private bool isDead;
    private bool isFacingRight = true;
    private Vector3 originalScale;

    // Animator参数哈希缓存
    private int speedParamHash;
    private int isGroundParamHash;
    private int verticalVelocityParamHash;
    private int throwTriggerHash;
    private int restoreTriggerHash;
    private int jumpTriggerHash;

    private float MoveSpeed => config != null ? config.moveSpeed : moveSpeed;
    private float JumpForce => config != null ? config.jumpForce : jumpForce;
    private float AnchorFireDuration => config != null ? config.anchorFireDuration : anchorFireDuration;
    private float HitDuration => config != null ? config.hitDuration : hitDuration;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        originalScale = transform.localScale;

        // 【新增】如果默认朝左，初始化时设置 isFacingRight = false
        if (defaultFacingLeft && originalScale.x < 0)
        {
            isFacingRight = false;
        }

        CacheAnimatorHashes();
    }

    void CacheAnimatorHashes()
    {
        if (anim == null) return;

        speedParamHash = Animator.StringToHash(speedParam);
        isGroundParamHash = Animator.StringToHash(isGroundParam);
        verticalVelocityParamHash = Animator.StringToHash(verticalVelocityParam);
        throwTriggerHash = Animator.StringToHash(throwTrigger);
        restoreTriggerHash = Animator.StringToHash(restoreTrigger);
        jumpTriggerHash = Animator.StringToHash(jumpTrigger);

        Debug.Log($"[Player] 动画参数哈希: speed={speedParamHash}, isGround={isGroundParamHash}, verticalVelocity={verticalVelocityParamHash}, Throw={throwTriggerHash}, Restore={restoreTriggerHash}, Jump={jumpTriggerHash}");
    }

    void Update()
    {
        if (currentState == PlayerState.Death) return;

        TickStateTimer();
        HandleInput();
        UpdateAnimation();
        UpdateFacing();
    }

    void FixedUpdate()
    {
        if (currentState == PlayerState.Death) return;
        UpdatePhysics();
        CheckGrounded();
    }

    void TickStateTimer()
    {
        if (stateTimer > 0)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0)
                OnStateTimerEnd();
        }
    }

    void OnStateTimerEnd()
    {
        switch (currentState)
        {
            case PlayerState.AnchorFire:
                ChangeState(isGrounded ? PlayerState.Idle : PlayerState.Falling);
                break;
            case PlayerState.Hit:
                ChangeState(PlayerState.Idle);
                break;
        }
    }

    void HandleInput()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
            case PlayerState.Moving:
                if (Input.GetButtonDown("Jump") && isGrounded)
                    ChangeState(PlayerState.Jumping);
                if (Input.GetMouseButtonDown(0))
                    FireAnchor();
                break;
            case PlayerState.Jumping:
            case PlayerState.Falling:
                if (Input.GetMouseButtonDown(0))
                    FireAnchor();
                break;
            case PlayerState.BeingPulled:
                if (Input.GetMouseButtonDown(0))
                    ReleaseAnchor();
                break;
        }
    }

    void UpdatePhysics()
    {
        if (targetPlanet == null) return;

        switch (currentState)
        {
            case PlayerState.Idle:
            case PlayerState.Moving:
            case PlayerState.Jumping:
            case PlayerState.Falling:
            case PlayerState.AnchorFire:
                ApplyGravity();
                ApplyMovement();
                break;
            case PlayerState.BeingPulled:
                break;
            case PlayerState.Hit:
                ApplyGravity();
                break;
        }
    }

    void ApplyGravity()
    {
        if (targetPlanet == null) return;
        Vector2 dir = targetPlanet.GetGravityDirection(transform.position);
        rb.AddForce(dir * targetPlanet.gravityStrength, ForceMode2D.Force);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
    }

    void ApplyMovement()
    {
        if (targetPlanet == null) return;
        Vector2 tangent = targetPlanet.GetMovementDirection(transform.position);
        Vector2 gravityDir = targetPlanet.GetGravityDirection(transform.position);
        float h = Input.GetAxisRaw("Horizontal");
        float radialSpeed = Vector2.Dot(rb.velocity, gravityDir);
        rb.velocity = tangent * h * MoveSpeed + gravityDir * radialSpeed;

        if (Mathf.Abs(h) > 0.1f && currentState == PlayerState.Idle)
            ChangeState(PlayerState.Moving);
        else if (Mathf.Abs(h) < 0.1f && currentState == PlayerState.Moving)
            ChangeState(PlayerState.Idle);
    }

    public bool IsGrounded() => isGrounded;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer(damageLayerName))
        {
            OnPlayerDie();
            return;
        }
        isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        isGrounded = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(damageLayerName))
            OnPlayerDie();
    }

    void CheckGrounded()
    {
        if (currentState == PlayerState.Falling && isGrounded)
            ChangeState(PlayerState.Idle);

        if (currentState == PlayerState.Jumping && targetPlanet != null)
        {
            Vector2 dir = targetPlanet.GetGravityDirection(transform.position);
            if (Vector2.Dot(rb.velocity, dir) > 0.5f)
                ChangeState(PlayerState.Falling);
        }
    }

    public void ChangeState(PlayerState newState)
    {
        if (currentState == PlayerState.Death) return;

        currentState = newState;

        switch (newState)
        {
            case PlayerState.Jumping:
                if (anim != null)
                {
                    anim.SetTrigger(jumpTriggerHash);
                    Debug.Log("[Player] ChangeState: Trigger Jump");
                }

                Vector2 up = -targetPlanet.GetGravityDirection(transform.position);
                rb.AddForce(up * JumpForce, ForceMode2D.Impulse);
                break;

            case PlayerState.AnchorFire:
                stateTimer = AnchorFireDuration;
                break;

            case PlayerState.Hit:
                stateTimer = HitDuration;
                break;

            case PlayerState.Death:
                rb.velocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
                EventCenter.Instance?.EventTrigger(E_EventType.E_PlayerDeath, this);
                break;
        }
        EventCenter.Instance?.EventTrigger(E_EventType.E_PlayerStateChanged, this);
    }

    public void OnPlayerDie()
    {
        if (currentState == PlayerState.Death || isDead) return;
        isDead = true;
        ChangeState(PlayerState.Death);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        GameManager.Instance?.HandlePlayerDeath(this);
    }

    // ===== 锚点回调接口 =====

    public void OnAnchorStartThrow()
    {
        isAnchorOut = true;
        ChangeState(PlayerState.AnchorFire);

        if (anim != null)
        {
            anim.SetTrigger(throwTriggerHash);
            Debug.Log("[Player] OnAnchorStartThrow: Trigger Throw");
        }
    }

    public void OnAnchorStartRetract()
    {
        if (anim != null)
        {
            anim.SetTrigger(restoreTriggerHash);
            Debug.Log("[Player] OnAnchorStartRetract: Trigger Restore");
        }
    }

    public void OnAnchorHit()
    {
        ChangeState(PlayerState.BeingPulled);
    }

    public void OnAnchorHanging()
    {
        Debug.Log("[Player] 进入悬挂状态，点击左键释放");
    }

    public void OnAnchorRelease()
    {
        Debug.Log("[Player] 开始释放锚点");
    }

    public void OnAnchorArrived()
    {
        ChangeState(isGrounded ? PlayerState.Idle : PlayerState.Falling);
    }

    public void OnAnchorReturned()
    {
        isAnchorOut = false;
        ChangeState(isGrounded ? PlayerState.Idle : PlayerState.Falling);
    }

    public void OnAnchorRecycled()
    {
        isAnchorOut = false;
    }

    public void TakeDamage()
    {
        if (currentState == PlayerState.Hit || currentState == PlayerState.Death) return;
        ChangeState(PlayerState.Hit);
    }

    public void Die() => OnPlayerDie();

    // ===== 发射锚点 =====

    void FireAnchor()
    {
        if (isAnchorOut) return;

        if (currentAnchor == null)
        {
            currentAnchor = Instantiate(anchorPrefab).GetComponent<Anchor>();
        }

        if (currentAnchor.gameObject.activeSelf)
            currentAnchor.Recycle();

        currentAnchor.gameObject.SetActive(true);
        currentAnchor.transform.position = transform.position;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector2 dir = (mouseWorld - transform.position).normalized;

        currentAnchor.Init(this, dir);
        EventCenter.Instance?.EventTrigger(E_EventType.E_AnchorFired, currentAnchor);
    }

    void ReleaseAnchor()
    {
        currentAnchor?.StartRetract();
    }

    // ===== 动画更新 =====

    void UpdateAnimation()
    {
        if (anim == null)
        {
            Debug.LogWarning("[Player] anim is null!");
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float speedValue = Mathf.Abs(h);

        // 1. speed：水平移动速度
        anim.SetFloat(speedParamHash, speedValue);

        // 2. isGround：是否着地
        anim.SetBool(isGroundParamHash, isGrounded);

        // 3. verticalVelocity：沿重力方向的垂直速度
        float verticalVel = 0;
        if (targetPlanet != null && rb != null)
        {
            Vector2 gravityDir = targetPlanet.GetGravityDirection(transform.position);
            verticalVel = Vector2.Dot(rb.velocity, gravityDir);
        }
        anim.SetFloat(verticalVelocityParamHash, verticalVel);

        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Player] UpdateAnimation: speed={speedValue:F2}, isGround={isGrounded}, verticalVelocity={verticalVel:F2}, state={currentState}");
        }
    }

    // 【修正】UpdateFacing：修复重复代码，支持默认朝左
    void UpdateFacing()
    {
        if (!autoFlip) return;

        float h = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(h) < 0.1f) return;

        bool shouldFaceRight = h > 0;
        if (shouldFaceRight == isFacingRight) return;

        isFacingRight = shouldFaceRight;

        if (useSpriteFlip)
        {
            // 方式1：仅翻转 Sprite
            if (sprite != null)
            {
                sprite.flipX = flipXWhenFacingRight ? !isFacingRight : isFacingRight;
            }
        }
        else
        {
            // 方式2：翻转 Transform.localScale.x（真正翻转整个动画方向）
            Vector3 scale = originalScale;

            // 【修正】根据默认朝向决定翻转逻辑
            if (defaultFacingLeft)
            {
                // 默认朝左：右移时 scale.x 应为正数（朝右），左移时保持负数（朝左）
                scale.x = isFacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            }
            else
            {
                // 默认朝右：右移时 scale.x 应为正数，左移时为负数
                scale.x = isFacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            }

            transform.localScale = scale;
        }
    }

    public void ResetAnimation()
    {
        if (anim == null) return;
        anim.Rebind();
        anim.Update(0);
        currentState = PlayerState.Idle;
        isAnchorOut = false;
        isFacingRight = !defaultFacingLeft;  // 【修正】根据默认朝向重置

        // 重置时恢复原始朝向
        transform.localScale = originalScale;
        if (sprite != null) sprite.flipX = false;
    }
}
