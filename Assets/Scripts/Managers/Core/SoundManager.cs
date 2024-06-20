using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{
    AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.MaxCount];

    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    public void Init()
    {
        GameObject root = GameObject.Find("@Sound");
        if (root == null)
        {
            root = new GameObject { name = "@Sound" };
            Object.DontDestroyOnLoad(root);

            string[] soundNames = System.Enum.GetNames(typeof(Define.Sound));
            for (int i = 0; i < soundNames.Length - 1; ++i)
            {
                GameObject go = new GameObject { name = soundNames[i] };
                _audioSources[i] = go.AddComponent<AudioSource>();
                _audioSources[i].dopplerLevel = 0f;
                _audioSources[i].reverbZoneMix = 0f;
                go.transform.parent = root.transform;
            }

            _audioSources[(int)Define.Sound.Bgm].loop = true;
        }
    }

    public void Clear()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }
        _audioClips.Clear();
    }

    /// <summary>
    /// {path} ==> 폴더이름/사운드이름
    /// </summary>
    public void Play(Define.SFXNames SFXname, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
    {
        Play($"{SFXname}", type, pitch);
    }
    public void Play(string path, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, type);
        Play(audioClip, type, pitch);
    }
    /// <summary>
    /// AudioClip을 받아와서 재생합니다.
    /// </summary>
    public void Play(AudioClip audioClip, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
    {
        if (audioClip == null)
            return;

        if (type == Define.Sound.Bgm)
        {
            if (!Managers.PlayerData.BgmOn)
                return;
            AudioSource audioSource = _audioSources[(int)Define.Sound.Bgm];
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.pitch = pitch;
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else
        {
            if (!Managers.PlayerData.SfxOn)
                return;
            AudioSource audioSource = _audioSources[(int)Define.Sound.Effect];
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
        }
    }

    public void ChangeBGMVolume()
    {
        AudioSource audioSource = _audioSources[(int)Define.Sound.Bgm];
    }

    AudioClip GetOrAddAudioClip(string path, Define.Sound type = Define.Sound.Effect)
    {
        if (path.Contains("Sounds/") == false)
        {
            path = $"Sounds/{path}";
        }

        AudioClip audioClip = null;
        if (type == Define.Sound.Bgm)
        {
            audioClip = Managers.Resource.Load<AudioClip>(path);
        }
        else
        {
            if (_audioClips.TryGetValue(path, out audioClip) == false)
            {
                audioClip = Managers.Resource.Load<AudioClip>(path);
                _audioClips.Add(path, audioClip);
            }
        }

        if (audioClip == null)
        {
            Debug.Log($"AudioClip Missing ! {path}");
        }

        return audioClip;
    }
}
