using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAmount = new Vector3(0, 50, 0);

    private void Update()
    {
        transform.Rotate(rotationAmount * Time.deltaTime);
    }
}
