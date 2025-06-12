using UnityEngine;
using System.Collections;

public class FishController : MonoBehaviour
{
    [SerializeField] private float speed = 1f; // Tốc độ di chuyển của cá
    [SerializeField] private float turnSpeed = 1.2f; // Tốc độ xoay của cá
    private Vector3 moveDirection; // Hướng di chuyển hiện tại
    private FishSpawner groupController; // Tham chiếu đến GroupFishController
    private bool isMoving = true; // Trạng thái di chuyển
    private Coroutine changeDirectionCoroutine; // Coroutine đổi hướng

    private void Start()
    {
        // Lấy GroupFishController từ parent
        groupController = GetComponentInParent<FishSpawner>();
        if (groupController == null)
        {
            Debug.LogWarning($"No GroupFishController found in parent of {gameObject.name}");
        }

        // Chọn hướng và trạng thái ban đầu
        SetRandomDirectionAndState();
        // Bắt đầu Coroutine đổi hướng
        changeDirectionCoroutine = StartCoroutine(ChangeDirectionRoutine());
    }

    private void Update()
    {
        // Kiểm tra nếu ra ngoài khoảng bơi
        if (groupController != null && IsOutsideSwimArea())
        {
            // Tính hướng quay về tâm khoảng bơi
            Vector3 center = groupController.transform.position;
            Vector3 directionToCenter = (center - transform.position).normalized;
            moveDirection = directionToCenter;
            isMoving = true; // Di chuyển khi quay về
        }

        // Xoay mượt về hướng di chuyển hoặc điều chỉnh tư thế khi đứng yên
        if (isMoving && moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
        else if (!isMoving)
        {
            // Khi đứng yên, điều chỉnh transform.up hợp với Vector3.up (góc ≤ 10°)
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
            float angleToUp = Vector3.Angle(transform.up, Vector3.up);
            if (angleToUp > 10f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }

        // Di chuyển nếu đang ở trạng thái di chuyển
        if (isMoving)
        {
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
        }
    }

    private bool IsOutsideSwimArea()
    {
        if (groupController == null) return false;

        Vector3 localPos = transform.position - groupController.transform.position;
        Vector3 swimArea = groupController.SwimArea;

        // Kiểm tra nếu ngoài giới hạn hình hộp chữ nhật khối
        return Mathf.Abs(localPos.x) > swimArea.x / 2 ||
               Mathf.Abs(localPos.y) > swimArea.y / 2 ||
               Mathf.Abs(localPos.z) > swimArea.z / 2;
    }

    private void SetRandomDirectionAndState()
    {
        // Ngẫu nhiên đứng yên hoặc di chuyển
        isMoving = Random.value < 0.7f; // 70% di chuyển, 30% đứng yên
        if (isMoving)
        {
            // Chọn hướng ngẫu nhiên trong không gian 3D
            moveDirection = Random.onUnitSphere.normalized;
        }
        else
        {
            moveDirection = Vector3.zero; // Không di chuyển
            // Đảm bảo transform.up hợp với Vector3.up (góc ≤ 10°)
            float randomAngle = Random.Range(-10f, 10f);
            Vector3 randomAxis = Random.onUnitSphere;
            transform.rotation = Quaternion.AngleAxis(randomAngle, randomAxis) * Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }
    }

    private IEnumerator ChangeDirectionRoutine()
    {
        while (true)
        {
            // Đợi khoảng 5 giây (ngẫu nhiên từ 4.5 đến 5.5 để thêm biến đổi)
            yield return new WaitForSeconds(Random.Range(4.5f, 5.5f));
            // Nếu không ngoài khu vực bơi, đổi hướng và trạng thái
            if (groupController == null || !IsOutsideSwimArea())
            {
                SetRandomDirectionAndState();
            }
        }
    }
}