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
    [SerializeField] private bool isMoving = false;
    [SerializeField] private int currentIndex = 0;
    [SerializeField] private bool moveCircle = false;

    private Coroutine moveCoroutine;
    private Coroutine moveThroughWaypointsCoroutine;
    private Coroutine hipShakeCoroutine;

    private void Update()
    {
        HandleMove();
    }

    private void HandleMove()
    {
        if (!isMoving || waypoints.Count == 0 || currentIndex >= waypoints.Count)
            return;

        Transform target = waypoints[currentIndex];
        Vector3 direction = (target.position - transform.position).normalized;

        // Di chuyển từng bước
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // Xoay về hướng di chuyển
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        // Đến gần đích -> chuyển waypoint tiếp theo
        if (Vector3.Distance(transform.position, target.position) <= 0.05f)
        {
            if (moveCircle)
            {
                currentIndex = (currentIndex + 1) % waypoints.Count;
            }
            else
            {
                currentIndex++;
                if (currentIndex >= waypoints.Count)
                {
                    isMoving = false;
                }
            }
        }
    }


    public void UpdateWayPoint(GameObject waypointParent, bool moveCircle)
    {
        Debug.Log("Update waypoint: " + moveCircle);

        waypoints.Clear();
        if (waypointParent != null)
        {
            for (int i = 0; i < waypointParent.transform.childCount; i++)
            {
                waypoints.Add(waypointParent.transform.GetChild(i));
            }
        }

        if (waypoints.Count == 0)
        {
            Debug.LogWarning("Không có waypoint nào được thêm!");
            return;
        }

        this.moveCircle = moveCircle;
        currentIndex = 0;
        isMoving = true;
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
