using UnityEngine;
using System.Collections;

/// <summary>
/// Coloca este script en el GameObject raiz de la PUERTA (o en el hijo "Bisagra").
/// El pivote del objeto debe estar en la esquina de la bisagra para que la rotacion sea correcta.
/// Funciona para CUALQUIER puerta de la escena, no importa su nombre.
/// </summary>
public class InteractiveDoor : MonoBehaviour
{
    [Header("Configuracion de Apertura")]
    [Tooltip("Angulo de apertura en grados. Positivo = abre hacia la derecha, negativo = hacia la izquierda.")]
    public float openAngle = 90f;

    [Tooltip("Segundos que tarda en abrirse o cerrarse")]
    public float openDuration = 0.55f;

    [Tooltip("Texto del prompt cuando la puerta esta cerrada")]
    public string labelClosed = "Abrir puerta";

    [Tooltip("Texto del prompt cuando la puerta esta abierta")]
    public string labelOpen = "Cerrar puerta";

    [Header("Animator (opcional)")]
    [Tooltip("Si tienes un Animator asignado, se usara en lugar de la rotacion por codigo")]
    public Animator doorAnimator;
    public string openTrigger  = "Open";
    public string closeTrigger = "Close";

    // --- Estado interno ---
    private bool isOpen = false;
    private bool isBusy = false;
    private Quaternion closedRot;
    private Quaternion openRot;

    void Awake()
    {
        closedRot = transform.localRotation;
        openRot   = closedRot * Quaternion.Euler(0f, 0f, openAngle);
    }

    /// <summary>
    /// Llamado por PlayerInteraction cuando el jugador pulsa E cerca de esta puerta.
    /// </summary>
    public void Interact(GameObject player)
    {
        if (isBusy) return;

        // Si hay Animator, delegamos en el
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(isOpen ? closeTrigger : openTrigger);
            isOpen = !isOpen;
            return;
        }

        // Sin Animator: rotacion suave por codigo
        StartCoroutine(RotateDoor(isOpen ? closedRot : openRot));
        isOpen = !isOpen;
    }

    IEnumerator RotateDoor(Quaternion targetRot)
    {
        isBusy = true;
        Quaternion startRot = transform.localRotation;
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            float t = elapsed / openDuration;
            // Smooth-step para movimiento organico (no lineal)
            t = t * t * (3f - 2f * t);
            transform.localRotation = Quaternion.Lerp(startRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = targetRot;
        isBusy = false;
    }

    /// <summary>Etiqueta que muestra el HUD segun el estado actual.</summary>
    public string GetLabel() => isOpen ? labelOpen : labelClosed;
}