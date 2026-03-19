using System;
using Unity.Netcode;
using UnityEngine;

public class MissionManager : NetworkBehaviour
{
    #region Parameter

    [SerializeField] private MissionBase missionBase;

    [SerializeField] private GameObject saveRoomDoor;

    public bool AllMissionCompleted { get; private set; }

    #endregion

    #region Execute

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;

        missionBase.OnMissionCompleted += HandleMissionCompleted;
        missionBase.OnMissionFailed += HandleMissionFailed;
        missionBase.StartMission(); 
    }

    public override void OnNetworkDespawn()
    {
        if(missionBase != null)
        {
            missionBase.OnMissionCompleted -= HandleMissionCompleted;
            missionBase.OnMissionFailed -= HandleMissionFailed;
        }
        
        base.OnNetworkDespawn();
    }

    #endregion

    #region Events

    private void HandleMissionCompleted(object sender, MissionBase e)
    {
        Debug.Log("Mission Complete!");

        AllMissionCompleted = true;

        UnlockSaveRoom();
    }

    private void HandleMissionFailed(object sender, MissionBase e)
    {
        Debug.Log("Mission Failed");
        GameEndManager.Instance.EndGame(false);
    }

    #endregion

    private void UnlockSaveRoom()
    {
        Debug.Log("Save room unlocked");

        saveRoomDoor.SetActive(true);
    }
}