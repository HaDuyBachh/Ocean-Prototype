using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class DolphinPullQuestController : InteractableObject
{
    [Header("Object Settings")]
    [SerializeField] private CameraScreenController playerCamera;
    [SerializeField] private GameObject interactObject;
    [SerializeField] private GameObject cameraFollow;

    [Header("Pull Settings")]
    [SerializeField] private float pullStep = 0.2f;              // Mỗi lần click sẽ kéo bao nhiêu đơn vị
    [SerializeField] private float targetLocalZ = -1.5f;         // Mốc z local để tính là hoàn thành
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private UnityEvent onActionCompleted;       // Sự kiện gọi khi kéo xong
    [SerializeField] private float pushBackSpeed = 0.1f;

    [SerializeField] private bool isInAction = false;
    private Quaternion targetRotation;
    private bool isRotating = false;

    private DolphinController dolphinController;
    private EPOOutline.Outlinable outlinable;

    private void Awake()
    {
        dolphinController = GetComponent<DolphinController>();
        if (dolphinController == null)
        {
            Debug.LogWarning($"No DolphinController found on {gameObject.name}");
        }
    }

    private void Start()
    {
        outlinable = GetComponent<EPOOutline.Outlinable>();
        if (outlinable != null) outlinable.enabled = false;
    }

    private void Update()
    {
        if (isRotating) HandleLookPlayer();
    }

    public override void OnClick()
    {
        if (!isInAction)
        {
            StartAction();
        }
        else
        {
            PullObject();
        }
    }

    private void StartAction()
    {
        isInAction = true;
        dolphinController?.HipShake();
        LookPlayer();
        cameraFollow.SetActive(true);
    }

    private void EndAction()
    {
        GetComponent<Animator>().SetBool(Animator.StringToHash("done"), true);
        isInAction = false;
        cameraFollow.SetActive(false);
        onActionCompleted?.Invoke();
        Destroy(this);
    }

    private void PullObject()
    {
        if (interactObject == null) return;

        // Di chuyển object theo trục Z local
        Vector3 localPos = interactObject.transform.localPosition;
        localPos.z -= pullStep;
        interactObject.transform.localPosition = localPos;

        // Kiểm tra nếu đã đạt mốc
        if (localPos.z >= targetLocalZ)
        {
            EndAction();
        }
    }

    public void LookPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            directionToPlayer.y = 0;
            if (directionToPlayer != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
                isRotating = true;
            }
        }
    }

    private void HandleLookPlayer()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
        {
            transform.rotation = targetRotation;
            isRotating = false;
        }
    }

    public override void OnLookEnter()
    {
        if (outlinable != null)
            outlinable.enabled = true;
    }

    public override void OnLookExit()
    {
        if (outlinable != null)
            outlinable.enabled = false;
    }

    public override bool Interact()
    {
        OnClick();
        return true;
    }

    public override void StopInteract()
    {
        EndAction();
    }

    public override string GetReplyText()
    {
        return isInAction ? "Kéo tiếp" : "Bắt đầu kéo";
    }
}
