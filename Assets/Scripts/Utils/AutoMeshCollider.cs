using UnityEngine;
using System.Linq;

public class AutoMeshCollider : MonoBehaviour
{

    [ContextMenu("Generate Colliders for Hierarchy")]
    public void GenerateColliders()
    {

        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>(true);
        
        int addedCount = 0;
        int skippedCount = 0;

        foreach (MeshFilter filter in filters)
        {
            if (filter.GetComponent<Collider>() == null)
            {
                MeshCollider newCollider = filter.gameObject.AddComponent<MeshCollider>();               
                addedCount++;
            }
            else
            {
                skippedCount++;
            }
        }

        Debug.Log($"<color=cyan>SISTEMA:</color> Proceso terminado. Se añadieron {addedCount} colliders. {skippedCount} ya tenían uno.");
    }

    [ContextMenu("Activate Static for Hierarchy")]

    public void ActivateStatic()
    {
        Transform[] allObjects = GetComponentsInChildren<Transform>(true);

        foreach (Transform obj in allObjects)
        {
            obj.gameObject.isStatic = true;
        }

        Debug.Log("<color=green>SISTEMA:</color> Todos los objetos en la jerarquía han sido marcados como estáticos.");
    }

    [ContextMenu("Remove All Colliders from Children")]
    public void RemoveColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider c in colliders)
        {
            DestroyImmediate(c);
        }
        Debug.Log("<color=red>SISTEMA:</color> Todos los colliders han sido eliminados de la jerarquía.");
    }
}