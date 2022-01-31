using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Threading.Tasks;

public class MusicManager : MonoBehaviour
{
    [System.Serializable]
    public class MusicAudio
    {
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
    }

    #region Singleton
    private static MusicManager _instance;
    public static MusicManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindGameObjectsWithTag("Music")[0].GetComponent<MusicManager>();
            }
            return _instance;

        }
    }
    #endregion

    public static float musicDefaultValue = 0.5f;

    public MusicAudio menuMusic;
    public MusicAudio intro;
    public MusicAudio gameMusic;

    private AudioSource musicSource;

    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Music");

        if (objs.Length > 1)
        {
            Debug.Log("Destroyng musicManager...");
            Destroy(this.gameObject);
        }
        else
        {
            Debug.Log("DontDestroyOnLoad musicManager...");
            _instance = this;
            var scene = SceneManager.GetActiveScene();
            SceneManager.sceneLoaded += SceneManager_sceneLoaded; ;
            SetMusicVolume(PlayerPrefs.GetFloat("music", musicDefaultValue));
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Instance.HandleSceneMusic(scene.name);
    }

    public void HandleSceneMusic(String sceneName)
    {
        Debug.Log("HandleSceneMusic for " + sceneName);

        if (musicSource != null)
        {
            FadeOut(musicSource);
        }
        if (sceneName.Equals("MainMenu"))
        {
            PlayIntro();
            PlayMenuSound();
        }
        else
        {
            PlayIntro();
            PlayGameSound();
        }
    }
    public async void FadeOut(AudioSource music)
    {
        Debug.Log("FadeOut " + music.gameObject.name);
        var originaVolume = music.volume;
        while(music.volume > 0)
        {
            music.volume -= originaVolume / 20;
            await Task.Delay(1000 / 20);
        }
        Destroy(music.gameObject);
    }
    public async void DestroyAfter(float seconds, GameObject obj)
    {
        await Task.Delay(Mathf.RoundToInt(seconds * 1000));
        GameObject.Destroy(obj);
    }


    private void PlayIntro()
    {
        var go = new GameObject("INTRO_SOUND");
        go.transform.parent = transform;
        var audioSource = go.AddComponent<AudioSource>();

        audioSource.volume = intro.volume * PlayerPrefs.GetFloat("music", musicDefaultValue);
        audioSource.clip = intro.clip;
        audioSource.Play();
        DestroyAfter(intro.clip.length, go);
    }
    public async void PlayMenuSound()
    {
        await Task.Delay(1000);
        var go = new GameObject("MENU_SOUND");
        go.transform.parent = transform;
        musicSource = go.AddComponent<AudioSource>();

        musicSource.volume = menuMusic.volume * PlayerPrefs.GetFloat("music", musicDefaultValue);
        musicSource.clip = menuMusic.clip;
        musicSource.loop = true;
        musicSource.Play();
        //Destroy(go, Instance.intro.clip.length);
    }
    public async void PlayGameSound()
    {
        await Task.Delay(1000);
        var go = new GameObject("GAME_SOUND");
        go.transform.parent = transform;
        musicSource = go.AddComponent<AudioSource>();

        musicSource.volume = gameMusic.volume * PlayerPrefs.GetFloat("music", musicDefaultValue);
        musicSource.clip = gameMusic.clip;
        musicSource.loop = true;
        musicSource.Play();
        //Destroy(go, Instance.intro.clip.length);
    }
    public static void SetMusicVolume(float value)
    {
        if (Instance.musicSource != null)
        {
            Instance.musicSource.volume = value;
        }
    }
}
