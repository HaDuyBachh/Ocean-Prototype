using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CrabController : MonoBehaviour
{
    // Danh sách các waypoint
    public List<Transform> waypoints;

    // NavMeshAgent để điều khiển di chuyển
    private NavMeshAgent agent;

    // Animator để quản lý animation
    private Animator animator;

    // Các thông số tốc độ
    private float walkSpeed = 3f;
    private float sprintSpeed = 5f;

    // Layer của Player
    public LayerMask playerLayer;

    // Phạm vi phát hiện player để tấn công
    public float attackRange = 2f;

    // Thời gian chờ ngẫu nhiên giữa các hành động
    private float waitTime;
    private float waitTimer;

    // Trạng thái hiện tại
    private bool isWaiting;
    private bool isAttacking;

    // Waypoint hiện tại
    private int currentWaypointIndex;

    void Start()
    {
        // Lấy component NavMeshAgent và Animator
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Khởi tạo
        isWaiting = false;
        isAttacking = false;
        waitTimer = 0f;
        currentWaypointIndex = 0;

        // Chọn waypoint đầu tiên
        if (waypoints.Count > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    void Update()
    {
        // Kiểm tra nếu đang tấn công
        if (CheckForPlayer())
        {
            isAttacking = true;
            agent.isStopped = true;
            animator.SetFloat("Speed", 0f);
            animator.SetBool("Attack", true);
            return;
        }
        else
        {
            isAttacking = false;
            animator.SetBool("Attack", false);
            agent.isStopped = false;
        }

        // Nếu đang chờ
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            animator.SetFloat("Speed", 0f);
            if (waitTimer <= 0)
            {
                isWaiting = false;
                ChooseNewAction();
            }
            return;
        }

        // Kiểm tra nếu cua đến gần waypoint
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // Chọn hành động ngẫu nhiên
            isWaiting = true;
            waitTimer = Random.Range(1f, 3f); // Chờ 1-3 giây
        }

        // Cập nhật tốc độ cho animator
        animator.SetFloat("Speed", agent.speed);
    }

    // Kiểm tra nếu có player trong phạm vi tấn công
    private bool CheckForPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, playerLayer);
        return hits.Length > 0;
    }

    // Chọn hành động mới (đi bộ, chạy hoặc đứng im)
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
        if (waypoints.Count == 0) return;

        // Chọn waypoint ngẫu nhiên
        currentWaypointIndex = Random.Range(0, waypoints.Count);
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    // Vẽ phạm vi tấn công trong editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}