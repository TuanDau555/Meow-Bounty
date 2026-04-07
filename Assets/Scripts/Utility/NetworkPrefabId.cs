using UnityEngine;
using Unity.Netcode;

public class NetworkPrefabId : MonoBehaviour
{
    [Tooltip("Reference to the original prefab this object belongs to.")]
    public GameObject prefabReference;

    // Bạn nên khóa lại ID để không bao giờ trùng
    [HideInInspector] 
    public string prefabId;

    private void Reset()
    {
        // Tự sinh ID khi add component vào prefab
        prefabId = System.Guid.NewGuid().ToString();
    }
}