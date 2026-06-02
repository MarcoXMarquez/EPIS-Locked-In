using UnityEngine;
using Unity.Cinemachine; 
using System.Collections;

public class CameraZoneTrigger : MonoBehaviour
{
    [Header("Referencias de Cámaras")]
    public CinemachineCamera backCamera; 
    public CinemachineCamera fpsCamera;  

    [Header("Referencias de Targets")]
    public Transform targetNormal; // Arrastra el "CameraTarget" original
    public Transform targetAlt;    // Arrastra el "CameraTarget(1)"

    [Header("Configuración de Prioridades")]
    public int highPriority = 20;
    public int lowPriority = 5;

    [Header("Sincronización de Cuerpo")]
    public float blendDuration = 0.5f;

    void Start()
    {
        if (backCamera != null) backCamera.Priority = highPriority;
        if (fpsCamera != null) fpsCamera.Priority = lowPriority;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            Renderer[] allRenderers = other.GetComponentsInChildren<Renderer>(true);
            
            // Obtenemos el script de movimiento para cambiar el target
            PlayerMovement pMove = other.GetComponent<PlayerMovement>();

            if (fpsCamera.Priority == lowPriority)
            {
                // 1. CAMBIO DE CÁMARA
                fpsCamera.Priority = highPriority;
                backCamera.Priority = lowPriority;

                // 2. CAMBIO DE TARGET EN EL SCRIPT DEL PLAYER
                if (pMove != null) pMove.cameraTarget = targetAlt;

                // 3. OCULTAR CUERPO
                StartCoroutine(ToggleRenderersWithDelay(allRenderers, false, blendDuration));
                
                Debug.Log("<color=cyan>SISTEMA:</color> Modo FPS - Target Alternativo Activo");
            }
            else
            {
                // 1. MOSTRAR CUERPO
                foreach (Renderer r in allRenderers) if (r != null) r.enabled = true;

                // 2. CAMBIO DE TARGET EN EL SCRIPT DEL PLAYER (Volver al normal)
                if (pMove != null) pMove.cameraTarget = targetNormal;

                // 3. CAMBIO DE CÁMARA
                fpsCamera.Priority = lowPriority;
                backCamera.Priority = highPriority;

                Debug.Log("<color=yellow>SISTEMA:</color> Modo TPS - Target Normal Restaurado");
            }
        }
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