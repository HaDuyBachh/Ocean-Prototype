using UnityEngine;
using System.Collections.Generic;

public class GroupObjectSpawner : MonoBehaviour
{
    [SerializeField] private List<ObjectSpawner> objectSpawners = new List<ObjectSpawner>();

    public void AddObjectSpawnList()
    {
        // Tìm tất cả ObjectSpawner trong con
        objectSpawners.AddRange(GetComponentsInChildren<ObjectSpawner>());
        if (objectSpawners.Count == 0)
        {
            Debug.LogWarning("No ObjectSpawner components found in children of " + gameObject.name);
        }
    }

    public void SpawnAllObjects()
    {
        foreach (ObjectSpawner spawner in objectSpawners)
        {
            if (spawner != null)
            {
                spawner.SpawnObjects();
            }
        }
    }

    public void ClearAllObjects()
    {
        foreach (ObjectSpawner spawner in objectSpawners)
        {
            if (spawner != null)
            {
                spawner.ClearObjects();
            }
        }
    }
}