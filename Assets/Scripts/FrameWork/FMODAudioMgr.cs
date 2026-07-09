using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// FMOD 音频单例管理器
/// 统一管理所有音效、音乐、环境音的播放
/// 挂在场景空物体上或依赖 SingletonAutoMono 自动创建
/// </summary>
public class FMODAudioMgr : SingletonAutoMono<FMODAudioMgr>
{
    // ===== 持续音效实例（需要生命周期管理） =====
    private EventInstance musicInstance;     // 当前音乐
    private EventInstance ambienceInstance;  // 当前环境音
    private EventInstance windInstance;      // 被拉动时风声

    // ===== 一次性音效 =====

    /// <summary>脚步声（参数 surface: snow/stone/ice）</summary>
    public void PlayFootstep(string surface)
    {
        var instance = RuntimeManager.CreateInstance("event:/player_footstep");
        instance.setParameterByNameWithLabel("surface", surface);
        instance.start();
        instance.release();
    }

    /// <summary>激光发射锚</summary>
    public void PlayThrow()
    {
        RuntimeManager.PlayOneShot("event:/throw");
    }

    /// <summary>锚命中（参数 hit: true=钩中, false=弹开）</summary>
    public void PlayHit(bool hit)
    {
        var instance = RuntimeManager.CreateInstance("event:/hit");
        instance.setParameterByName("hit", hit ? 1f : 0f);
        instance.start();
        instance.release();
    }

    /// <summary>玩家跳跃</summary>
    public void PlayJump()
    {
        RuntimeManager.PlayOneShot("event:/junp");
    }

    /// <summary>玩家落地</summary>
    public void PlayDown()
    {
        RuntimeManager.PlayOneShot("event:/down");
    }

    /// <summary>玩家死亡</summary>
    public void PlayDie()
    {
        RuntimeManager.PlayOneShot("event:/die");
    }

    /// <summary>UI 选中</summary>
    public void PlayUISelect()
    {
        RuntimeManager.PlayOneShot("event:/ui_select");
    }

    /// <summary>UI 确认</summary>
    public void PlayUIConfirm()
    {
        RuntimeManager.PlayOneShot("event:/ui_confirm");
    }

    /// <summary>播放开场 CG 音频</summary>
    public void PlayCG()
    {
        StopMusic();
        RuntimeManager.PlayOneShot("event:/cg");
    }

    // ===== 持续音效（需要生命周期管理） =====

    /// <summary>开始播放被拉动风声</summary>
    public void StartWind()
    {
        if (windInstance.isValid()) return;
        windInstance = RuntimeManager.CreateInstance("event:/speed_wind");
        windInstance.start();
    }

    /// <summary>更新风声速度参数（归一化到 0-1）</summary>
    /// <param name="normalizedSpeed">当前速度 / 最大速度</param>
    public void UpdateWindSpeed(float normalizedSpeed)
    {
        if (!windInstance.isValid()) return;
        windInstance.setParameterByName("speed", Mathf.Clamp01(normalizedSpeed));
    }

    /// <summary>停止风声</summary>
    public void StopWind()
    {
        if (!windInstance.isValid()) return;
        windInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        windInstance.release();
        windInstance.clearHandle();
    }

    // ===== 背景音乐 =====

    /// <summary>播放开始界面音乐</summary>
    public void PlayBGMMain()
    {
        StopMusic();
        musicInstance = RuntimeManager.CreateInstance("event:/bgm_main");
        musicInstance.start();
    }

    /// <summary>播放游戏配乐</summary>
    public void PlayBGMGame()
    {
        StopMusic();
        musicInstance = RuntimeManager.CreateInstance("event:/bgm_game");
        musicInstance.start();
    }

    /// <summary>停止当前音乐</summary>
    public void StopMusic()
    {
        if (!musicInstance.isValid()) return;
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
        musicInstance.clearHandle();
    }

    // ===== 环境音 =====

    /// <summary>播放游戏环境音</summary>
    public void PlayAmbience()
    {
        StopAmbience();
        ambienceInstance = RuntimeManager.CreateInstance("event:/amb");
        ambienceInstance.start();
    }

    /// <summary>停止环境音</summary>
    public void StopAmbience()
    {
        if (!ambienceInstance.isValid()) return;
        ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        ambienceInstance.release();
        ambienceInstance.clearHandle();
    }

    // ===== 清理 =====

    void OnDestroy()
    {
        StopMusic();
        StopAmbience();
        StopWind();
    }
}
