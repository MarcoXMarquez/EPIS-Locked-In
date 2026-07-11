using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Eliminamos System.Numerics y Unity.Mathematics para evitar conflictos
public class PlayerInteraction : MonoBehaviour
{
    [Header("Ajustes de Detección")]
    public float interactionDistance = 7f; 
    public float detectionRadius = 0.5f;   
    public LayerMask interactableLayer;    

    [Header("UI Feedback")]
    public GameObject promptPanel;         
    public TextMeshProUGUI promptText;     

    [Header("Cursores Dinámicos")]
    public Image cursorImage;          
    public Sprite cursorDefault;      
    public Sprite cursorItem;         
    public Sprite cursorPuzzle;     

    [Header("Ajustes de Inercia (Sway)")]
    public RectTransform cursorRect;
    public float swayIntensity = 0.02f; // Ajustado para sensibilidades altas
    public float maxSway = 50f;
    public float returnSpeed = 8f;

    [Header("Sincronización")]
    public Animator anim;
    
    private Camera mainCam;
    private RaycastHit lastHit; 
    private bool hasHit;
    private ItemWorldObject currentPendingItem;
    private PlayerMovement playerMovement;
    private Vector2 currentSwayPos; // Vector2 estándar de Unity

    void Start()
    {
        mainCam = Camera.main;
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        
        if (promptPanel != null) promptPanel.SetActive(false);
        if (cursorImage != null) cursorImage.sprite = cursorDefault;
    }

    void Update()
    {
        // 1. Detección por Raycast/SphereCast
        HandleDetection();

        // 2. Movimiento visual del cursor (Sway)
        HandleCursorSway();
    }

    void HandleDetection()
    {
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        hasHit = Physics.SphereCast(ray, detectionRadius, out lastHit, interactionDistance, interactableLayer);

        if (hasHit)
        {
            Debug.Log($"Hit detected: {lastHit.collider.name} at distance {lastHit.distance}");
            // CASO A: Ítem para recoger
            ItemWorldObject itemMundo = lastHit.collider.GetComponent<ItemWorldObject>();
            if (itemMundo != null)
            {
                SetCursor(cursorItem);
                ShowUI("Recoger " + itemMundo.itemData.itemName);
                if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) 
                {
                    currentPendingItem = itemMundo; 
                    anim.SetTrigger("PickUp");
                }
                return;
            }

            // CASO B: Objeto de PUZZLE
            PuzzleObject puzzle = lastHit.collider.GetComponent<PuzzleObject>();
            if (puzzle != null)
            {
                SetCursor(cursorPuzzle); // Cambiado a cursorPuzzle para diferenciar
                ShowUI("Usar objeto en " + puzzle.name);
                if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) 
                {
                    InventoryManager inv = Object.FindAnyObjectByType<InventoryManager>();
                    puzzle.IntentarUsar(inv.GetSelectedItem());
                }
                return;
            }
        }

        // CASO C: Nada detectado
        SetCursor(cursorDefault);
        HideUI();
    }

    void HandleCursorSway()
    {
        if (cursorRect == null || playerMovement == null) return;

        // Obtenemos input puro para evitar doble suavizado
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");
        float sens = playerMovement.mouseSensitivity;

        // Calculamos el objetivo (Multiplicamos por -1 para el efecto de retraso)
        Vector2 targetSway = new Vector2(-mouseX * sens, -mouseY * sens) * swayIntensity;

        // Limitamos para que no se salga de la zona central
        targetSway.x = Mathf.Clamp(targetSway.x, -maxSway, maxSway);
        targetSway.y = Mathf.Clamp(targetSway.y, -maxSway, maxSway);

        // Interpolación suave
        currentSwayPos = Vector2.Lerp(currentSwayPos, targetSway, Time.deltaTime * returnSpeed);  
        
        // Aplicamos posición y una rotación sutil
        cursorRect.anchoredPosition = currentSwayPos;
        float tiltZ = -currentSwayPos.x * 0.5f;
        cursorRect.localRotation = Quaternion.Euler(0, 0, tiltZ);
    }

    void SetCursor(Sprite newSprite)
    {
        if (cursorImage == null || cursorImage.sprite == newSprite) return;
        
        cursorImage.sprite = newSprite;
        cursorImage.rectTransform.localScale = (newSprite == cursorDefault) ? Vector3.one : new Vector3(1.2f, 1.2f, 1f);
    }

    void ShowUI(string name)
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(true);
            promptText.text = name;
        }
    }

    void HideUI()
    {
        if (promptPanel != null) promptPanel.SetActive(false);
    }

    public void OnPickUpAnimationEvent()
    {
        if (currentPendingItem != null)
        {
            currentPendingItem.PickUp();
            currentPendingItem = null;
        }
    }

    // GIZMOS DE DEPURACIÓN
    void OnDrawGizmos()
    {
        if (mainCam == null) return;
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Gizmos.color = hasHit ? Color.green : Color.red;
        Gizmos.DrawRay(ray.origin, ray.direction * interactionDistance);
    }
}