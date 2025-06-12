using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private float spawnRadius = 5f; // Bán kính vòng tròn sinh object
    [SerializeField] private List<GameObject> objectPrefabs; // Danh sách prefab object
    [SerializeField] private int maxObjectCount = 10; // Số lượng object tối đa
    [SerializeField] private LayerMask groundLayer; // Layer để raycast tìm mặt đất
    [SerializeField] private float surfaceOffset = 0.1f; // Offset để tránh chìm xuống mặt đất

    public void SpawnObjects()
    {
        if (objectPrefabs.Count == 0 || maxObjectCount <= 0)
            return;

        for (int i = 0; i < maxObjectCount; i++)
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
            Vector3 rayStart = circlePosition + Vector3.up * 5f; // Bắt đầu từ trên cao
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

            // Chọn ngẫu nhiên prefab object
            GameObject objectPrefab = objectPrefabs[Random.Range(0, objectPrefabs.Count)];

            // Tạo rotation: chỉ xoay quanh trục Y
            float yRotation = Random.Range(0f, 360f);
            Quaternion randomRotation = Quaternion.Euler(0, yRotation, 0);

            // Sinh object tại vị trí với rotation
            GameObject spawnedObject = Instantiate(objectPrefab, circlePosition, randomRotation);
            spawnedObject.transform.SetParent(this.transform); // Đặt object làm con của spawner
        }
    }

    // Xóa tất cả object con
    public void ClearObjects()
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

    // Hiển thị vòng tròn sinh object trong Scene View
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
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
}