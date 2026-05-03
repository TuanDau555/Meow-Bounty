using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Global Audio Data")]
public class GlobalAudioSO : ScriptableObject
{
    [Header("UI SFX")]
    public List<SoundEntry> uiSfxList;

    [Space(10)]
    [Header("Default Music")]
    [Tooltip("This is the music that is played when the scene changes.")]
    public AudioClip defaultMusic;

}