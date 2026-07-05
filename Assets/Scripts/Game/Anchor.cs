using UnityEngine;

/// <summary>
/// 抓钩锚点：鼠标点击方向发射，命中物体后拉动角色飞向锚点
///
/// 生命周期：
/// 1. Init() 初始化，朝鼠标方向飞行
/// 2. 飞行中累计距离，超过 maxDistance 未命中 → 原路返回 → 消失
/// 3. 命中 Hitch 层物体 → 停下，成为悬挂点，拉动角色
/// 4. 命中其他层物体 → 直接收回（钩子返回）
/// 5. 角色到达悬挂点 → 线收回 → 悬挂状态（隐藏激光，保持钩住）
/// 6. 点击鼠标左键 → 释放 → 回收
///
/// 通过 EventCenter 广播 E_AnchorFired / E_AnchorHit / E_AnchorArrived 事件
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Anchor : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private ControlConfig config;

    [Header("激光锁链")]
    /// <summary>激光渲染器</summary>
    public LineRenderer chainRenderer;
    /// <summary>激光材质</summary>
    public Material laserMaterial;
    /// <summary>锁链宽度</summary>
    public float chainWidth = 0.2f;
    /// <summary>锁链颜色</summary>
    public Color chainColor = Color.cyan;

    private Rigidbody2D rb;
    /// <summary>发射此锚点的角色</summary>
    private Player owner;
    /// <summary>是否已命中悬挂点</summary>
    private bool hasHit;
    /// <summary>命中的悬挂点物体（手动跟随，避免 SetParent 导致畸变）</summary>
    private Transform hitTarget;
    /// <summary>命中时相对物体的偏移量</summary>
    private Vector2 hitOffset;
    /// <summary>是否正在返回（未命中或命中非Hitch层）</summary>
    private bool isReturning;
    /// <summary>已飞行距离（用于判断是否超过 maxDistance）</summary>
    private float traveledDistance;
    /// <summary>上一帧位置（用于计算每帧飞行距离）</summary>
    private Vector2 lastPosition;

    /// <summary>是否处于悬挂状态（线已收回，保持钩住）</summary>
    private bool isHanging;
    /// <summary>是否已请求释放（等待线收回）</summary>
    private bool releasePending;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetupChainRenderer();
    }

    /// <summary>
    /// 初始化激光渲染器
    /// </summary>
    void SetupChainRenderer()
    {
        if (chainRenderer == null)
        {
            chainRenderer = gameObject.AddComponent<LineRenderer>();
        }

        chainRenderer.positionCount = 2;
        chainRenderer.startWidth = chainWidth;
        chainRenderer.endWidth = chainWidth;
        chainRenderer.useWorldSpace = true;

        // 使用简单材质
        if (laserMaterial == null)
        {
            laserMaterial = new Material(Shader.Find("Unlit/Color"));
            laserMaterial.color = chainColor;
        }

        chainRenderer.material = laserMaterial;
        chainRenderer.startColor = chainColor;
        chainRenderer.endColor = chainColor;
        chainRenderer.enabled = false;
    }

    /// <summary>
    /// 初始化锚点（由 Player.FireAnchor 调用）
    /// 重置所有状态，设置飞行方向和速度
    /// </summary>
    /// <param name="player">发射者</param>
    /// <param name="direction">发射方向（鼠标指向）</param>
    public void Init(Player player, Vector2 direction)
    {
        owner = player;
        // 确保 rb 引用有效（复用时 Awake 不会再执行）
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0;
        rb.velocity = direction.normalized * config.anchorSpeed;
        GetComponent<CircleCollider2D>().isTrigger = true;

        // 重置所有状态
        hasHit = false;
        isReturning = false;
        isHanging = false;
        releasePending = false;
        traveledDistance = 0f;
        lastPosition = transform.position;

        // 激活激光
        chainRenderer.enabled = true;

        // 通知 Player 开始投掷，触发 Throw 动画
        owner?.OnAnchorStartThrow();
    }

    void Update()
    {
        // 悬挂状态下点击左键释放
        if (isHanging && Input.GetMouseButtonDown(0))
        {
            // 调用公共方法释放
            StartRetract();
        }

        // 更新激光锁链：连接玩家和锚点
        UpdateChainVisual();
    }

    /// <summary>
    /// 公共接口：开始收回锚点（由 Player 调用）
    /// 触发 Restore 动画
    /// </summary>
    public void StartRetract()
    {
        if (releasePending) return;

        releasePending = true;
        isHanging = false;
        chainRenderer.enabled = true;  // 重新显示线（收线过程）

        // 通知 Player 开始收回，触发 Restore 动画
        owner?.OnAnchorStartRetract();
        owner?.OnAnchorRelease();
    }

    /// <summary>
    /// 更新激光锁链视觉效果
    /// </summary>
    void UpdateChainVisual()
    {
        if (!chainRenderer.enabled || owner == null) return;

        // 悬挂状态隐藏激光
        if (isHanging)
        {
            chainRenderer.enabled = false;
            return;
        }

        // 起点：玩家位置
        Vector2 startPos = owner.transform.position;
        // 终点：锚点位置
        Vector2 endPos = transform.position;

        chainRenderer.SetPosition(0, startPos);
        chainRenderer.SetPosition(1, endPos);
    }

    /// <summary>
    /// 碰撞检测：
    /// 命中 Hitch 层 → 悬挂点，停下，拉动角色
    /// 命中其他层（包括 Ground）→ 直接收回（钩子返回）
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || isReturning || rb == null) return;
        // 未飞出安全距离，忽略碰撞
        if (traveledDistance < config.anchorCollisionStartDistance) return;
        // 忽略 tag 为 Player 和 Planet 的物体
        if (other.CompareTag("Player") || other.CompareTag("Planet")) return;
        // 忽略 Trigger 碰撞体（如星球重力范围）
        if (other.isTrigger) return;

        // 检测是否为 Hitch 层（悬挂点）
        if (other.gameObject.layer == LayerMask.NameToLayer(config.hitchLayerName))
        {
            // 命中 Hitch 层，成为悬挂点
            hasHit = true;
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;

            // 记录命中物体和偏移量（手动跟随，避免 SetParent 导致畸变）
            hitTarget = other.transform;
            hitOffset = (Vector2)transform.position - (Vector2)hitTarget.position;

            // 通知 Player 切换到 BeingPulled 状态
            if (owner != null)
                owner.OnAnchorHit();

            // 广播命中事件
            EventCenter.Instance.EventTrigger(E_EventType.E_AnchorHit, this);
        }
        else
        {
            // 命中非 Hitch 层（Ground等），直接收回钩子
            // 触发 Restore 动画
            StartReturn();
        }
    }

    /// <summary>
    /// 开始返回（钩子收回）
    /// 触发 Restore 动画
    /// </summary>
    void StartReturn()
    {
        isReturning = true;
        rb.velocity = Vector2.zero;

        // 通知 Player 开始收回，触发 Restore 动画
        owner?.OnAnchorStartRetract();
    }

    void FixedUpdate()
    {
        if (owner == null) return;

        // ===== 返回模式：朝角色飞去，到达后回收 =====
        if (isReturning)
        {
            Vector2 returnDir = ((Vector2)owner.transform.position - (Vector2)transform.position).normalized;
            rb.velocity = returnDir * config.anchorSpeed;

            float returnDist = Vector2.Distance(transform.position, owner.transform.position);
            if (returnDist < config.anchorArriveDistance)
            {
                Recycle();
            }
            return;
        }

        // ===== 飞行模式：累计距离，超过 maxDistance 则返回 =====
        if (!hasHit)
        {
            traveledDistance += Vector2.Distance(transform.position, lastPosition);
            lastPosition = transform.position;
            // 超过最大飞行距离，开始返回
            if (traveledDistance > config.anchorMaxDistance)
            {
                // 触发 Restore 动画
                StartReturn();
                return;
            }
        }

        // ===== 命中模式：拉动角色向悬挂点移动 =====
        if (!hasHit) return;

        // 手动跟随命中物体（保持偏移量，不受父物体 scale/rotation 影响）
        if (hitTarget != null)
            transform.position = (Vector2)hitTarget.position + hitOffset;

        // 等待释放状态：收线到玩家位置
        if (releasePending)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                owner.transform.position,
                config.anchorSpeed * Time.fixedDeltaTime
            );

            // 检查是否完全收回
            float dist = Vector2.Distance(transform.position, owner.transform.position);
            if (dist < config.anchorArriveDistance)
            {
                Recycle();
            }
            return;
        }

        // 正常拉动角色
        Vector2 newPos = Vector2.MoveTowards(
            owner.transform.position,
            transform.position,
            config.anchorPullSpeed * Time.fixedDeltaTime
        );
        owner.transform.position = newPos;

        // 到达判定：角色足够接近悬挂点时进入悬挂
        float arriveDist = Vector2.Distance(transform.position, owner.transform.position);
        if (arriveDist < config.anchorArriveDistance)
        {
            EnterHanging();
        }
    }

    /// <summary>
    /// 进入悬挂状态：线收回，隐藏激光，保持钩住
    /// </summary>
    void EnterHanging()
    {
        isHanging = true;
        rb.velocity = Vector2.zero;
        chainRenderer.enabled = false;

        // 通知 Player 进入悬挂状态
        if (owner != null)
            owner.OnAnchorHanging();
    }

    /// <summary>
    /// 回收锚点：隐藏物体，重置状态，通知 Player
    /// 不销毁物体，可由 Player 复用
    /// </summary>
    public void Recycle()
    {
        // 通知 Player：允许再次发射 + 恢复状态
        if (owner != null)
        {
            owner.OnAnchorRecycled();
            owner.OnAnchorReturned();
        }

        // 隐藏激光
        chainRenderer.enabled = false;

        // 重置所有状态
        hasHit = false;
        isReturning = false;
        isHanging = false;
        releasePending = false;
        hitTarget = null;
        rb.velocity = Vector2.zero;
        rb.isKinematic = false;
        owner = null;
        gameObject.SetActive(false);
    }
}
