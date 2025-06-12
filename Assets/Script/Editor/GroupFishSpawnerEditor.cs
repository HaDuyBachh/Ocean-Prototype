using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GroupFishSpawner))]
public class GroupFishSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Vẽ giao diện Inspector mặc định
        DrawDefaultInspector();

        // Lấy reference đến GroupFishSpawner
        GroupFishSpawner spawner = (GroupFishSpawner)target;

        // Thêm khoảng cách
        EditorGUILayout.Space();

        if (GUILayout.Button("Add Spawn Fish"))
        {
            spawner.AddSpawnFish();
        }

        // Nút "Spawn All Fish"
        if (GUILayout.Button("Spawn All Fish"))
        {
            spawner.SpawnAllFish();
        }

        // Nút "Clear All Fish"
        if (GUILayout.Button("Clear All Fish"))
        {
            spawner.ClearAllFish();
        }
    }
}