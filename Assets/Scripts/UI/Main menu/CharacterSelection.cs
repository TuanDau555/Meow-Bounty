using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{

    #region Parameter
    [Header("UI Component Ref")]
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button previousBtn;
    [SerializeField] private Button selectedBtn;

    [Tooltip("Select Button's sprite when selected")]
    [SerializeField] private Sprite selectedSprite;
    [Tooltip("Select Button's sprite when not selected")]
    [SerializeField] private Sprite notSelectedSprite;
    
    [SerializeField] private Transform previewRoot;

    [Header("Setting Animation")]
    [SerializeField] private float slideDistance = 4f;
    [SerializeField] private float slideDuration = 0.45f;
    [SerializeField] private float scaleStart = 0.6f;

    [Header("Characters")]
    [SerializeField] private CharacterDatabaseSO databaseSO;

    private List<CharacterDefinitionSO> ownedCharacters;
    private int currentIndex;
    private GameObject currentPreview;
    private PlayerProfileService playerProfileService;
    private bool isAnimating;
    private enum SlideDirection { None, Next, Previous }
    private Tween selectedTween; // Tweener when selected character


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
        
        ShowCharacter(SlideDirection.None);
        UpdateSelectedBtn();
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
        ShowCharacter(SlideDirection.None);
    }
    
    #endregion
    
    #region Selected

    private void ShowCharacter(SlideDirection slideDirection)
    {
        if(isAnimating) return;

        GameObject oldPreview = currentPreview;

        // if(currentPreview != null)
        // {
        //    Destroy(currentPreview); 
        // }

        var previewCharacter = ownedCharacters[currentIndex];
        if(previewCharacter.previewPrefab == null)
        {
            Debug.LogError($"Preview prefab missing for {previewCharacter.id}");
            return;
        }
        
        currentPreview = Instantiate(previewCharacter.previewPrefab, previewRoot);

        // Just want to make sure it don't have weird thing
        currentPreview.transform.localRotation = Quaternion.identity;

        PlayShowAnim(oldPreview, currentPreview.transform, slideDirection);
        UpdateSelectedBtn();
    }
    
    private void NextCharacter()
    {
        if (ownedCharacters == null || ownedCharacters.Count == 0) return;

        // Get next character
        currentIndex = (currentIndex + 1) % ownedCharacters.Count;
        ShowCharacter(SlideDirection.Next);
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

        ShowCharacter(SlideDirection.Previous);
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
        UpdateSelectedBtn();
    }

    private void UpdateSelectedBtn()
    {
        if(playerProfileService == null || ownedCharacters == null) return;

        string equippedCharacter = playerProfileService.PlayerData.equippedCharacter;
        string currentCharacter = ownedCharacters[currentIndex].id;

        bool isSelected = equippedCharacter == currentCharacter;

        selectedBtn.image.sprite = isSelected ? selectedSprite : notSelectedSprite;

        PlaySelectedPulse(isSelected);
    }
    
    private void RefreshOwnedCharacters()
    {
        ownedCharacters = databaseSO.characters
            .Where(c => playerProfileService.PlayerData.ownedCharacter.Contains(c.id))
            .ToList();

        currentIndex = 0;
        ShowCharacter(SlideDirection.None);
    }
    
    #endregion

    #region DOTWeen

    private void PlayShowAnim(GameObject oldPreview, Transform characterTranform, SlideDirection direction)
    {
        isAnimating = true;
        if(direction == SlideDirection.None)
        {
            characterTranform.localPosition = Vector3.zero;
            characterTranform.localScale = Vector3.one;
            isAnimating = false;

            if(oldPreview != null)
            {
                Destroy(oldPreview);
            }
            return;
        }
        
        // Character scale is small at intial
        characterTranform.localScale = Vector3.one * scaleStart;
        
        // Start position based on direction
        Vector3 startPos = Vector3.zero;
        Vector3 endPos = Vector3.zero;

        if(direction == SlideDirection.Next)
        {
            startPos = new Vector3(slideDistance, 0, 0); // from rigth
        }
        else
        {
            startPos = new Vector3(-slideDistance, 0, 0); // from left
        }

        characterTranform.localPosition = startPos;

        // Tween new model in
        Sequence sequence = DOTween.Sequence();

        sequence.Join(characterTranform.DOLocalMove(endPos, slideDuration).SetEase(Ease.OutCubic));
        sequence.Join(characterTranform.DOScale(1f, slideDuration).SetEase(Ease.OutBack));

        // Tween old model out
        if(oldPreview != null)
        {
            Transform oldPreviewTransform = oldPreview.transform;
            
            Vector3 exitPos = Vector3.zero;
            if(direction == SlideDirection.Next)
            {
                exitPos = new Vector3(-slideDistance, 0, 0); // exit left
            }
            else
            {
                exitPos = new Vector3(slideDistance, 0, 0); // exit right
            }

            sequence.Join(oldPreviewTransform.DOLocalMove(exitPos, slideDuration).SetEase(Ease.InCubic));
            sequence.Join(oldPreviewTransform.DOScale(scaleStart, slideDuration * 0.8f));

            sequence.OnComplete(() =>
            {
                Destroy(oldPreview);
                isAnimating = false;
            });
        }
        else
        {
            sequence.OnComplete(() => isAnimating = false);
        }
    }

    private void PlaySelectedPulse(bool isSelected)
    {
        selectedTween?.Kill();

        if (!isSelected)
        {
            selectedBtn.image.transform.localScale = Vector3.one;
            return;
        }

        selectedTween = selectedBtn.image.transform
            .DOScale(1.1f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
    
    #endregion
}
