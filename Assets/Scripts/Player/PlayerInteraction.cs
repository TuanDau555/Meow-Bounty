using Unity.Netcode;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    #region Parameters

    [SerializeField] private PlayerStatsSO playerStatsSO;

    private IInteractable _currentInteractable;
    private InteractableUI _currentUI;
    private bool _isInteractable;

    private NetworkObject _networkObject;

    #endregion

    #region Execute

    private void Awake()
    {
        _networkObject = GetComponent<NetworkObject>();
    }
    
    private void Update()
    {
        RaycastCheck();
        HandleInput();    
    }

    #endregion

    #region Handle Interact

    private void RaycastCheck()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        bool hit = Physics.Raycast(
            ray, 
            out RaycastHit hitInfo, 
            playerStatsSO.interactStats.interactDistance, 
            playerStatsSO.interactStats.interactMask
        );

        if (hit)
        {
            var interactable = hitInfo.collider.GetComponent<IInteractable>();
            var ui = hitInfo.collider.GetComponent<InteractableUI>();

            if(interactable != _currentInteractable)
            {
                _currentUI?.HideUI();
                _currentInteractable = interactable;
                _currentUI = ui;

                _currentUI?.ShowUI(Camera.main.transform);
                
                Debug.Log($"You are able to interact with {_currentInteractable}");
            }
               
        }
        else
        {
            if(_currentInteractable != null)
            {
                _currentInteractable.StopInteract(_networkObject.OwnerClientId);
                _currentUI?.HideUI();
                _currentUI = null;
                _currentInteractable = null;
                _isInteractable = false;
            }
        }
    }

    private void HandleInput()
    {
        if(_currentInteractable == null) return;

        ulong ownerId = _networkObject.OwnerClientId;
        
        if (InputManager.Instance.IsInteractPressed())
        {
            if(!_isInteractable)
            {
                _isInteractable = true;
                _currentInteractable.StartInteract(ownerId);
                Debug.Log($"You are currently interacted with {_currentInteractable}");
            }
        }
        else
        {
            if (_isInteractable)
            {
                _isInteractable = false;               
                _currentInteractable.StopInteract(ownerId);
            }
        }
    }
    
    #endregion

}