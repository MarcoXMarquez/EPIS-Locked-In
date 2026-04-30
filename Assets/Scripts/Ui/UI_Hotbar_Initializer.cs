using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Hotbar_Initializer : MonoBehaviour
{
    void Awake() // Usamos Awake para que ocurra antes de que el jugador vea nada
    {
        // Recorremos todos los slots hijos del grupo
        foreach (Transform slot in transform)
        {
            // 1. Limpiar la imagen del icono
            Transform iconTransform = slot.Find("Item_Icon_Img");
            if (iconTransform != null)
            {
                Image img = iconTransform.GetComponent<Image>();
                if (img != null)
                {
                    // Ponemos el color en blanco pero con Alpha (transparencia) en 0
                    img.color = new Color(1, 1, 1, 0);
                    // También lo desactivamos por si acaso
                    img.enabled = false;
                }
            }

            // 2. Limpiar el texto de TextMeshPro
            TextMeshProUGUI text = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "";
            }
        }
    }
}