using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    private InputSystem inputSystem;

    #region Execute
    protected override void Awake()
    {
        base.Awake();
        inputSystem = new InputSystem();
    }

    void OnEnable()
    {
        inputSystem.Enable();
    }

    void OnDisable()
    {
        inputSystem.Disable();
    }
    #endregion
    
    #region Input Method
    
    public void DisableGameInput() => inputSystem.Player.Disable();
    public void EnableGameInput() => inputSystem.Player.Enable();
    public void DisableMFiring() => inputSystem.Player.Fire.Disable();
    public void EnableFiring() => inputSystem.Player.Fire.Enable();
    
    public Vector2 GetPlayerMovement() => inputSystem.Player.Moving.ReadValue<Vector2>();

    public Vector2 GetMouseDelta() => inputSystem.Player.Look.ReadValue<Vector2>();

    public bool IsFiringPressed() => inputSystem.Player.Fire.WasPressedThisFrame();
    public bool IsFiringReleased() => inputSystem.Player.Fire.WasReleasedThisFrame();
    public bool IsFiringHeld() => inputSystem.Player.Fire.IsPressed();

    public bool IsInteractPressed() => inputSystem.Player.Interact.IsPressed();
    
    /// <summary>
    /// Player pressed tab button
    /// </summary>
    public bool IsSwitchPress() => inputSystem.Player.Interact.IsPressed();

    public bool IsPausedGame() => inputSystem.Player.Pause.IsPressed();
    public bool IsChatPressed() => inputSystem.Chat.Chat.IsPressed();
    public bool IsEnterPressed() => inputSystem.Chat.Submit.IsPressed();
    public bool IsClosedChat() => inputSystem.Chat.Cancel.IsPressed();
    public bool IsUseVoiceChat() => inputSystem.Chat.Voice.WasPressedThisFrame();
    
    #endregion
}
