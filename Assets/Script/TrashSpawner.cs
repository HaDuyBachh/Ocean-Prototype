using UnityEngine;
using System.Collections.Generic;

public class TrashSpawner : MonoBehaviour
{
    [SerializeField] private float spawnRadius = 5f; // Bán kính vòng tròn sinh rác
    [SerializeField] private List<GameObject> trashPrefabs; // Danh sách prefab rác
    [SerializeField] private int maxTrashCount = 10; // Số lượng rác tối đa
    [SerializeField] private GameObject parentObject; // GameObject cha
    [SerializeField] private float maxRotationX = 90f; // Giới hạn rotation X
    [SerializeField] private LayerMask groundLayer; // Layer để raycast tìm mặt đất
    [SerializeField] private float surfaceOffset = 0.1f; // Offset để tránh chìm xuống mặt đất

    private void Start()
    {
        // Đặt TrashSpawner làm con của parentObject
        if (parentObject != null)
        {
            transform.SetParent(parentObject.transform);
        }

        // Sinh toàn bộ rác ngay lập tức
        SpawnTrash();

        // Xoay GameObject sau khi sinh rác
        RotateSpawner();
    }

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
            Vector3 rayStart = circlePosition + Vector3.up * 20f; // Bắt đầu từ trên cao
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 200f, groundLayer))
            {
                // Đặt vị trí trên mặt đất, cộng offset theo pháp tuyến
                circlePosition = hit.point + hit.normal * surfaceOffset;
            }
            else
            {
                // Nếu không hit, giữ vị trí gốc (có thể điều chỉnh tùy nhu cầu)
                circlePosition = circlePosition + Vector3.up * surfaceOffset;
            }

            // Chọn ngẫu nhiên prefab rác
            GameObject trashPrefab = trashPrefabs[Random.Range(0, trashPrefabs.Count)];

            // Tạo rotation ngẫu nhiên nhưng đảm bảo transform.up không hướng xuống
            Quaternion randomRotation;
            do
            {
                randomRotation = Quaternion.Euler(
                    Random.Range(0f, 360f), // X
                    Random.Range(0f, 360f), // Y
                    Random.Range(0f, 360f)  // Z
                );
            } while (Vector3.Angle(randomRotation * Vector3.up, Vector3.up) > 90f);

            // Sinh rác tại vị trí với rotation ngẫu nhiên
            GameObject trash = Instantiate(trashPrefab, circlePosition, randomRotation);
            trash.transform.SetParent(this.transform); // Đặt rác làm con của spawner
        }
    }

    private void RotateSpawner()
    {
        // Tạo rotation X ngẫu nhiên trong khoảng [-90, 90]
        float randomX = Random.Range(-maxRotationX, maxRotationX);
        transform.rotation = Quaternion.Euler(randomX, 0, 0);
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