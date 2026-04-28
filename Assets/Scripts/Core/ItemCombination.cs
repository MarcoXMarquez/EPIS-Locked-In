using UnityEngine; 
[CreateAssetMenu(fileName = "NuevaReceta", menuName = "EPIS/Inventario/Receta de Combinacion")]
public class ItemCombination : ScriptableObject
{
    public ItemData itemA;
    public ItemData itemB;
    public ItemData resultItem;

    [Tooltip("¿Se necesita un orden específico? (Ej. A sobre B)")]
    public bool requiresOrder;

    public bool IsMatch(ItemData a, ItemData b)
    {
        if (requiresOrder)
            return (itemA == a && itemB == b);
        else
            return (itemA == a && itemB == b) || (itemA == b && itemB == a);
    }
}