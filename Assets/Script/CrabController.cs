using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CrabController : MonoBehaviour
{
    // Danh sách các waypoint để cua di chuyển
    public List<Transform> waypoints;

    // NavMeshAgent để điều khiển di chuyển
    private NavMeshAgent agent;

    // Animator để quản lý animation
    private Animator animator;

    // Tốc độ đi bộ và chạy
    private float walkSpeed = 1f;
    private float sprintSpeed = 3f;

    // Layer của Player để phát hiện tấn công
    public LayerMask playerLayer;

    // Phạm vi phát hiện Player để tấn công
    public float attackRange = 2f;

    // Thời gian chờ ngẫu nhiên giữa các hành động
    private float waitTime;
    private float waitTimer;

    // Trạng thái hiện tại của cua
    private bool isWaiting;
    private bool isAttacking;

    // Chỉ số của waypoint hiện tại
    private int currentWaypointIndex;

    // Tốc độ xoay để căn chỉnh transform.left
    private float rotationSpeed = 12f; // Độ/giây

    // Giá trị Speed hiện tại của Animator, để chuyển đổi mượt
    private float currentAnimSpeed;

    // Thời gian để Speed chuyển đổi mượt
    private float speedSmoothTime = 0.3f;

    // Khoảng cách để bắt đầu giảm tốc độ khi đến gần waypoint
    private float slowDownDistance = 2f;

    void Start()
    {
        // Lấy component NavMeshAgent và Animator
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Khởi tạo các biến trạng thái
        isWaiting = false;
        isAttacking = false;
        waitTimer = 0f;
        currentWaypointIndex = 0;
        currentAnimSpeed = 0f;

        // Tắt auto rotation để tự quản lý xoay
        agent.updateRotation = false;

        // Nếu có waypoint, đặt đích đến là waypoint đầu tiên
        if (waypoints.Count > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    void Update()
    {
        // Xử lý tấn công
        if (HandleAttack()) return;

        // Xử lý trạng thái chờ
        if (HandleWaiting()) return;

        // Xử lý di chuyển và tốc độ
        HandleMovementAndSpeed();

        // Cập nhật xoay để di chuyển ngang
        UpdateSidewaysRotation();
    }

    // Xử lý hành động tấn công
    private bool HandleAttack()
    {
        // Kiểm tra nếu có Player trong phạm vi tấn công
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, playerLayer);
        if (hits.Length > 0)
        {
            // Chuyển sang trạng thái tấn công
            isAttacking = true;
            agent.isStopped = true;

            Debug.Log("Đang tấn công ở đây");

            // Tính hướng đến Player
            Vector3 directionToPlayer = (hits[0].transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);

            // Xoay mượt mà về phía Player
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Giảm Speed mượt mà về 0
            currentAnimSpeed = Mathf.Lerp(currentAnimSpeed, 0f, Time.deltaTime / speedSmoothTime);
            animator.SetFloat("Speed", currentAnimSpeed);

            // Chỉ tấn công khi đã xoay gần đúng hướng

            Debug.Log("Góc tấn công là:" + Quaternion.Angle(transform.rotation, targetRotation));

            if (Quaternion.Angle(transform.rotation, targetRotation) < 10f)
            {
                animator.SetBool("Attack", true);
            }
            return true;
        }

        // Thoát trạng thái tấn công
        isAttacking = false;
        animator.SetBool("Attack", false);
        agent.isStopped = false;
        return false;
    }

    // Xử lý trạng thái chờ
    private bool HandleWaiting()
    {
        // Nếu đang chờ
        if (isWaiting)
        {
            // Giảm thời gian chờ
            waitTimer -= Time.deltaTime;

            // Giảm Speed mượt mà về 0
            currentAnimSpeed = Mathf.Lerp(currentAnimSpeed, 0f, Time.deltaTime / speedSmoothTime);
            animator.SetFloat("Speed", currentAnimSpeed);

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

            // Giảm Speed mượt mà dựa trên khoảng cách
            currentAnimSpeed = Mathf.Lerp(currentAnimSpeed, targetSpeed, Time.deltaTime / speedSmoothTime);
            animator.SetFloat("Speed", currentAnimSpeed);

            // Nếu đã đến gần waypoint, chuyển sang trạng thái chờ
            if (agent.remainingDistance < 0.5f)
            {
                isWaiting = true;
                waitTimer = Random.Range(1f, 3f);
            }
        }
        else
        {
            // Cập nhật Speed mượt mà theo tốc độ di chuyển
            currentAnimSpeed = Mathf.Lerp(currentAnimSpeed, agent.speed, Time.deltaTime / speedSmoothTime);
            animator.SetFloat("Speed", currentAnimSpeed);
        }
    }

    // Chọn hành động mới: đứng im, đi bộ, hoặc chạy
    private void ChooseNewAction()
    {
        // Chọn hành động ngẫu nhiên
        int action = Random.Range(0, 3);
        switch (action)
        {
            case 0: // Đứng im
                agent.speed = 0f;
                break;
            case 1: // Đi bộ
                agent.speed = walkSpeed;
                MoveToNextWaypoint();
                break;
            case 2: // Chạy
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

        // Chọn waypoint ngẫu nhiên
        currentWaypointIndex = Random.Range(0, waypoints.Count);
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    // Cập nhật xoay để di chuyển theo transform.left
    private void UpdateSidewaysRotation()
    {
        // Chỉ xoay khi có đường đi và không chờ/tấn công
        if (agent.hasPath && !isWaiting && !isAttacking)
        {
            // Tính hướng đến waypoint
            Vector3 direction = (agent.steeringTarget - transform.position).normalized;

            // Xoay để transform.left hướng về waypoint
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(0, -90, 0);

            // Xoay mượt mà
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime / 180f);
        }
    }

    // Vẽ phạm vi tấn công trong Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}