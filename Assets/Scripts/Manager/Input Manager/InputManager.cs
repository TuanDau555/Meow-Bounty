using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    private InputSystem inputSystem;

    #region Excute
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
    
    public Vector2 GetPlayerMovement() => inputSystem.Player.Moving.ReadValue<Vector2>();

    public Vector2 GetMouseDelta() => inputSystem.Player.Look.ReadValue<Vector2>();

    public bool IsFiringPressed() => inputSystem.Player.Fire.WasPressedThisFrame();

    public bool IsInteractPressed() => inputSystem.Player.Interact.IsPressed();

    /// <summary>
    /// Player pressed tab button
    /// </summary>
    public bool IsSwitchPress() => inputSystem.Player.Interact.IsPressed();
    
    #endregion
}
