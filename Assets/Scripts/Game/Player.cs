using UnityEngine;
using System.Collections;

/// <summary>
/// 角色控制器：状态机管理移动、跳跃、锚点、受击、死亡
/// 8 个状态：Idle / Moving / Jumping / Falling / AnchorFire / BeingPulled / Hit / Death
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    // ===== 状态机 =====
    /// <summary>玩家状态枚举</summary>
    public enum PlayerState
    {
        Idle,           // 待机（站在星球表面）
        Moving,         // 移动（在星球表面行走）
        Jumping,        // 跳跃（离开星球表面上升中）
        Falling,        // 下落（离开星球表面下降中）
        AnchorFire,     // 发射船锚（短暂锁定输入）
        BeingPulled,    // 被牵引（被锚点拉动中）
        Hit,            // 受击（短暂无敌闪烁）
        Death           // 死亡（终态）
    }

    /// <summary>当前状态（Inspector 可见用于调试）</summary>
    [HideInInspector] public PlayerState currentState = PlayerState.Idle;
    /// <summary>通用状态计时器（AnchorFire / Hit 超时退出）</summary>
    private float stateTimer;

    // ===== 引用 =====
    [Header("References")]
    /// <summary>当前受引力影响的星球</summary>
    public Planet targetPlanet;
    /// <summary>锚点预制体（Inspector 手动拖入）</summary>
    public GameObject anchorPrefab;
    /// <summary>角色动画控制器</summary>
    public Animator anim;
    /// <summary>角色精灵渲染器（用于翻转）</summary>
    public SpriteRenderer sprite;

    // ===== 配置 =====
    [Header("Config")]
    [SerializeField] private ControlConfig config;

    // ===== 参数（兼容直接配置，当 config 为 null 时使用） =====
    [Header("Movement")]
    /// <summary>移动速度（config 为 null 时使用）</summary>
    public float moveSpeed = 5f;
    /// <summary>跳跃力度（config 为 null 时使用）</summary>
    public float jumpForce = 8f;

    [Header("Anchor")]
    /// <summary>发射船锚后锁定输入的时长（秒）（config 为 null 时使用）</summary>
    public float anchorFireDuration = 0.3f;

    [Header("Hit")]
    /// <summary>受击无敌时长（秒）（config 为 null 时使用）</summary>
    public float hitDuration = 0.5f;

    // ===== 死亡检测 =====
    [Header("死亡检测")]
    /// <summary>死亡点层名称</summary>
    public string damageLayerName = "Damage";

    // ===== 内部 =====
    private Rigidbody2D rb;
    /// <summary>缓存的锚点实例（只创建一次，反复复用）</summary>
    private Anchor currentAnchor;
    /// <summary>锚点是否已发射且未回收（防止连续发射多个）</summary>
    private bool isAnchorOut;
    /// <summary>是否站在地面上（由碰撞检测自动维护）</summary>
    private bool isGrounded;
    /// <summary>标记是否已经触发死亡，防止重复调用</summary>
    private bool isDead;

    // 配置属性访问器，优先使用 config，否则使用直接字段
    private float MoveSpeed => config != null ? config.moveSpeed : moveSpeed;
    private float JumpForce => config != null ? config.jumpForce : jumpForce;
    private float AnchorFireDuration => config != null ? config.anchorFireDuration : anchorFireDuration;
    private float HitDuration => config != null ? config.hitDuration : hitDuration;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;      // 禁用 Unity 内置重力，使用自定义星球重力
        rb.freezeRotation = true;  // 冻结旋转，由代码控制朝向
    }

    void Update()
    {
        // 死亡后不再处理输入和动画更新
        if (currentState == PlayerState.Death) return;

        TickStateTimer();   // 递减状态计时器
        HandleInput();      // 根据当前状态处理输入
        UpdateAnimation();  // 同步 Animator 参数
    }

    void FixedUpdate()
    {
        // 死亡后不再处理物理
        if (currentState == PlayerState.Death) return;

        UpdatePhysics();    // 根据当前状态执行物理
        CheckGrounded();    // 检测着地/下落状态切换
    }

    // ===== 状态计时器 =====

    /// <summary>
    /// 每帧递减状态计时器，超时时触发退出逻辑
    /// 用于 AnchorFire（发射锁定时长）和 Hit（无敌时长）
    /// </summary>
    void TickStateTimer()
    {
        if (stateTimer > 0)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0)
                OnStateTimerEnd();
        }
    }

    /// <summary>状态计时器到期时的退出逻辑</summary>
    void OnStateTimerEnd()
    {
        switch (currentState)
        {
            case PlayerState.AnchorFire:
                // 发射动画结束，根据是否着地切到 Idle 或 Falling
                ChangeState(isGrounded ? PlayerState.Idle : PlayerState.Falling);
                break;
            case PlayerState.Hit:
                // 无敌结束，恢复 Idle
                ChangeState(PlayerState.Idle);
                break;
        }
    }

    // ===== 输入处理 =====

    /// <summary>
    /// 根据当前状态处理玩家输入
    /// Idle/Moving：可跳跃、可发射锚点
    /// Jumping/Falling：可发射锚点
    /// AnchorFire/BeingPulled/Hit/Death：锁定输入
    /// </summary>
    void HandleInput()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
            case PlayerState.Moving:
                // 空格跳跃（必须着地）
                if (Input.GetButtonDown("Jump") && isGrounded)
                    ChangeState(PlayerState.Jumping);
                // 鼠标左键发射锚点
                if (Input.GetMouseButtonDown(0))
                    FireAnchor();
                break;

            case PlayerState.Jumping:
            case PlayerState.Falling:
                // 空中也可发射锚点
                if (Input.GetMouseButtonDown(0))
                    FireAnchor();
                break;

                // AnchorFire / BeingPulled / Hit / Death 锁定输入
        }
    }

    // ===== 物理更新 =====

    /// <summary>
    /// 根据当前状态执行物理逻辑
    /// Idle/Moving：重力 + 移动
    /// Jumping/Falling：仅重力
    /// BeingPulled：物理由 Anchor 控制
    /// Hit：仅重力（受击后仍受重力影响）
    /// </summary>
    void UpdatePhysics()
    {
        if (targetPlanet == null) return;

        switch (currentState)
        {
            case PlayerState.Idle:
            case PlayerState.Moving:
                ApplyGravity();
                ApplyMovement();
                break;

            case PlayerState.Jumping:
            case PlayerState.Falling:
                ApplyGravity();
                ApplyMovement();
                break;

            case PlayerState.BeingPulled:
                // 物理由 Anchor 控制，此处暂停
                break;

            case PlayerState.Hit:
                ApplyGravity();
                break;
        }
    }

    // ===== 公共物理方法 =====

    /// <summary>
    /// 向星球中心施加重力，同时调整角色朝向（头朝外脚朝内）
    /// </summary>
    void ApplyGravity()
    {
        if (targetPlanet == null) return;

        // 获取指向星球中心的重力方向
        Vector2 dir = targetPlanet.GetGravityDirection(transform.position);
        rb.AddForce(dir * targetPlanet.gravityStrength, ForceMode2D.Force);

        // 旋转角色：up 方向始终背离星球
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
    }

    /// <summary>
    /// 沿星球表面切线方向移动
    /// 保留重力方向的速度分量，只替换切线方向分量
    /// </summary>
    void ApplyMovement()
    {
        if (targetPlanet == null) return;

        Vector2 tangent = targetPlanet.GetMovementDirection(transform.position);
        Vector2 gravityDir = targetPlanet.GetGravityDirection(transform.position);
        float h = Input.GetAxisRaw("Horizontal");

        // 保留重力方向分量（径向速度），只替换切线方向分量
        float radialSpeed = Vector2.Dot(rb.velocity, gravityDir);
        rb.velocity = tangent * h * MoveSpeed + gravityDir * radialSpeed;

        // Idle / Moving 状态切换
        if (Mathf.Abs(h) > 0.1f && currentState == PlayerState.Idle)
            ChangeState(PlayerState.Moving);
        else if (Mathf.Abs(h) < 0.1f && currentState == PlayerState.Moving)
            ChangeState(PlayerState.Idle);
    }

    /// <summary>
    /// 地面检测：由 OnCollisionEnter2D/Exit2D 自动维护
    /// 碰到星球大地（非 Trigger 碰撞体）时为 true，离开时为 false
    /// </summary>
    public bool IsGrounded()
    {
        return isGrounded;
    }

    /// <summary>
    /// 碰撞进入：碰到星球大地碰撞体时，标记在地面上
    /// 碰到 Damage 层死亡点立即死亡
    /// </summary>
    void OnCollisionEnter2D(Collision2D col)
    {
        // 检测死亡点
        if (col.gameObject.layer == LayerMask.NameToLayer(damageLayerName))
        {
            // 碰到死亡点，调用死亡方法
            OnPlayerDie();
            return;
        }

        isGrounded = true;
    }

    /// <summary>
    /// 碰撞离开：离开星球大地碰撞体时，标记不在地面
    /// </summary>
    void OnCollisionExit2D(Collision2D col)
    {
        isGrounded = false;
    }

    /// <summary>
    /// 触发器进入：检测 Trigger 类型的死亡点
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 检测死亡点（Trigger 类型）
        if (other.gameObject.layer == LayerMask.NameToLayer(damageLayerName))
        {
            // 碰到死亡点，调用死亡方法
            OnPlayerDie();
        }
    }

    /// <summary>
    /// 检测着地/下落状态切换
    /// Falling → 着地 → Idle
    /// Jumping → 速度方向与重力同向（下降中） → Falling
    /// </summary>
    void CheckGrounded()
    {
        // 下落中着地 → Idle
        if (currentState == PlayerState.Falling && isGrounded)
            ChangeState(PlayerState.Idle);

        // 跳跃中开始下降 → Falling（需要 targetPlanet 才能判断方向）
        if (currentState == PlayerState.Jumping && targetPlanet != null)
        {
            // Dot(velocity, gravityDir) > 0 表示速度方向与重力同向 = 下降中
            Vector2 dir = targetPlanet.GetGravityDirection(transform.position);
            float dot = Vector2.Dot(rb.velocity, dir);
            if (dot > 0.5f)
                ChangeState(PlayerState.Falling);
        }
    }

    // ===== 状态切换 =====

    /// <summary>
    /// 统一状态切换入口
    /// 包含状态进入逻辑（跳跃弹射、计时器启动、解绑星球等）
    /// 广播 E_PlayerStateChanged 事件
    /// </summary>
    /// <param name="newState">目标状态</param>
    public void ChangeState(PlayerState newState)
    {
        // 死亡不可切换
        if (currentState == PlayerState.Death) return;

        currentState = newState;

        // 状态进入逻辑
        switch (newState)
        {
            case PlayerState.Jumping:
                // 沿背离星球方向弹射
                Vector2 up = -targetPlanet.GetGravityDirection(transform.position);
                rb.AddForce(up * JumpForce, ForceMode2D.Impulse);
                break;

            case PlayerState.AnchorFire:
                // 启动发射锁定计时器
                stateTimer = AnchorFireDuration;
                break;

            case PlayerState.BeingPulled:
                // 不解绑星球，由 Planet trigger 管理父子关系
                break;

            case PlayerState.Hit:
                // 启动无敌计时器
                stateTimer = HitDuration;
                break;

            case PlayerState.Death:
                // 停止所有运动，冻结物理
                rb.velocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
                // 广播死亡事件
                EventCenter.Instance?.EventTrigger(E_EventType.E_PlayerDeath, this);
                break;
        }

        // 广播状态变化事件（供音效、特效等系统监听）
        EventCenter.Instance?.EventTrigger(E_EventType.E_PlayerStateChanged, this);
    }

    // ===== 死亡处理 =====

    /// <summary>
    /// 玩家死亡：直接调用 GameManager 处理重开
    /// 不依赖 EventCenter 广播（保留 EventCenter 作为备选）
    /// </summary>
    public void OnPlayerDie()
    {
        // 死亡状态不可重复触发
        if (currentState == PlayerState.Death || isDead) return;

        // 标记已死亡，防止重复触发
        isDead = true;

        // 切换到死亡状态
        ChangeState(PlayerState.Death);

        // 禁用碰撞体，防止继续触发死亡事件
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 确保 GameManager 存在
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[Player] GameManager.Instance 为 null，尝试查找场景中的 GameManager");
            // 尝试通过 MonoBehaviour 方式查找（兼容旧版本）
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm == null)
            {
                Debug.LogError("[Player] 场景中未找到 GameManager，无法处理死亡重开");
                return;
            }
        }

        // 调用 GameManager 处理死亡和重开
        GameManager.Instance.HandlePlayerDeath(this);
    }

    // ===== 外部调用接口（供 Anchor.cs 调用） =====

    /// <summary>锚点命中 Hitch 悬挂点时调用，切换到 BeingPulled 状态</summary>
    public void OnAnchorHit()
    {
        ChangeState(PlayerState.BeingPulled);
    }

    /// <summary>进入悬挂状态（线已收回，保持钩住，等待手动释放）</summary>
    public void OnAnchorHanging()
    {
        // 悬挂状态：保持 BeingPulled 状态，等待点击左键释放
        // 可以在这里播放悬挂动画或音效
        Debug.Log("[Player] 进入悬挂状态，点击左键释放");
    }

    /// <summary>开始释放锚点（收线中）</summary>
    public void OnAnchorRelease()
    {
        // 可以在这里播放收线动画或音效
        Debug.Log("[Player] 开始释放锚点");
    }

    /// <summary>角色到达锚点位置时调用，根据是否着地切到 Idle 或 Falling</summary>
    public void OnAnchorArrived()
    {
        ChangeState(isGrounded ? PlayerState.Idle : PlayerState.Falling);
    }

    /// <summary>锚点返回消失时调用，根据是否着地切到 Idle 或 Falling</summary>
    public void OnAnchorReturned()
    {
        ChangeState(isGrounded ? PlayerState.Idle : PlayerState.Falling);
    }

    /// <summary>锚点回收时调用，允许再次发射</summary>
    public void OnAnchorRecycled()
    {
        isAnchorOut = false;
    }

    /// <summary>受击（外部调用，如 Trap 碰撞）</summary>
    public void TakeDamage()
    {
        // 受击/死亡期间不重复触发
        if (currentState == PlayerState.Hit || currentState == PlayerState.Death) return;
        ChangeState(PlayerState.Hit);
    }

    /// <summary>死亡（外部调用，兼容旧版接口）</summary>
    public void Die()
    {
        OnPlayerDie();
    }

    // ===== 发射锚点 =====

    /// <summary>
    /// 发射锚点：首次创建实例，之后复用
    /// 如果锚点还在外面（isAnchorOut），不能再次发射
    /// </summary>
    void FireAnchor()
    {
        // 船锚还在外面，不能再次发射
        if (isAnchorOut) return;

        // 首次使用时实例化锚点
        if (currentAnchor == null)
        {
            GameObject go = Instantiate(anchorPrefab);
            currentAnchor = go.GetComponent<Anchor>();
        }

        // 如果当前锚点还在活跃中，先回收
        if (currentAnchor.gameObject.activeSelf)
            currentAnchor.Recycle();

        // 激活并设置位置
        currentAnchor.gameObject.SetActive(true);
        currentAnchor.transform.position = transform.position;

        // 计算鼠标世界坐标方向
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector2 dir = (mouseWorld - transform.position).normalized;

        // 初始化锚点并标记已发射
        currentAnchor.Init(this, dir);
        isAnchorOut = true;
        ChangeState(PlayerState.AnchorFire);
        EventCenter.Instance?.EventTrigger(E_EventType.E_AnchorFired, currentAnchor);
    }

    // ===== 动画 =====

    /// <summary>
    /// 同步 Animator 的 state 参数
    /// Falling / BeingPulled 复用 Jumping 动画（state == 2）
    /// </summary>
    void UpdateAnimation()
    {
        if (anim == null) return;

        int animState;
        switch (currentState)
        {
            case PlayerState.Idle: animState = 0; break;
            case PlayerState.Moving: animState = 1; break;
            case PlayerState.Jumping: animState = 2; break;
            case PlayerState.Falling: animState = 2; break;  // 复用 Jumping 动画
            case PlayerState.AnchorFire: animState = 4; break;
            case PlayerState.BeingPulled: animState = 2; break;  // 复用 Jumping 动画
            case PlayerState.Hit: animState = 6; break;
            case PlayerState.Death: animState = 7; break;
            default: animState = 0; break;
        }
        anim.SetInteger("state", animState);
    }
}
