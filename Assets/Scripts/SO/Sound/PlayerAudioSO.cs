using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Player Audio")]
public class PlayerAudioSO : ScriptableObject 
{
    [Header("Local only")]
    public SoundEntry damageTaken;
    public SoundEntry hitmarker;

    [Header("Network")]
    public SoundEntry fire;
    public SoundEntry downed;
}