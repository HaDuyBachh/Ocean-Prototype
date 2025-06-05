using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CrabController : MonoBehaviour
{
    [Header("Way Point")]
    // Danh sách các waypoint để cua di chuyển
    public List<Transform> waypoints;

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

    [Header("Attack")]
    // Layer của Player để phát hiện tấn công
    [SerializeField] private LayerMask playerLayer;

    // Phạm vi phát hiện Player để tấn công
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private bool hasDealtDamage = false;

    // Thời gian chờ ngẫu nhiên giữa các hành động
    private float waitTimer;

    [Header("State")]
    // Trạng thái hiện tại của cua
    [SerializeField] private bool isWaiting;
    [SerializeField] private bool isAttacking;

    [Header("Movement & Waypoint")]
    // Chỉ số của waypoint hiện tại
    [SerializeField] private int currentWaypointIndex;
    [SerializeField] private bool isUsingLeft = true;

    // Tốc độ xoay để căn chỉnh
    private float rotationSpeed = 6f;

    // Giá trị Speed hiện tại của Animator, để chuyển đổi mượt
    private float currentAnimSpeed;

    // Thời gian để Speed chuyển đổi mượt
    private float speedSmoothTime = 0.3f;

    // Khoảng cách để bắt đầu giảm tốc độ khi đến gần waypoint
    private float slowDownDistance = 2f;

    private float previousNormalizedTime;
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
        //// Xử lý tấn công
        if (HandleAttack()) return;

        // Xử lý trạng thái chờ
        if (HandleWaiting()) return;

        // Xử lý di chuyển và tốc độ
        HandleMovementAndSpeed();

        // Cập nhật xoay để di chuyển ngang
        UpdateSidewaysRotation();
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

        isUsingLeft = (Random.Range(1, 1000) % 2 == 0) ? true : false;

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
            Quaternion targetRotation = isUsingLeft ?
                Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(0, -90, 0) :
                Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(0, 90, 0);

            // Xoay mượt mà
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime / 3);
        }
    }

    // Vẽ phạm vi tấn công trong Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // Xử lý hành động tấn công
    private bool HandleAttack()
    {
        // Kiểm tra nếu đang tấn công
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isPlayingAttack = stateInfo.IsName("Attack");

        // Nếu đang phát animation Attack
        if (isAttacking && isPlayingAttack)
        {
            // Giảm Speed mượt mà về 0
            currentAnimSpeed = Mathf.Lerp(currentAnimSpeed, 0f, Time.deltaTime / speedSmoothTime);
            animator.SetFloat("Speed", currentAnimSpeed);

            // Xoay về phía Player
            Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, playerLayer);
            if (hits.Length > 0)
            {
                Vector3 directionToPlayer = (hits[0].transform.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Kiểm tra thời điểm 1/2 animation để gây sát thương
            if (stateInfo.normalizedTime >= 0.5f && !hasDealtDamage && hits.Length > 0)
            {
                foreach (Collider hit in hits)
                {
                    // Gây sát thương cho GameObject có HealthController
                    HealthController health = hit.GetComponentInParent<HealthController>();
                    Debug.Log("Tấn công: " + (health != null ? health.gameObject.name : "Không tìm thấy HealthController"));
                    if (health != null)
                    {
                        health.HandleDamage(10); // Gây 10 sát thương
                        break;
                    }
                }
                hasDealtDamage = true; // Đánh dấu đã gây sát thương
            }

            // Reset hasDealtDamage khi animation loop lại
            if (stateInfo.normalizedTime < previousNormalizedTime)
            {
                hasDealtDamage = false;
            }
            previousNormalizedTime = stateInfo.normalizedTime;

            // Thoát trạng thái tấn công khi animation hoàn thành
            if (stateInfo.normalizedTime >= 1.0f)
            {
                isAttacking = false;
                animator.SetBool("Attack", false);
                agent.isStopped = false;
            }
            return true;
        }

        // Kiểm tra nếu có Player trong phạm vi tấn công để bắt đầu tấn công
        Collider[] initialHits = Physics.OverlapSphere(transform.position, attackRange, playerLayer);
        if (initialHits.Length > 0)
        {
            // Chuyển sang trạng thái tấn công
            isAttacking = true;
            agent.isStopped = true;
            animator.SetBool("Attack", true);
            hasDealtDamage = false; // Reset để chuẩn bị gây sát thương
            previousNormalizedTime = 0f; // Reset thời gian animation
            return true;
        }

        // Không có Player hoặc không tấn công
        return false;
    }
}