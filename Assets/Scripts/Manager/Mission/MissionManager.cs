using System;
using Unity.Netcode;
using UnityEngine;

public class MissionManager : NetworkBehaviour
{
    #region Parameter

    [Header("Reference")]
    [SerializeField] private MissionBase missionBase;
    [SerializeField] private MissionUI missionUI;

    [SerializeField] private GameObject saveRoomDoor;

    // Net Variables
    public NetworkVariable<int> CurrentObjectiveIndex = new NetworkVariable<int>();
    public NetworkVariable<bool> IsMissionCompleted = new NetworkVariable<bool>();

    public bool AllMissionCompleted { get; private set; }

    #endregion

    #region Execute

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            missionBase.OnMissionCompleted += HandleMissionCompleted;
            missionBase.OnMissionFailed += HandleMissionFailed;
            missionBase.OnProgressChanged += HandleProgressChanged;
            
            missionBase.StartMission();
        }
        
        missionUI.Init(this);

    }

    public override void OnNetworkDespawn()
    {
        if(missionBase != null)
        {
            missionBase.OnMissionCompleted -= HandleMissionCompleted;
            missionBase.OnMissionFailed -= HandleMissionFailed;
            missionBase.OnProgressChanged -= HandleProgressChanged;
        }
        
        base.OnNetworkDespawn();
    }

    #endregion

    #region Events

    private void HandleMissionCompleted(object sender, MissionBase e)
    {
        Debug.Log("Mission Complete!");

        var reward = CalculateReward(true);

        GrantRewardClientRpc(reward.softCurrency, reward.exp);
        
        AllMissionCompleted = true;
        IsMissionCompleted.Value = true;

        UnlockSaveRoom();
    }

    
    private void HandleProgressChanged(object sender, MissionBase e)
    {
        if(!NetworkManager.Singleton.IsServer) return;
        
        CurrentObjectiveIndex.Value = e.CurrentProgress;
    }

    private void HandleMissionFailed(object sender, MissionBase e)
    {
        Debug.Log("Mission Failed");
        GameEndManager.Instance.EndGame(false);
    }

    #endregion
    #region After Match
        
    private void UnlockSaveRoom()
    {
        Debug.Log("Save room unlocked");

        saveRoomDoor.SetActive(true);
    }

    private RewardData CalculateReward(bool isWin)
    {
        RewardData rewardData = new RewardData();

        if (isWin)
        {
            rewardData.softCurrency = 100;
            rewardData.exp = 50;
        }
        else
        {
            rewardData.softCurrency = 20;
            rewardData.exp = 10;
        }
        
        return rewardData;
    }
    
    #endregion

    #region RPC

    [ClientRpc]
    private void GrantRewardClientRpc(int coins, int exp)
    {
        Debug.Log($"Receive reward: {coins}");

        RewardManager.Instance.ReceiveReward(coins, exp);
    }
    
    #endregion
}