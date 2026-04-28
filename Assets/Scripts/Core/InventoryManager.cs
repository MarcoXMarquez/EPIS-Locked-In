using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private string currentTab = "objetos";
    private bool isInventoryOpen = false;

    void Start()
    {
        fullMenuOverlay.SetActive(false);
        UpdateInventoryUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) ToggleInventory();
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
    void UpdateInventoryUI()
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
}