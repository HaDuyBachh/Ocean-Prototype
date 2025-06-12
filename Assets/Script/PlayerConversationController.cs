using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerConversationController : MonoBehaviour
{
    [Header("Conversation Settings")]
    [SerializeField] private GameObject conversationUI; // GameObject chứa UI hội thoại
    [SerializeField] private TextMeshProUGUI reply_1;
    [SerializeField] private TextMeshProUGUI reply_2;
    [SerializeField] private bool isConversationActive = false; // Trạng thái hội thoại
    [SerializeField] private InteractableObject interact;

    private void Start()
    {
        reply_1.transform.parent.GetComponent<Button>().onClick.AddListener(Reply1);
        reply_2.transform.parent.GetComponent<Button>().onClick.AddListener(Reply2);
    }

    private void Update()
    {
        // Thoát hội thoại khi bấm Esc
        if (isConversationActive && Input.GetKeyDown(KeyCode.Escape))
        {
            StopConversation();
        }
    }

    private IEnumerator OpenConversationUIObject()
    {
        yield return new WaitForSeconds(FindAnyObjectByType<Cinemachine.CinemachineBrain>().m_DefaultBlend.m_Time);

        SetReplyText();

        if (conversationUI != null)
        {
            conversationUI.SetActive(true);
            isConversationActive = true;

            // Hiển thị con trỏ chuột và khóa di chuyển chuột (nếu cần)
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Debug.LogWarning("Conversation UI is not assigned in PlayerConversationController.");
        }
    }

    // Bắt đầu hội thoại
    public void StartConversation(InteractableObject interact)
    {
        this.interact = interact;
        GetComponent<CameraScreenController>().StartConversation();
        StartCoroutine(OpenConversationUIObject());
    }

    // Kết thúc hội thoại
    private void StopConversation()
    {
        GetComponent<CameraScreenController>().StopConversation();
        interact.StopInteract();

        if (conversationUI != null)
        {
            conversationUI.SetActive(false);
            isConversationActive = false;

            // Ẩn con trỏ chuột và khóa lại (tùy theo game)
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void SetReplyText()
    {
        reply_1.SetText(interact.GetReplyText());
        reply_2.SetText("Bah, ce n'est rien.");
    }    

    public void Reply1()
    {
        Debug.Log("Đang click Reply1");
        if (!interact.Interact())
        {
            StopConversation();
        }

        SetReplyText();
    }

    public void Reply2()
    {
        Debug.Log("Đang click Reply2");
        StopConversation();
    }
}