using Unity.Netcode;
using UnityEngine;

public class GameLevelManager : NetworkBehaviour
{
    public static GameLevelManager Instance;
    public NetworkVariable<int> zombieCount = new NetworkVariable<int>();
    public GameObject exitPortal;

    void Awake() => Instance = this;

    public override void OnNetworkSpawn()
    {
        if (IsServer) zombieCount.Value = GameObject.FindGameObjectsWithTag("Zombie").Length;
    }

    public void ZombieDied()
    {
        zombieCount.Value--;
        if (zombieCount.Value <= 0) exitPortal.SetActive(true);
    }
}