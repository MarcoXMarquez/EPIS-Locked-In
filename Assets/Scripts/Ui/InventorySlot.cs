using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Añadimos las interfaces de arrastre
public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    public ItemData itemContenido;
    private InventoryManager manager;
    private Image slotImage;

    void Awake()
    {
        slotImage = GetComponent<Image>();
        manager = Object.FindAnyObjectByType<InventoryManager>();
    }

    // --- LÓGICA DE MOSTRAR DETALLES (CLIC NORMAL) ---
    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemContenido != null) manager.DisplayItemDetails(itemContenido);
    }

    // --- INICIO DEL ARRASTRE ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemContenido == null) return;

        // Activamos el icono fantasma y le ponemos la imagen del ítem
        manager.dragIconProxy.gameObject.SetActive(true);
        manager.dragIconProxy.sprite = itemContenido.gridIcon;
        manager.itemSiendoArrastrado = itemContenido; // Guardamos qué estamos moviendo
        
        // Opcional: Hacer que el icono original se vea más transparente mientras arrastras
        slotImage.color = new Color(1, 1, 1, 0.5f);
    }

    // --- MIENTRAS SE ARRASTRA ---
    public void OnDrag(PointerEventData eventData)
    {
        if (itemContenido == null) return;
        // El icono fantasma sigue la posición del mouse
        manager.dragIconProxy.transform.position = Input.mousePosition;
    }

    // --- AL SOLTAR (EN CUALQUIER LUGAR) ---
    // Se ejecuta siempre que sueltas el mouse, no importa dónde
    public void OnEndDrag(PointerEventData eventData)
    {
        // Llamamos a una función de limpieza en el manager
        manager.CleanDragAndDrop();
        slotImage.color = Color.white; // Restauramos la transparencia del slot original
    }

    // Al soltar sobre un slot destino
    public void OnDrop(PointerEventData eventData)
    {
        if (manager.itemSiendoArrastrado != null && manager.itemSiendoArrastrado != itemContenido)
        {
            manager.TryCombine(manager.itemSiendoArrastrado, itemContenido);
            
            // Limpiamos inmediatamente después de intentar combinar
            manager.CleanDragAndDrop();
        }
    }
}