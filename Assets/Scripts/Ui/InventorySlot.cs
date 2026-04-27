using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Necesario para detectar el mouse

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image slotImage;
    private Color originalColor;
    public Color hoverColor = new Color(0f, 1f, 1f, 1f); // Cian brillante

    void Awake()
    {
        slotImage = GetComponent<Image>();
        originalColor = slotImage.color;
    }

    // Cuando el mouse entra al slot
    public void OnPointerEnter(PointerEventData eventData)
    {
        slotImage.color = hoverColor;
    }

    // Cuando el mouse sale del slot
    public void OnPointerExit(PointerEventData eventData)
    {
        slotImage.color = originalColor;
    }
}