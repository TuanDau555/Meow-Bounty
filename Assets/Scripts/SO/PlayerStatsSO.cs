using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Player")]
public class PlayerStatsSO : ScriptableObject
{
    public LookStats lookStats;
    public HeadBobStats headBobStats;
    
    #region Look
    [Serializable]
    public class LookStats
    {
        [Header("Look Sensitive")]

        [Tooltip("Mouse Speed")]
        [Range(0.0001f, 10)]
        public float lookSensitive;

        [Tooltip("Is the limit that player can look up and down")]
        [Range(45, 90)]
        public float lookLimit;

        [Tooltip("Is the limit that player can look up and down when player get knocked down")]
        [Range(45, 60)]
        public float lookLimitWhenDowned = 45f;
    }

    #endregion

    #region Head Bob

    [Serializable]
    public class HeadBobStats
    {
        [Range(1, 100)]
        public float walkBobSpeed = 14f;

        [Range(0, 30)]
        public float walkBobAmount = 0.5f;
        
        [Range(1, 100)]
        public float sprintBobSpeed = 18f;
    }
    
    #endregion
}