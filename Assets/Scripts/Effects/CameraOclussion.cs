using UnityEngine;
using System.Collections.Generic;

public class CameraOcclusion : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public LayerMask wallLayer;

    [Header("Configuración")]
    public float fadeAmount = 0.3f;
    
    private List<Renderer> currentObstructions = new List<Renderer>();

    void Update()
    {
        Vector3 direction = player.position - transform.position;
        float distance = Vector3.Distance(transform.position, player.position);

        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, distance, wallLayer);

        ResetObstructions();

        foreach (var hit in hits)
        {    
            Debug.Log("<color=yellow>OCLUSIÓN:</color> Golpeando a " + hit.collider.name);

            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null)
            {
                ApplyFade(renderer);
                currentObstructions.Add(renderer);
            }
        }
    }

    void ApplyFade(Renderer renderer)
    {
        // "_FadeAlpha" debe ser el nombre de la propiedad en tu Shader Graph
        foreach (Material mat in renderer.materials)
        {
            if (mat.HasProperty("_FadeAlpha"))
            {
                mat.SetFloat("_FadeAlpha", fadeAmount);
            }
        }
    }

    void ResetObstructions()
    {
         foreach (var renderer in currentObstructions)
        {
            if (renderer != null)
            {
                foreach (Material mat in renderer.materials)
                {
                    if (mat.HasProperty("_FadeAlpha"))
                    {
                        mat.SetFloat("_FadeAlpha", 1f);
                    }
                }
            }
        }
        currentObstructions.Clear();
    }
}