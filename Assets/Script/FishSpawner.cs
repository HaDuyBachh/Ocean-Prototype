using UnityEngine;
using System.Collections.Generic;

public class FishSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> fishPrefabs; // Danh sách prefab cá
    [SerializeField] private int maxFishCount = 10; // Số lượng cá tối đa
    [SerializeField] private Vector3 swimArea = new Vector3(10f, 5f, 10f); // Kích thước khoảng bơi (hình hộp chữ nhật khối)
    [SerializeField] private float spawnRadius = 5f; // Bán kính sinh cá

    private List<GameObject> spawnedFish = new List<GameObject>(); // Danh sách cá đã sinh

    public Vector3 SwimArea => swimArea; // Getter cho khoảng bơi
    public void SpawnFish()
    {
        if (fishPrefabs.Count == 0 || maxFishCount <= 0)
            return;

        for (int i = 0; i < maxFishCount; i++)
        {
            // Chọn điểm ngẫu nhiên trong vòng tròn XZ
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(0f, spawnRadius);
            Vector3 spawnPosition = transform.position + new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                Random.Range(-swimArea.y / 2, swimArea.y / 2), // Ngẫu nhiên theo trục Y trong swimArea
                Mathf.Sin(angle * Mathf.Deg2Rad) * distance
            );

            // Chọn prefab cá ngẫu nhiên
            GameObject fishPrefab = fishPrefabs[Random.Range(0, fishPrefabs.Count)];

            // Sinh cá với rotation ngẫu nhiên quanh trục Y
            Quaternion spawnRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            GameObject fish = Instantiate(fishPrefab, spawnPosition, spawnRotation);
            fish.transform.SetParent(this.transform); // Đặt cá làm con của spawner
            spawnedFish.Add(fish);
        }
    }

    public void ClearFish()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child != null)
            {
                DestroyImmediate(child); // Xóa trong Editor
            }
        }
        spawnedFish.Clear();
    }

    // Hiển thị khoảng bơi trong Scene View
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, swimArea);
    }
}