using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public class CrabInteractController : InteractableObject
{
    [Header("Object Setting")]
    [SerializeField] private CameraScreenController playerCamera;
    [SerializeField] private PlayerConversationController playerConversation;
    [SerializeField] private GameObject trashHandle;

    [Header("Conversation")]
    [SerializeField] private bool _isCanTalk;
    [SerializeField] private GameObject UIObject;
    [SerializeField] private TextMeshProUGUI talkText;
    [SerializeField] private string _name;
    [SerializeField] private int currentTalkId;
    [SerializeField] private List<string> _talk = new List<string>();
    [SerializeField] private List<string> _reply = new List<string>();
    [SerializeField] private int RemoveTrashTalkID;
    [SerializeField] private bool _isInConversation;

    [Header("Camera Conversation")]
    [SerializeField] private GameObject cameraFollow;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 1f; // Tốc độ xoay mượt (độ/giây)
    [SerializeField] private bool isRotating = false;
    private Quaternion targetRotation;

    private CrabController crabController;
    private EPOOutline.Outlinable outlinable;
    void Awake()
    {
        // Lấy component CrabController
        crabController = GetComponent<CrabController>();
        if (crabController == null)
        {
            Debug.LogWarning($"No CrabController found on {gameObject.name}");
        }
    }

    private void Start()
    {
        // Khởi tạo Outlinable và tắt mặc định
        outlinable = GetComponent<EPOOutline.Outlinable>();
        if (outlinable != null)
        {
            outlinable.enabled = false;
        }
        else
        {
            Debug.LogWarning($"No EPOOutline.Outlinable found on {gameObject.name}");
        }
    }

    private void Update()
    {
        if (isRotating) HandleLookPlayer();
    }

    public void HandleLookPlayer()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
        {
            transform.rotation = targetRotation; // Đặt chính xác để kết thúc
            isRotating = false;
        }
    }

    public void LookPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            directionToPlayer.y = 0; // Giữ xoay trên mặt phẳng XZ
            if (directionToPlayer != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
                isRotating = true;
            }
        }
    }

    public override void OnLookEnter()
    {
        if (outlinable != null)
        {
            outlinable.enabled = true;
        }
    }

    public override void OnLookExit()
    {
        if (outlinable != null)
        {
            outlinable.enabled = false;
        }
    }

    public override void OnClick()
    {
        // Gọi StopBehavior nếu có CrabController

        if (crabController != null && !_isInConversation)
        {
            _isInConversation = true;
            crabController.StopBehavior();
            LookPlayer();
            if (_isCanTalk) StartConversation();
        }
    }

    private IEnumerator OpenUIObject()
    {
        yield return new WaitForSeconds(FindAnyObjectByType<Cinemachine.CinemachineBrain>().m_DefaultBlend.m_Time);
        UIObject.SetActive(true);
    }

    public void StartConversation()
    {
        GetComponent<Rigidbody>().useGravity = true;

        if (!playerConversation) playerConversation = FindAnyObjectByType<PlayerConversationController>();
        playerConversation.StartConversation(this);
        cameraFollow.SetActive(true);

        StartCoroutine(OpenUIObject());
        talkText.SetText(_talk[0]);
        currentTalkId = 0;
        _isInConversation = true;
    }

    public void StopConversation()
    {
        GetComponent<Rigidbody>().useGravity = false;

        if (!playerConversation) playerConversation = FindAnyObjectByType<PlayerConversationController>();
        cameraFollow.SetActive(false);

        UIObject.SetActive(false);
        currentTalkId = 0;
        crabController.ResumeBehavior();
        _isInConversation = false;
    }

    public override bool Interact()
    {   
        return Reply();
    }
    public override void StopInteract()
    {
        StopConversation();
    }

    public bool Reply()
    {
        if (currentTalkId == _talk.Count - 1)
        {
            return false;
        }

        talkText.SetText(_talk[++currentTalkId]);

        if (currentTalkId == RemoveTrashTalkID)
        {
            RemoveTrash();
            SetFriendly();
        }

        return true;
    }

    public override string GetReplyText()
    {
        return _reply[currentTalkId];
    }

    public void RemoveTrash()
    {
        trashHandle.GetComponent<TrashObjectController>().OnClick();
    }    
    public void SetFriendly()
    {
        crabController.IsFriendly = true;
    }    

}