using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Ajustes de Detección")]
    public float interactionDistance = 5f; 
    public float detectionRadius = 0.3f;   
    public LayerMask interactableLayer;    

    [Header("UI Feedback")]
    public GameObject promptPanel;         
    public TextMeshProUGUI promptText;     

    private Camera mainCam;
    private RaycastHit lastHit; // Guardamos el hit para el Gizmo
    private bool hasHit;

    [Header("Sincronización")]
    public Animator anim;
    private ItemWorldObject currentPendingItem;
    void Start()
    {
        mainCam = Camera.main;
        if (promptPanel != null) promptPanel.SetActive(false);
    }

    void Update()
    {
        // Rayo desde el centro de la pantalla
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        
        hasHit = Physics.SphereCast(ray, detectionRadius, out lastHit, interactionDistance, interactableLayer);

        
         if (hasHit)
        {
            // CASO A: Es un ítem para recoger (Lo que ya tenías)
            ItemWorldObject itemMundo = lastHit.collider.GetComponent<ItemWorldObject>();
            if (itemMundo != null)
            {
                ShowUI(itemMundo.itemData.itemName);
                if (Input.GetKeyDown(KeyCode.E)) {
                    currentPendingItem = itemMundo; 
                    anim.SetTrigger("PickUp");
                }
            }

            // CASO B: Es un objeto de PUZZLE (Lo nuevo)
            PuzzleObject objetoPuzzle = lastHit.collider.GetComponent<PuzzleObject>();
            if (objetoPuzzle != null)
            {
                ShowUI("Usar ítem en " + objetoPuzzle.name);

                if (Input.GetKeyDown(KeyCode.E)) // O podrías usar Clic Derecho para "Usar"
                {
                    // Le preguntamos al Manager qué tenemos en la mano
                    ItemData itemEnMano = Object.FindAnyObjectByType<InventoryManager>().GetSelectedItem();
                    objetoPuzzle.IntentarUsar(itemEnMano);
                }
            }
        }
        else
        {
            HideUI();
        }
    }

    // GIZMOS MEJORADOS
    void OnDrawGizmos()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        float dist = hasHit ? lastHit.distance : interactionDistance;

        // Color según impacto
        Gizmos.color = hasHit ? new Color(0, 1, 0, 0.4f) : new Color(1, 0, 0, 0.4f);

        // DIBUJAR EL TÚNEL (Varios pasos para ver el grosor)
        int stepCount = 5;
        for (int i = 0; i <= stepCount; i++)
        {
            float stepDist = (dist / stepCount) * i;
            Vector3 point = ray.GetPoint(stepDist);
            Gizmos.DrawWireSphere(point, detectionRadius);
        }

        // ESFERA DE IMPACTO (Sólida y grande)
        Gizmos.color = hasHit ? Color.green : Color.red;
        Vector3 finalPoint = ray.GetPoint(dist);
        Gizmos.DrawSphere(finalPoint, detectionRadius);

        if (hasHit)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(lastHit.collider.bounds.center, lastHit.collider.bounds.size + Vector3.one * 0.1f);
        }
    }
    void ShowUI(string name)
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(true);
            promptText.text = "Presiona [E] para recoger " + name;
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
            currentPendingItem.PickUp(); // Aquí es donde realmente se suma al inventario y se destruye
            currentPendingItem = null;
        }
    }
}