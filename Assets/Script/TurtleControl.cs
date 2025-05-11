using UnityEngine;

public class TurtleController : MonoBehaviour
{
    // Components
    private Animator animator;
    private Rigidbody rb;
    private Camera mainCamera;

    // Movement settings
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float swimSpeed = 3f;
    [SerializeField] private float turnSpeed = 100f;

    // Water and fog settings
    [SerializeField] private GameObject waterPlane; // Plane làm mặt nước
    [SerializeField] private GameObject fog; // Object Fog
    [SerializeField] private float depthThreshold = 2f; // Độ sâu để kích hoạt fog

    // Rotation stabilization in water
    [SerializeField] private float stabilizeSpeed = 2f; // Tốc độ ổn định rotation trong nước

    // State
    private bool isSwimming = false;
    private Vector3 movement;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        // Ensure Rigidbody is set to non-kinematic for physics-based movement
        rb.isKinematic = false;

        // Ensure fog is initially disabled
        if (fog != null) fog.SetActive(false);

        // Validate water plane
        if (waterPlane == null)
        {
            Debug.LogError("Water Plane chưa được gán trong Inspector!");
        }
    }

    void Update()
    {
        // Input for movement
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate movement direction relative to camera
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;
        if (isSwimming)
        {
            // Dưới nước: Di chuyển theo hướng camera (bao gồm lên/xuống)
            movement = mainCamera.transform.forward * vertical + mainCamera.transform.right * horizontal;
            movement = movement.normalized;
        }
        else
        {
            // Trên bờ: Di chuyển trên mặt phẳng XZ
            Vector3 forward = mainCamera.transform.forward;
            Vector3 right = mainCamera.transform.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            movement = (forward * vertical + right * horizontal).normalized;
        }

        // Toggle swimming state based on water plane position
        if (waterPlane != null)
        {
            float waterLevel = waterPlane.transform.position.y;
            isSwimming = transform.position.y < waterLevel;
            animator.SetBool("IsSwimming", isSwimming);

            // Activate fog when below depth threshold
            if (fog != null)
            {
                fog.SetActive(isSwimming && transform.position.y < waterLevel - depthThreshold);
            }
        }

        // Play animations
        if (movement.magnitude > 0)
        {
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }

        // Lock camera above water when swimming
        if (isSwimming && mainCamera != null && waterPlane != null)
        {
            float waterLevel = waterPlane.transform.position.y;
            if (mainCamera.transform.position.y > waterLevel)
            {
                Vector3 camPos = mainCamera.transform.position;
                camPos.y = waterLevel;
                mainCamera.transform.position = camPos;
            }
        }
    }

    void FixedUpdate()
    {
        // Apply movement
        float currentSpeed = isSwimming ? swimSpeed : moveSpeed;
        rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);

        // Rotate turtle to face movement direction or stabilize in water
        if (isSwimming)
        {
            rb.useGravity = false;
            rb.drag = 2f; // Simulate water resistance

            if (movement.magnitude > 0)
            {
                // Rotate to face movement direction when swimming
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            }
            else
            {
                // Stabilize rotation to keep turtle upright when not moving
                Quaternion targetRotation = Quaternion.Euler(0, rb.rotation.eulerAngles.y, 0); // Keep Y rotation, straighten X and Z
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, stabilizeSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            rb.useGravity = true;
            rb.drag = 0.5f; // Less resistance on ground

            // Rotate to face movement direction on ground
            if (movement.magnitude > 0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            }
        }
    }
}