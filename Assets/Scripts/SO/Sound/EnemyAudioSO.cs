using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Enemy Audio")]
public class EnemyAudioSO : ScriptableObject 
{
    [Header("Network")]
    public SoundEntry attack;
    public SoundEntry death;
}