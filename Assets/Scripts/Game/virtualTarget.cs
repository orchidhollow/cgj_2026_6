using UnityEngine;

/// <summary>
/// 将此脚本挂载到一个独立的空物体上 (CameraTarget)，
/// 依然让 Cinemachine Follow 这个空物体。
/// </summary>
public class CameraMouseFollow : MonoBehaviour
{
    [Header("核心引用")]
    public Transform player; 

    [Header("偏移设置")]
    public float deadZone = 2f;      
    public float maxOffset = 4f;     
    public float smoothSpeed = 5f;   

    // 内部记录当前的“平滑偏移量”
    private Vector3 currentOffset = Vector3.zero; 

    // ⚠️ 必须用 LateUpdate：确保小行星转完了，玩家跟着转完了，我们再算摄像机！
    void LateUpdate()
    {
        if (player == null) return;

        // 1. 获取鼠标的世界坐标
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = player.position.z; 

        // 2. 计算玩家到鼠标的距离
        float distance = Vector3.Distance(player.position, mouseWorldPos);
        
        // 目标偏移量，默认是 0（在死区内就是0）
        Vector3 targetOffset = Vector3.zero; 

        // 3. 如果鼠标移出了死区，就开始计算相对偏移
        if (distance > deadZone)
        {
            Vector3 direction = (mouseWorldPos - player.position).normalized;
            float offsetMagnitude = Mathf.Min(distance - deadZone, maxOffset);
            targetOffset = direction * offsetMagnitude;
        }

        // 4. 【核心魔法】我们不去 Lerp 绝对坐标，而是 Lerp 这根“隐形的皮筋（偏移量）”
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * smoothSpeed);

        // 5. 替身的最终位置 = 玩家这一帧死死钉住的位置 + 平滑后的皮筋偏移
        transform.position = player.position + currentOffset;
    }
}