using UnityEngine;

public class CameraScreenController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera; // Camera chính của nhân vật
    [SerializeField] private float rayDistance = 5f; // Khoảng cách raycast
    [SerializeField] private LayerMask interactableLayer; // Layer của vật thể tương tác
    [SerializeField] private float circleRadius = 100f; // Bán kính hình tròn trên màn hình (pixels)

    [SerializeField] private InteractableObject currentTarget; // Vật thể đang được chọn

    private void Update()
    {
        // Raycast từ tâm camera
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        RaycastHit hit;
        InteractableObject raycastTarget = null;

        // Kiểm tra raycast và vẽ đường ray
        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
        {
            raycastTarget = hit.collider.GetComponent<InteractableObject>();
            if (raycastTarget == null) 
                raycastTarget = hit.collider.GetComponentInParent<InteractableObject>();
            else
                raycastTarget = hit.collider.GetComponentInChildren<InteractableObject>();

            // Vẽ raycast đến điểm va chạm
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green, 0.1f);
        }
        else
        {
            // Vẽ raycast đến khoảng cách tối đa nếu không trúng
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 0.1f);
        }

        // Kiểm tra vật thể hiện tại có còn trong khoảng hình tròn không
        bool keepCurrentTarget = false;
        if (currentTarget != null)
        {
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(currentTarget.transform.position);
            float distanceFromCenter = Vector2.Distance(screenPoint, new Vector2(Screen.width / 2f, Screen.height / 2f));
            if (distanceFromCenter <= circleRadius && screenPoint.z > 0)
            {
                keepCurrentTarget = true; // Giữ vật thể hiện tại nếu trong vòng tròn
            }
        }

        // Chọn vật thể mới nếu không giữ vật thể hiện tại
        InteractableObject newTarget = null;
        if (!keepCurrentTarget)
        {
            // Ưu tiên vật thể trúng raycast
            if (raycastTarget != null)
            {
                newTarget = raycastTarget;
            }
            // Nếu không trúng raycast, kiểm tra vật thể trong khoảng hình tròn
            else
            {
                Collider[] colliders = Physics.OverlapSphere(mainCamera.transform.position, rayDistance, interactableLayer);
                float minDistance = circleRadius + 1;
                foreach (Collider col in colliders)
                {
                    InteractableObject obj = col.GetComponent<InteractableObject>();
                    if (obj != null)
                    {
                        Vector3 screenPoint = mainCamera.WorldToScreenPoint(obj.transform.position);
                        float distanceFromCenter = Vector2.Distance(screenPoint, new Vector2(Screen.width / 2f, Screen.height / 2f));
                        if (distanceFromCenter <= circleRadius && screenPoint.z > 0 && distanceFromCenter < minDistance)
                        {
                            minDistance = distanceFromCenter;
                            newTarget = obj;
                        }
                    }
                }
            }
        }

        // Cập nhật target và gọi hàm tương ứng
        if (newTarget != currentTarget)
        {
            if (currentTarget != null)
                currentTarget.OnLookExit(); // Thoát khỏi vật thể cũ
            if (newTarget != null)
                newTarget.OnLookEnter(); // Gọi khi hướng tới vật thể mới
            currentTarget = newTarget;
        }

        // Gọi hàm OnClick khi bấm chuột trái
        if (Input.GetMouseButtonDown(0) && currentTarget != null)
        {
            currentTarget.OnClick();
        }
    }

    // Hiển thị vòng tròn trong Scene View để debug
    private void OnDrawGizmos()
    {
        if (mainCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(mainCamera.transform.position, rayDistance);
        }
    }
}