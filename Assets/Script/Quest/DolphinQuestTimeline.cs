using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DolphinQuestTimeline : MonoBehaviour
{
    [SerializeField] private DolphinController rescuerDolphin; // Cá heo cứu hộ
    [SerializeField] private DolphinController victimDolphin; // Cá heo cần cứu
    [SerializeField] private List<GameObject> waypointParents; // Danh sách GameObject chứa waypoint cho từng phase
    [SerializeField] private float hipShakeInterval = 5f; // Khoảng thời gian giữa các HipShake của rescuer

    private bool isQuestActive = false; // Trạng thái nhiệm vụ
    [SerializeField] private int currentPhase = 0; // Phase hiện tại

    private void Start()
    {
        PreQuestBehavior();
    }

    public void PreQuestBehavior()
    {
        StartPhase(0);
    }    

    // Bắt đầu nhiệm vụ
    public void StartQuest()
    {
        if (rescuerDolphin == null || victimDolphin == null)
        {
            Debug.LogWarning("Rescuer or Victim DolphinController is not assigned.");
            return;
        }

        if (waypointParents == null || waypointParents.Count == 0)
        {
            Debug.LogWarning("WaypointParents list is empty or not assigned.");
            return;
        }

        isQuestActive = true;
        currentPhase = 1;

        // Phase 0: Gắn waypoint đầu tiên cho rescuerDolphin
        StartPhase(1);

        // Khởi động hành vi cho cá heo cần cứu
        //StartCoroutine(VictimRoutine());
    }

    // Bắt đầu một phase cụ thể
    public void StartPhase(int phaseIndex)
    {
        if (phaseIndex >= 0 && phaseIndex < waypointParents.Count && waypointParents[phaseIndex] != null)
        {
            bool moveCircle = true;
            if (phaseIndex == 1) moveCircle = false;
            if (phaseIndex == 2)
            {
                StopCoroutine(nameof(VictimRoutine));
                StartCoroutine(VictimRoutine());
                moveCircle = false;
                rescuerDolphin.moveSpeed = 1.8f;
            }

            currentPhase = phaseIndex;
            rescuerDolphin.UpdateWayPoint(waypointParents[phaseIndex], moveCircle);
        }
    }

    // Dừng nhiệm vụ
    public void StopQuest()
    {
        isQuestActive = false;
        StopAllCoroutines();

        // Dừng hành vi của cả hai cá heo
        if (rescuerDolphin != null)
        {
            rescuerDolphin.HipShake(); // HipShake lần cuối
        }
        if (victimDolphin != null)
        {
            victimDolphin.HipShake(); // HipShake lần cuối
        }
    }

    private IEnumerator RescuerRoutine()
    {
        while (isQuestActive)
        {
            // Thi thoảng gọi HipShake
            rescuerDolphin.HipShake();
            yield return new WaitForSeconds(hipShakeInterval + Random.Range(-1f, 1f)); // Ngẫu nhiên ±1 giây
        }
    }

    private IEnumerator VictimRoutine()
    {
        while (isQuestActive)
        {
            // Liên tục gọi HipShake để giãy giụa
            victimDolphin.HipShake();
            yield return new WaitForSeconds(victimDolphin.HipShakeDuration); // Đợi hết HipShake
        }
    }
}