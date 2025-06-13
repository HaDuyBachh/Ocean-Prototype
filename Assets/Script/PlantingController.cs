using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class PlantingController : MonoBehaviour
{
    [Header("Plant Settings")]
    [SerializeField] private List<GameObject> plantPrefabs;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float surfaceOffset = 0.1f;

    [Header("Planting Event")]
    public UnityEvent OnPlanting;

    [SerializeField] private bool isPlayerInside = false;

    private void IsPlayerInside(bool state)
    {
        isPlayerInside = state;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }

    private void Update()
    {
        if (!isPlayerInside) return;

        if (Input.GetMouseButtonDown(1)) // Right click
        {
            PlantAtGround();
        }
    }

    private void PlantAtGround()
    {
        if (plantPrefabs == null || plantPrefabs.Count == 0)
            return;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, groundLayer.value))
        {
            Debug.DrawLine(ray.origin,hit.point,Color.yellow, 20.0f);

            Vector3 spawnPos = hit.point + hit.normal * surfaceOffset;
            Quaternion spawnRot = Quaternion.FromToRotation(Vector3.up, hit.normal);

            GameObject plantPrefab = plantPrefabs[Random.Range(0, plantPrefabs.Count)];
            Instantiate(plantPrefab, spawnPos, spawnRot, transform);

            OnPlanting?.Invoke();
        }
    }
}
