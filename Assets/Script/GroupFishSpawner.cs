using UnityEngine;
using System.Collections.Generic;

public class GroupFishSpawner : MonoBehaviour
{
    [SerializeField] private List<FishSpawner> fishSpawners = new List<FishSpawner>();

    public void AddSpawnFish()
    {
        // Tìm tất cả FishSpawner trong con
        fishSpawners.AddRange(GetComponentsInChildren<FishSpawner>());
        if (fishSpawners.Count == 0)
        {
            Debug.LogWarning("No FishSpawner components found in children of " + gameObject.name);
        }
    }

    public void SpawnAllFish()
    {
        foreach (FishSpawner spawner in fishSpawners)
        {
            if (spawner != null)
            {
                spawner.SpawnFish();
            }
        }
    }

    public void ClearAllFish()
    {
        foreach (FishSpawner spawner in fishSpawners)
        {
            if (spawner != null)
            {
                spawner.ClearFish();
            }
        }
    }
}