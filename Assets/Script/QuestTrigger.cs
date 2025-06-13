using UnityEngine;
using UnityEngine.Events;

public class QuestTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent onPlayerTrigger; // Sự kiện kích hoạt khi Player va chạm
    private bool hasTriggered = false; // Trạng thái đã kích hoạt

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra nếu va chạm với Player và chưa kích hoạt
        if (other.CompareTag("Player") && !hasTriggered)
        {
            TriggerQuest();
        }
    }

    // Kích hoạt UnityAction và disable GameObject
    public void TriggerQuest()
    {
        if (!hasTriggered)
        {
            hasTriggered = true;
            onPlayerTrigger?.Invoke(); // Gọi sự kiện
            gameObject.SetActive(false); // Disable GameObject
        }
    }
}