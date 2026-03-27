using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkHealth))]
[Tooltip("Control Player UI")]
public class PlayerInGameUIController : NetworkBehaviour 
{
    #region Parameters
    
    [Header("Reference")]
    [SerializeField] private PlayerInfoHUD playerInfoHUD;
    private NetworkHealth _networkHealth;

    #endregion

    #region Execute

    public override void OnNetworkSpawn()
    {
        if(!IsOwner) return;

        _networkHealth = GetComponent<NetworkHealth>();

        InitHealth();
        
    }
    
    #endregion

    #region Init
    
    private void InitHealth()
    {
        if(playerInfoHUD == null)
        {
            Debug.LogWarning("Player Info HUD is missing");
        }

        playerInfoHUD.SetMaxHealth(_networkHealth.GetMaxHealth());
        _networkHealth.OnHealthChanged += HandleHealthChanged;
        playerInfoHUD.SetHealth(_networkHealth.GetCurrentHealth());
    }

    #endregion

    #region Events
    
    private void HandleHealthChanged(object sender, float newHP)
    {
        playerInfoHUD.SetHealth(newHP);
    }

    #endregion

}