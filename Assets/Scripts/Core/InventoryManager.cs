using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

public class InventoryManager : MonoBehaviour
{
    [Header("Paneles y Grids")]
    public GameObject fullMenuOverlay;
    public Transform containerObjetos;
    public Transform containerContactos;
    public Transform containerLlaves;

    [Header("Prefabs y Datos")]
    public GameObject slotPrefab; 
    public List<ItemData> listaObjetos = new List<ItemData>();
    public List<ItemData> listaContactos = new List<ItemData>();
    public List<ItemData> listaLlaves = new List<ItemData>();
    public List<ItemCombination> todasLasRecetas; // Base de datos de combinaciones

    [Header("UI Tabs")]
    public Image tabBtnObjetos;
    public Image tabBtnContactos;
    public Image tabBtnLlaves;
    public Color activeTabColor = new Color(0f, 0.5f, 0.5f, 1f);
    public Color inactiveTabColor = Color.white;

    [Header("HUD Bar (Modo Mini)")]
    public RectTransform hudBarRect;
    public float transitionSpeed = 10f;
    public Vector2 normalPos = new Vector2(0, 50);
    public Vector2 miniPos = new Vector2(0, -20);
    public Vector3 normalScale = Vector3.one;
    public Vector3 miniScale = new Vector3(0.75f, 0.75f, 0.75f);

    [Header("Panel Derecho (Detalles)")]
    public TextMeshProUGUI detailNameText;
    public TextMeshProUGUI detailDescriptionText;
    public Image detailMainImage;

    [Header("Arrastre (Drag & Drop)")]
    public Image dragIconProxy;
    [HideInInspector] public ItemData itemSiendoArrastrado;
    [HideInInspector] public InventorySlot slotSiendoArrastrado;
    [Header("Hotbar System")]
    public RectTransform selectionFrame; 
    public Vector2 selectionOffset;
    public List<ItemData> hotbarItems = new List<ItemData>(9); 
    private int selectedSlotIndex = 0; 

