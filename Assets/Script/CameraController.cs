using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target; // Target (rùa)
    [SerializeField] private float distance = 10f; // Khoảng cách từ camera đến rùa
    [SerializeField] private float minDistance = 2f; // Khoảng cách tối thiểu
    [SerializeField] private float maxDistance = 20f; // Khoảng cách tối đa
    [SerializeField] private float zoomSpeed = 30f; // Tốc độ zoom
    [SerializeField] private float rotationSpeed = 100f; // Tốc độ xoay
    [SerializeField] private float verticalAngleMin = -30f; // Góc dọc tối thiểu
    [SerializeField] private float verticalAngleMax = 80f; // Góc dọc tối đa
    [SerializeField] private float positionSmoothTime = 0.2f; // Thời gian làm mượt vị trí
    [SerializeField] private float targetSmoothTime = 0.1f; // Thời gian làm mượt vị trí rùa
    [SerializeField] private LayerMask obstacleLayerMask; // Layer của các vật cản
    [SerializeField] private float collisionOffset = 0.5f; // Khoảng cách an toàn từ vật cản

    private float currentX = 0f; // Góc ngang hiện tại
    private float currentY = 30f; // Góc dọc hiện tại
    private Vector3 velocity = Vector3.zero; // Vận tốc làm mượt camera
    private Vector3 targetVelocity = Vector3.zero; // Vận tốc làm mượt vị trí rùa
    private Vector3 smoothedTargetPos; // Vị trí rùa đã làm mượt
    private float currentDistance; // Khoảng cách hiện tại (đã điều chỉnh)

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Target (rùa) chưa được gán trong Inspector!");
            enabled = false;
            return;
        }

        // Ẩn và khóa con trỏ chuột khi bắt đầu game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Khởi tạo khoảng cách và vị trí rùa
        currentDistance = distance;
        smoothedTargetPos = target.position;
    }

    void Update()
    {
        // Bật/tắt con trỏ chuột khi nhấn Esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Làm mượt vị trí của rùa
        smoothedTargetPos = Vector3.SmoothDamp(smoothedTargetPos, target.position, ref targetVelocity, targetSmoothTime);

        // Input chuột để xoay
        currentX += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        currentY -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
        currentY = Mathf.Clamp(currentY, verticalAngleMin, verticalAngleMax);

        // Zoom bằng scroll wheel
        float zoomInput = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime;
        float newDistance = currentDistance - zoomInput;
        newDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);

        // Tính toán vị trí camera
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = rotation * Vector3.back;
        Vector3 desiredPosition = smoothedTargetPos + direction * newDistance;

        // Kiểm tra va chạm với vật cản
        Ray ray = new Ray(smoothedTargetPos, direction);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, newDistance, obstacleLayerMask))
        {
            // Bỏ qua nếu va chạm với rùa
            if (hit.transform != target)
            {
                float hitDistance = hit.distance - collisionOffset;
                currentDistance = Mathf.Clamp(hitDistance, minDistance, newDistance);
                desiredPosition = smoothedTargetPos + direction * currentDistance;
                Debug.Log($"Camera va chạm với: {hit.transform.name}, khoảng cách: {currentDistance}");
            }
            else
            {
                currentDistance = newDistance;
            }
        }
        else
        {
            currentDistance = newDistance;
        }

        // Làm mượt vị trí camera
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, positionSmoothTime);

        // Camera luôn nhìn vào vị trí rùa đã làm mượt
        transform.LookAt(smoothedTargetPos);
    }
}