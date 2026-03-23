using Unity.Netcode;
using UnityEngine;

public class ReviveZone : NetworkBehaviour
{
    private NetworkHealth _networkHealth;
    private SphereCollider sCollider;

    private void Awake()
    {
        _networkHealth = GetComponentInParent<NetworkHealth>();
        sCollider = GetComponent<SphereCollider>();
        sCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!IsServer) return;

        
        var interaction = other.GetComponent<ReviveInteractor>();
        if(interaction == null)
        {
            Debug.Log($"There is no ReviveInteractor in {other.gameObject.name}");
            return;
        }

        interaction.SetReviveTargetClientRpc(_networkHealth.NetworkObjectId);
    }

    private void OnTriggerExit(Collider other)
    {
        if(!IsServer) return;
        
        var interaction = other.GetComponent<ReviveInteractor>();
        if(interaction == null)
        {
            Debug.Log($"There is no ReviveInteractor in {other.gameObject.name}");
            return;
        }

        interaction.ClearReviveTargetClientRpc();
    }

    public void EnableZone()
    {
        if(!IsServer) return;

        sCollider.enabled = true;
    }

    public void DisableZone()
    {
        if(!IsServer) return;

        sCollider.enabled = false;
    }
}
