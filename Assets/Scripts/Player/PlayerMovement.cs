using UnityEngine;
using System.Collections; // Necesario para las Corrutinas

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 6.0f;
    public float rotationSpeed = 10.0f;
    public float gravity = -9.81f;

    [Header("Configuración Crouch")]
    public float normalHeight = 1.86f;
    public float crouchHeight = 0.98f;
    public float normalCenterY = 1f;
    public float crouchCenterY = 0.5f;

    [Header("Ajustes de Cámara y Enfoque")]
    public Transform cameraTarget; 
    public float mouseSensitivity = 100f;
    public float normalTargetHeight = 1.5f;
    public float crouchTargetHeight = 0.8f;

    [Header("Ajustes de Transición")]
    [Tooltip("Tiempo que tarda en bajar/subir la cámara. Ajustar según la velocidad de la animación en el Animator.")]
    public float crouchTransitionTime = 0.5f; 
    private bool isTransitioning = false;

    private CharacterController controller;
    private Animator anim;
    private InventoryManager inventory;
    private Vector3 velocity; 
    private Transform cam;
    [Header("Ajustes de Cámara Vertical")]
    public float minPitch = -20f; // Límite para mirar hacia arriba
    public float maxPitch = 45f;  // Límite para mirar hacia abajo (suelo)
    private float verticalRotation = 0f;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        cam = Camera.main.transform;
        
        inventory = Object.FindAnyObjectByType<InventoryManager>();

        // Bloquear cursor al inicio
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. SEGURIDAD: Si el inventario está abierto, no hacemos nada
        if ((inventory != null && inventory.fullMenuOverlay.activeSelf) || 
         anim.GetCurrentAnimatorStateInfo(0).IsName("Action_PickUp"))
        {
            StopMovement();
            return;
        }
        // 2. SEGURIDAD: Si está agachándose o levantándose, solo aplicamos gravedad
        if (isTransitioning)
        {
            ApplyGravity();
            return;
        }

        // 3. Ejecutar movimiento normal
        Move();
    }

    void Move()
    {
        // --- ROTACIÓN CON EL MOUSE ---
        if (!(inventory != null && inventory.fullMenuOverlay.activeSelf) && !isTransitioning)
        {
            // Rotación Horizontal (Gira el cuerpo del personaje)
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            transform.Rotate(Vector3.up * mouseX);

            // Rotación Vertical (Gira el OBJETIVO de la cámara)
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            verticalRotation -= mouseY; // Invertimos para que sea intuitivo
            
            // LIMITAR LA ROTACIÓN (Clamping)
            // Esto evita que la cámara de la vuelta completa o vea debajo del suelo
            verticalRotation = Mathf.Clamp(verticalRotation, minPitch, maxPitch);
            
            // Aplicamos la rotación local solo al CameraTarget
            cameraTarget.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        // --- INPUT WASD ---
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 moveDirection = (transform.forward * vertical + transform.right * horizontal).normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (moveDirection.magnitude >= 0.1f)
        {
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        ApplyGravity();

        // --- ANIMATOR ---
        float animSpeedY = vertical * (isRunning ? 1f : 0.5f);
        float animSpeedX = horizontal * (isRunning ? 1f : 0.5f);

        anim.SetFloat("VelY", Mathf.Lerp(anim.GetFloat("VelY"), animSpeedY, Time.deltaTime * 10f));
        anim.SetFloat("VelX", Mathf.Lerp(anim.GetFloat("VelX"), animSpeedX, Time.deltaTime * 10f));

        // --- ACTIVAR TRANSICIÓN DE AGACHADO ---
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartCoroutine(CrouchRoutine());
        }
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    IEnumerator CrouchRoutine()
    {
        isTransitioning = true;
        
        // Determinar estado objetivo
        bool currentlyCrouching = anim.GetBool("isCrouching");
        bool targetCrouch = !currentlyCrouching;
        
        // Activar animación
        anim.SetBool("isCrouching", targetCrouch);

        // Guardar valores iniciales para la interpolación (Lerp)
        Vector3 startCamPos = cameraTarget.localPosition;
        Vector3 endCamPos = new Vector3(0, targetCrouch ? crouchTargetHeight : normalTargetHeight, 0);
        
        float startHeight = controller.height;
        float endHeight = targetCrouch ? crouchHeight : normalHeight;
        
        Vector3 startCenter = controller.center;
        Vector3 endCenter = new Vector3(0, targetCrouch ? crouchCenterY : normalCenterY, 0);

        float elapsed = 0;
        while (elapsed < crouchTransitionTime)
        {
            float t = elapsed / crouchTransitionTime;
            // Curva de suavizado (SmoothStep)
            t = t * t * (3f - 2f * t);

            // Interpolar cámara
            cameraTarget.localPosition = Vector3.Lerp(startCamPos, endCamPos, t);
            
            // Interpolar cápsula de colisión
            controller.height = Mathf.Lerp(startHeight, endHeight, t);
            controller.center = Vector3.Lerp(startCenter, endCenter, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Asegurar valores finales exactos
        cameraTarget.localPosition = endCamPos;
        controller.height = endHeight;
        controller.center = endCenter;

        isTransitioning = false;
    }

    void StopMovement()
    {
        anim.SetFloat("VelY", 0);
        anim.SetFloat("VelX", 0);
        ApplyGravity();
    }
}