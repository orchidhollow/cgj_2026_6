using UnityEngine;
using FMODUnity;

/// <summary>
/// 脚步声测试：摄像机水平移动一定距离后播放一次脚步音效
/// 根据地面 Tag 切换材质参数
/// </summary>
public class FootstepTest : MonoBehaviour
{
    [Header("脚步音效")]
    public EventReference footstepEvent;

    [Header("移动设置")]
    [Tooltip("每移动多少米播放一次脚步声")]
    public float stepDistance = 2f;

    [Header("地面检测")]
    [Tooltip("射线检测距离")]
    public float rayDistance = 10f;

    [Tooltip("FMOD 材质参数名")]
    public string materialParam = "switch_footstep_mat";

    private Vector3 _lastPosition;
    private float _distanceMoved;

    private void Start()
    {
        _lastPosition = transform.position;
        _distanceMoved = 0f;
    }

    private void Update()
    {
        // 只计算水平移动距离（忽略上下）
        Vector3 currentPos = transform.position;
        Vector3 delta = currentPos - _lastPosition;
        delta.y = 0f;

        _distanceMoved += delta.magnitude;
        _lastPosition = currentPos;

        // 移动距离达到阈值，播放脚步声
        if (_distanceMoved >= stepDistance)
        {
            _distanceMoved -= stepDistance;
            PlayFootstep();
        }
    }

    private void PlayFootstep()
    {
        if (footstepEvent.IsNull) return;

        // 向下发射射线，检测地面材质
        int materialValue = 0;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayDistance))
        {
            if (hit.collider.CompareTag("m1"))
            {
                materialValue = 0;
            }
            else if (hit.collider.CompareTag("m2"))
            {
                materialValue = 1;
            }
        }

        // 播放音效并设置材质参数
        var instance = RuntimeManager.CreateInstance(footstepEvent);
        instance.setParameterByName(materialParam, materialValue);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        instance.start();
        instance.release();

        Debug.Log($"[Footstep] 材质: {(materialValue == 0 ? "m1" : "m2")}");
    }
}
