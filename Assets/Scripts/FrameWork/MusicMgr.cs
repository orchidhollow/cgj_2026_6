using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 音效音乐管理器
/// </summary>
public class MusicMgr : BaseManager<MusicMgr>
{
    // 背景音乐组件
    private AudioSource bkMusic = null;
    // 背景音乐大小
    private float bkMusicValue = 0.1f;

    // 正在播放的音效列表
    private List<AudioSource> soundList = new List<AudioSource>();
    // 音效播放大小
    private float soundValue = 0.1f;
    // 判断音效播放是否暂停
    private bool soundIsPlay = true;

    private MusicMgr()
    {
        MonoMgr.Instance.AddUpateListener(Update);
    }

    private void Update()
    {
        if (!soundIsPlay) return;

        for (int i = soundList.Count - 1; i >= 0; i--)
        {
            if (!soundList[i].isPlaying)
            {
                soundList[i].clip = null;
                PoolMgr.Instance.PushObj(soundList[i].gameObject);
                soundList.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void PlayerBKMusic(string path)
    {
        if (bkMusic == null)
        {
            GameObject obj = new GameObject();
            obj.name = "BKMusic";
            GameObject.DontDestroyOnLoad(obj);
            bkMusic = obj.AddComponent<AudioSource>();
        }
        ResourcesMgr.Instance.LoadAsync<AudioClip>(path, (AudioClip) =>
        {
            bkMusic.clip = AudioClip;
            bkMusic.loop = true;
            bkMusic.volume = bkMusicValue;
            bkMusic.Play();
        });
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBKMusic()
    {
        if (bkMusic == null) return;
        bkMusic.Stop();
    }

    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseBKMusic()
    {
        if (bkMusic == null) return;
        bkMusic.Pause();
    }

    /// <summary>
    /// 设置背景音乐大小
    /// </summary>
    public void ChangeBKMusicValue(float value)
    {
        bkMusicValue = value;
        if (bkMusic == null) return;
        bkMusic.volume = bkMusicValue;
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="path">音效路径</param>
    /// <param name="isLoop">是否循环</param>
    /// <param name="isSync">是否同步加载</param>
    /// <param name="callback">加载完成后的回调</param>
    public void PlaySound(string path, bool isLoop = false, bool isSync = false, UnityAction<AudioSource> callback = null)
    {
        AudioSource source = PoolMgr.Instance.GetObj("Sounds/Prefabs/soundObj").GetComponent<AudioSource>();

        if (isSync)
        {
            AudioClip audioClip = ResourcesMgr.Instance.Load<AudioClip>(path);
            source.Stop();
            source.clip = audioClip;
            source.loop = isLoop;
            source.volume = soundValue;
            source.Play();
            if (!soundList.Contains(source))
                soundList.Add(source);
        }
        else
        {
            ResourcesMgr.Instance.LoadAsync<AudioClip>(path, (AudioClip) =>
            {
                source.clip = AudioClip;
                source.loop = isLoop;
                source.volume = soundValue;
                source.Play();
                soundList.Add(source);
                callback?.Invoke(source);
            });
        }
    }

    /// <summary>
    /// 停止播放指定音效
    /// </summary>
    public void StopSound(AudioSource source)
    {
        if (soundList.Contains(source))
        {
            source.Stop();
            soundList.Remove(source);
            source.clip = null;
            PoolMgr.Instance.PushObj(source.gameObject);
        }
    }

    /// <summary>
    /// 改变音效大小
    /// </summary>
    public void ChangeSoudnValue(float value)
    {
        soundValue = value;
        foreach (AudioSource source in soundList)
        {
            source.volume = value;
        }
    }

    /// <summary>
    /// 播放或暂停音效
    /// </summary>
    /// <param name="isPlay">true为播放 false为暂停</param>
    public void PlayOrPauseSound(bool isPlay)
    {
        if (isPlay)
        {
            foreach (AudioSource source in soundList)
            {
                source.UnPause();
            }
        }
        else
        {
            foreach (AudioSource source in soundList)
            {
                source.Pause();
            }
        }
    }

    /// <summary>
    /// 清空音效相关记录 需要时调用
    /// </summary>
    public void ClearSound()
    {
        for (int i = 0; i < soundList.Count; i++)
        {
            soundList[i].Stop();
            soundList[i].clip = null;
            PoolMgr.Instance.PushObj(soundList[i].gameObject);
        }
        soundList.Clear();
    }
}
