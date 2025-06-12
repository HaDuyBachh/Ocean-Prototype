using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CameraScreenController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera; // Camera chính của nhân vật
    [SerializeField] private InteractableObject currentTarget; // Vật thể hiện tại (đầu tiên trong danh sách)

    private TriggerCollider triggerCollider; // TriggerCollider trong con
    [SerializeField]  private List<InteractableObject> targetList = new (); // Danh sách các vật thể trong vùng

    private void Start()
    {
        // Tìm TriggerCollider trong con
        triggerCollider = GetComponentInChildren<TriggerCollider>();
        if (triggerCollider == null)
        {
            Debug.LogWarning("No TriggerCollider found in children of " + gameObject.name);
            return;
        }

        // Đăng ký listener cho OnObjectEnter và OnObjectExit
        triggerCollider.OnObjectEnter.AddListener(OnObjectEnter);
        triggerCollider.OnObjectExit.AddListener(OnObjectExit);
    }

    private void OnObjectEnter(GameObject obj)
    {
        // Tìm InteractableObject trên GameObject hoặc cha/con
        InteractableObject interactable = obj.GetComponent<InteractableObject>();
        if (interactable == null) interactable = obj.GetComponentInParent<InteractableObject>();
        if (interactable == null) interactable = obj.GetComponentInChildren<InteractableObject>();

        if (interactable != null && !targetList.Contains(interactable))
        {
            targetList.Add(interactable);
            UpdateCurrentTarget();
        }
    }

    private void OnObjectExit(GameObject obj)
    {
        // Tìm InteractableObject trên GameObject hoặc cha/con
        InteractableObject interactable = obj.GetComponent<InteractableObject>();
        if (interactable == null) interactable = obj.GetComponentInParent<InteractableObject>();
        if (interactable == null) interactable = obj.GetComponentInChildren<InteractableObject>();

        if (interactable != null && targetList.Contains(interactable))
        {
            targetList.Remove(interactable);
            if (currentTarget == interactable)
            {
                currentTarget?.OnLookExit();
                UpdateCurrentTarget();
            }
        }
    }

    private void UpdateCurrentTarget()
    {
        // Xóa các InteractableObject có GameObject bị hủy
        targetList.RemoveAll(target => target == null || target.gameObject == null);

        // Cập nhật currentTarget
        if (currentTarget == null || currentTarget.gameObject == null || !targetList.Contains(currentTarget))
        {
            if (targetList.Count > 0)
            {
                if (currentTarget != null && currentTarget.gameObject != null)
                {
                    currentTarget.OnLookExit();
                }
                currentTarget = targetList[0]; // Lấy GameObject đầu tiên
                currentTarget.OnLookEnter();
            }
            else
            {
                if (currentTarget != null && currentTarget.gameObject != null)
                {
                    currentTarget.OnLookExit();
                }
                currentTarget = null;
            }
        }
    }

    private void Update()
    {
        // Kiểm tra nếu currentTarget bị hủy
        if (targetList.Any(target => target == null || target.gameObject == null))
        {
            UpdateCurrentTarget();
        }

        // Gọi hàm OnClick khi bấm chuột trái
        if (Input.GetMouseButtonDown(0) && currentTarget != null)
        {
            currentTarget.OnClick();
        }
    }
}