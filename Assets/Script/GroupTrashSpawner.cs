using UnityEngine;
using System.Collections.Generic;

public class GroupTrashSpawner : MonoBehaviour
{
    [SerializeField] private List<TrashSpawner> trashSpawners = new List<TrashSpawner>();

    public void AddTrashSpawnList()
    {
        trashSpawners.AddRange(GetComponentsInChildren<TrashSpawner>());
        if (trashSpawners.Count == 0)
        {
            Debug.LogWarning("No TrashSpawner components found in children of " + gameObject.name);
        }
    }    

    public void SpawnAllTrash()
    {
        foreach (TrashSpawner spawner in trashSpawners)
        {
            if (spawner != null)
            {
                spawner.SpawnTrash();
            }
        }
    }

    public void ClearAllTrash()
    {
        foreach (TrashSpawner spawner in trashSpawners)
        {
            if (spawner != null)
            {
                spawner.ClearTrash();
            }
        }
    }
}