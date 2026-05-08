using UnityEditor;
using UnityEngine;

public class CombinedEditor
{
    [MenuItem("Tools/Combine Selected Mesh")]
    private static void CombineSelectedMesh()
    {
        // Only combine selected object
        GameObject[] selectedObject = Selection.gameObjects;
        
        // If you not select anything
        if(selectedObject.Length == 0) 
        {
            Debug.LogError("Nothing to combine. You need to the parrent of object to combine");
            return;
        }

        // Get the mesh
        MeshFilter[] meshFilters = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        // Combine all the mesh that get 
        for(int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;

        }

        // New Mesh after collected
        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; 
        combinedMesh.CombineMeshes(combine); // Combined it

        // Create new asset of that mesh, so we can use it
        GameObject combinedObject = new GameObject("Combined Mesh");

        // Attach the mesh fillter
        MeshFilter meshFilter = combinedObject.AddComponent<MeshFilter>();
        meshFilter.mesh = combinedMesh;

        // attach the material
        MeshRenderer meshRenderer = combinedObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = meshFilters[0].GetComponent<MeshRenderer>().sharedMaterial;

        AssetDatabase.CreateAsset(combinedMesh, "Assets/Art/Meshes/Combined/CombinedMesh.asset");

        // Don't need old object, but still need to exit in the scene
        foreach(var item in selectedObject)
        {
            item.gameObject.SetActive(false);
        }
    }
}