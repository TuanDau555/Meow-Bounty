using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayConnectionManager : SingletonPersistent<RelayConnectionManager>
{
    private UnityTransport _transport;
    private string _currentJointCode;

    protected override void Awake()
    {
        base.Awake();
        _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    #region HOST

    /// <summary>
    /// Host creates relay allocation
    /// </summary>
    /// <returns>Join code</returns>
    /// <remarks> Do not start Host in this func </remark>
    public async Task<string> SetUpAsHostRelayAsync(int maxPlayers)
    {
        try
        {
            // Doesn't count Host as player in room so I minus 1
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);

            _currentJointCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            // Protocol and server set up
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            _transport.SetRelayServerData(relayServerData);
            
            return  _currentJointCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Host Set up failed {e}");
            return null;
        }
    }

    public void StartHost()
    {
        Debug.Log("[Relay] Starting Host");

        NetworkManager.Singleton.StartHost();
    }

    #endregion

    #region CLIENT
    
    /// <summary>
    /// Client join Relay using join code
    /// </summary>
    /// <param name="joinCode">Get from Lobby</param>
    /// <returns>Join success or failed</returns>
    /// <remarks> Do not start Client in this func </remark>
    public async Task<bool> SetUpAsClientAsync(string joinCode)
    {
        try
        {
            _currentJointCode = joinCode;

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            _transport.SetRelayServerData(relayServerData);
            
            Debug.Log("[Relay] Client joined allocation");
            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"[Relay] Client join failed: {e}");
            return false;
        }
    }

    public void StartClient()
    {
        Debug.Log("[Relay] Starting Client");
        NetworkManager.Singleton.StartClient();        
    }
    
    #endregion
}
