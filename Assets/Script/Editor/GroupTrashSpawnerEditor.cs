using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GroupTrashSpawner))]
public class GroupTrashSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Vẽ giao diện Inspector mặc định
        DrawDefaultInspector();

        // Lấy reference đến GroupTrashSpawner
        GroupTrashSpawner spawner = (GroupTrashSpawner)target;

        // Thêm khoảng cách
        EditorGUILayout.Space();

        // Nút "Spawn All Trash"
        if (GUILayout.Button("Add Trash Spawn"))
        {
            spawner.AddTrashSpawnList();
        }

        // Nút "Spawn All Trash"
        if (GUILayout.Button("Spawn All Trash"))
        {
            spawner.SpawnAllTrash();
        }

        // Nút "Clear All Trash"
        if (GUILayout.Button("Clear All Trash"))
        {
            spawner.ClearAllTrash();
        }
    }
}