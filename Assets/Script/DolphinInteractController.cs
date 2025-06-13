using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

public class DolphinInteractController : InteractableObject
{
    [Header("Object Setting")]
    [SerializeField] private CameraScreenController playerCamera;
    [SerializeField] private PlayerConversationController playerConversation;
    [SerializeField] private GameObject trashHandle;

    [Header("Conversation")]
    [SerializeField] private bool isCanTalk;
    [SerializeField] private GameObject UIObject;
    [SerializeField] private TextMeshProUGUI talkText;
    [SerializeField] private string _name;
    [SerializeField] private int currentTalkId;
    [SerializeField] private List<string> talk = new List<string>();
    [SerializeField] private List<string> reply = new List<string>();
    [SerializeField] private int TriggerTalkID;
    [SerializeField] private UnityEvent TriggerEvent;
    [SerializeField] private bool isInConversation;
    [SerializeField] private float delyInvoke = 1.5f;

    [Header("Camera Conversation")]
    [SerializeField] private GameObject cameraFollow;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private bool isRotating = false;
    private Quaternion targetRotation;

    private DolphinController dolphinController;
    private EPOOutline.Outlinable outlinable;

    void Awake()
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
            transform.rotation = targetRotation;
            isRotating = false;
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
        if (dolphinController != null && !isInConversation)
        {
            isInConversation = true;
            dolphinController.HipShake();
            LookPlayer();
            if (isCanTalk) StartConversation();
        }
    }

    private IEnumerator OpenUIObject()
    {
        yield return new WaitForSeconds(FindAnyObjectByType<Cinemachine.CinemachineBrain>().m_DefaultBlend.m_Time);
        UIObject.SetActive(true);
    }

    public void StartConversation()
    {
        if (!playerConversation) playerConversation = FindAnyObjectByType<PlayerConversationController>();
        playerConversation.StartConversation(this);
        cameraFollow.SetActive(true);

        StartCoroutine(OpenUIObject());
        talkText.SetText(talk[0]);
        currentTalkId = 0;
        isInConversation = true;
    }

    public void StopConversation()
    {
        if (!playerConversation) playerConversation = FindAnyObjectByType<PlayerConversationController>();
        cameraFollow.SetActive(false);

        UIObject.SetActive(false);
        currentTalkId = 0;
        dolphinController.HipShake();
        isInConversation = false;
    }

    public override bool Interact()
    {
        return Reply();
    }

    public override void StopInteract()
    {
        StopConversation();
    }

    private IEnumerator DelayedInvokeCoroutine(UnityEvent myEvent)
    {
        yield return new WaitForSeconds(delyInvoke);
        myEvent.Invoke();
    }

    public bool Reply()
    {
        talkText.SetText(talk[++currentTalkId]);

        if (currentTalkId == TriggerTalkID)
        {
            StartCoroutine(DelayedInvokeCoroutine(TriggerEvent));
        }

        if (currentTalkId == talk.Count - 1)
        {
            return false;
        }

        return true;
    }

    public override string GetReplyText()
    {
        return reply[currentTalkId];
    }

    public void DestroyThisScript()
    {
        Destroy(this);
    }
}