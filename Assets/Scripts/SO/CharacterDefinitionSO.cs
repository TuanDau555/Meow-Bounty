using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Definition")]
public class CharacterDefinitionSO : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public Transform characterPrefab;
    public CharacterStats characterStats;

    [System.Serializable]
    public class CharacterStats
    {
        [Header("HP/Defend")]
        [Range(1, 10)]
        public float HP;

        [Range(1, 10)]
        public float defend;

        [Space(10)]
        [Header("Stamina")]

        [Range(1, 10)]
        public float stamina;

        [Tooltip("This variable just for testing")]
        public bool useStamina;

        [Tooltip("Use stamina value")]
        [Range(1, 10)]
        public float staminaUseMultiplier = 5f;
        
        [Tooltip("Stamina cool down")]
        [Range(1, 5)]
        public float timeBeforeStaminaRegenStarts = 3f;
        
        [Tooltip("Stamina cool down value")]
        [Range(1, 5)]
        public float staminaIncrement = 2f;
        
        [Tooltip("Time between stamina increment")]
        [Range(0.01f, 1f)]
        public float staminaTimeIncrement = 0.1f;

        [Space(10)]
        [Header("Move")]
        public float walkSpeed;
        public float sprintSpeed;
    }
}
