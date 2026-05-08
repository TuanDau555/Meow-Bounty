using Unity.Netcode;
using UnityEngine;

public class EnemyAudioController : NetworkBehaviour
{
    [SerializeField] private EnemyAudioSO audioSO;

    // 
    public void PlayAttack()
    {
        if (!IsServer) return;
        PlaySoundClientRpc(SoundType.Attack);
    }

    public void PlayDeath()
    {
        if (!IsServer) return;
        PlaySoundClientRpc(SoundType.Death);
    }

    [ClientRpc]
    private void PlaySoundClientRpc(SoundType type)
    {
        SoundEntry entry = type switch
        {
            SoundType.Attack => audioSO.attack,
            SoundType.Death  => audioSO.death,
            _                => null
        };

        SceneSoundManager.Instance.PlaySFXAt(entry, transform.position);
    }

    private enum SoundType { Attack, Death }
}