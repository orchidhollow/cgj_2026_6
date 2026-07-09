using UnityEngine;

/// <summary>
/// 抓钩锚点：鼠标点击方向发射，命中物体后拉动角色飞向锚点
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Anchor : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private ControlConfig config;

    [Header("激光锁链")]
    public LineRenderer chainRenderer;
    public Material laserMaterial;
    public float chainWidth = 0.2f;
    public Color chainColor = Color.cyan;
    public Vector3 chainStartOffset = Vector3.zero;

    // 备用材质（如果 laserMaterial 未赋值）
    [SerializeField] private Material fallbackMaterial;

    private Rigidbody2D rb;
    private Player owner;
    private bool hasHit;
    private Transform hitTarget;
    private Vector2 hitOffset;
    private bool isReturning;
    private float traveledDistance;
    private Vector2 lastPosition;
    private bool isHanging;
    private bool releasePending;
    private bool isInitialized = false;  // 标记是否已初始化

    // 缓存配置值（防止 config 为 null）
    private float anchorSpeed;
    private float anchorPullSpeed;
    private float anchorMaxDistance;
    private float anchorArriveDistance;
    private float anchorCollisionStartDistance;
    private int hitchLayer;

    void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// 初始化组件和配置
    /// </summary>
    void Initialize()
    {
        if (isInitialized) return;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("[Anchor] Rigidbody2D 缺失！");
            enabled = false;
            return;
        }

        SetupChainRenderer();
        CacheConfig();  // 缓存配置值

        isInitialized = true;
    }

    /// <summary>
    /// 缓存配置值，避免运行时频繁访问 config
    /// </summary>
    void CacheConfig()
    {
        if (config != null)
        {
            anchorSpeed = config.anchorSpeed;
            anchorPullSpeed = config.anchorPullSpeed;
            anchorMaxDistance = config.anchorMaxDistance;
            anchorArriveDistance = config.anchorArriveDistance;
            anchorCollisionStartDistance = config.anchorCollisionStartDistance;
            hitchLayer = LayerMask.NameToLayer(config.hitchLayerName);
        }
        else
        {
            Debug.LogWarning("[Anchor] config 为 null，使用默认值！");
            // 设置默认值
            anchorSpeed = 20f;
            anchorPullSpeed = 15f;
            anchorMaxDistance = 30f;
            anchorArriveDistance = 0.5f;
            anchorCollisionStartDistance = 0.5f;
            hitchLayer = LayerMask.NameToLayer("Hitch");
        }

        if (hitchLayer == -1)
        {
            Debug.LogError("[Anchor] Hitch 层不存在！请检查 Layer 设置。");
        }
    }

    void SetupChainRenderer()
    {
        if (chainRenderer == null)
        {
            chainRenderer = GetComponent<LineRenderer>();
            if (chainRenderer == null)
            {
                chainRenderer = gameObject.AddComponent<LineRenderer>();
            }
        }

        chainRenderer.positionCount = 2;
        chainRenderer.startWidth = chainWidth;
        chainRenderer.endWidth = chainWidth;
        chainRenderer.useWorldSpace = true;

        // 安全地创建材质
        if (laserMaterial == null)
        {
            if (fallbackMaterial != null)
            {
                laserMaterial = new Material(fallbackMaterial);
            }
            else
            {
                // 使用内置 Sprite-Default 作为备用
                laserMaterial = new Material(Shader.Find("Sprites/Default"));
                if (laserMaterial == null)
                {
                    Debug.LogError("[Anchor] 无法创建材质！Shader 可能缺失。");
                    return;
                }
            }
            laserMaterial.color = chainColor;
        }

        chainRenderer.material = laserMaterial;
        chainRenderer.startColor = chainColor;
        chainRenderer.endColor = chainColor;
        chainRenderer.sortingOrder = 100;
        chainRenderer.enabled = false;
    }

    /// <summary>
    /// 初始化锚点
    /// </summary>
    public void Init(Player player, Vector2 direction)
    {
        // 确保已初始化（处理物体禁用后再启用的情况）
        if (!isInitialized)
        {
            Initialize();
        }

        owner = player;
        if (owner == null)
        {
            Debug.LogError("[Anchor] Init: owner 为 null！");
            return;
        }

        // 重置 Rigidbody
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.isKinematic = false;
        rb.velocity = direction.normalized * anchorSpeed;

        // 旋转朝向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 确保碰撞体启用
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            col.enabled = true;
        }

        // 重置状态
        hasHit = false;
        isReturning = false;
        isHanging = false;
        releasePending = false;
        traveledDistance = 0f;
        lastPosition = transform.position;

        // 确保激光启用
        if (chainRenderer != null)
        {
            chainRenderer.enabled = true;
        }
        else
        {
            Debug.LogError("[Anchor] chainRenderer 为 null！");
        }
    }

    void Update()
    {
        // 悬挂状态下点击左键释放
        if (isHanging && Input.GetMouseButtonDown(0))
        {
            Release();
        }

        UpdateChainVisual();
    }

    void Release()
    {
        if (releasePending) return;

        releasePending = true;
        isHanging = false;

        if (chainRenderer != null)
            chainRenderer.enabled = true;

        owner?.OnAnchorRelease();
    }

    void UpdateChainVisual()
    {
        if (chainRenderer == null || !chainRenderer.enabled || owner == null) return;

        if (isHanging)
        {
            chainRenderer.enabled = false;
            return;
        }

        Vector2 startPos = owner.transform.position + owner.transform.TransformDirection(chainStartOffset);
        Vector2 endPos = transform.position;

        chainRenderer.SetPosition(0, startPos);
        chainRenderer.SetPosition(1, endPos);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || isReturning || rb == null || owner == null) return;
        if (traveledDistance < anchorCollisionStartDistance) return;
        if (other.CompareTag("Player") || other.CompareTag("Planet")) return;
        if (other.isTrigger) return;

        // 使用缓存的 hitchLayer
        if (other.gameObject.layer == hitchLayer)
        {
            hasHit = true;
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;

            hitTarget = other.transform;
            hitOffset = (Vector2)transform.position - (Vector2)hitTarget.position;

            owner?.OnAnchorHit();

            // 安全调用事件系统
            SafeEventTrigger(E_EventType.E_AnchorHit, this);
            SafePlayHit(true);
        }
        else
        {
            StartReturn();
            SafePlayHit(false);
        }
    }

    /// <summary>
    /// 安全触发事件（防止 EventCenter 为 null）
    /// </summary>
    void SafeEventTrigger(E_EventType eventType, object param)
    {
        try
        {
            EventCenter.Instance?.EventTrigger(eventType, param);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Anchor] 事件触发失败: {e.Message}");
        }
    }

    /// <summary>
    /// 安全播放音效（防止 FMODAudioMgr 为 null）
    /// </summary>
    void SafePlayHit(bool isHitch)
    {
        try
        {
            FMODAudioMgr.Instance?.PlayHit(isHitch);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Anchor] 音效播放失败: {e.Message}");
        }
    }

    public void StartReturn()
    {
        isReturning = true;
        if (rb != null)
            rb.velocity = Vector2.zero;
    }

    void FixedUpdate()
    {
        if (owner == null) return;

        // 返回模式
        if (isReturning)
        {
            Vector2 returnDir = ((Vector2)owner.transform.position - (Vector2)transform.position).normalized;
            if (rb != null)
                rb.velocity = returnDir * anchorSpeed;

            float returnDist = Vector2.Distance(transform.position, owner.transform.position);
            if (returnDist < anchorArriveDistance)
            {
                Recycle();
            }
            return;
        }

        // 飞行模式
        if (!hasHit)
        {
            traveledDistance += Vector2.Distance(transform.position, lastPosition);
            lastPosition = transform.position;

            if (traveledDistance > anchorMaxDistance)
            {
                StartReturn();
                return;
            }
        }

        // 命中模式
        if (!hasHit) return;

        // 手动跟随
        if (hitTarget != null)
            transform.position = (Vector2)hitTarget.position + hitOffset;

        // 等待释放：收线
        if (releasePending)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                owner.transform.position,
                anchorSpeed * Time.fixedDeltaTime
            );

            float dist = Vector2.Distance(transform.position, owner.transform.position);
            if (dist < anchorArriveDistance)
            {
                Recycle();
            }
            return;
        }

        // 正常拉动角色
        Vector2 newPos = Vector2.MoveTowards(
            owner.transform.position,
            transform.position,
            anchorPullSpeed * Time.fixedDeltaTime
        );
        owner.transform.position = newPos;

        float arriveDist = Vector2.Distance(transform.position, owner.transform.position);
        if (arriveDist < anchorArriveDistance)
        {
            EnterHanging();
        }
    }

    void EnterHanging()
    {
        isHanging = true;
        if (rb != null)
            rb.velocity = Vector2.zero;

        if (chainRenderer != null)
            chainRenderer.enabled = false;

        owner?.OnAnchorHanging();
    }

    public void Recycle()
    {
        // 通知 Player
        if (owner != null)
        {
            owner.OnAnchorRecycled();
            owner.OnAnchorReturned();
        }

        // 隐藏激光
        if (chainRenderer != null)
            chainRenderer.enabled = false;

        // 重置状态
        hasHit = false;
        isReturning = false;
        isHanging = false;
        releasePending = false;
        hitTarget = null;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = false;
        }

        owner = null;
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        // 物体禁用时清理
        isInitialized = false;
    }
}
