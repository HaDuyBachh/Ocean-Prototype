using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrashSpawner))]
public class TrashSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Vẽ giao diện Inspector mặc định
        DrawDefaultInspector();

        // Lấy reference đến TrashSpawner
        TrashSpawner spawner = (TrashSpawner)target;

        // Thêm khoảng cách
        EditorGUILayout.Space();

        // Tạo nút "Spawn Trash" trong Inspector
        if (GUILayout.Button("Spawn Trash"))
        {
            spawner.SpawnTrash();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Clear Trash"))
        {
            spawner.ClearTrash();
        }    
    }
}