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
            ItemWorldObject item = lastHit.collider.GetComponent<ItemWorldObject>();
            if (item != null)
            {
                ShowUI(item.itemData.itemName);

                if (Input.GetKeyDown(KeyCode.E) && !anim.GetCurrentAnimatorStateInfo(0).IsName("Action_PickUp"))
                {
                    // 1. Guardamos la referencia del ítem
                    currentPendingItem = item;
                    
                    // 2. Disparamos la animación
                    anim.SetTrigger("PickUp");
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