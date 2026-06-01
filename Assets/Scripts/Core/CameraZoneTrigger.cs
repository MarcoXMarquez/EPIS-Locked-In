using UnityEngine;
using Unity.Cinemachine; 
using System.Collections;

public class CameraZoneTrigger : MonoBehaviour
{
    [Header("Referencias de Cámaras")]
    public CinemachineCamera backCamera; 
    public CinemachineCamera fpsCamera;  

    [Header("Configuración de Prioridades")]
    public int highPriority = 20;
    public int lowPriority = 5;

    [Header("Sincronización de Cuerpo")]
    [Tooltip("Debe coincidir con el Default Blend del Cinemachine Brain")]
    public float blendDuration = 0.5f;

    void Start()
    {
        // Estado inicial: TPS activa por defecto
        if (backCamera != null) backCamera.Priority = highPriority;
        if (fpsCamera != null) fpsCamera.Priority = lowPriority;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Mantenemos tu estructura de IFs para detectar al Player
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines(); // Evita errores si pasas muy rápido
            Renderer[] allRenderers = other.GetComponentsInChildren<Renderer>(true);

            // Si la cámara actual es la de ESPALDA, cambiamos a PRIMERA PERSONA
            if (fpsCamera.Priority == lowPriority)
            {
                fpsCamera.Priority = highPriority;
                backCamera.Priority = lowPriority;

                // Esperamos a que la cámara llegue a los ojos para ocultar el cuerpo
                StartCoroutine(ToggleRenderersWithDelay(allRenderers, false, blendDuration));
                
                Debug.Log("<color=cyan>SISTEMA:</color> Modo FPS Activado");
            }
            // Si ya estábamos en FPS, volvemos a TERCERA PERSONA
            else
            {
                // Mostramos el cuerpo INMEDIATAMENTE para que se vea la espalda al alejarse
                foreach (Renderer r in allRenderers) if (r != null) r.enabled = true;

                fpsCamera.Priority = lowPriority;
                backCamera.Priority = highPriority;

                Debug.Log("<color=yellow>SISTEMA:</color> Modo TPS Restaurado");
            }
        }
    }

    // Función auxiliar para el retraso
    private IEnumerator ToggleRenderersWithDelay(Renderer[] renderers, bool state, float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (Renderer r in renderers)
        {
            if (r != null) r.enabled = state;
        }
    }
}