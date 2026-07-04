using UnityEngine;

/// <summary>
/// 星球：自转、重力范围、玩家父子关系管理
///
/// 需要两个碰撞体：
/// 1. 实体 Collider2D（Is Trigger ✗）— 地面，角色站立
/// 2. Trigger Collider2D（Is Trigger ✓）— 重力范围，比星球大一圈
///
/// 玩家进入 Trigger → 绑定重力 + 成为子物体
/// 玩家离开 Trigger → 解绑重力 + 取消子物体
/// </summary>
public class Planet : MonoBehaviour
{
    /// <summary>重力强度</summary>
    public float gravityStrength = 15f;
    /// <summary>自转速度（度/秒，顺时针）</summary>
    public float rotateSpeed = 10f;

    void Start()
    {
        // 游戏开始时，检测是否已有 Player 在 Trigger 范围内
        // OnTriggerEnter2D 只在进入时触发，开始时已在范围内的不会触发
        var player = FindObjectOfType<Player>();
        if (player == null) return;

        // 遍历所有碰撞体，找到 Trigger 碰撞体
        var colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            if (col.isTrigger && col.bounds.Contains(player.transform.position))
            {
                player.targetPlanet = this;
                player.transform.SetParent(transform, true);
                break;
            }
        }
    }

    void Update()
    {
        // 顺时针自转
        transform.Rotate(0, 0, -rotateSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 获取指向星球中心的重力方向（单位向量）
    /// 由 Player 调用，替代手算 dirToCenter
    /// </summary>
    /// <param name="objectPosition">物体的世界坐标</param>
    /// <returns>指向星球中心的单位向量</returns>
    public Vector2 GetGravityDirection(Vector2 objectPosition)
    {
        return ((Vector2)transform.position - objectPosition).normalized;
    }

    /// <summary>
    /// 获取沿星球表面的移动方向（切线，与重力垂直）
    /// 由 Player 调用，替代手算 tangent
    /// </summary>
    /// <param name="objectPosition">物体的世界坐标</param>
    /// <returns>切线方向的单位向量</returns>
    public Vector2 GetMovementDirection(Vector2 objectPosition)
    {
        Vector2 gravity = GetGravityDirection(objectPosition);
        return new Vector2(-gravity.y, gravity.x);
    }

    /// <summary>
    /// 玩家进入重力范围 Trigger：
    /// 1. 绑定 targetPlanet
    /// 2. 延迟一帧设置为子物体（避免激活/禁用冲突）
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        var p = other.GetComponent<Player>();
        if (p != null)
        {
            p.targetPlanet = this;
            // 直接设置父物体（worldPositionStays=true 保持世界坐标不变）
            other.transform.SetParent(transform, true);
            // 根据星球名称切换摄像机
            if (LevelManager.Instance != null)
                LevelManager.Instance.SwitchCameraByPlanet(gameObject.name);
        }
    }

    /// <summary>
    /// 玩家离开重力范围 Trigger：
    /// 1. 解绑 targetPlanet
    /// 2. 取消子物体
    /// </summary>
    void OnTriggerExit2D(Collider2D other)
    {
        var p = other.GetComponent<Player>();
        if (p != null && p.targetPlanet == this)
        {
            p.targetPlanet = null;
            other.transform.SetParent(null, true);
        }
    }
}
