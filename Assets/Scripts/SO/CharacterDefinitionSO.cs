using UnityEngine;

[CreateAssetMenu(menuName = "Character/Definition")]
public class CharacterDefinitionSO : ScriptableObject
{   
    [Header("ID")]
    public string id;
    
    
    [Space(10)]
    [Header("General")]
    public string displayName;
    public Sprite icon;

    [TextArea(3, 5)]
    public string description;

    [Space(10)]
    [Header("Model")]
    public Transform characterPrefab;

    [Tooltip("This is just empty prefab, just visual and animation no logic")]
    public GameObject previewPrefab;
    public CharacterStats characterStats;

    [System.Serializable]
    public class CharacterStats
    {
        [Header("Basic stats")]
        [Range(1, 10)]
        public float HP;

        [Range(1, 10)]
        public float defend;
        [Range(1, 100)]
        public float damage;

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
