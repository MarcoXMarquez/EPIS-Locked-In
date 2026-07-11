using UnityEngine;
using System.Collections;

public class InteractiveDoor : MonoBehaviour
{
    [Header("Configuración")]
    public Transform playerSnapPoint; // ElInteraction_Point que creamos
    public Animator doorAnimator;     // El animator de la bisagra (si tiene)
    public float interactionDelay = 0.2f;

    private bool isBusy = false;

    public void OnDoorInteract(GameObject player)
    {
        if (isBusy) return;
        StartCoroutine(DoorSequence(player));
    }

    IEnumerator DoorSequence(GameObject player)
    {
        isBusy = true;

        // 1. Obtener referencias
        PlayerMovement moveScript = player.GetComponent<PlayerMovement>();
        CharacterController controller = player.GetComponent<CharacterController>();
        Animator playerAnim = player.GetComponent<Animator>();

        // 2. Bloquear controles del jugador
        if (moveScript != null) moveScript.enabled = false;

        // 3. "Snap": Mover al jugador al punto exacto (Teletransporte suave)
        // Esto asegura que la mano toque el handle siempre
        player.transform.position = playerSnapPoint.position;
        player.transform.rotation = playerSnapPoint.rotation;

        // 4. Disparar animación del jugador
        playerAnim.SetTrigger("OpenDoor");

        // 5. Esperar el momento justo para que la puerta se mueva
        // Aquí puedes usar un Animation Event en el clip del player
        yield return new WaitForSeconds(interactionDelay);
        
        if (doorAnimator != null) doorAnimator.SetTrigger("Open");

        // 6. Esperar a que termine la animación completa (aprox 3-4 segundos)
        yield return new WaitForSeconds(3.5f);

        // 7. Devolver el control al jugador
        if (moveScript != null) moveScript.enabled = true;
        isBusy = false;
    }
}