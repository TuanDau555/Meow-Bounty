using System.Collections.Generic;
using UnityEngine;

public class SpectatorManager : Singleton<SpectatorManager>
{
    private List<Transform> alivePlayers = new List<Transform>();

    // Current Spectator Player
    private int _currentIndex = 0;

    public void RegisterPlayer(Transform t)
    {
        if(!alivePlayers.Contains(t))
            alivePlayers.Add(t);
    }

    public void UnRegisterPlayer(Transform t)
    {
        int removedIndex = alivePlayers.IndexOf(t);
        alivePlayers.Remove(t);

        // Clamp index after remove
        if (alivePlayers.Count == 0)
        {
            _currentIndex = 0;
            return;
        }

        if (removedIndex <= _currentIndex)
        {
            _currentIndex = Mathf.Max(0, _currentIndex - 1);
        }
    }

    /// <summary>
    /// Get next player spectator pos
    /// </summary>
    /// <returns>Next alive player</returns>
    public Transform GetNextTarget()
    {
        if(alivePlayers.Count == 0) return null;

        _currentIndex = (_currentIndex + 1) % alivePlayers.Count;

        return alivePlayers[_currentIndex];
    }

    /// <summary>
    /// Get current player spectator pos
    /// </summary>
    /// <returns>Current alive player</returns>
    public Transform GetCurrentTarget()
    {
        if(alivePlayers.Count == 0) return null;

        return alivePlayers[_currentIndex];
    }
}