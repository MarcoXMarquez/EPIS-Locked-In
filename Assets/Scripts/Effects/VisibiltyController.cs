using UnityEngine;
using System.Collections;

public class VisibilityController : MonoBehaviour
{
    [Header("Configuración de Visibilidad")]
    [SerializeField] private string propertyName = "_Visibility";
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    // Para desvanecer (Visibilidad 1 -> 0)
    public void FadeOut() => StartCoroutine(FadeRoutine(1f, 0f));

    // Para reaparecer (Visibilidad 0 -> 1)
    public void FadeIn() => StartCoroutine(FadeRoutine(0f, 1f));

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Evaluamos la curva para un movimiento más orgánico
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, transitionCurve.Evaluate(t));

            // Aplicamos al Shader sin instanciar materiales
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(propertyName, currentAlpha);
            _renderer.SetPropertyBlock(_propBlock);

            yield return null;
        }

        // Asegurar estado final
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(propertyName, endAlpha);
        _renderer.SetPropertyBlock(_propBlock);

        if (endAlpha <= 0) 
        {
            if(TryGetComponent(out Collider col)) col.enabled = false;
            gameObject.SetActive(false);
        }
        }
}