using UnityEngine;

/// <summary>
/// 抓钩锚点：鼠标点击方向发射，命中物体后拉动角色飞向锚点
/// 通过 EventCenter 广播事件
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Anchor : MonoBehaviour
{
    [Header("Settings")]
    /// <summary>飞行速度</summary>
    public float speed = 20f;
    /// <summary>拉动角色的速度</summary>
    public float pullSpeed = 15f;
    /// <summary>到达判定距离（与角色距离小于此值时回收）</summary>
    public float arriveDistance = 0.5f;
    /// <summary>最大飞行距离（超过则自动回收）</summary>
    public float maxDistance = 20f;

    private Rigidbody2D rb;
    /// <summary>发射此锚点的角色</summary>
    private Player owner;
    /// <summary>是否已命中物体</summary>
    private bool hasHit;
    /// <summary>是否正在返回（未命中到达最大距离后）</summary>
    private bool isReturning;
    /// <summary>已飞行距离</summary>
    private float traveledDistance;
    /// <summary>上一帧位置（用于计算飞行距离）</summary>
    private Vector2 lastPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 初始化锚点（由 Player.FireAnchor 调用）
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
        rb.velocity = direction.normalized * speed;
        GetComponent<CircleCollider2D>().isTrigger = true;
        hasHit = false;
        isReturning = false;
        traveledDistance = 0f;
        lastPosition = transform.position;
    }

    /// <summary>
    /// 碰撞检测：命中任何非玩家物体后停下
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || rb == null) return;
        // 忽略 tag 为 Player 的物体
        if (other.CompareTag("Player")||other.CompareTag("Planet")) return;

        hasHit = true;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        // 命中后暂停角色常规移动
        if (owner != null)
            owner.isBeingPulled = true;

        // 成为碰撞物体的子物体，跟随移动
        transform.SetParent(other.transform);

        // 广播命中事件
        EventCenter.Instance.EventTrigger(E_EventType.E_AnchorHit, this);
    }

    void FixedUpdate()
    {
        if (owner == null) return;

        // 正在返回：朝角色飞去，到达后消失
        if (isReturning)
        {
            Vector2 returnDir = ((Vector2)owner.transform.position - (Vector2)transform.position).normalized;
            rb.velocity = returnDir * speed;

            float returnDist = Vector2.Distance(transform.position, owner.transform.position);
            if (returnDist < arriveDistance)
            {
                Recycle();
            }
            return;
        }

        // 未命中时累计飞行距离
        if (!hasHit)
        {
            traveledDistance += Vector2.Distance(transform.position, lastPosition);
            lastPosition = transform.position;
            // 超过最大飞行距离，开始返回
            if (traveledDistance > maxDistance)
            {
                isReturning = true;
                rb.velocity = Vector2.zero;
                return;
            }
        }

        // 未命中，跳过拉动逻辑
        if (!hasHit) return;

        // 直接将角色位置向锚点移动
        Vector2 newPos = Vector2.MoveTowards(
            owner.transform.position,
            transform.position,
            pullSpeed * Time.fixedDeltaTime
        );
        owner.transform.position = newPos;

        // 到达判定：角色足够接近锚点时回收
        float dist = Vector2.Distance(transform.position, owner.transform.position);
        if (dist < arriveDistance)
        {
            EventCenter.Instance.EventTrigger(E_EventType.E_AnchorArrived, this);
            Recycle();
        }
    }

    /// <summary>
    /// 回收锚点（隐藏物体，不销毁）
    /// </summary>
    public void Recycle()
    {
        // 恢复角色常规移动
        if (owner != null)
            owner.isBeingPulled = false;

        hasHit = false;
        isReturning = false;
        rb.velocity = Vector2.zero;
        rb.isKinematic = false;
        owner = null;
        transform.SetParent(null);
        gameObject.SetActive(false);
    }
}
