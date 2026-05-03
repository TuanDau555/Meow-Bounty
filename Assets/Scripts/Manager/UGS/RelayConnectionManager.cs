using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayConnectionManager : SingletonPersistent<RelayConnectionManager>
{
    private UnityTransport _transport;
    private string _currentJointCode;
    public Dictionary<ulong, string> _clientUgsMap { get; private set; }= new Dictionary<ulong, string>();

    protected override void Awake()
    {
        base.Awake();
        _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
    }

    private void OnDestroy()
    {
        if(NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
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

            string connectionType = GetConnectionType();
            
            _currentJointCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
    #if UNITY_WEBGL && !UNITY_EDITOR
            _transport.UseWebSockets = true;
    #endif

            // Protocol and server set up
            RelayServerData relayServerData = new RelayServerData(allocation, connectionType);
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

        var payload = System.Text.Encoding.UTF8.GetBytes(AuthenticationService.Instance.PlayerId);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payload;
        
        NetworkManager.Singleton.StartHost();
    }

    /// <summary>
    /// Basic Netcode will decline remote player to connect at default so we give them the approval
    /// </summary>
    /// <param name="request">Connected to Relay</param>
    /// <param name="response">Give remote player permission to join</param>
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string ugsId = System.Text.Encoding.UTF8.GetString(request.Payload);

        _clientUgsMap[request.ClientNetworkId] = ugsId;

        response.Approved = true;
        response.CreatePlayerObject = false; // we manually do it
        response.Pending = false;

        Debug.Log($"[Approval] client {request.ClientNetworkId} = {ugsId}");
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
            string connectionType = GetConnectionType();
            _currentJointCode = joinCode;

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

    #if UNITY_WEBGL && !UNITY_EDITOR
            _transport.UseWebSockets = true;
    #endif

            RelayServerData relayServerData = new RelayServerData(joinAllocation, connectionType);
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

        var payload = System.Text.Encoding.UTF8.GetBytes(AuthenticationService.Instance.PlayerId);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payload;
        
        NetworkManager.Singleton.StartClient();        
    }
    
    #endregion

    #region Connection Type

    private string GetConnectionType()
    {
    #if UNITY_WEBGL && !UNITY_EDITOR
        return "wss";
    #else
        return "dtls";
    #endif
    }
    
    #endregion
}
