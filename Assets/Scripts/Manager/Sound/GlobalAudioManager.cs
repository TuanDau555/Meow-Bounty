using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handle music of the scene and ui sfx
/// Note: this script will the play music's source for the scene
/// </summary>
public class GlobalAudioManager : SingletonPersistent<GlobalAudioManager>
{
#region Parameters

    [Header("Audio Data")]
    [Tooltip("Global Audio SO")]
    [SerializeField] private GlobalAudioSO globalAudioSO;
    
    [Header("Audio Source")]
    [Tooltip("Music Source in current scene")]
    [SerializeField] private AudioSource musicSourceA;

    [Tooltip("Music Source in the next scene")]
    [SerializeField] private AudioSource musicSourceB;
    
    [SerializeField] private AudioSource sfxPrefab;

    private Dictionary<string, SoundEntry> uiSfxDict;

    private AudioSource activeMusic;
    private AudioSource idleMusic;
    private bool _isCrossfading = false;

    #endregion

    #region Execute

    protected override void Awake()
    {
        base.Awake();

        
        
        BuildDictionary();
        SetupMusicSources();
        PlayDefaultMusic();
    }
    
    #endregion

    #region Set up 

    private void BuildDictionary()
    {
        uiSfxDict = new Dictionary<string, SoundEntry>();

        foreach (var sfx in globalAudioSO.uiSfxList)
        {
            if (!uiSfxDict.ContainsKey(sfx.id))
                uiSfxDict.Add(sfx.id, sfx);
        }
    }

    private void SetupMusicSources()
    {
        activeMusic = musicSourceA;
        idleMusic = musicSourceB;
    }
    
    private void PlayDefaultMusic()
    {
        if(globalAudioSO.defaultMusic == null) return;

        activeMusic.clip = globalAudioSO.defaultMusic;
        activeMusic.loop = true;
        activeMusic.Play();
    }
    
    #endregion

    #region UI SFX

    public void PlayUISFX(string id)
    {
        if(!uiSfxDict.TryGetValue(id, out var entry))
        {
            Debug.LogWarning($"UI SFX not found: {id}");
            return;
        }

        AudioSource src = ObjectPoolManager.SpawnObject(
            sfxPrefab,
            transform,
            Quaternion.identity,
            PoolType.SoundFX
        );

        src.clip = entry.clip;
        src.volume = entry.volume;
        src.loop = false;
        src.pitch = Random.Range(entry.minPitch, entry.maxPitch);
        
        src.Play();

        StartCoroutine(ReturnAfterPlay(src, entry.clip.length));
    }

    private IEnumerator ReturnAfterPlay(AudioSource src, float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPoolManager.ReturnObjectToPool(src.gameObject, PoolType.SoundFX);
    }

    #endregion

    #region Music Crossfade

    public void CrossFadeMusic(AudioClip newMusic, float fadeTime = 1f)
    {
        if(_isCrossfading) return;
        if(newMusic == null) return;

        if(activeMusic.clip == newMusic && activeMusic.isPlaying) return;
        
        StartCoroutine(CrossfadeRoutine(newMusic, fadeTime));
    }

    private IEnumerator CrossfadeRoutine(AudioClip newMusic, float fadeTime)
    {
        _isCrossfading = true;
        
        idleMusic.clip = newMusic;
        idleMusic.loop = true;
        idleMusic.volume = 0;
        idleMusic.Play();
        
        float t = 0;

        while(t < fadeTime)
        {
            t += Time.deltaTime;

            float ratio = t / fadeTime;

            activeMusic.volume = 1 - ratio;
            idleMusic.volume = ratio;
            
            yield return null;
        }

        activeMusic.Stop();

        // Swap Music
        var temp = activeMusic;
        activeMusic = idleMusic;
        idleMusic = temp;

        Debug.Log($"Crossfade: active = {activeMusic.clip?.name}, idle = {idleMusic.clip?.name}");
        _isCrossfading = false;
    }

    public void ResetMusicSource()
    {
        StopAllCoroutines();
        
        activeMusic.Stop();
        idleMusic.Stop();

        activeMusic.volume = 1;
        idleMusic.volume = 0;

        activeMusic = musicSourceA;
        idleMusic = musicSourceB;

        _isCrossfading = false;
    }
    
    #endregion
}