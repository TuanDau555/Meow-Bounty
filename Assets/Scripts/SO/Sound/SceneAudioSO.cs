using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Audio/Scene Audio")]
public class SceneAudioSO : ScriptableObject
{
    [Header("Music")]
    public AudioClip music;

    [Space(10)]
    [Header("Ambient Sounds")]
    public List<SoundEntry> ambientSound;

    [Space(10)]
    [Header("SFX")]
    public List<SoundEntry> sfxList;
}

[System.Serializable]
public class SoundEntry
{
    [Tooltip("Id of the audio clip")]
    public string id;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;
    public bool loop = false;

    [Space(10)]
    [Header("Pitch Variation")]
    
    [Range(0.1f, 1)]
    public float minPitch = 0.2f;

    [Range(0.5f, 1)]
    public float maxPitch = 1f;
}