using UnityEngine;

public class ItemWorldObject : MonoBehaviour
{
    [Header("Ficha del Ítem")]
    public ItemData itemData; // Arrastra aquí el Scriptable Object (ej. Data_Cable)

    public void PickUp()
    {
        InventoryManager manager = Object.FindAnyObjectByType<InventoryManager>();
        if (manager != null)
        {
            if (manager.AddItem(itemData))
            {
                // Aquí podrías disparar un sonido o efecto antes de destruir
                Destroy(gameObject);
            }
        }
    }
}