using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;

public class CrabQuestManager : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private GameObject playerCameraFollow; // Camera của người chơi

    [Header("Object Activation")]
    [SerializeField] private List<GameObject> objectsToActivate = new List<GameObject>();

    [Header("Trash Settings")]
    [SerializeField] private GameObject trashGroup; // Object cha chứa các trash

    [Header("Timing")]
    [SerializeField] private bool isTest = false;
    [SerializeField] private bool isDone = false;
    [SerializeField] private bool complete = false;
    [SerializeField] private float timeBlend = 3.0f;

    private float brainTimeDefault;
    private CinemachineBrain camBrain;

    private List<TrashObjectController> trashObjects = new();

    void Start()
    {
        camBrain = FindObjectOfType<CinemachineBrain>();
        if (camBrain == null)
        {
            Debug.LogError("CinemachineBrain not found in the scene!");
        }

        brainTimeDefault = camBrain.m_DefaultBlend.m_Time;
    }

    public void Update()
    {
        if (isTest)
        {
            DoneQuest();
            isTest = false;
        }

        if (isDone)
        {
            isDone = false;
            var newBlend = camBrain.m_DefaultBlend;
            newBlend.m_Time = brainTimeDefault;
            camBrain.m_DefaultBlend = newBlend;

            objectsToActivate[0].SetActive(false);
        }


    }

    public void DoneQuest()
    {
        var newBlend = camBrain.m_DefaultBlend;
        newBlend.m_Time = timeBlend;
        camBrain.m_DefaultBlend = newBlend;

        objectsToActivate[0].SetActive(true);
        StartCoroutine(DelyInvoke());
    }

    IEnumerator DelyInvoke()
    {
        yield return new WaitForSeconds(1.0f);

        int half = trashObjects.Count / 2;
        for (int i = 0; i < half; i++)
        {
            if (trashObjects[i] != null)
                trashObjects[i].OnClick();
        }

        yield return new WaitForSeconds(2.0f);

        // Lấy các trash mới từ trashGroup và thêm vào nếu chưa có
        var newTrash = trashGroup.GetComponentsInChildren<TrashObjectController>();
        foreach (var trash in newTrash)
        {
            if (!trashObjects.Contains(trash))
                trashObjects.Add(trash);
        }

        for (int i = half; i < trashObjects.Count; i++)
        {
            if (trashObjects[i] != null)
                trashObjects[i].OnClick();
        }

        yield return new WaitForSeconds(3.0f);

        isDone = true;
    }
}
