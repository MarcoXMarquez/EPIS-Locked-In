using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryPanelDrop : MonoBehaviour, IDropHandler
{
    private InventoryManager manager;

    void Awake()
    {
        manager = Object.FindAnyObjectByType<InventoryManager>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Si soltamos un ítem aquí y venía de la Hotbar...
        if (manager.itemSiendoArrastrado != null && manager.slotSiendoArrastrado != null)
        {
            bool origenEsHotbar = manager.slotSiendoArrastrado.transform.parent.name == "Inv_Slot_Group";

            if (origenEsHotbar)
            {
                int indexOrigen = manager.slotSiendoArrastrado.transform.GetSiblingIndex();
                
                // 1. Lo devolvemos a la mochila
                manager.AddItem(manager.itemSiendoArrastrado);
                
                // 2. Lo quitamos de la Hotbar
                manager.hotbarItems[indexOrigen] = null;

                // 3. Refrescamos y limpiamos
                manager.UpdateInventoryUI();
                manager.CleanDragAndDrop();
                
                Debug.Log("SISTEMA: Ítem devuelto a la mochila desde el panel.");
            }
        }
    }
}