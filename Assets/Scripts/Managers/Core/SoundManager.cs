using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{
    AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.MaxCount];

    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
    Queue<AudioSource> _audioSourcePool = new Queue<AudioSource>();

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

    public void Play(Define.UISFX SFXname, Define.Sound type = Define.Sound.Effect)
    {
        Play($"{SFXname}", type);
    }

    public void Play(string path, Define.Sound type = Define.Sound.Effect)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, type);
        Play(audioClip, type);
    }

    public void Play(AudioClip audioClip, Define.Sound type = Define.Sound.Effect)
    {
        if (audioClip == null)
            return;

        if (type == Define.Sound.Bgm)
        {
            AudioSource audioSource = _audioSources[(int)Define.Sound.Bgm];
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.pitch = 1f;
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else
        {
            if (!Managers.PlayerData.SfxOn)
                return;
            AudioSource audioSource = _audioSources[(int)Define.Sound.Effect];
            audioSource.pitch = Managers.Time.CurTimeScale;
            audioSource.PlayOneShot(audioClip);
        }
    }

    public AudioSource PlayOnObject(string path, Vector3 position)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, Define.Sound.Effect);
        return PlayOnObject(audioClip, position);
    }

    public AudioSource PlayOnObjectLoop(string path, Vector3 position)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, Define.Sound.Effect);
        return PlayOnObject(audioClip, position,loop: true);
    }

    public AudioSource PlayOnObject(AudioClip audioClip, Vector3 position, bool loop = false)
    {
        if (audioClip == null)
            return null;

        if (!Managers.PlayerData.SfxOn)
            return null;

        AudioSource audioSource = GetPooledAudioSource();
        audioSource.transform.position = position;
        audioSource.clip = audioClip;
        audioSource.pitch = Managers.Time.CurTimeScale;
        audioSource.loop = loop;
        audioSource.Play();

        if (!loop)
        {
            Managers.Timer.StartTimer(audioClip.length, () => ReturnToPool(audioSource));
        }

        return audioSource;
    }

    public void StopAndReturnToPool(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            ReturnToPool(audioSource);
        }
    }

    AudioSource GetPooledAudioSource()
    {
        if (_audioSourcePool.Count > 0)
        {
            AudioSource audioSource = _audioSourcePool.Dequeue();
            audioSource.gameObject.SetActive(true);
            return audioSource;
        }
        else
        {
            GameObject audioObject = new GameObject("PooledAudioSource");
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.volume = 1f;
            audioSource.spatialBlend = 1.0f;
            audioSource.minDistance = 1.0f;
            audioSource.maxDistance = 50.0f;
            return audioSource;
        }
    }

    void ReturnToPool(AudioSource audioSource)
    {
        audioSource.gameObject.SetActive(false);
        audioSource.clip = null;
        _audioSourcePool.Enqueue(audioSource);
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
