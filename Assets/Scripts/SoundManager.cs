using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoundManager : MonoBehaviour
{
    #region Singleton
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType<SoundManager>();
            }
            return _instance;

        }
    }
    #endregion

    public static float soundDefaultValue = 0.7f;
    public static float musicDefaultValue = 0.5f;

    public static float musicPosition = 0f;

    [System.Serializable]
    public enum SoundType
    {
        CAR_MOTOR, 
        TOWER_TESLA,
        TOWER_INFRA_SOUND,
        TOWER_BLACK_HOLE,
        TOWER_MISSLE_START,
        TOWER_MISSLE_EXPLOSION,
        TOWER_ANTI_MATER,
        TOWER_ANTI_MATER_COLISION,
        CHICKEN_TRANSMISION,
        CAR_TRANSMISION,
        BOOST,
        RACE_COUNTDOWN,
        RACE_START,
        CAR_HIT,
        RESPAWN,
        POINT_ADDED,
        MONEY_SPEND,
        SCREEN_CHANGED,
        PLAYER_READY,
        PLAYER_FINISHED,
        BUTTON_CLICKED
    }

    [System.Serializable]
    public class SoundAudio
    {
        public SoundType type;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        public bool canOverlap = true;
    }
    private class DictValue
    {
        public float time = 0;
        public AudioSource source = null;
    }
    private static Dictionary<SoundType, DictValue> audioStart;

    public List<SoundAudio> audios;
    public SoundAudio music;
    private AudioSource musicSource;
    private void Start()
    {
        audioStart = new Dictionary<SoundType, DictValue>();
        foreach (var i in audios)
        {
            audioStart[i.type] = new DictValue();
            audioStart[i.type].time = Time.time;
        }

        var go = new GameObject("MUSIC_SOUND");
        go.transform.parent = transform;
        musicSource = go.AddComponent<AudioSource>();

        musicSource.volume = music.volume;
        musicSource.loop = true;
        musicSource.clip = music.clip;
        musicSource.time = musicPosition;
        musicSource.Play();

        SetSoundVolume(PlayerPrefs.GetFloat("sound", soundDefaultValue));
        SetMusicVolume(PlayerPrefs.GetFloat("music", musicDefaultValue));

        MenuManager.Instance.onGamePaused.AddListener(HandlePause((s) => s.Pause()));
        MenuManager.Instance.onGameResumed.AddListener(HandlePause((s) => s.UnPause()));

    }

    private UnityAction HandlePause(Action<AudioSource> action)
    {
        return () =>
        {
            foreach (Transform t in Instance.transform)
            {
                if (t.name != "MUSIC_SOUND")
                {
                    var source = t.GetComponent<AudioSource>();
                    action.Invoke(source);
                }
            }
        };
    }

    public static AudioSource PlaySound(SoundType type)
    {
        var sound = Instance.GetSound(type);

        if (sound == null || sound.clip == null)
        {
            Debug.LogWarning("Can't find audioClip for: " + type.ToString());
            return null;
        }

        var go = new GameObject(type.ToString() + "_SOUND");
        go.transform.parent = Instance.transform;
        var audioSource = go.AddComponent<AudioSource>();

        if (CanPlay(sound))
        {
            audioSource.volume = sound.volume * PlayerPrefs.GetFloat("sound", soundDefaultValue);
            audioSource.clip = sound.clip;
            audioSource.Play();
            Destroy(go, sound.clip.length);
        }
        else
        {
            return audioStart[type].source;
        }
        audioStart[type].source = audioSource;
        return audioSource;
    }

    private static bool CanPlay(SoundAudio sound)
    {
        if (!sound.canOverlap)
        {
            if (audioStart[sound.type].time + sound.clip.length >= Time.time)
            {
                return false;
            }
            else
            {
                audioStart[sound.type].time = Time.time;
            }
        }
        return true;
    }

    public SoundAudio GetSound(SoundType type)
    {
        return audios.Where(e => e.type.Equals(type)).FirstOrDefault();
    }

    public static void SetSoundVolume(float value)
    {
        foreach(Transform t in Instance.transform)
        {
            if (t.name != "MUSIC_SOUND")
            {
                var source = t.GetComponent<AudioSource>();
                var sound = Instance.audios.Where(e => e.clip.Equals(source.clip)).FirstOrDefault();
                source.volume = sound.volume * value;
            }
        }
    }

    public void OnSceneChange()
    {
        musicPosition = musicSource.time;
    }
    public static void SetMusicVolume(float value)
    {
        Instance.musicSource.volume = value * Instance.music.volume;
    }
}
