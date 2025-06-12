using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GroupObjectSpawner))]
public class GroupObjectSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Vẽ giao diện Inspector mặc định
        DrawDefaultInspector();

        // Lấy reference đến GroupObjectSpawner
        GroupObjectSpawner spawner = (GroupObjectSpawner)target;

        // Thêm khoảng cách
        EditorGUILayout.Space();

        if (GUILayout.Button("Add Spawn Objects"))
        {
            spawner.AddObjectSpawnList();
        }

        // Nút "Spawn All Objects"
        if (GUILayout.Button("Spawn All Objects"))
        {
            spawner.SpawnAllObjects();
        }

        // Nút "Clear All Objects"
        if (GUILayout.Button("Clear All Objects"))
        {
            spawner.ClearAllObjects();
        }
    }
}