using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{

    #region Parameter
    [Header("UI Component Ref")]
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button previousBtn;
    [SerializeField] private Button selectedBtn;
    [SerializeField] private Transform previewRoot;

    [Header("Characters")]
    [SerializeField] private CharacterDatabaseSO databaseSO;

    private List<CharacterDefinitionSO> ownedCharacters;
    private int currentIndex;
    private GameObject currentPreview;
    private PlayerProfileService playerProfileService;


    #endregion

    #region Execute

    private void Start()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }

        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OnCharacterPurchase += HandleCharacterPurchase;
        }
        else
        {
            Debug.LogError("ShopManager not ready!");
        }
        
    }

    private void OnEnable()
    {
        nextBtn.onClick.AddListener(NextCharacter);
        previousBtn.onClick.AddListener(PreviousCharacter);
        selectedBtn.onClick.AddListener(ConfirmSelected);
    }

    private void OnDisable()
    {
        nextBtn.onClick.RemoveListener(NextCharacter);
        previousBtn.onClick.RemoveListener(PreviousCharacter);       
        selectedBtn.onClick.RemoveListener(ConfirmSelected);

    }

    private void OnDestroy()
    {
        ShopManager.Instance.OnCharacterPurchase -= HandleCharacterPurchase;
    }

    #endregion

    #region Init

    public void Init(PlayerProfileService profileService)
    {
        playerProfileService = profileService;

        RefreshOwnedCharacters();

        foreach (var c in databaseSO.characters)
        {
            Debug.Log($"DB character id: '{c.id}'");
        }

        foreach (var id in profileService.PlayerData.ownedCharacter)
        {
            Debug.Log($"Owned character id: '{id}'");
        }

        if (profileService == null || profileService.PlayerData == null)
        {
            Debug.LogError("CharacterSelection.Init called before PlayerProfile ready");
            return;
        }
        
        // Find ownedCharacters in database
        ownedCharacters = databaseSO.characters
                            .ToList()
                            .FindAll(
                                c => 
                                profileService
                                    .PlayerData
                                    .ownedCharacter.Contains(c.id)
                            );
        
        if (ownedCharacters == null || ownedCharacters.Count == 0)
        {
            Debug.LogError(
                "Player owns NO valid characters. Check PlayFab data or CharacterDatabaseSO IDs."
            );

            nextBtn.interactable = false;
            previousBtn.interactable = false;
            selectedBtn.interactable = false;
            return;
        }
        
        // euipped owened character
        currentIndex = ownedCharacters
                        .FindIndex(
                            c => 
                            c.id == profileService.PlayerData.equippedCharacter
                        );

        // Equipped character not owned anymore, so we fallback to first
        if(currentIndex < 0) currentIndex = 0;
        
        ShowCharacter();
    }
    
    #endregion

    #region Events
    
    private void HandleCharacterPurchase(string characterId)
    {
        if (playerProfileService == null)
        {
            Debug.LogWarning("CharacterSelection: playerProfileService is null, skip refresh");
            return;
        }

        var newCharacter = databaseSO.characters.FirstOrDefault(c => c.id == characterId);

        if(newCharacter == null) return;
        
        if(!ownedCharacters.Contains(newCharacter))
        {
            ownedCharacters.Add(newCharacter);
        }
        currentIndex = ownedCharacters.Count - 1;
        ShowCharacter();
    }
    
    #endregion
    
    #region Selected

    private void ShowCharacter()
    {
        if(currentPreview != null)
        {
           Destroy(currentPreview); 
        }

        var previewCharacter = ownedCharacters[currentIndex];
        if(previewCharacter.previewPrefab == null)
        {
            Debug.LogError($"Preview prefab missing for {previewCharacter.id}");
            return;
        }
        
        currentPreview = Instantiate(previewCharacter.previewPrefab, previewRoot);

        // Just want to make sure it don't have weird thing
        currentPreview.transform.localPosition = Vector3.zero;
        currentPreview.transform.localRotation = Quaternion.identity;
    }
    
    private void NextCharacter()
    {
        if (ownedCharacters == null || ownedCharacters.Count == 0) return;

        // Get next character
        currentIndex = (currentIndex + 1) % ownedCharacters.Count;
        ShowCharacter();
    }

    private void PreviousCharacter()
    {
        if (ownedCharacters == null || ownedCharacters.Count == 0) return;
        
        // reduce the index number
        currentIndex--;

        // return to the first character
        if(currentIndex < 0)
        {
            currentIndex = ownedCharacters.Count - 1;
        }

        ShowCharacter();
    }

    private void ConfirmSelected()
    {
        // This case may never happened, but must careful
        if(ownedCharacters == null || ownedCharacters.Count == 0)
        {
            selectedBtn.interactable = false;
            return;    
        }

        // get character id
        var selectedId = ownedCharacters[currentIndex].id;


        playerProfileService.SetEquippedCharacter(selectedId);
    }

    private void RefreshOwnedCharacters()
    {
        ownedCharacters = databaseSO.characters
            .Where(c => playerProfileService.PlayerData.ownedCharacter.Contains(c.id))
            .ToList();

        currentIndex = 0;
        ShowCharacter();
    }
    
    #endregion
}
