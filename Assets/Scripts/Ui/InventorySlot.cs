using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

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
        // Si el slot no tiene data asignada, salimos
        if (itemContenido == null) return;

        manager.itemSiendoArrastrado = itemContenido;
        manager.slotSiendoArrastrado = this;
        
        manager.dragIconProxy.gameObject.SetActive(true);
        manager.dragIconProxy.sprite = itemContenido.gridIcon;
        
        // Opcional: bajar opacidad del icono original para feedback visual
        transform.Find("Item_Icon_Img").GetComponent<Image>().color = new Color(1,1,1,0.5f);
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
        if (manager.itemSiendoArrastrado == null) return;

        bool origenEsHotbar = manager.slotSiendoArrastrado.transform.parent.name == "Inv_Slot_Group";
        bool destinoEsHotbar = transform.parent.name == "Inv_Slot_Group";

        // Si soltamos un slot sobre OTRO slot...
        if (origenEsHotbar && !destinoEsHotbar) // Hotbar -> Slot ocupado en Mochila
        {
            int indexOrigen = manager.slotSiendoArrastrado.transform.GetSiblingIndex();
            manager.AddItem(manager.itemSiendoArrastrado);
            manager.hotbarItems[indexOrigen] = null;
        }
        else if (!origenEsHotbar && destinoEsHotbar) // Mochila -> Hotbar
        {
            int indexDestino = transform.GetSiblingIndex();
            if (manager.hotbarItems[indexDestino] != null) manager.AddItem(manager.hotbarItems[indexDestino]);
            manager.hotbarItems[indexDestino] = manager.itemSiendoArrastrado;
            RemoveFromMochila(manager.itemSiendoArrastrado);
        }
        else if (origenEsHotbar && destinoEsHotbar) // Hotbar -> Hotbar
        {
            int indexOrigen = manager.slotSiendoArrastrado.transform.GetSiblingIndex();
            int indexDestino = transform.GetSiblingIndex();
            ItemData temp = manager.hotbarItems[indexDestino];
            manager.hotbarItems[indexDestino] = manager.itemSiendoArrastrado;
            manager.hotbarItems[indexOrigen] = temp;
        }
        else if (!origenEsHotbar && !destinoEsHotbar && manager.itemSiendoArrastrado != itemContenido) // Mochila -> Mochila (Combinar)
        {
            manager.TryCombine(manager.itemSiendoArrastrado, itemContenido);
        }

        manager.UpdateInventoryUI();
        manager.CleanDragAndDrop();
    }
  void RemoveFromMochila(ItemData item)
    {
        if (manager.listaObjetos.Contains(item)) manager.listaObjetos.Remove(item);
        if (manager.listaContactos.Contains(item)) manager.listaContactos.Remove(item);
        if (manager.listaLlaves.Contains(item)) manager.listaLlaves.Remove(item);
    }
}