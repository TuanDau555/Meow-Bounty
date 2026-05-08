using UnityEngine;
using Unity.Netcode;

public class PlayerAudioController : NetworkBehaviour 
{
    #region Parameters

    [SerializeField] private PlayerAudioSO playerAudioSO;

    [Tooltip("Source for local")]
    [SerializeField] private AudioSource localAudioSource;

    [Tooltip("Source for remote")]
    [SerializeField] private AudioSource worldAudioSource;
    
    #endregion

    #region Local Only

    public void PlayerDamgeTaken()
    {
        if(!IsOwner) return;

        PlayLocal(playerAudioSO.damageTaken);
    }

    public void PlayHitmarker()
    {
        if(!IsOwner) return;

        PlayLocal(playerAudioSO.hitmarker);
    }
    
    #endregion

    #region Networked

    public void PlayFire()
    {
        if(IsOwner)
        {
            PlayLocal(playerAudioSO.fire);
        }

        if (IsServer)
        {
            PlayFireClientRpc();
        }
        else
        {
            PlayFireServerRpc();
        }
    }

    public void PlayDowned()
    {
        if (IsServer)
            PlayDownedClientRpc();
        else
            PlayDownedServerRpc();
    }
    
    #endregion

    #region RPC

    [ServerRpc(RequireOwnership = false)]
    private void PlayFireServerRpc() => PlayFireClientRpc();

    [ClientRpc]
    private void PlayFireClientRpc()
    {
        if(IsOwner) return; // local player already play the sound
        PlayWorld(playerAudioSO.fire);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayDownedServerRpc() => PlayDownedClientRpc();

    [ClientRpc]
    private void PlayDownedClientRpc()
    {
        PlayWorld(playerAudioSO.downed);
    }
    
    #endregion

    #region Local Helper

    private void PlayLocal(SoundEntry entry)
    {
        if(entry?.clip == null) return;

        localAudioSource.pitch = Random.Range(entry.minPitch, entry.maxPitch);
        localAudioSource.PlayOneShot(entry.clip, entry.volume);
    }

    private void PlayWorld(SoundEntry entry)
    {
        if(entry?.clip == null) return;

        SceneSoundManager.Instance.PlaySFXAt(entry, transform.position);
    }
    
    #endregion
}