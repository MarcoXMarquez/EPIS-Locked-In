using UnityEngine;
using System.Collections;

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

    [Header("Suavizado de Rotación (Fix Tirones)")]
    public float rotationSmoothTime = 0.05f; // Tiempo de suavizado para eliminar saltos
    private float currentRotationVelocity;
    private float horizontalRotation;

    [Header("Ajustes de Transición")]
    public float crouchTransitionTime = 0.5f; 
    private bool isTransitioning = false;

    private CharacterController controller;
    private Animator anim;
    private InventoryManager inventory;
    private Vector3 velocity; 
    private Transform cam;

    [Header("Ajustes de Cámara Vertical")]
    public float minPitch = -20f; 
    public float maxPitch = 45f;  
    private float verticalRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        cam = Camera.main.transform;
        
        inventory = Object.FindAnyObjectByType<InventoryManager>();

        // Inicializar la rotación horizontal con la actual del personaje
        horizontalRotation = transform.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        ShikakuManager shikaku = Object.FindAnyObjectByType<ShikakuManager>();

        // SEGURIDAD: Bloqueo si el inventario O el puzzle están abiertos
        if ((inventory != null && inventory.fullMenuOverlay.activeSelf) || 
            (shikaku != null && shikaku.isPuzzleActive) ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Action_PickUp"))
        {
            StopMovement();
            return;
        }

        if (isTransitioning) { ApplyGravity(); return; }
        Move();
    }

    void Move()
    {
        // --- ROTACIÓN CON EL MOUSE (CORREGIDA PARA TIRONES) ---
        if (!(inventory != null && inventory.fullMenuOverlay.activeSelf) && !isTransitioning)
        {
            // Usamos GetAxisRaw para obtener el movimiento puro del mouse sin suavizado de Unity
            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Rotación Horizontal: Calculamos el destino y suavizamos el ángulo
            horizontalRotation += mouseX;
            float smoothedYAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, horizontalRotation, ref currentRotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothedYAngle, 0f);

            // Rotación Vertical: Se mantiene igual pero con GetAxisRaw para consistencia
            verticalRotation -= mouseY; 
            verticalRotation = Mathf.Clamp(verticalRotation, minPitch, maxPitch);
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
        bool currentlyCrouching = anim.GetBool("isCrouching");
        bool targetCrouch = !currentlyCrouching;
        anim.SetBool("isCrouching", targetCrouch);

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
            t = t * t * (3f - 2f * t);

            cameraTarget.localPosition = Vector3.Lerp(startCamPos, endCamPos, t);
            controller.height = Mathf.Lerp(startHeight, endHeight, t);
            controller.center = Vector3.Lerp(startCenter, endCenter, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

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