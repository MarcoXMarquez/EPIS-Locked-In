using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    [Header("--- GRIDS DE LA MOCHILA ---")]
    public Transform containerObjetos;
    public Transform containerContactos;
    public Transform containerLlaves;

    [Header("--- PESTAÑAS (TABS) ---")]
    public Image tabBtnObjetos;
    public Image tabBtnContactos;
    public Image tabBtnLlaves;
    public Color activeTabColor = new Color(0f, 0.5f, 0.5f, 1f);
    public Color inactiveTabColor = Color.white;

    [Header("--- HOTBAR (BARRA INFERIOR) ---")]
    public RectTransform hudBarRect;
    public Transform hotbarSlotGroup; // Asegúrate de que este GameObject se llame "Inv_Slot_Group" en el Inspector
    public RectTransform selectionFrame; 
    public Vector2 selectionOffset;
    public TextFadeEffect selectedItemNameTxt;

    [Header("--- ANIMACIÓN HUD ---")]
    public float transitionSpeed = 10f;
    public Vector2 normalPos = new Vector2(0, 50);
    public Vector2 miniPos = new Vector2(0, -20);
    public Vector3 normalScale = Vector3.one;
    public Vector3 miniScale = new Vector3(0.75f, 0.75f, 0.75f);

    [Header("--- PANEL DE DETALLES (DERECHA) ---")]
    public TextMeshProUGUI detailNameText;
    public TextMeshProUGUI detailDescriptionText;
    public Image detailMainImage;

    [Header("--- AJUSTES DE CUADRÍCULA ---")]
    public int maxSlots = 18;

    [Header("--- SISTEMA CORE ---")]
    public GameObject slotPrefab; 
    public List<ItemData> hotbarItems = new List<ItemData>(9); 
    public List<ItemCombination> todasLasRecetas;
    public Image dragIconProxy;

    [Header("--- MENÚ CONTEXTUAL (CLICK DERECHO) ---")]
    public GameObject contextMenuPanel; 
    private InventorySlot slotClickeadoDerecho;
    [HideInInspector] public ItemData itemSiendoArrastrado;
    [HideInInspector] public InventorySlot slotSiendoArrastrado;
    [HideInInspector] public List<ItemData> listaObjetos = new List<ItemData>();
    [HideInInspector] public List<ItemData> listaContactos = new List<ItemData>();
    [HideInInspector] public List<ItemData> listaLlaves = new List<ItemData>();

    private int selectedSlotIndex = 0; 
    private string currentTab = "objetos";
    
    private UIManager uiManager;

    void Awake() 
    {
        selectionFrame.gameObject.SetActive(true);
        uiManager = FindAnyObjectByType<UIManager>(); // Referencia al nuevo UIManager
    }

    void Start()
    {
        GenerarSlotsHotbar();
        StartCoroutine(InitHotbarRoutine());
    }

    private void GenerarSlotsHotbar()
    {
        foreach (Transform child in hotbarSlotGroup) Destroy(child.gameObject);

        hotbarItems.Clear();
        for (int i = 0; i < 9; i++) 
        {
            hotbarItems.Add(null);
            GameObject nuevoSlot = Instantiate(slotPrefab, hotbarSlotGroup);
            nuevoSlot.name = "Slot_Hotbar_" + i;
            
            TextMeshProUGUI txt = nuevoSlot.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = ""; 
        }
    }

    System.Collections.IEnumerator InitHotbarRoutine()
    {
        UpdateInventoryUI();
        yield return new WaitForEndOfFrame();
        SetSelectedSlot(0);
    }

    void Update()
    {
        // Verificamos si el inventario está abierto a través del UIManager
        bool isInventoryOpen = uiManager.inventoryLayer.activeSelf;

        if (!isInventoryOpen) HandleHotbarInput();
        
        if (Application.isPlaying && selectionFrame.gameObject.activeSelf) 
            SetSelectedSlot(selectedSlotIndex);

        // Animación dinámica de la hotbar según el estado del UIManager
        AnimateBarFrame(isInventoryOpen ? miniPos : normalPos, isInventoryOpen ? miniScale : normalScale);
    }

    public bool AddItem(ItemData nuevoItem)
    {
        switch (nuevoItem.type) {
            case ItemData.ItemType.Objeto:
                if (listaObjetos.Count < 18) { listaObjetos.Add(nuevoItem); UpdateInventoryUI(); return true; }
                break;
            case ItemData.ItemType.Contacto:
                if (listaContactos.Count < 18) { listaContactos.Add(nuevoItem); UpdateInventoryUI(); return true; }
                break;
            case ItemData.ItemType.Llave:
                if (listaLlaves.Count < 18) { listaLlaves.Add(nuevoItem); UpdateInventoryUI(); return true; }
                break;
        }
        return false;
    }

    public void TryCombine(ItemData a, ItemData b)
    {
        CleanDragAndDrop();
        foreach (ItemCombination receta in todasLasRecetas) {
            if (receta.IsMatch(a, b)) {
                RemoveItemFromLists(a);
                RemoveItemFromLists(b);
                AddItem(receta.resultItem);
                DisplayItemDetails(receta.resultItem);
                return;
            }
        }
    }

    public void SwitchTab(string tabName)
    {
        currentTab = tabName;
        containerObjetos.gameObject.SetActive(tabName == "objetos");
        containerContactos.gameObject.SetActive(tabName == "contactos");
        containerLlaves.gameObject.SetActive(tabName == "llaves");

        tabBtnObjetos.color = (tabName == "objetos") ? activeTabColor : inactiveTabColor;
        tabBtnContactos.color = (tabName == "contactos") ? activeTabColor : inactiveTabColor;
        tabBtnLlaves.color = (tabName == "llaves") ? activeTabColor : inactiveTabColor;

        UpdateInventoryUI();
        
    }

    public void UpdateInventoryUI()
    {
        if (currentTab == null) currentTab = "objetos";
        
        List<ItemData> lista = (currentTab == "objetos") ? listaObjetos : (currentTab == "contactos") ? listaContactos : listaLlaves;
        Transform container = (currentTab == "objetos") ? containerObjetos : (currentTab == "contactos") ? containerContactos : containerLlaves;

        if (container == null) return;

        // Limpiar contenedor antes de redibujar
        foreach (Transform child in container) Destroy(child.gameObject);

        // Generar SIEMPRE la cantidad fija de slots
        for (int i = 0; i < maxSlots; i++) 
        {
            GameObject nuevoSlot = Instantiate(slotPrefab, container);
            nuevoSlot.name = $"Slot_{currentTab}_{i}";
            
            var script = nuevoSlot.GetComponent<InventorySlot>();
            Transform iconTransform = nuevoSlot.transform.Find("Item_Icon");
            Image iconImg = iconTransform != null ? iconTransform.GetComponent<Image>() : null;
            TextMeshProUGUI txt = nuevoSlot.GetComponentInChildren<TextMeshProUGUI>();

            // Si el índice actual tiene un objeto asignado en la lista
            if (i < lista.Count && lista[i] != null) 
            {
                ItemData item = lista[i];
                if (script != null) script.itemContenido = item;
                
                if (iconImg != null) {
                    iconImg.sprite = item.gridIcon;
                    iconImg.enabled = true;
                    iconImg.color = Color.white;
                }
                if (txt != null) txt.text = item.itemName;
            } 
            else 
            {
                // Inicializar como Slot Vacío
                if (script != null) script.itemContenido = null;
                
                if (iconImg != null) {
                    iconImg.enabled = false;
                    iconImg.color = new Color(1, 1, 1, 0); // Transparente
                }
                if (txt != null) txt.text = "";
            }
        }
        UpdateHotbarUI();
    }

    public void UpdateHotbarUI()
    {
        for (int i = 0; i < 9; i++) {
            Transform slot = hotbarSlotGroup.GetChild(i);
            var icon = slot.Find("Item_Icon").GetComponent<Image>();
            var txt = slot.GetComponentInChildren<TextMeshProUGUI>();

            if (hotbarItems[i] != null) {
                icon.sprite = hotbarItems[i].gridIcon;
                icon.enabled = true;
                icon.color = Color.white;
                if (txt != null) txt.text = hotbarItems[i].itemName;
            } else {
                icon.enabled = false;
                icon.color = new Color(1,1,1,0);
                if (txt != null) txt.text = "";
            }
        }
    }

    void HandleHotbarInput()
    {
        for (int i = 0; i < 9; i++) if (Input.GetKeyDown(KeyCode.Alpha1 + i)) SetSelectedSlot(i);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0) {
            int newIndex = selectedSlotIndex + (scroll > 0 ? -1 : 1);
            if (newIndex > 8) newIndex = 0;
            if (newIndex < 0) newIndex = 8;
            SetSelectedSlot(newIndex);
        }
    }

    public void SetSelectedSlot(int index)
    {
        selectedSlotIndex = index;
        selectionFrame.anchoredPosition = hotbarSlotGroup.GetChild(index).GetComponent<RectTransform>().anchoredPosition + selectionOffset;
        selectionFrame.gameObject.SetActive(true);

        if (hotbarItems[index] != null && selectedItemNameTxt != null)
            selectedItemNameTxt.ShowText(hotbarItems[index].itemName, 2f);
    }

    public ItemData GetSelectedItem() 
    {
        return (selectedSlotIndex >= 0 && selectedSlotIndex < hotbarItems.Count) ? hotbarItems[selectedSlotIndex] : null;
    }

    public void RemoveSelectedItem() 
    {
        hotbarItems[selectedSlotIndex] = null;
        UpdateHotbarUI();
    }

    public void DisplayItemDetails(ItemData data) 
    {
        if (data == null) return;
        detailNameText.text = data.itemName;
        detailDescriptionText.text = data.description;
        detailMainImage.sprite = data.detailImage;
        detailMainImage.enabled = true;
    }

    private void AnimateBarFrame(Vector2 targetPos, Vector3 targetScale)
    {
        hudBarRect.anchoredPosition = Vector2.Lerp(hudBarRect.anchoredPosition, targetPos, Time.deltaTime * transitionSpeed);
        hudBarRect.localScale = Vector3.Lerp(hudBarRect.localScale, targetScale, Time.deltaTime * transitionSpeed);
    }

    public void CleanDragAndDrop() 
    {
        if (dragIconProxy != null) dragIconProxy.gameObject.SetActive(false);
        
        if (slotSiendoArrastrado != null && slotSiendoArrastrado.gameObject != null) {
            Transform iconTransform = slotSiendoArrastrado.transform.Find("Item_Icon");
            if (iconTransform != null) {
                iconTransform.GetComponent<Image>().color = Color.white;
            }
        }
        
        itemSiendoArrastrado = null;
        slotSiendoArrastrado = null;
    }
    private void RemoveItemFromLists(ItemData item) 
    {
        listaObjetos.Remove(item); 
        listaContactos.Remove(item); 
        listaLlaves.Remove(item);
        for (int i = 0; i < hotbarItems.Count; i++) if (hotbarItems[i] == item) hotbarItems[i] = null;
        UpdateInventoryUI();
    }
    public void showTabObjects()
    {
        HideAllTabs();
        currentTab = "objetos";
        containerObjetos.gameObject.SetActive(true);
        tabBtnObjetos.color = activeTabColor;
        UpdateInventoryUI();
    }
    public void showTabContacts()
    {
        HideAllTabs();
        currentTab = "contactos";
        containerContactos.gameObject.SetActive(true);
        tabBtnContactos.color = activeTabColor;
        UpdateInventoryUI();
    }
    public void showTabKeys()
    {
        HideAllTabs();
        currentTab = "llaves";
        containerLlaves.gameObject.SetActive(true);
        tabBtnLlaves.color = activeTabColor;
        UpdateInventoryUI();
    }
    public void HideAllTabs()
    {
        containerObjetos.gameObject.SetActive(false);
        containerContactos.gameObject.SetActive(false);
        containerLlaves.gameObject.SetActive(false);

        tabBtnObjetos.color = inactiveTabColor;
        tabBtnContactos.color = inactiveTabColor;
        tabBtnLlaves.color = inactiveTabColor;
    }
     public void RemoveItemFromGridListsOnly(ItemData item)
    {
        listaObjetos.Remove(item);
        listaContactos.Remove(item);
        listaLlaves.Remove(item);
    }