    private string currentTab = "objetos";
    private bool isInventoryOpen = false;
    void Awake()
    {
        selectionFrame.gameObject.SetActive(false);
    }
    void Start()
    {
        fullMenuOverlay.SetActive(false);
        StartCoroutine(InitHotbarRoutine());

    }
    // Corrutina para asegurar que la hotbar se inicialice correctamente después de que el UI se haya configurado
    System.Collections.IEnumerator InitHotbarRoutine()
    {
        UpdateInventoryUI();
        yield return new WaitForEndOfFrame();
        SetSelectedSlot(0);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) ToggleInventory();
        if (isInventoryOpen == false) HandleHotbarInput();
        if(Application.isPlaying) SetSelectedSlot(selectedSlotIndex); 

    }

    // Control de apertura/cierre y estado del mouse
    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        fullMenuOverlay.SetActive(isInventoryOpen);
        UpdateHUDBarPosition(isInventoryOpen);

        if (isInventoryOpen) {
            SwitchTab(currentTab);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Añadir items a la lista correspondiente según su tipo
    public bool AddItem(ItemData nuevoItem)
    {
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

    /// Función para limpiar el rastro del arrastre
    public void CleanDragAndDrop()
    {
        if (dragIconProxy != null)
        {
            dragIconProxy.gameObject.SetActive(false);
        }
        itemSiendoArrastrado = null;
    }

    // Y un pequeño ajuste en TryCombine para mayor seguridad
    public void TryCombine(ItemData a, ItemData b)
    {
        // Antes de hacer nada, nos aseguramos que el fantasma se apague
        CleanDragAndDrop();

        foreach (ItemCombination receta in todasLasRecetas)
        {
            if (receta.IsMatch(a, b))
            {
                RemoveItemFromLists(a);
                RemoveItemFromLists(b);
                AddItem(receta.resultItem);
                DisplayItemDetails(receta.resultItem);
                UpdateInventoryUI();
                return;
            }
        }
        // Si no hubo combinación, el UpdateInventoryUI no se llama, 
        // pero CleanDragAndDrop ya hizo su trabajo.
    }

    private void RemoveItemFromLists(ItemData item)
    {
        if (listaObjetos.Contains(item)) listaObjetos.Remove(item);
        if (listaContactos.Contains(item)) listaContactos.Remove(item);
        if (listaLlaves.Contains(item)) listaLlaves.Remove(item);

        // 2. Borrar de la Hotbar (Módulo 2)
        for (int i = 0; i < hotbarItems.Count; i++)
        {
            if (hotbarItems[i] == item)
            {
                hotbarItems[i] = null; // Quitamos la data
                ClearHotbarSlotVisual(i); // Limpiamos la imagen de la barra de abajo
            }
        }
    }
    public void ClearHotbarSlotVisual(int index)
    {
        // Buscamos el slot físico en la barra de abajo
        Transform slotGroup = hudBarRect.transform.Find("Inv_Background_Bar_Img/Inv_Slot_Group");
        if (slotGroup != null)
        {
            Transform slot = slotGroup.GetChild(index);
            
            // Apagamos icono y texto (lo mismo que hacía tu Initializer)
            Image img = slot.Find("Item_Icon_Img").GetComponent<Image>();
            img.enabled = false;
            img.color = new Color(1, 1, 1, 0);

            TMPro.TextMeshProUGUI txt = slot.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (txt != null) txt.text = "";
        }
    }

    // Cambio visual y lógico entre pestañas
    public void SwitchTab(string tabName)
    {
        currentTab = tabName;
        containerObjetos.gameObject.SetActive(false);
        containerContactos.gameObject.SetActive(false);
        containerLlaves.gameObject.SetActive(false);

        tabBtnObjetos.color = inactiveTabColor;
        tabBtnContactos.color = inactiveTabColor;
        tabBtnLlaves.color = inactiveTabColor;

        if (tabName == "objetos") { containerObjetos.gameObject.SetActive(true); tabBtnObjetos.color = activeTabColor; }
        else if (tabName == "contactos") { containerContactos.gameObject.SetActive(true); tabBtnContactos.color = activeTabColor; }
        else { containerLlaves.gameObject.SetActive(true); tabBtnLlaves.color = activeTabColor; }
        
        UpdateInventoryUI();
    }

    // Refrescar los slots visuales en el Grid activo
    public void UpdateInventoryUI()
    {
        List<ItemData> listaActual = (currentTab == "objetos") ? listaObjetos : (currentTab == "contactos") ? listaContactos : listaLlaves;
        Transform contenedorActual = (currentTab == "objetos") ? containerObjetos : (currentTab == "contactos") ? containerContactos : containerLlaves;

        foreach (Transform child in contenedorActual) Destroy(child.gameObject);

        foreach (ItemData item in listaActual)
        {
            GameObject nuevoSlot = Instantiate(slotPrefab, contenedorActual);
            InventorySlot slotScript = nuevoSlot.GetComponent<InventorySlot>();
            slotScript.itemContenido = item; 

            Image iconImage = nuevoSlot.transform.Find("Item_Icon_Img").GetComponent<Image>();
            iconImage.sprite = item.gridIcon;
            iconImage.enabled = true;

            TextMeshProUGUI label = nuevoSlot.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = item.itemName;
        }
        detailNameText.text = ""; 
        detailDescriptionText.text = "Selecciona un objeto";
        detailMainImage.enabled = false;
        UpdateHotbarUI();

    }
    public void UpdateHotbarUI()
    {
        // Buscamos el grupo de slots de la barra
        Transform slotGroup = hudBarRect.transform.Find("Inv_Background_Bar_Img/Inv_Slot_Group");
        
        for (int i = 0; i < hotbarItems.Count; i++)
        {
            Transform slot = slotGroup.GetChild(i);
            InventorySlot slotScript = slot.GetComponent<InventorySlot>();
            
            // Buscamos Icono y Texto
            Image iconImage = slot.Find("Item_Icon_Img").GetComponent<Image>();
            TMPro.TextMeshProUGUI textComp = slot.GetComponentInChildren<TMPro.TextMeshProUGUI>();

            if (hotbarItems[i] != null)
            {
                slotScript.itemContenido = hotbarItems[i];
                
                // Sincronizar Icono
                iconImage.sprite = hotbarItems[i].gridIcon;
                iconImage.enabled = true;
                iconImage.color = Color.white;

                // Sincronizar Texto (NUEVO)
                if (textComp != null)
                {
                    textComp.text = hotbarItems[i].itemName;
                    textComp.enabled = true;
                    textComp.color = Color.white; // Alpha al 100%
                }
            }
            else
            {
                slotScript.itemContenido = null;
                
                // Limpiar Icono
                iconImage.enabled = false;
                iconImage.color = new Color(1, 1, 1, 0);

                // Limpiar Texto (NUEVO)
                if (textComp != null)
                {
                    textComp.text = "";
                    textComp.enabled = false;
                }
            }
        }
    }

    // Animación suave de la barra HUD
    void UpdateHUDBarPosition(bool isMini)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateBar(isMini ? miniPos : normalPos, isMini ? miniScale : normalScale));
    }

    System.Collections.IEnumerator AnimateBar(Vector2 targetPos, Vector3 targetScale)
    {
        while (Vector2.Distance(hudBarRect.anchoredPosition, targetPos) > 0.1f)
        {
            hudBarRect.anchoredPosition = Vector2.Lerp(hudBarRect.anchoredPosition, targetPos, Time.deltaTime * transitionSpeed);
            hudBarRect.localScale = Vector3.Lerp(hudBarRect.localScale, targetScale, Time.deltaTime * transitionSpeed);
            yield return null;
        }
        hudBarRect.anchoredPosition = targetPos;
        hudBarRect.localScale = targetScale;
    }

    // Actualizar panel de detalles al hacer clic
    public void DisplayItemDetails(ItemData data)
    {
        if (data == null) return;
        detailNameText.text = data.itemName;
        detailDescriptionText.text = data.description;
        detailMainImage.sprite = data.detailImage;
        detailMainImage.enabled = true;
    }
    void HandleHotbarInput()
    {
        // 1. Selección por Teclado (Alpha1 es la tecla 1, Alpha2 la 2...)
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetSelectedSlot(i);
            }
        }

        // 2. Selección por Rueda del Mouse (Scroll)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // Si scroll > 0 vamos a la izquierda, si < 0 a la derecha
            int step = scroll > 0 ? -1 : 1;
            int newIndex = selectedSlotIndex + step;
            
            // Loop: Si pasas del 8 vuelve al 0, si bajas del 0 vuelve al 8
            if (newIndex > 8) newIndex = 0;
            if (newIndex < 0) newIndex = 8;
            
            SetSelectedSlot(newIndex);
        }
    }
    public void SetSelectedSlot(int index)
    {
        if (hudBarRect == null || selectionFrame == null) return;

        selectedSlotIndex = index;

        // Buscamos el grupo de slots
        Transform slotGroup = hudBarRect.transform.Find("Inv_Background_Bar_Img/Inv_Slot_Group");
        
        if (slotGroup != null && index < slotGroup.childCount)
        {
            // 1. Obtenemos el slot objetivo
            RectTransform targetSlot = slotGroup.GetChild(index).GetComponent<RectTransform>();
            
            // 2. MAGIA DE INGENIERÍA: Igualamos la posición local del marco a la del slot
            // y le sumamos nuestro offset cómodamente.
            selectionFrame.anchoredPosition = targetSlot.anchoredPosition + selectionOffset;
            
            selectionFrame.gameObject.SetActive(true);
        }

        // Actualizar ítem en mano
        if (index < hotbarItems.Count)
        {
            ItemData heldItem = hotbarItems[selectedSlotIndex];
        }
        selectionFrame.gameObject.SetActive(true); 
    }
    public ItemData GetSelectedItem()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < hotbarItems.Count)
        {
            return hotbarItems[selectedSlotIndex];
        }
        return null;
    }
    public void RemoveSelectedItem()
    {
        // 1. Borramos la data de la lista de la Hotbar
        hotbarItems[selectedSlotIndex] = null;

        // 2. Refrescamos la UI para que desaparezca visualmente
        UpdateHotbarUI();
        
        // 3. (Opcional) Limpiar el panel de detalles si estaba mostrando ese ítem
        detailNameText.text = "";
        detailDescriptionText.text = "Selecciona un objeto";
        detailMainImage.enabled = false;
    }
}