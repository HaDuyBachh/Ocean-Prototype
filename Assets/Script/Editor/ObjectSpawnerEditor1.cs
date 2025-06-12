using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectSpawner))]
public class ObjectSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Vẽ giao diện Inspector mặc định
        DrawDefaultInspector();

        // Lấy reference đến ObjectSpawner
        ObjectSpawner spawner = (ObjectSpawner)target;

        // Thêm khoảng cách
        EditorGUILayout.Space();

        // Nút "Spawn Objects"
        if (GUILayout.Button("Spawn Objects"))
        {
            spawner.SpawnObjects();
        }

        // Nút "Clear Objects"
        if (GUILayout.Button("Clear Objects"))
        {
            spawner.ClearObjects();
        }
    }
}