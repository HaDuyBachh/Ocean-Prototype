using UnityEngine;
using System.Collections;

public class ParticleEraseController : MonoBehaviour
{
    [SerializeField] private Transform targetParent; // Nơi chứa các con cần kiểm tra (mặc định là this)
    [SerializeField] private bool waitForEffect = true; // Có chờ hiệu ứng xong không

    private void Awake()
    {
        if (targetParent == null)
            targetParent = this.transform;
    }

    /// <summary>
    /// Xoá đứa con đầu tiên có ParticleSystem.
    /// </summary>
    public void EraseParticleChild()
    {
        StartCoroutine(EraseCoroutine());
    }

    private IEnumerator EraseCoroutine()
    {
        foreach (Transform child in targetParent)
        {
            ParticleSystem ps = child.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();

                if (waitForEffect)
                {
                    float duration = ps.main.duration + ps.main.startLifetime.constantMax;
                    yield return new WaitForSeconds(duration);
                }

                Destroy(child.gameObject);
                yield break;
            }
        }

        Debug.LogWarning("Không tìm thấy đứa con nào có ParticleSystem.");
    }
}
