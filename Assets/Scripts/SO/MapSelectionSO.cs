using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Map/MapSelectionSO")]
public class MapSelectionSO : ScriptableObject 
{
    public List<MapInfo> maps;
}

[System.Serializable]
public struct MapInfo
{
    [Tooltip("Map preview that will display to player")]
    public Sprite mapThumbnail;

    [Tooltip("Map locked preview that will display to player")]
    public Sprite mapThumbnailLocked;    

    [Tooltip("Map name that will display to player")]
    public string mapName;

    [Tooltip("Scene that will load when start game")]
    public string sceneName;
}