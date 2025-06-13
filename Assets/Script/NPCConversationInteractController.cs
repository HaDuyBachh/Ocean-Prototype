using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NPCConversationInteractController : InteractableObject
{
    [Header("References")]
    [SerializeField] private CameraScreenController playerCamera;
    [SerializeField] private PlayerConversationController playerConversation;
    [SerializeField] private GameObject UIObject;
    [SerializeField] private GameObject cameraFollow;
    [SerializeField] private TextMeshProUGUI talkText;

    [Header("Conversation Settings")]
    [SerializeField] private bool canTalk = true;
    [SerializeField] private string npcName;
    [SerializeField] private List<string> talk = new List<string>();
    [SerializeField] private List<string> reply = new List<string>();
    [SerializeField] private int triggerTalkID = -1;
    [SerializeField] private float delayInvoke = 1.5f;

    [Header("Events")]
    public UnityEvent OnConversationStart;
    public UnityEvent OnConversationEnd;
    public UnityEvent OnConversationTriggerPoint;
    public UnityEvent OnLookEnterEvent;
    public UnityEvent OnLookExitEvent;
    public UnityEvent OnClickEvent;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 1f;
    private Quaternion targetRotation;
    private bool isRotating = false;

    private int currentTalkId = 0;
    private bool isInConversation = false;

    private EPOOutline.Outlinable outlinable;

    private void Awake()
    {
        outlinable = GetComponent<EPOOutline.Outlinable>();
        if (outlinable != null) outlinable.enabled = false;
    }

    private void Update()
    {
        if (isRotating) HandleLookPlayer();
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

    private void LookAtPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 dir = (player.transform.position - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(dir, Vector3.up);
                isRotating = true;
            }
        }
    }

    public override void OnLookEnter()
    {
        if (outlinable) outlinable.enabled = true;
        OnLookEnterEvent?.Invoke();
    }

    public override void OnLookExit()
    {
        if (outlinable) outlinable.enabled = false;
        OnLookExitEvent?.Invoke();
    }

    public override void OnClick()
    {
        if (!isInConversation && canTalk)
        {
            isInConversation = true;
            LookAtPlayer();
            StartConversation();
            OnClickEvent?.Invoke();
        }
    }

    private IEnumerator OpenUI()
    {
        yield return new WaitForSeconds(FindAnyObjectByType<Cinemachine.CinemachineBrain>().m_DefaultBlend.m_Time);
        UIObject?.SetActive(true);
    }

    public void StartConversation()
    {
        if (!playerConversation) playerConversation = FindAnyObjectByType<PlayerConversationController>();
        playerConversation.StartConversation(this);

        cameraFollow?.SetActive(true);
        StartCoroutine(OpenUI());

        currentTalkId = 0;
        talkText?.SetText(talk[currentTalkId]);
        OnConversationStart?.Invoke();
    }

    public void StopConversation()
    {
        cameraFollow?.SetActive(false);
        UIObject?.SetActive(false);

        currentTalkId = 0;
        isInConversation = false;
        OnConversationEnd?.Invoke();
    }

    public override bool Interact()
    {
        return Reply();
    }

    public override void StopInteract()
    {
        StopConversation();
    }

    private IEnumerator DelayedInvoke(UnityEvent unityEvent)
    {
        yield return new WaitForSeconds(delayInvoke);
        unityEvent?.Invoke();
    }

    public bool Reply()
    {
        currentTalkId++;

        if (currentTalkId < talk.Count)
        {
            talkText?.SetText(talk[currentTalkId]);

            if (currentTalkId == triggerTalkID)
            {
                StartCoroutine(DelayedInvoke(OnConversationTriggerPoint));
            }

            return true;
        }
        else
        {
            StopConversation();
            return false;
        }
    }

    public override string GetReplyText()
    {
        return (currentTalkId >= 0 && currentTalkId < reply.Count) ? reply[currentTalkId] : string.Empty;
    }

    public void DestroyThisScript()
    {
        Destroy(this);
    }
}