public void MostrarMenuContextual(InventorySlot slot, Vector2 posicionMouse)
    {
        slotClickeadoDerecho = slot;
        contextMenuPanel.transform.position = posicionMouse;
        contextMenuPanel.SetActive(true);
    }

    // Se ejecuta al pulsar el botón "Mover a la Hotbar" en la interfaz
    public void AccionMoverAHotbar()
    {
        if (slotClickeadoDerecho == null || slotClickeadoDerecho.itemContenido == null)
        {
            OcultarMenuContextual();
            return;
        }

        ItemData item = slotClickeadoDerecho.itemContenido;

        // Buscar el primer slot vacío (null) en la Hotbar
        int slotVacioIndex = -1;
        for (int i = 0; i < hotbarItems.Count; i++)
        {
            if (hotbarItems[i] == null)
            {
                slotVacioIndex = i;
                break;
            }
        }

        // Si encontramos espacio libre, lo movemos
        if (slotVacioIndex != -1)
        {
            RemoveItemFromGridListsOnly(item);
            hotbarItems[slotVacioIndex] = item;
            UpdateInventoryUI();
            Debug.Log($"SISTEMA: {item.itemName} movido al slot {slotVacioIndex} de la Hotbar.");
        }
        else
        {
            Debug.LogWarning("SISTEMA: No hay espacio libre en la Hotbar.");
        }

        OcultarMenuContextual();
    }

    public void OcultarMenuContextual()
    {
        contextMenuPanel.SetActive(false);
        slotClickeadoDerecho = null;
    }
}