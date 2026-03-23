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
            weaponContext.Fire(point.position, cameraTransform.forward);
        }
    }
}
