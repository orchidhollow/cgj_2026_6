using UnityEngine;
using FMODUnity;

/// <summary>
/// 滑动音效测试：根据物体速度归一化后设置 FMOD speed 参数
/// 音效启动后一直播放，speed 控制音效大小
/// </summary>
public class SlideTest : MonoBehaviour
{
    [Header("FMOD 事件")]
    public EventReference slideEvent;

    [Tooltip("到达目标距离时播放的事件")]
    public EventReference endEvent;

    [Header("设置")]
    [Tooltip("最大速度（用于归一化）")]
    public float maxSpeed = 10f;

    [Tooltip("FMOD 速度参数名")]
    public string speedParam = "speed";

    [Tooltip("触发结束事件的距离")]
    public float targetDistance = 10f;

    private Vector3 _lastPosition;
    private float _currentSpeed;
    private float _normalizedSpeed;
    private float _totalDistance;
    private FMOD.Studio.EventInstance _instance;
    private bool _isPlaying;
    private bool _hasStarted;
    private bool _endTriggered;

    private void Start()
    {
        _lastPosition = transform.position;
    }

    private void Update()
    {
        // 计算速度和距离
        Vector3 delta = transform.position - _lastPosition;
        float frameDistance = delta.magnitude;
        _currentSpeed = frameDistance / Time.deltaTime;
        _totalDistance += frameDistance;
        _lastPosition = transform.position;

        // 归一化速度 (0~1)
        _normalizedSpeed = Mathf.Clamp01(_currentSpeed / maxSpeed);

        // 开始移动时播放，之后一直播放
        if (!_hasStarted && _currentSpeed > 0.01f)
        {
            Play();
            _hasStarted = true;
        }

        // 到达目标距离时播放结束事件
        if (!_endTriggered && _totalDistance >= targetDistance)
        {
            PlayEndEvent();
            _endTriggered = true;
        }

        // 更新参数和位置
        if (_isPlaying)
        {
            _instance.setParameterByName(speedParam, _normalizedSpeed);
            _instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        }
    }

    public void Play()
    {
        if (slideEvent.IsNull || _isPlaying) return;

        _instance = RuntimeManager.CreateInstance(slideEvent);
        _instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        _instance.start();
        _isPlaying = true;

        Debug.Log("[SlideTest] 开始播放");
    }

    public void Stop()
    {
        if (!_isPlaying) return;

        _instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _instance.release();
        _isPlaying = false;

        Debug.Log("[SlideTest] 停止播放");
    }

    private void PlayEndEvent()
    {
        if (endEvent.IsNull) return;

        RuntimeManager.PlayOneShot(endEvent, transform.position);
        Debug.Log($"[SlideTest] 到达目标距离 {targetDistance}m，播放结束事件");
    }

    private void OnDestroy()
    {
        if (_isPlaying)
        {
            _instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _instance.release();
        }
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 220, 300, 100));
        GUILayout.BeginVertical("box");

        GUILayout.Label($"<b>Slide Debug</b>");
        GUILayout.Label($"速度: {_currentSpeed:F2}");
        GUILayout.Label($"speed: {_normalizedSpeed:F2}");
        GUILayout.Label($"距离: {_totalDistance:F1}m / {targetDistance}m");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("播放")) Play();
        if (GUILayout.Button("停止")) Stop();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
#endif
}
