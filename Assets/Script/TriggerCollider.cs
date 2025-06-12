using UnityEngine;
using UnityEngine.Events;

public class TriggerCollider : MonoBehaviour
{
    [SerializeField] private LayerMask triggerLayer; // Layer của các GameObject cần va chạm
    [SerializeField] private UnityEvent<GameObject> onObjectEnter; // Sự kiện khi va chạm
    [SerializeField] private UnityEvent<GameObject> onObjectExit; // Sự kiện khi thoát ra

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra layer của GameObject va chạm
        if (((1 << other.gameObject.layer) & triggerLayer) != 0)
        {
            onObjectEnter?.Invoke(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Kiểm tra layer của GameObject thoát ra
        if (((1 << other.gameObject.layer) & triggerLayer) != 0)
        {
            onObjectExit?.Invoke(other.gameObject);
        }
    }

    // Getter để truy cập UnityEvent từ code
    public UnityEvent<GameObject> OnObjectEnter => onObjectEnter;
    public UnityEvent<GameObject> OnObjectExit => onObjectExit;
}