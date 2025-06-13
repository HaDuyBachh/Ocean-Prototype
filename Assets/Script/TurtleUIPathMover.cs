using UnityEngine;
using System.Collections.Generic;

public class TurtleUIPathMover : MonoBehaviour
{
    public RectTransform turtle;                    // Con rùa UI
    public List<RectTransform> points;              // Danh sách điểm UI
    public float moveSpeed = 200f;                  // Tốc độ di chuyển (pixel/second)
    public float rotateSpeed = 720f;                // Tốc độ xoay (degree/second)

    private int currentIndex = 0;
    private bool isMoving = false;
    private Vector2 targetPosition;

    void Start()
    {
        if (turtle != null && points.Count > 0)
        {
            targetPosition = points[0].anchoredPosition;
            isMoving = true;
        }
    }

    void Update()
    {
        if (!isMoving || currentIndex >= points.Count)
            return;

        // Di chuyển bằng Lerp
        Vector2 currentPos = turtle.anchoredPosition;
        targetPosition = points[currentIndex].anchoredPosition;

        float step = moveSpeed * Time.deltaTime;
        turtle.anchoredPosition = Vector2.MoveTowards(currentPos, targetPosition, step);

        // Tính hướng và xoay
        Vector2 direction = (targetPosition - currentPos).normalized;
        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0, 0, angle);
            turtle.rotation = Quaternion.RotateTowards(turtle.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        // Nếu đã tới target, chuyển sang điểm tiếp theo
        if (Vector2.Distance(turtle.anchoredPosition, targetPosition) < 1f)
        {
            currentIndex++;
            if (currentIndex >= points.Count)
            {
                isMoving = false;
            }
        }
    }
}
