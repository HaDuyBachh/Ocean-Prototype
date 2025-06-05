using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AnimationUIController : MonoBehaviour
{
    [Header("UI Components")]
    // Image để hiển thị animation
    [SerializeField] private Image targetImage;

    [Header("Animation Settings")]
    // Danh sách các sprite để tạo animation
    [SerializeField] private List<Sprite> frames = new List<Sprite>();

    // Tốc độ frame (giây giữa mỗi frame)
    [SerializeField] private float frameDuration = 0.1f;

    // Có lặp lại animation không
    [SerializeField] private bool loopAnimation = true;

    // Có chạy animation khi start không
    [SerializeField] private bool playOnStart = true;

    // Chỉ số frame hiện tại
    private int currentFrameIndex;

    // Bộ đếm thời gian để chuyển frame
    private float frameTimer;

    // Trạng thái animation
    private bool isPlaying;

    void Start()
    {
        // Khởi tạo
        currentFrameIndex = 0;
        frameTimer = 0f;
        isPlaying = playOnStart;

        // Kiểm tra và hiển thị frame đầu tiên nếu có
        if (targetImage != null && frames.Count > 0)
        {
            targetImage.sprite = frames[0];
        }
        else
        {
            Debug.LogWarning("Target Image hoặc Frames chưa được gán trong Inspector.");
        }
    }

    void Update()
    {
        // Chỉ cập nhật nếu đang chạy animation
        if (!isPlaying || frames.Count == 0 || targetImage == null) return;

        // Tăng bộ đếm thời gian
        frameTimer += Time.deltaTime;

        // Kiểm tra nếu đến lúc chuyển frame
        if (frameTimer >= frameDuration)
        {
            // Chuyển sang frame tiếp theo
            currentFrameIndex = (currentFrameIndex + 1) % frames.Count;
            targetImage.sprite = frames[currentFrameIndex];

            // Reset bộ đếm thời gian
            frameTimer = 0f;

            // Dừng animation nếu không lặp và đã chạy hết
            if (!loopAnimation && currentFrameIndex == frames.Count - 1)
            {
                isPlaying = false;
            }
        }
    }

    // Bắt đầu chạy animation
    public void PlayAnimation()
    {
        isPlaying = true;
        currentFrameIndex = 0;
        frameTimer = 0f;
        if (frames.Count > 0 && targetImage != null)
        {
            targetImage.sprite = frames[0];
        }
    }

    // Dừng animation
    public void StopAnimation()
    {
        isPlaying = false;
    }

    // Gán danh sách frames mới
    public void SetFrames(List<Sprite> newFrames)
    {
        frames = newFrames;
        currentFrameIndex = 0;
        if (frames.Count > 0 && targetImage != null)
        {
            targetImage.sprite = frames[0];
        }
    }

    // Gán tốc độ frame mới
    public void SetFrameDuration(float newDuration)
    {
        frameDuration = Mathf.Max(0.01f, newDuration);
    }
}