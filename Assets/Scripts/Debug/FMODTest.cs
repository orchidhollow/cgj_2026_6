using UnityEngine;
using FMODUnity;

/// <summary>
/// FMOD 音效调试工具
/// 挂载到场景物体上，可在 Inspector 中选择事件并播放/停止
/// </summary>
public class FMODTest : MonoBehaviour
{
    [Header("FMOD 事件")]
    [Tooltip("从 FMOD Event Browser 中选择一个事件")]
    public EventReference eventReference;

    [Header("播放设置")]
    [Tooltip("是否在 Awake 时自动播放")]
    public bool playOnAwake = false;

    [Tooltip("是否跟随物体位置（3D音效）")]
    public bool attachToTransform = true;

    [Tooltip("自动重复播放")]
    public bool autoRepeat = false;

    [Tooltip("重复播放间隔（秒）")]
    public float repeatInterval = 2f;

    [Header("事件参数调试")]
    [Tooltip("事件参数名")]
    public string paramName = "";

    [Tooltip("事件参数值")]
    [Range(0f, 1f)]
    public float paramValue = 0f;

    [Header("全局参数调试")]
    [Tooltip("全局参数名")]
    public string globalParamName = "";

    [Tooltip("全局参数值")]
    [Range(0f, 1f)]
    public float globalParamValue = 0f;

    // 运行时实例
    private FMOD.Studio.EventInstance _instance;
    private bool _isPlaying = false;
    private float _lastGlobalParamValue;
    private float _lastParamValue;
    private float _repeatTimer;

    private void Awake()
    {
        if (playOnAwake && !eventReference.IsNull)
        {
            Play();
        }
    }

    private void Update()
    {
        // 更新事件参数 → 重新播放
        if (_isPlaying && !string.IsNullOrEmpty(paramName))
        {
            if (paramValue != _lastParamValue)
            {
                Debug.Log($"[FMODTest] 参数变化，重新播放: {paramName} = {paramValue:F2}");
                Play();
            }
        }

        // 更新全局参数 → 重新播放
        if (!string.IsNullOrEmpty(globalParamName) && globalParamValue != _lastGlobalParamValue)
        {
            RuntimeManager.StudioSystem.setParameterByName(globalParamName, globalParamValue);
            _lastGlobalParamValue = globalParamValue;
            Debug.Log($"[FMODTest] 全局参数变化，重新播放: {globalParamName} = {globalParamValue:F2}");
            Play();
        }

        // 更新 3D 位置
        if (_isPlaying && attachToTransform)
        {
            _instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        }

        // 自动重复播放
        if (autoRepeat)
        {
            _repeatTimer += Time.deltaTime;
            if (_repeatTimer >= repeatInterval)
            {
                _repeatTimer = 0f;
                Play();
            }
        }
    }

    /// <summary>
    /// 播放 FMOD 事件
    /// </summary>
    public void Play()
    {
        if (eventReference.IsNull)
        {
            Debug.LogWarning("[FMODTest] 未设置 EventReference");
            return;
        }

        // 如果已有实例在播放，先停止
        if (_isPlaying)
        {
            Stop();
        }

        _instance = RuntimeManager.CreateInstance(eventReference);

        // 播放前设置事件参数
        if (!string.IsNullOrEmpty(paramName))
        {
            _instance.setParameterByName(paramName, paramValue);
            _lastParamValue = paramValue;
            Debug.Log($"[FMODTest] 播放时设置参数: {paramName} = {paramValue:F2}");
        }

        if (attachToTransform)
        {
            _instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        }

        _instance.start();
        _isPlaying = true;

        Debug.Log($"[FMODTest] 播放: {eventReference.Guid}");
    }

    /// <summary>
    /// 停止 FMOD 事件
    /// </summary>
    public void Stop()
    {
        if (!_isPlaying) return;

        _instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _instance.release();
        _isPlaying = false;

        Debug.Log("[FMODTest] 停止播放");
    }

    /// <summary>
    /// 立即停止（无淡出）
    /// </summary>
    public void StopImmediate()
    {
        if (!_isPlaying) return;

        _instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _instance.release();
        _isPlaying = false;

        Debug.Log("[FMODTest] 立即停止");
    }
   

    /// <summary>
    /// 设置参数并播放
    /// </summary>
    public void PlayWithParam(string name, float value)
    {
        paramName = name;
        paramValue = value;
        Play();
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
    // Editor 下显示调试信息
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.BeginVertical("box");

        GUILayout.Label($"<b>FMOD Debug</b>", new GUIStyle(GUI.skin.label) { fontSize = 14 });
        GUILayout.Space(5);
        GUILayout.Label($"事件: {eventReference.Guid}");
        GUILayout.Label($"播放中: {_isPlaying}");

        if (!string.IsNullOrEmpty(paramName))
        {
            GUILayout.Label($"事件参数: {paramName} = {paramValue:F2}");
        }

        if (!string.IsNullOrEmpty(globalParamName))
        {
            GUILayout.Label($"全局参数: {globalParamName} = {globalParamValue:F2}");
            globalParamValue = GUILayout.HorizontalSlider(globalParamValue, 0f, 1f);
        }

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("播放")) Play();
        if (GUILayout.Button("停止")) Stop();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("立即停止（无淡出）")) StopImmediate();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
#endif
}
