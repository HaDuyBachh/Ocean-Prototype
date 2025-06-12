using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class TurtleController : MonoBehaviour
{
    [Header("Way Point")]
    public bool isAutoGetListWayPoint = true;
    public Transform listWayPoint;
    public int waypointID = 0;

    // Danh sách các waypoint để rùa di chuyển
    [SerializeField] private List<Transform> waypoints;

    [Header("Navmesh Agent")]
    // NavMeshAgent để điều khiển di chuyển
    private NavMeshAgent agent;

    [Header("Animator")]
    // Animator để quản lý animation
    private Animator animator;

    [Header("Speed")]
    // Tốc độ đi bộ và chạy
    [SerializeField] private float walkSpeed = 1f;
    [SerializeField] private float sprintSpeed = 3f;

    [Header("State")]
    // Trạng thái hiện tại của rùa
    [SerializeField] private bool isWaiting;

    [Header("Movement & Waypoint")]
    // Chỉ số của waypoint hiện tại
    [SerializeField] private int currentWaypointIndex;

    // Giá trị motion hiện tại của Animator, để chuyển đổi mượt
    private float currentAnimMotion;

    // Thời gian để motion chuyển đổi mượt
    private float motionSmoothTime = 0.3f;

    // Khoảng cách để bắt đầu giảm tốc độ khi đến gần waypoint
    private float slowDownDistance = 2f;

    [Header("Stuck Detection")]
    // Thời gian giữa các lần kiểm tra bị kẹt
    [SerializeField] private float checkInterval = 3f;
    // Ngưỡng khoảng cách để coi là bị kẹt
    [SerializeField] private float stuckDistanceThreshold = 0.5f;
    // Vị trí ghi nhận lần trước
    private Vector3 lastCheckPosition;
    // Bộ đếm thời gian kiểm tra
    private float checkTimer;

    [Header("Ground Check")]
    // Kiểm tra rùa có trên mặt đất không
    [SerializeField] private bool grounded = true;
    // Độ lệch để kiểm tra mặt đất
    [SerializeField] private float groundedOffset = -0.14f;
    // Bán kính kiểm tra mặt đất
    [SerializeField] private float groundedRadius = 0.28f;
    // Layer mặt đất
    [SerializeField] private LayerMask groundLayers;

    [Header("Slope Handling")]
    // Khoảng cách kiểm tra sườn dốc
    [SerializeField] private float slopeCheckDistance = 0.5f;

    // Thời gian chờ ngẫu nhiên giữa các hành động
    private float waitTimer;

    void Start()
    {
        // Lấy component NavMeshAgent và Animator
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Khởi tạo các biến trạng thái
        isWaiting = false;
        waitTimer = 0f;
        currentWaypointIndex = 0;
        currentAnimMotion = 0f;

        // Khởi tạo biến kiểm tra kẹt
        checkTimer = 0f;
        lastCheckPosition = transform.position;

        // Cho phép NavMeshAgent tự quản lý xoay
        agent.updateRotation = true;

        if (isAutoGetListWayPoint) SetupWayPoint(listWayPoint);
    }

    private void SetupWayPoint(Transform listWayPoint)
    {
        waypoints.Clear();

        if (listWayPoint != null)
        {
            Transform[] childTransforms = listWayPoint.GetComponentsInChildren<Transform>();
            foreach (Transform child in childTransforms)
            {
                if (child != listWayPoint.transform && !waypoints.Contains(child))
                {
                    waypoints.Add(child);
                }
            }
        }
        else
        {
            Debug.LogWarning("Không tìm thấy ListWayPoint trong scene.");
        }
    }

    void Update()
    {
        // Xử lý trạng thái chờ
        if (HandleWaiting()) return;

        // Kiểm tra xem rùa có bị kẹt không
        CheckIfStuck();

        // Kiểm tra mặt đất và nghiêng theo sườn
        GroundedCheck();
        AlignToSlope();

        // Xử lý di chuyển và tốc độ
        HandleMovementAndSpeed();
    }

    // Kiểm tra xem rùa có bị kẹt không
    private void CheckIfStuck()
    {
        // Chỉ kiểm tra khi đang di chuyển
        if (agent.speed > 0 && !isWaiting)
        {
            // Tăng bộ đếm thời gian
            checkTimer += Time.deltaTime;

            // Kiểm tra sau mỗi checkInterval giây
            if (checkTimer >= checkInterval)
            {
                // Tính khoảng cách di chuyển từ lần kiểm tra trước
                float distanceMoved = Vector3.Distance(transform.position, lastCheckPosition);

                // Nếu di chuyển quá ít, coi là bị kẹt
                if (distanceMoved < stuckDistanceThreshold)
                {
                    // Chọn waypoint mới để thoát kẹt
                    MoveToNextWaypoint();
                }

                // Cập nhật vị trí và reset bộ đếm
                lastCheckPosition = transform.position;
                checkTimer = 0f;
            }
        }
    }

    // Xử lý trạng thái chờ
    private bool HandleWaiting()
    {
        // Nếu đang chờ
        if (isWaiting)
        {
            // Giảm thời gian chờ
            waitTimer -= Time.deltaTime;

            // Giảm motion mượt mà về 0
            currentAnimMotion = Mathf.Lerp(currentAnimMotion, 0f, Time.deltaTime / motionSmoothTime);
            animator.SetFloat("motion", currentAnimMotion);

            // Khi hết thời gian chờ, chọn hành động mới
            if (waitTimer <= 0)
            {
                isWaiting = false;
                ChooseNewAction();
            }
            return true;
        }
        return false;
    }

    // Xử lý di chuyển và tốc độ
    private void HandleMovementAndSpeed()
    {
        // Kiểm tra khoảng cách còn lại đến waypoint
        if (!agent.pathPending && agent.remainingDistance < slowDownDistance)
        {
            // Tính tỷ lệ khoảng cách để giảm tốc độ dần
            float distanceRatio = agent.remainingDistance / slowDownDistance;
            float targetSpeed = agent.speed * distanceRatio;

            // Giảm motion mượt mà dựa trên khoảng cách
            currentAnimMotion = Mathf.Lerp(currentAnimMotion, targetSpeed, Time.deltaTime / motionSmoothTime);
            animator.SetFloat("motion", currentAnimMotion);

            // Nếu đã đến gần waypoint, chuyển sang trạng thái chờ
            if (agent.remainingDistance + 0.01f <= agent.stoppingDistance)
            {
                isWaiting = true;
                waitTimer = Random.Range(1f, 2f);
            }
        }
        else
        {
            // Cập nhật motion mượt mà theo tốc độ di chuyển
            currentAnimMotion = Mathf.Lerp(currentAnimMotion, agent.speed, Time.deltaTime / motionSmoothTime);
            animator.SetFloat("Motion", currentAnimMotion);
        }
    }

    // Kiểm tra rùa có trên mặt đất không
    private void GroundedCheck()
    {
        // Tính vị trí kiểm tra mặt đất
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    // Nghiêng rùa theo sườn dốc
    private void AlignToSlope()
    {
        // Vị trí kiểm tra ngay dưới chân rùa
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        RaycastHit hit;

        // Raycast xuống dưới để lấy pháp tuyến mặt đất
        if (Physics.Raycast(origin, Vector3.down, out hit, slopeCheckDistance + 0.2f, groundLayers))
        {
            // Lấy vận tốc hiện tại của NavMeshAgent
            Vector3 moveDirection = agent.velocity;
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                // Tính hướng forward mới dựa trên vận tốc và pháp tuyến mặt đất
                Vector3 slopeForward = Vector3.Cross(transform.right, hit.normal).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(slopeForward, hit.normal);

                // Chỉ xoay theo pitch, giữ nguyên yaw
                Vector3 euler = targetRotation.eulerAngles;
                euler.y = transform.eulerAngles.y;
                targetRotation = Quaternion.Euler(euler);

                // Xoay mượt mà
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }

    // Chọn hành động mới: đứng im, đi bộ, hoặc chạy
    private void ChooseNewAction()
    {
        // Chọn hành động ngẫu nhiên
        int action = Random.Range(0, 2);
        switch (action)
        {
            //case 0: // Đứng im
            //    agent.speed = 0f;
            //    break;
            case 0: // Đi bộ
                agent.speed = walkSpeed;
                MoveToNextWaypoint();
                break;
            case 1: // Chạy
                agent.speed = sprintSpeed;
                MoveToNextWaypoint();
                break;
        }
    }

    // Di chuyển đến waypoint tiếp theo
    private void MoveToNextWaypoint()
    {
        // Nếu không có waypoint, thoát
        if (waypoints.Count == 0) return;
        if (currentWaypointIndex >= waypoints.Count) return;

        agent.SetDestination(waypoints[currentWaypointIndex++].position);
    }

    // Vẽ phạm vi kiểm tra mặt đất trong Scene view
    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = grounded ? transparentGreen : transparentRed;
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z), groundedRadius);
    }
}