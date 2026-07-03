using UnityEngine;
using FMODUnity;

/// <summary>
/// 混响测试：根据地面 Tag 切换混响快照
/// </summary>
public class ReverbTest : MonoBehaviour
{
    [Header("混响快照")]
    [Tooltip("m1 对应的混响快照")]
    public EventReference reverbSnapshot1;

    [Tooltip("m2 对应的混响快照")]
    public EventReference reverbSnapshot2;

    [Header("地面检测")]
    [Tooltip("射线检测距离")]
    public float rayDistance = 10f;

    private FMOD.Studio.EventInstance _reverbInstance;
    private int _currentMaterial = -1;

    private void Update()
    {
        int materialValue = GetGroundMaterial();

        if (materialValue != _currentMaterial)
        {
            SwitchReverb(materialValue);
            _currentMaterial = materialValue;
        }
    }

    private int GetGroundMaterial()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayDistance))
        {
            if (hit.collider.CompareTag("m2"))
                return 1;
        }
        return 0;
    }

    private void SwitchReverb(int materialValue)
    {
        // 停止当前快照
        if (_reverbInstance.isValid())
        {
            _reverbInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _reverbInstance.release();
        }

        // 选择快照
        EventReference snapshot = materialValue == 0 ? reverbSnapshot1 : reverbSnapshot2;
        if (snapshot.IsNull) return;

        // 播放新快照
        _reverbInstance = RuntimeManager.CreateInstance(snapshot);
        _reverbInstance.start();

        Debug.Log($"[ReverbTest] 切换到: {(materialValue == 0 ? "reverb1 (m1)" : "reverb2 (m2)")}");
    }

    private void OnDestroy()
    {
        if (_reverbInstance.isValid())
        {
            _reverbInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _reverbInstance.release();
        }
    }
}
