using UnityEngine;
using System.Collections;

public class DissolveEffect : MonoBehaviour
{
    public Material dissolveMaterial;
    public float duration = 1.5f;

    // Esta función la puedes llamar desde el UnityEvent de tu PuzzleObject
    public void StartDissolve()
    {
        StartCoroutine(DissolveRoutine());
    }

    IEnumerator DissolveRoutine()
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Lerp(0, 1, elapsed / duration);
            
            // "DissolveAmount" debe llamarse igual que el nombre de referencia en el Shader
            dissolveMaterial.SetFloat("_DissolveAmount", progress);
            
            yield return null;
        }
        
        // Al terminar, desactivamos el objeto
        gameObject.SetActive(false);
    }
}