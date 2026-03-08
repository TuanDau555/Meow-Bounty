using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cam == null) cam = Camera.main.transform;
        if (cam != null)
        {
            // Luôn xoay mặt về phía camera
            transform.LookAt(transform.position + cam.forward);
        }
    }
}