using UnityEngine;
using System.Collections.Generic;

public class TrashSpawner : MonoBehaviour
{
    [SerializeField] private float spawnRadius = 5f; // Bán kính vòng tròn sinh rác
    [SerializeField] private List<GameObject> trashPrefabs; // Danh sách prefab rác
    [SerializeField] private int maxTrashCount = 10; // Số lượng rác tối đa
    [SerializeField] private float maxRotationX = 90f; // Giới hạn rotation X
    [SerializeField] private LayerMask groundLayer; // Layer để raycast tìm mặt đất
    [SerializeField] private float surfaceOffset = 0.1f; // Offset để tránh chìm xuống mặt đất

    [Header("Offset Đẩy")]
    [SerializeField] private float upAdjustmentOffset = 0.2f; // Offset đẩy lên nếu transform.up hướng 

    [Header("Độ nghiêng")]
    [SerializeField] private float tiltAngleMin = 75.0f;
    [SerializeField] private float tiltAngleMax = 90.0f;

    public void SpawnTrash()
    {
        if (trashPrefabs.Count == 0 || maxTrashCount <= 0)
            return;

        for (int i = 0; i < maxTrashCount; i++)
        {
            // Chọn điểm ngẫu nhiên trong vòng tròn (mặt phẳng XZ)
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(0f, spawnRadius);
            Vector3 circlePosition = transform.position + new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                0,
                Mathf.Sin(angle * Mathf.Deg2Rad) * distance
            );

            // Chiếu tia xuống mặt đất để lấy vị trí
            RaycastHit hit;
            Vector3 rayStart = circlePosition + Vector3.up * 100f; // Bắt đầu từ trên cao
            Vector3 normal = Vector3.up; // Mặc định pháp tuyến
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 200f, groundLayer))
            {
                // Đặt vị trí trên mặt đất, cộng offset theo pháp tuyến
                circlePosition = hit.point + hit.normal * surfaceOffset;
                normal = hit.normal; // Lưu pháp tuyến mặt đất
            }
            else
            {
                // Nếu không hit, giữ vị trí gốc với offset
                circlePosition = circlePosition + Vector3.up * surfaceOffset;
            }

            // Chọn ngẫu nhiên prefab rác
            GameObject trashPrefab = trashPrefabs[Random.Range(0, trashPrefabs.Count)];

            // Tạo rotation: transform.up hợp với Vector3.up góc tiltAngleMin đến tiltAngleMax, xoay Y ngẫu nhiên
            float tiltAngle = Random.Range(tiltAngleMin, tiltAngleMax); // Góc nghiêng 
            float yRotation = Random.Range(0f, 360f); // Xoay Y ngẫu nhiên
            Vector3 randomAxis = Random.onUnitSphere; // Trục ngẫu nhiên để nghiêng
            Quaternion tiltRotation = Quaternion.AngleAxis(tiltAngle, randomAxis); // Nghiêng ngẫu nhiên
            Quaternion yRotationQuat = Quaternion.Euler(0, yRotation, 0); // Xoay quanh Y
            Quaternion randomRotation = yRotationQuat * tiltRotation;

            // Kiểm tra transform.up
            Vector3 upDirection = randomRotation * Vector3.up;
            if (Vector3.Angle(upDirection, Vector3.up) > 90f)
            {
                // Nếu transform.up hướng xuống, đẩy lên và điều chỉnh rotation
                circlePosition += normal * upAdjustmentOffset;
                randomRotation = Quaternion.Euler(0, yRotation, 0); // Fallback: chỉ xoay Y
            }

            // Sinh rác tại vị trí với rotation
            GameObject trash = Instantiate(trashPrefab, circlePosition, randomRotation);
            trash.transform.SetParent(this.transform); // Đặt rác làm con của spawner
        }
    }

    // Hiển thị vòng tròn sinh rác trong Scene View
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        int segments = 36;
        Vector3 lastPoint = Vector3.zero;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * 360f / segments;
            Vector3 point = transform.position + new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius,
                0,
                Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius
            );
            if (i > 0)
            {
                Gizmos.DrawLine(lastPoint, point);
            }
            lastPoint = point;
        }
    }

    public void ClearTrash()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child != null)
            {
                DestroyImmediate(child); // Dùng DestroyImmediate trong Editor
            }
        }
    }
}