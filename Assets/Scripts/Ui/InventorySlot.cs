using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    public ItemData itemContenido;
    
    private InventoryManager manager;
    private bool isHotbarSlot;

    void Awake()
    {
        manager = Object.FindAnyObjectByType<InventoryManager>();
        // Detecta de forma dinámica si este slot reside dentro de la barra inferior
        isHotbarSlot = transform.parent != null && transform.parent.name == "Slot_Group";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemContenido == null) return;

        manager.itemSiendoArrastrado = itemContenido;
        manager.slotSiendoArrastrado = this;

        if (manager.dragIconProxy != null)
        {
            manager.dragIconProxy.sprite = itemContenido.gridIcon;
            manager.dragIconProxy.gameObject.SetActive(true);
            
            // Feedback visual: atenuamos ligeramente el icono del slot de origen
            Transform iconTransform = transform.Find("Item_Icon");
            if (iconTransform != null)
            {
                Image img = iconTransform.GetComponent<Image>();
                if (img != null) img.color = new Color(1f, 1f, 1f, 0.4f);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (manager.itemSiendoArrastrado == null) return;

        if (manager.dragIconProxy != null)
        {
            manager.dragIconProxy.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        manager.CleanDragAndDrop();
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Validaciones de seguridad iniciales
        if (manager.itemSiendoArrastrado == null || manager.slotSiendoArrastrado == null) return;

        InventorySlot slotOrigen = manager.slotSiendoArrastrado;
        ItemData itemOrigen = manager.itemSiendoArrastrado;

        // Si soltamos sobre un slot que ya contiene un objeto distinto
        if (this.itemContenido != null && this.itemContenido != itemOrigen)
        {
            manager.TryCombine(itemOrigen, this.itemContenido);
            return;
        }

        if (this.isHotbarSlot && this.itemContenido == null)
        {
            int indexDestino = transform.GetSiblingIndex();

            if (slotOrigen.isHotbarSlot)
            {
                // Reordenamiento interno dentro de la misma Hotbar
                int indexOrigen = slotOrigen.transform.GetSiblingIndex();
                manager.hotbarItems[indexOrigen] = null;
                manager.hotbarItems[indexDestino] = itemOrigen;
            }
            else
            {
                // Mover desde la mochila general a un slot vacío de la Hotbar
                manager.RemoveItemFromGridListsOnly(itemOrigen);
                manager.hotbarItems[indexDestino] = itemOrigen;
            }

            manager.UpdateInventoryUI();
            manager.CleanDragAndDrop();
            return;
        }

        // --- MECÁNICA EXTRA: MOVER DE HOTBAR A UN SLOT VACÍO DEL INVENTARIO GENERAL ---
        if (!this.isHotbarSlot && slotOrigen.isHotbarSlot && this.itemContenido == null)
        {
            int indexOrigenHotbar = slotOrigen.transform.GetSiblingIndex();
            
            if (manager.AddItem(itemOrigen))
            {
                manager.hotbarItems[indexOrigenHotbar] = null;
            }

            manager.UpdateInventoryUI();
            manager.CleanDragAndDrop();
            return;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // Detecta clic derecho, verifica que tenga un ítem y que NO esté ya en la Hotbar
        if (eventData.button == PointerEventData.InputButton.Right && itemContenido != null && !isHotbarSlot)
        {
            manager.MostrarMenuContextual(this, eventData.position);
        }
    }
}