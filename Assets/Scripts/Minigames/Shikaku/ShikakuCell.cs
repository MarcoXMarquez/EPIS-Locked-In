using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Necesario para detectar clics y drags
using TMPro;

public class ShikakuCell : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
{
    [Header("Datos de Celda")]
    public int x;
    public int y;
    public int value; // El número que contiene (0 si está vacía)

    [Header("Referencias")]
    public TextMeshProUGUI numberText;
    private ShikakuManager manager;
    [HideInInspector] public GameObject blockOccupyingMe;

    void Start()
    {
        manager = Object.FindAnyObjectByType<ShikakuManager>();
    }

    public void SetValue(int val)
    {
        value = val;
        numberText.text = (val > 0) ? val.ToString() : "";
    }

    // Detecta el primer clic
    public void OnPointerDown(PointerEventData eventData)
    {
        manager.OnCellPointerDown(this);
    }

    // Detecta cuando el mouse entra en la celda mientras se mantiene presionado
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.dragging || Input.GetMouseButton(0))
        {
            manager.OnCellPointerEnter(this);
        }
    }

    // Detecta cuando soltamos el clic
    public void OnPointerUp(PointerEventData eventData)
    {
        manager.OnCellPointerUp(this);
    }
}