using UnityEngine;
using Unity.Cinemachine; 
using System.Collections;
using UnityEngine.Rendering; // Necesario para controlar el Post-processing

public class CameraZoneTrigger : MonoBehaviour
{
    [Header("Referencias de Cámaras")]
    public CinemachineCamera backCamera; 
    public CinemachineCamera fpsCamera;  

    [Header("Referencias de Targets")]
    public Transform targetNormal; 
    public Transform targetAlt;    

    [Header("Configuración de Prioridades")]
    public int highPriority = 20;
    public int lowPriority = 5;

    [Header("Sincronización de Cuerpo")]
    public float blendDuration = 0.5f;

    [Header("Efectos Visuales (IA/Kernel)")]
    public bool activarEfectoIA = true;    // Checkbox para decidir en el Inspector
    public Volume volumenEfecto;           // Referencia al Volume local
    public GameObject canvasIA;            // El Canvas de estilo cámara de seguridad

    void Start()
    {
        if (backCamera != null) backCamera.Priority = highPriority;
        if (fpsCamera != null) fpsCamera.Priority = lowPriority;
        
        // Aseguramos que los efectos empiecen apagados
        if (volumenEfecto != null) volumenEfecto.weight = 0;
        if (canvasIA != null) canvasIA.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            Renderer[] allRenderers = other.GetComponentsInChildren<Renderer>(true);
            PlayerMovement pMove = other.GetComponent<PlayerMovement>();

            if (fpsCamera.Priority == lowPriority)
            {
                // MODO FPS (ENTRANDO)
                fpsCamera.Priority = highPriority;
                backCamera.Priority = lowPriority;

                if (pMove != null) pMove.cameraTarget = targetAlt;

                StartCoroutine(ToggleRenderersWithDelay(allRenderers, false, blendDuration));
                
                // ACTIVAR EFECTOS SI ESTÁ MARCADO
                if (activarEfectoIA) {
                    if (volumenEfecto != null) StartCoroutine(FadeVolume(1, blendDuration));
                    if (canvasIA != null) canvasIA.SetActive(true);
                }

                Debug.Log("<color=cyan>SISTEMA:</color> Modo FPS + Efectos IA");
            }
            else
            {
                // MODO TPS (SALIENDO)
                foreach (Renderer r in allRenderers) if (r != null) r.enabled = true;

                if (pMove != null) pMove.cameraTarget = targetNormal;

                fpsCamera.Priority = lowPriority;
                backCamera.Priority = highPriority;

                // APAGAR EFECTOS
                if (volumenEfecto != null) StartCoroutine(FadeVolume(0, blendDuration));
                if (canvasIA != null) canvasIA.SetActive(false);

                Debug.Log("<color=yellow>SISTEMA:</color> Modo TPS - Efectos Limpios");
            }
        }
    }

    // Corrutina para que el ruido de la cámara aparezca suavemente
    private IEnumerator FadeVolume(float targetWeight, float duration)
    {
        float startWeight = volumenEfecto.weight;
        float elapsed = 0;
        while (elapsed < duration)
        {
            volumenEfecto.weight = Mathf.Lerp(startWeight, targetWeight, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        volumenEfecto.weight = targetWeight;
    }

    private IEnumerator ToggleRenderersWithDelay(Renderer[] renderers, bool state, float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (Renderer r in renderers)
        {
            if (r != null) r.enabled = state;
        }
    }
}