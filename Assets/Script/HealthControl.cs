using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
    // Máu tối đa của nhân vật
    [SerializeField] private int maxHealth = 100;

    // Máu hiện tại
    private int currentHealth;

    // Sự kiện được gọi khi nhân vật chết
    public UnityEvent onDeath;

    // Sự kiện được gọi khi máu thay đổi, truyền tỷ lệ máu (0-1)
    public UnityEvent<float> onHealthChanged;

    void Start()
    {
        // Khởi tạo máu hiện tại bằng máu tối đa
        currentHealth = maxHealth;

        // Gọi sự kiện để cập nhật UI (nếu có)
        onHealthChanged.Invoke((float)currentHealth / maxHealth);
    }

    // Hàm xử lý sát thương
    public void HandleDamage(int damage)
    {
        // Giảm máu hiện tại
        currentHealth = Mathf.Max(0, currentHealth - damage);

        // Gọi sự kiện thay đổi máu
        onHealthChanged.Invoke((float)currentHealth / maxHealth);

        // Kiểm tra nếu nhân vật chết
        if (currentHealth <= 0)
        {
            onDeath.Invoke();
        }
    }

    // Hàm lấy máu hiện tại (cho các script khác truy cập)
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // Hàm lấy máu tối đa (cho các script khác truy cập)
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    // Hàm phục hồi máu
    public void Heal(int amount)
    {
        // Tăng máu hiện tại, không vượt quá máu tối đa
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        // Gọi sự kiện thay đổi máu
        onHealthChanged.Invoke((float)currentHealth / maxHealth);
    }
}