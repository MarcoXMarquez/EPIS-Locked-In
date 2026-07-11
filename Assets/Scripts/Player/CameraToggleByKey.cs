using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraToggleByKey : MonoBehaviour
{
    [Header("Referencias de Camaras")]
    public CinemachineCamera tpsCamera; 
    public CinemachineCamera fpsCamera;  

    [Header("Referencias de Targets")]
    public Transform targetNormal; // Camera_Normal
    public Transform targetAlt;    // Camera_Alt

    [Header("Teclas y Ajustes")]
    public KeyCode toggleKey = KeyCode.V; // Tecla para alternar
    public float blendDuration = 0.5f;

    [Header("Configuracion de Prioridades")]
    public int highPriority = 20;
    public int lowPriority = 5;

    private bool isFirstPerson = false;
    private PlayerMovement playerMovement;
    private Renderer[] playerRenderers;
    private Coroutine toggleRenderersCoroutine;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerRenderers = GetComponentsInChildren<Renderer>(true);

        // Inicializar prioridades segun estado inicial
        if (tpsCamera != null) tpsCamera.Priority = highPriority;
        if (fpsCamera != null) fpsCamera.Priority = lowPriority;

        if (playerMovement != null && targetNormal != null)
        {
            playerMovement.cameraTarget = targetNormal;
        }
    }

    void Update()
    {
        // Evitamos cambiar de camara si el menu de inventario o algun puzzle esta activo
        InventoryManager inventory = Object.FindAnyObjectByType<InventoryManager>();
        ShikakuManager shikaku = Object.FindAnyObjectByType<ShikakuManager>();

        if ((inventory != null && inventory.fullMenuOverlay.activeSelf) || 
            (shikaku != null && shikaku.isPuzzleActive))
        {
            return;
        }

        if (Input.GetKeyDown(toggleKey))
        {
            ToggleCamera();
        }
    }

    public void ToggleCamera()
    {
        if (tpsCamera == null || fpsCamera == null || playerMovement == null)
        {
            Debug.LogWarning("[CameraToggleByKey] Referencias faltantes en el script.");
            return;
        }

        isFirstPerson = !isFirstPerson;

        if (toggleRenderersCoroutine != null)
        {
            StopCoroutine(toggleRenderersCoroutine);
        }

        if (isFirstPerson)
        {
            // --- ACTIVAR PRIMERA PERSONA (FPS) ---
            fpsCamera.Priority = highPriority;
            tpsCamera.Priority = lowPriority;

            if (targetAlt != null)
            {
                playerMovement.cameraTarget = targetAlt;
            }

            // Ocultamos la malla del jugador despues de que la camara haga la transicion
            toggleRenderersCoroutine = StartCoroutine(ToggleRenderersWithDelay(false, blendDuration));

            Debug.Log("<color=cyan>SISTEMA CAMARA:</color> Cambiado a Primera Persona (FPS)");
        }
        else
        {
            // --- ACTIVAR TERCERA PERSONA (TPS) ---
            // Mostramos los renderers de inmediato para que se vea el cuerpo en el retroceso
            ToggleRenderers(true);

            tpsCamera.Priority = highPriority;
            fpsCamera.Priority = lowPriority;

            if (targetNormal != null)
            {
                playerMovement.cameraTarget = targetNormal;
            }

            Debug.Log("<color=yellow>SISTEMA CAMARA:</color> Cambiado a Tercera Persona (TPS)");
        }
    }

    private IEnumerator ToggleRenderersWithDelay(bool state, float delay)
    {
        yield return new WaitForSeconds(delay);
        ToggleRenderers(state);
    }

    private void ToggleRenderers(bool state)
    {
        if (playerRenderers == null) return;
        foreach (Renderer r in playerRenderers)
        {
            if (r != null)
            {
                r.enabled = state;
            }
        }
    }
}
