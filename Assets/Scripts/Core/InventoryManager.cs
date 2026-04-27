using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("Paneles Principales")]
    public GameObject fullMenuOverlay;
    
    [Header("Grids de Contenido")]
    public GameObject gridObjetos;
    public GameObject gridContactos;
    public GameObject gridLlaves;

    [Header("Botones de Pestañas")]
    public Image tabBtnObjetos;
    public Image tabBtnContactos;
    public Image tabBtnLlaves;

    [Header("Colores de Pestañas")]
    public Color activeTabColor = new Color(0f, 0.5f, 0.5f, 1f); // Cian oscuro
    public Color inactiveTabColor = Color.white;

    private bool isInventoryOpen = false;

    void Update()
    {
        // Abrir y cerrar con la tecla I
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        fullMenuOverlay.SetActive(isInventoryOpen);
        
        if (isInventoryOpen) {
            SwitchTab("objetos"); // Abrir por defecto en objetos
            Cursor.lockState = CursorLockMode.None; // Liberar mouse
            Cursor.visible = true;
        } else {
            Cursor.lockState = CursorLockMode.Locked; // Bloquear mouse al juego
            Cursor.visible = false;
        }
    }

    // Función para cambiar de pestaña (Llamada por los botones)
    public void SwitchTab(string tabName)
    {
        // 1. Desactivar todos los grids
        gridObjetos.SetActive(false);
        gridContactos.SetActive(false);
        gridLlaves.SetActive(false);

        // 2. Resetear colores de botones
        tabBtnObjetos.color = inactiveTabColor;
        tabBtnContactos.color = inactiveTabColor;
        tabBtnLlaves.color = inactiveTabColor;

        // 3. Activar el seleccionado
        switch (tabName)
        {
            case "objetos":
                gridObjetos.SetActive(true);
                tabBtnObjetos.color = activeTabColor;
                break;
            case "contactos":
                gridContactos.SetActive(true);
                tabBtnContactos.color = activeTabColor;
                break;
            case "llaves":
                gridLlaves.SetActive(true);
                tabBtnLlaves.color = activeTabColor;
                break;
        }
    }
}