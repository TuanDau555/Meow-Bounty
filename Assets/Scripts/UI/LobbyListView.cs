using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListView : MonoBehaviour
{
    #region Constant
    private const int k_maxPlayer = 4;
    #endregion
    
    #region Variables
    [Header("Lobby List Panel")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject roomInfoPrefabs;
    [SerializeField] private GameObject roomInfoContent;
    [SerializeField] private Button refreshBtn;

    [Space(10)]
    [Header("Create Room Panel")]
    [SerializeField] private TMP_InputField roomNameIF;
    [SerializeField] private TMP_InputField roomPasswordIF;
    [SerializeField] private Button createRoomBtn;

    [Space(10)]
    [Header("Room Panel")]
    [SerializeField] private GameObject roomPanel;
    [SerializeField] private GameObject playerInfoPrefabs;
    [SerializeField] private GameObject playerInfoContent;
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private Button leaveRoomBtn;
    
    [Tooltip("Only visualize for the host")]
    [SerializeField] private Button startGameBtn;

    [Tooltip("Don't show this button for the host")]
    [SerializeField] private Button readyBtn;

    private IGameLobbyService _gameLobbyService;
    private IHostAuthority _hostAuthority;
    private bool _isBound;
    #endregion

    #region Execute
    private void Update()
    {
        if (_isBound) return;

        if (!ServiceLocator.HasLobbyService) return;

        BindLobbyService();
    }

    private void OnEnable()
    {        
        refreshBtn.onClick.AddListener(OnRefreshClicked);
        createRoomBtn.onClick.AddListener(OnCreateClicked);
        leaveRoomBtn.onClick.AddListener(OnLeaveRoomClicked);
        readyBtn.onClick.AddListener(OnReadyClicked);
    }

    private void OnDisable()
    {
        refreshBtn.onClick.RemoveListener(OnRefreshClicked);
        createRoomBtn.onClick.RemoveListener(OnCreateClicked);
        leaveRoomBtn.onClick.RemoveListener(OnLeaveRoomClicked);
        readyBtn.onClick.RemoveListener(OnReadyClicked);

        if (_isBound)
        {
            _gameLobbyService.OnLocalLobbyUpdated -= OnLocalLobbyUpdated;
            _gameLobbyService.OnLobbyUpdated -= OnRefreshPlayerList;
            _gameLobbyService.OnLobbyLeft -= OnLobbyLeft;
            _isBound = false;
        }
    }
    #endregion

    #region Lobby Handle
    private void CreateRoomItem(LobbyInfoData data)
    {

        // The room that appear in the Scroll content
        GameObject item = Instantiate(roomInfoPrefabs, roomInfoContent.transform);

        var roomDetailText = item.GetComponentsInChildren<TextMeshProUGUI>();
        roomDetailText[0].text = data.lobbyName;
        roomDetailText[1].text = $"{data.currentPlayers} / {data.maxPlayer}";

         string lobbyCode = data.lobbyId;

        item.GetComponentInChildren<Button>()
            .onClick
            .AddListener(async () =>
            {
                if (string.IsNullOrEmpty(lobbyCode))
                {
                    Debug.LogError("LobbyCode is null or empty");
                    return;
                }

                await _gameLobbyService.JoinLobbyByIdAsync(lobbyCode);
            });
            Debug.Log($"Render Lobby: {data.lobbyName} | Code: {data.lobbyId}");

    }
    
    private async Task RefreshLobbyList()
    {
        ClearLobbyList();

        var rooms = await _gameLobbyService.QueryRoomsAsync();
        foreach(var room in rooms)
        {
            CreateRoomItem(room);
        }
    }
    private async Task LeaveRoom()
    {
        await _gameLobbyService.LeaveLobbyAsync();
    }
    #endregion
    
    #region UI Updated
    private void ShowRoom(LobbyData lobby)
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);

        roomNameText.text = lobby.lobbyName;
        roomCodeText.text = lobby.lobbyCode;

        RenderPlayerList(lobby);
    }

    private bool AreAllMembersReady(LobbyData lobby)
    {
        if(lobby == null) return false;

        foreach(var player in lobby.Players)
        {
            if(player.isHost) continue;

            if(!player.isReady) return false;
        }
        
        return true;
    }

    private void RenderPlayerList(LobbyData lobby)
    {
        // Clear all old player data
        foreach(Transform child in playerInfoContent.transform)
        {
            Destroy(child.gameObject);
        }

        // Visualize the player info
        foreach(var player in lobby.Players)
        {
            // Create a new player info UI element for each player...
            GameObject item = Instantiate(playerInfoPrefabs, playerInfoContent.transform);

            // ...and set its parent to the player info content
            // Because the player info prefab is a child of the player info content, it will be displayed in the player info content
            var playerInfoDetail = item.GetComponent<PlayerInfoUI>();
            playerInfoDetail.SetUp(
                player,
                _gameLobbyService,
                _hostAuthority
            );
        }
    }

    private void OnRefreshPlayerList(object sender, Lobby lobby)
    {
        RenderPlayerList(_gameLobbyService.MapToLobbyData(lobby));
    }
    #endregion

    #region On Clicked
    private async void OnCreateClicked()
    {
        try
        {
            
            if(string.IsNullOrEmpty(roomNameIF.text)) return;

            await _gameLobbyService.CreateLobbyAsync(
                roomNameIF.text,
                k_maxPlayer,
                options: null
            );
            Debug.Log("Create Room Click");
        }
        catch(LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async void OnRefreshClicked()
    {
        await RefreshLobbyList();
    }

    private async void OnLeaveRoomClicked()
    {
        await LeaveRoom();
    }

    private async void OnReadyClicked()
    {
        var lobby = ServiceLocator.GameLobbyService;
        var myId = AuthenticationService.Instance.PlayerId; // Id of the player that is clicked ready

        bool isReady = lobby.CurrentLobby.GetPLayer(myId).isReady;

        Debug.Log($"Player {myId} is ready");

        await lobby.SetPlayerReadyAsync(myId, !isReady);

    }

    //TODO: JOIN ROOM BY CODE BUTTOn
    // private async Task OnJoinByCodeClicked()
    // {
    //     await _gameLobbyService.JoinLobbyByCodeAsync(joinCodeIF.text);
    // }
    #endregion
    
    #region Lobby Updated
    private void OnLocalLobbyUpdated(object sender, LobbyData lobby)
    {
        ClearLobbyList();
        ShowRoom(lobby);

        UpdateStartGameButton(lobby);
    }

    private void OnLobbyLeft(object sender, EventArgs e)
    {
        Debug.Log("Lobby panel show");
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    /// <summary>
    /// Show start Btn to host and ready btn to memeber
    /// </summary>
    /// <param name="lobby">Current Lobby</param>
    private void UpdateStartGameButton(LobbyData lobby)
    {
        bool isLocalReady = AreAllMembersReady(lobby);
        
        // if not host
        if (!_gameLobbyService.HostAuthority.IsHost)
        {
            readyBtn.gameObject.SetActive(true);
            startGameBtn.gameObject.SetActive(false);
            leaveRoomBtn.interactable = !isLocalReady;
            return;
        }
        

        startGameBtn.gameObject.SetActive(true);

        startGameBtn.interactable = isLocalReady;
    }
    #endregion

    #region Utils
    private void ClearLobbyList()
    {
        foreach(Transform child in roomInfoContent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void BindLobbyService()
    {
        _gameLobbyService = ServiceLocator.GameLobbyService;
        _hostAuthority = _gameLobbyService.HostAuthority;

        _gameLobbyService.OnLocalLobbyUpdated += OnLocalLobbyUpdated;
        _gameLobbyService.OnLobbyLeft += OnLobbyLeft;
        _gameLobbyService.OnLobbyUpdated += OnRefreshPlayerList;

        _isBound = true;

        Debug.Log("LobbyService bind SUCCESS");
    }


    #endregion
}
