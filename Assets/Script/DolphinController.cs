using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DolphinController : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 2f;
    [SerializeField] private float turnSpeed = 3f;
    [SerializeField] private float hipShakeAmplitude = 10f;
    [SerializeField] private float hipShakeDuration = 2.5f;
    public float HipShakeDuration => hipShakeDuration;

    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    private Coroutine moveCoroutine;
    private Coroutine hipShakeCoroutine;
    [SerializeField] private bool isMoving = false;
    [SerializeField] private int currentIndex = 0;

    private void Update()
    {
        if (isMoving && moveCoroutine != null && waypoints.Count > 0)
        {
            Vector3 direction = (waypoints[currentIndex].position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }
    }

    public void MoveByWaypoint(Transform waypoint)
    {
        if (waypoint == null)
        {
            Debug.LogWarning("Waypoint is null in MoveByWaypoint.");
            return;
        }

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        int index = waypoints.IndexOf(waypoint);
        if (index >= 0)
        {
            currentIndex = index;
        }
        else
        {
            Debug.LogWarning($"Waypoint {waypoint.name} không có trong danh sách waypoints.");
            return;
        }

        moveCoroutine = StartCoroutine(MoveToWaypoint(waypoint));
    }

    public void UpdateWayPoint(GameObject waypointParent, bool moveCircle)
    {
        waypoints.Clear();
        if (waypointParent != null)
        {
            Debug.Log($"WaypointParent: {waypointParent.name}, ChildCount: {waypointParent.transform.childCount}");
            for (int i = 0; i < waypointParent.transform.childCount; i++)
            {
                Transform child = waypointParent.transform.GetChild(i);
                waypoints.Add(child);
                Debug.Log($"Đang thêm: {child.name}");
            }
            Debug.Log($"Số waypoint sau UpdateWayPoint: {waypoints.Count}");
            foreach (Transform wp in waypoints)
            {
                Debug.Log($"Waypoint trong danh sách: {wp.name}");
            }
        }
        else
        {
            Debug.LogWarning("WaypointParent is null in UpdateWayPoint.");
        }

        if (waypoints.Count > 0)
        {
            currentIndex = 0;
            MoveByWaypoint(waypoints[currentIndex]);
            StartCoroutine(MoveThroughWaypoints(moveCircle));
        }
    }

    public void LookAt(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    public void HipShake()
    {
        if (hipShakeCoroutine != null)
        {
            StopCoroutine(hipShakeCoroutine);
        }

        hipShakeCoroutine = StartCoroutine(HipShakeRoutine());
    }

    private IEnumerator MoveToWaypoint(Transform waypoint)
    {
        isMoving = true;
        Debug.Log($"Di chuyển đến: {waypoint.name}");
        while (Vector3.Distance(transform.position, waypoint.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, waypoint.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = waypoint.position;
        isMoving = false;
    }

    private IEnumerator MoveThroughWaypoints(bool moveCircle)
    {
        while (waypoints.Count > 0)
        {
            yield return StartCoroutine(MoveToWaypoint(waypoints[currentIndex]));
            if (moveCircle)
            {
                currentIndex = (currentIndex + 1) % waypoints.Count;
            }
            else
            {
                currentIndex++;
                if (currentIndex >= waypoints.Count)
                {
                    break;
                }
            }
            if (waypoints.Count > 0)
            {
                MoveByWaypoint(waypoints[currentIndex]);
            }
        }
    }

    private IEnumerator HipShakeRoutine()
    {
        float elapsedTime = 0f;
        Quaternion originalRotation = transform.rotation;

        while (elapsedTime < hipShakeDuration)
        {
            float angle = Mathf.Sin(elapsedTime * Mathf.PI * 2 / hipShakeDuration) * hipShakeAmplitude;
            transform.rotation = originalRotation * Quaternion.Euler(0, angle, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = originalRotation;
    }
}