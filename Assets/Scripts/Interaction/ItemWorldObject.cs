using UnityEngine;

public class ItemWorldObject : MonoBehaviour
{
    [Header("Configuración del Ítem")]
    public ItemData itemData;

    public void PickUp()
    {
        InventoryManager manager = Object.FindAnyObjectByType<InventoryManager>();

        if (manager != null)
        {
            bool wasAdded = manager.AddItem(itemData);
            if (wasAdded)
            {
                Destroy(gameObject);
            }
        }
    }
}