using UnityEngine;
using UnityEngine.Events; 

public class PuzzleObject : MonoBehaviour
{
    [Header("Requisitos del Puzzle")]
    public ItemData itemRequerido; 

    [Header("Configuración IA (Mensajes)")]
    public string mensajeSiFaltaItem = "SISTEMA: Hardware de conexión no detectado.";
    public string mensajeSiItemEquivocado = "SISTEMA: Error de compatibilidad. Archivo corrupto o ítem no válido.";
    public string mensajeAlResolver = "SISTEMA: Acceso concedido. Iniciando proceso...";

    [Header("Consecuencias (Unity Events)")]
    public UnityEvent alResolverPuzzle; 
    [Header("Lógica de Consumo")]
    public bool eliminarItemAlUsar = true;
    private bool resuelto = false;

    public void IntentarUsar(ItemData itemEnMano)
    {
        if (resuelto) return;

        // Obtenemos el manager para poder borrar el ítem si es necesario
        InventoryManager manager = Object.FindFirstObjectByType<InventoryManager>();

        if (itemEnMano == null)
        {
            Debug.Log("<color=red>IA:</color> " + mensajeSiFaltaItem);
            return;
        }

        if (itemEnMano == itemRequerido)
        {
            resuelto = true;
            Debug.Log("<color=green>IA:</color> " + mensajeAlResolver);
            alResolverPuzzle.Invoke();

            if (eliminarItemAlUsar && manager != null)
            {
                manager.RemoveSelectedItem();
            }
        }
        else
        {
            Debug.Log("<color=orange>IA:</color> " + mensajeSiItemEquivocado);
        }
    }
    public void destroyWall(GameObject wall)
    {
        Destroy(wall);
    }
}