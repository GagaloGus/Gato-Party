using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    static public AudioManager instance;
    public List<AudioSource> activeAudioSources;
    public GameObjPool audioPool;
    AudioSource ambientSource;

    [Header("Volume")]
    [Range(0, 1)] public float volumeMusic;
    [Range(0, 1)]public float volumeSFX;

    void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            activeAudioSources = new List<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }
        audioPool = transform.Find("GameObjPoolAudios").gameObject.GetComponent<GameObjPool>();
        ambientSource = transform.Find("AmbientMusic").GetComponent<AudioSource>();

        AudioListener.volume = 1;
    }

    private void Update()
    {
        ambientSource.volume = volumeMusic;
    }
    public void SetMusicVolume(float volume)
    {
        volumeMusic = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        volumeSFX = Mathf.Clamp01(volume);
    }

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
    }

    void PlaySFX(AudioClip clip, Vector3 position, bool audio3d, float volume = 1)
    {
        GameObject sourceObj = audioPool.GetFirstInactiveGameObject();
        sourceObj.SetActive(true);
        sourceObj.transform.position = position;

        AudioSource source = sourceObj.GetComponent<AudioSource>();
        activeAudioSources.Add(source);

        source.spatialBlend = ( audio3d ? 1 : 0 );

        source.clip = clip;
        source.volume = volume * volumeSFX;
        source.Play();
        StartCoroutine(PlayAudio(source));

        //Debug.Log($"Play SFX: {clip.name}, {(audio3d ? "3D" : "2D")}, {volume}");
    }

    public void PlaySFX2D(AudioClip clip, float volume = 1)
    {
        PlaySFX(clip, Camera.main.transform.position, false ,volume);    
    }

    public void PlaySFX3D(AudioClip clip, Vector3 position, float volume = 1)
    {
        PlaySFX(clip, position, true,volume);    
    }

    public void PlayAmbientMusic(AudioClip clip, float volume = 1)
    {
        StopCoroutine(nameof(FadeOutInAmbientMusic));

        StartCoroutine(FadeOutInAmbientMusic(clip, volume));
    }

    public void StopAmbientMusic()
    {
        StartCoroutine(FadeOutAmbientMusic());
    }

    public void ForcePlayAmbientMusic(AudioClip clip, float volume = 1)
    {
        ambientSource.clip = clip;
        ambientSource.volume = volume;
        AudioListener.volume = 1;

        ambientSource.Play();
    }

    IEnumerator FadeOutAmbientMusic()
    {
        float originalVolume = AudioListener.volume;

        for (float i = originalVolume; i > 0; i -= 0.02f)
        {
            AudioListener.volume = i;
            yield return null;
        }

        ambientSource.Stop();
        AudioListener.volume = 1;
    }

    IEnumerator FadeOutInAmbientMusic(AudioClip clip, float volume = 1)
    {
        //Observa si se esta reproduciendo alguna cancion
        if(ambientSource.isPlaying)
        {
            float originalVolume = AudioListener.volume;
            
            //Baja el volumen gradualmente
            for (float i = originalVolume; i > 0; i -= 0.02f)
            {
                AudioListener.volume = i;
                yield return null;
            }

            AudioListener.volume = 0;
            yield return new WaitForSeconds(0.3f);
        }
        
        //Pone el volumen a 0 del AudioListener
        AudioListener.volume = 0;

        //Cambia de cancion
        ambientSource.clip = clip;
        ambientSource.volume = volume * volumeMusic;
        ambientSource.Play();

        //Sube el volumen gradualmente
        for (float i = 0; i <= volume; i+= 0.02f)
        {
            AudioListener.volume = i;
            yield return null;
        }

        AudioListener.volume = 1;
    }

    public void ClearAudioList()
    {
        foreach (AudioSource source in activeAudioSources)
        {
            source.Stop();
            source.gameObject.SetActive(false);
        }

        ambientSource.Stop();
        StopAllCoroutines();
        AudioListener.volume = 1;
        activeAudioSources.Clear();
    }

    IEnumerator PlayAudio(AudioSource source)
    {
        while (source && source.isPlaying)
        {
            yield return new WaitForSeconds(0.05f);
        }
        if (source)
        {
            source.gameObject.SetActive(false);
        }
    }
}
