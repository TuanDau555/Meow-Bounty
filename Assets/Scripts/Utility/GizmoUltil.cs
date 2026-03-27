using UnityEngine;

public class GizmoUltil : MonoBehaviour 
{
    [SerializeField] private float radius = 30f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}