using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FishSpawner))]
public class FishSpawnerEditro : Editor
{
    public override void OnInspectorGUI()
    {
        // Vẽ giao diện Inspector mặc định
        DrawDefaultInspector();

        // Lấy reference đến GroupFishController
        FishSpawner spawner = (FishSpawner)target;

        // Thêm khoảng cách
        EditorGUILayout.Space();

        // Nút "Spawn Fish"
        if (GUILayout.Button("Spawn Fish"))
        {
            spawner.SpawnFish();
        }

        // Nút "Clear Fish"
        if (GUILayout.Button("Clear Fish"))
        {
            spawner.ClearFish();
        }
    }
}