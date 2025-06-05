using UnityEngine;

public class WayPoint : MonoBehaviour
{
    // Bán kính của Gizmos hình tròn
    [SerializeField] private float radius = 0.5f;

    // Màu sắc của Gizmos
    [SerializeField] private Color gizmoColor = Color.yellow;

    // Độ cao của Gizmos so với vị trí waypoint (để tránh bị chìm vào mặt đất)
    [SerializeField] private float heightOffset = 0.0f;

    // Vẽ Gizmos trong Scene view
    private void OnDrawGizmos()
    {
        // Lưu màu Gizmos hiện tại
        Color previousColor = Gizmos.color;

        // Đặt màu cho Gizmos
        Gizmos.color = gizmoColor;

        // Tính vị trí vẽ Gizmos (nâng lên theo heightOffset)
        Vector3 gizmoPosition = transform.position + Vector3.up * heightOffset;

        // Vẽ hình tròn (wire disc) tại vị trí waypoint
        Gizmos.DrawWireSphere(gizmoPosition, radius);

        // Khôi phục màu Gizmos
        Gizmos.color = previousColor;
    }
}