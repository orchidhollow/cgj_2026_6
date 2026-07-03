using UnityEngine;

/// <summary>
/// 角色控制器：星球重力、移动、跳跃、投掷锚点
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Planet")]
    /// <summary>当前受引力影响的星球</summary>
    public Planet targetPlanet;

    [Header("Movement")]
    /// <summary>移动速度</summary>
    public float moveSpeed = 5f;
    /// <summary>跳跃力度</summary>
    public float jumpForce = 8f;

    [Header("Anchor")]
    /// <summary>锚点预制体（Inspector 手动拖入）</summary>
    public GameObject anchorPrefab;

    private Rigidbody2D rb;
    /// <summary>缓存的锚点实例（只创建一次，反复复用）</summary>
    private Anchor currentAnchor;
    /// <summary>是否正在被锚点拉动（拉动期间暂停常规移动和重力）</summary>
    [HideInInspector] public bool isBeingPulled;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        // 被锚点拉动时，暂停常规移动和重力
        if (isBeingPulled) return;

        if (targetPlanet == null) return;

        // 计算指向星球中心的方向和距离
        Vector2 toCenter = (Vector2)(targetPlanet.transform.position - transform.position);
        float dist = toCenter.magnitude;
        Vector2 dirToCenter = toCenter / dist;

        // 重力：向星球中心施加力
        rb.AddForce(dirToCenter * targetPlanet.gravityStrength, ForceMode2D.Force);

        // 调整朝向：角色 up 方向始终背离星球（头朝外脚朝内）
        float angle = Mathf.Atan2(dirToCenter.y, dirToCenter.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90f);

        // 沿星球表面切线方向移动（与重力方向垂直）
        Vector2 tangent = new Vector2(-dirToCenter.y, dirToCenter.x);
        float h = Input.GetAxisRaw("Horizontal");
        rb.velocity = tangent * h * moveSpeed + Vector2.Dot(rb.velocity, dirToCenter) * dirToCenter;
    }

    void Update()
    {
        // 跳跃：沿背离星球方向弹射
        if (targetPlanet != null && Input.GetButtonDown("Jump"))
        {
            Vector2 up = -((Vector2)(targetPlanet.transform.position - transform.position)).normalized;
            rb.AddForce(up * jumpForce, ForceMode2D.Impulse);
        }

        // 鼠标左键投掷锚点
        if (Input.GetMouseButtonDown(0))
        {
            FireAnchor();
        }
    }

    /// <summary>
    /// 投掷锚点：首次创建实例，之后复用
    /// </summary>
    void FireAnchor()
    {
        // 首次使用时实例化锚点
        if (currentAnchor == null)
        {
            GameObject go = Instantiate(anchorPrefab);
            currentAnchor = go.GetComponent<Anchor>();
        }

        // 如果当前锚点还在活跃中，先回收
        if (currentAnchor.gameObject.activeSelf)
        {
            currentAnchor.Recycle();
        }

        // 激活并初始化
        currentAnchor.gameObject.SetActive(true);
        currentAnchor.transform.position = transform.position;

        // 计算鼠标世界坐标方向
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector2 dir = (mouseWorld - transform.position).normalized;

        currentAnchor.Init(this, dir);
        EventCenter.Instance.EventTrigger(E_EventType.E_AnchorFired, currentAnchor);
    }
}
