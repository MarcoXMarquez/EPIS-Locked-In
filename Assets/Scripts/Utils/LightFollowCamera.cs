using UnityEngine;

public class LightFollowCamera : MonoBehaviour
{
    private Transform camTransform;

    void Start()
    {
        // Buscamos la cámara principal al inicio
        if (Camera.main != null)
            camTransform = Camera.main.transform;
    }

    // Usamos LateUpdate para que la luz se mueva DESPUÉS de que Cinemachine mueva la cámara
    void LateUpdate()
    {
        if (camTransform != null)
        {
            // La luz apunta exactamente en la misma dirección que la cámara
            transform.forward = camTransform.forward;
        }
    }
}