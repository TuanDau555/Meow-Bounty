using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Each scene will have difference Sound Manager
/// Note: this script will only request the music change
/// </summary>
public class SceneSoundManager : Singleton<SceneSoundManager>
{
    #region Parameters

    [Header("Audio Data")]
    [Tooltip("Audio SO")]
    [SerializeField] private SceneAudioSO sceneAudioSO;

    [Space(10)]
    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxPrefab;

    private Dictionary<string, SoundEntry> sfxDict;

    #endregion

    #region Execute

    protected override void Awake()
    {
        base.Awake();
        BuidDictionary();
    }

    private IEnumerator Start()
    {
        Debug.Log($"SceneSoundManager Start() called — {gameObject.name}");
        
        yield return WaitForGlobalAudio();

        GlobalAudioManager.Instance.ResetMusicSource();
        
        RequestSceneMusic();
        PlayAmbientLoop();
    }

    #endregion

    #region Set Up

    private void BuidDictionary()
    {
        sfxDict = new Dictionary<string, SoundEntry>();

        foreach(var sfx in sceneAudioSO.sfxList)
        {
            if (!sfxDict.ContainsKey(sfx.id))
            {
                sfxDict.Add(sfx.id, sfx);
            }
        }
    }
    
    #endregion
    
    #region Music

    private void RequestSceneMusic()
    {
        if(sceneAudioSO.music == null) return;

        if (GlobalAudioManager.Instance == null)
        {
            Debug.LogWarning("GlobalAudioManager not ready yet!");
            return;
        }
        
        GlobalAudioManager.Instance.CrossFadeMusic(sceneAudioSO.music);
    }    
    
    #endregion

    #region Ambient

    private void PlayAmbientLoop()
    {
        foreach(var ambient in sceneAudioSO.ambientSound)
        {
            PlayLoop(ambient);
        }
    }
    
    private void PlayLoop(SoundEntry entry)
    {
        AudioSource src = ObjectPoolManager.SpawnObject(
            sfxPrefab,
            transform,
            Quaternion.identity,
            PoolType.SoundFX
        );

        src.clip = entry.clip;
        src.volume = entry.volume;
        src.loop = true;
        src.Play();
    }
    
    #endregion

    #region SFX

    /// <summary>
    /// SFX that suitalbe with the enviroment
    /// </summary>
    /// <param name="id">Id of the audio clip</param>
    public void PlaySFX(string id)
    {
        if(!sfxDict.TryGetValue(id, out var entry))
        {
            Debug.LogWarning($"SFX ID not Found: {id}");
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
        src.Play();

        StartCoroutine(ReturnAfterDelay(src, entry.clip.length));
    }

    public void PlaySFXAt(SoundEntry entry, Vector3 position)
    {
        if(entry?.clip == null) return;

        AudioSource src = ObjectPoolManager.SpawnObject(
            sfxPrefab,
            position,
            Quaternion.identity,
            PoolType.SoundFX
        );

        src.transform.position = position;
        src.spatialBlend = 1f;
        src.clip = entry.clip;
        src.volume = entry.volume;
        src.pitch = Random.Range(entry.minPitch, entry.maxPitch);
        src.loop = false;
        src.Play();

        StartCoroutine(ReturnAfterDelay(src, entry.clip.length));
    }
    
    private IEnumerator ReturnAfterDelay(AudioSource src, float delay)
    {
        yield return new WaitForSeconds(delay);

        ObjectPoolManager.ReturnObjectToPool(src.gameObject, PoolType.SoundFX);
    }
    
    #endregion

    #region Ultils

    private IEnumerator WaitForGlobalAudio()
    {
        float timeout = 10f;
        float timer = 0;

        while (GlobalAudioManager.Instance == null && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (GlobalAudioManager.Instance == null)
            Debug.LogError("GlobalAudioManager never spawned!");
    }
    
    #endregion
}