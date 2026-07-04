using UnityEngine;

/// <summary>
/// 关卡终点：Player 进入 Trigger 时通知 LevelManager
/// 挂在终点物体上，需要有 Collider2D（Is Trigger ✓）
/// </summary>
public class EndPoint : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (LevelManager.Instance != null)
                LevelManager.Instance.OnReachEndPoint();
        }
    }
}
