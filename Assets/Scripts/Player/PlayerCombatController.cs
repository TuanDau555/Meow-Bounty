using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    [SerializeField] private WeaponContext weaponContext;
    [SerializeField] private Transform firePoint;
    private Transform cameraTransform;

    private InputManager _inputManager;

    private void Awake()
    {
        _inputManager = InputManager.Instance;
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        HandleFire(firePoint);
    }

    private void HandleFire(Transform point)
    {
        if(!weaponContext.IsOwner) return;

        if (_inputManager.IsFiringPressed())
        {
            weaponContext.StartFire(point.position, cameraTransform.forward);
        }

        if (_inputManager.IsFiringHeld())
        {
            weaponContext.HoldFire(point.position, cameraTransform.forward);
        }

        if (_inputManager.IsFiringReleased())
        {
            weaponContext.StopFire();
        }
    }
}
