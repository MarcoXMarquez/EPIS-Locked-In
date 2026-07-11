using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Capas de Interfaz")]
    public GameObject hudLayer;
    public GameObject inventoryLayer;
    public GameObject pauseLayer;
    public GameObject settingsLayer;

    private bool isMenuOpen = false;

    void Start()
    {
        // Estado inicial
        CloseAllMenus();
    }

    void Update()
    {
        // Tecla ESC para Pausa
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsLayer.activeSelf) OpenPause(); // Si está en ajustes, vuelve a pausa
            else TogglePause();
        }

        // Tecla I para Inventario
        if (Input.GetKeyDown(KeyCode.I) && !pauseLayer.activeSelf)
        {
            InventoryManager inventoryManager = FindAnyObjectByType<InventoryManager>();
            if (inventoryManager != null)
            {
                inventoryManager.OcultarMenuContextual();
            }

            ToggleInventory();
        }
    }

    public void CloseAllMenus()
    {
        inventoryLayer.SetActive(false);
        pauseLayer.SetActive(false);
        settingsLayer.SetActive(false);
        hudLayer.SetActive(true);
        
        isMenuOpen = false;
        SetCursorState(false);
    }

    public void ToggleInventory()
    {
        isMenuOpen = !inventoryLayer.activeSelf;
        inventoryLayer.SetActive(isMenuOpen);
        hudLayer.SetActive(!isMenuOpen);
        SetCursorState(isMenuOpen);
    }

    public void TogglePause()
    {
        isMenuOpen = !pauseLayer.activeSelf;
        pauseLayer.SetActive(isMenuOpen);
        hudLayer.SetActive(!isMenuOpen);
        SetCursorState(isMenuOpen);
        
        // Pausar el tiempo del juego
        Time.timeScale = isMenuOpen ? 0 : 1;
    }

    public void OpenSettings()
    {
        pauseLayer.SetActive(false);
        settingsLayer.SetActive(true);
    }

    public void OpenPause()
    {
        settingsLayer.SetActive(false);
        pauseLayer.SetActive(true);
    }
    public void QuitGame()
    {
        Debug.Log("Saliendo del sistema...");
        Application.Quit();
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}