using UnityEngine;

// Esto permite crear el ítem haciendo clic derecho en la carpeta Assets
[CreateAssetMenu(fileName = "NuevoItem", menuName = "EPIS/Inventario/Item")]
public class ItemData : ScriptableObject
{
    [Header("Información Básica")]
    public string itemName;
    [TextArea(3, 10)]
    public string description;

    [Header("Categoría")]
    public ItemType type;

    [Header("Parámetros Visuales (Estándar EPIS)")]
    [Tooltip("Icono para el Grid. Tamaño sugerido: 130x130 px")]
    public Sprite gridIcon;

    [Tooltip("Imagen para el Panel de Detalles. Tamaño sugerido: 400x400 px")]
    public Sprite detailImage;

    public enum ItemType { Objeto, Contacto, Llave }
}