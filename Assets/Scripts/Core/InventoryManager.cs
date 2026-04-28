using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Necesario para los textos

public class InventoryManager : MonoBehaviour
{
    [Header("Paneles Principales")]
    public GameObject fullMenuOverlay;
    
    [Header("Grids de Contenido (Transform)")]
    public Transform containerObjetos;
    public Transform containerContactos;
    public Transform containerLlaves;

    [Header("Prefabs")]
    public GameObject slotPrefab; 

    [Header("Listas de Datos")]
    public List<ItemData> listaObjetos = new List<ItemData>();
    public List<ItemData> listaContactos = new List<ItemData>();
    public List<ItemData> listaLlaves = new List<ItemData>();

    [Header("Botones de Pestañas")]
    public Image tabBtnObjetos;
    public Image tabBtnContactos;
    public Image tabBtnLlaves;

    [Header("Colores")]
    public Color activeTabColor = new Color(0f, 0.5f, 0.5f, 1f);
    public Color inactiveTabColor = Color.white;

    private string currentTab = "objetos";
    private bool isInventoryOpen = false;

    void Start()
    {
        fullMenuOverlay.SetActive(false);
        // Inicializamos la UI vacía
        UpdateInventoryUI();
    }

    void Update()
    {
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
            SwitchTab(currentTab);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Lógica para añadir un ítem dinámicamente
    public bool AddItem(ItemData nuevoItem)
    {
        // Generalización: El sistema decide a qué lista enviarlo según el TYPE
        switch (nuevoItem.type)
        {
            case ItemData.ItemType.Objeto:
                if (listaObjetos.Count < 18) { listaObjetos.Add(nuevoItem); break; }
                return false;
            case ItemData.ItemType.Contacto:
                if (listaContactos.Count < 18) { listaContactos.Add(nuevoItem); break; }
                return false;
            case ItemData.ItemType.Llave:
                if (listaLlaves.Count < 18) { listaLlaves.Add(nuevoItem); break; }
                return false;
        }

        UpdateInventoryUI();
        return true;
    }

    public void SwitchTab(string tabName)
    {
        currentTab = tabName;

        // Apagar todos los contenedores visuales
        containerObjetos.gameObject.SetActive(false);
        containerContactos.gameObject.SetActive(false);
        containerLlaves.gameObject.SetActive(false);

        // Reset colores
        tabBtnObjetos.color = inactiveTabColor;
        tabBtnContactos.color = inactiveTabColor;
        tabBtnLlaves.color = inactiveTabColor;

        switch (tabName)
        {
            case "objetos":
                containerObjetos.gameObject.SetActive(true);
                tabBtnObjetos.color = activeTabColor;
                break;
            case "contactos":
                containerContactos.gameObject.SetActive(true);
                tabBtnContactos.color = activeTabColor;
                break;
            case "llaves":
                containerLlaves.gameObject.SetActive(true);
                tabBtnLlaves.color = activeTabColor;
                break;
        }
        
        UpdateInventoryUI();
    }

    // ESTA ES LA PARTE CLAVE: Actualiza los cuadraditos de la UI
    void UpdateInventoryUI()
    {
        // 1. Elegimos qué lista y qué contenedor usar
        List<ItemData> listaActual = (currentTab == "objetos") ? listaObjetos : 
                                     (currentTab == "contactos") ? listaContactos : listaLlaves;
        
        Transform contenedorActual = (currentTab == "objetos") ? containerObjetos : 
                                      (currentTab == "contactos") ? containerContactos : containerLlaves;

        // 2. Limpiamos el contenedor (Borramos los slots viejos)
        foreach (Transform child in contenedorActual) {
            Destroy(child.gameObject);
        }

        // 3. Creamos los slots nuevos basados en la lista de items
        foreach (ItemData item in listaActual)
        {
            GameObject nuevoSlot = Instantiate(slotPrefab, contenedorActual);
            
            // Buscamos el componente de imagen del icono dentro del prefab (Item_Icon_Img)
            // Asumimos que el prefab tiene un hijo llamado "Item_Icon_Img"
            Image iconImage = nuevoSlot.transform.Find("Item_Icon_Img").GetComponent<Image>();
            iconImage.sprite = item.gridIcon; // Usamos el de 130x130
            iconImage.enabled = true;

            // Opcional: Poner el nombre abajo si el prefab tiene texto
            TextMeshProUGUI label = nuevoSlot.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = item.itemName;
        }
    }
}