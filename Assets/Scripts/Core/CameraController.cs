using UnityEngine;
using Unity.Cinemachine; // Namespace obligatorio para Cinemachine 3

public class CameraController : MonoBehaviour
{
    [Header("Diagnóstico")]
    [ReadOnly] public string activeCameraName; // Verás el nombre en el Inspector
    
    private CinemachineBrain brain;
    private ICinemachineCamera lastActiveCamera;

    void Start()
    {
        // Obtenemos el componente de esta misma cámara
        brain = GetComponent<CinemachineBrain>();

        if (brain == null)
        {
            Debug.LogError("<color=red>ERROR:</color> CameraMonitor necesita estar en el mismo objeto que el CinemachineBrain.");
        }
    }

    void Update()
    {
        if (brain == null) return;

        // Obtenemos la cámara que está "viva" ahora mismo
        ICinemachineCamera currentCam = brain.ActiveVirtualCamera;

        // Solo actuamos si la cámara cambió para no saturar la memoria
        if (currentCam != null && currentCam != lastActiveCamera)
        {
            lastActiveCamera = currentCam;
            
            // Obtenemos el nombre del objeto en la Hierarchy
            activeCameraName = currentCam.Name;

            // Imprimimos en consola con un color llamativo
            Debug.Log($"<color=magenta>CINEMACHINE:</color> Cámara Activa detectada: <b>{activeCameraName}</b>");
        }
    }
}

// Atributo pequeño para que el nombre se vea pero no se pueda editar en el Inspector
public class ReadOnlyAttribute : PropertyAttribute { }