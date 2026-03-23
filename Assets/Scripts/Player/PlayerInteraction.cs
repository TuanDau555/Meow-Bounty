using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    #region Parameters

    [SerializeField] private PlayerStatsSO playerStatsSO;

    private IInteractable currentInteractable;

    #endregion

    #region Execute

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

        bool isInteractable = Physics.Raycast(ray, out RaycastHit hit, playerStatsSO.interactStats.interactDistance, playerStatsSO.interactStats.interactMask);

        if (isInteractable)
        {
            currentInteractable = hit.collider.GetComponent<IInteractable>();
            Debug.Log($"You are able to interact with {currentInteractable}");
        }
        else
        {
            currentInteractable = null;
        }
    }

    private void HandleInput()
    {
        if(currentInteractable == null) return;

        if (InputManager.Instance.IsInteractPressed())
        {
            currentInteractable.StartInteract(0);
            Debug.Log($"You are currently interacted with {currentInteractable}");
        }
        else
        {
            currentInteractable.StopInteract(0);
        }
    }
    
    #endregion

}