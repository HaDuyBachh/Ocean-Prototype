using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class CardController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] public RectTransform cardRect;
    [SerializeField] public TextMeshProUGUI detailText;

    public float collapsedHeight = 340f;
    public float expandedHeight = 600f;
    public float width = 450f;
    public float expandSpeed = 600f;

    private Coroutine animationRoutine;

    [Header("Debug")]
    public bool isExpand;
    public bool isCollapse;

    private bool isExpanded = false;

    void Start()
    {
        if (cardRect != null)
        {
            cardRect.sizeDelta = new Vector2(width, collapsedHeight);
        }

        if (detailText != null)
        {
            detailText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isExpand)
        {
            isExpand = false;
            OnExpand();
        }

        if (isCollapse)
        {
            isCollapse = false;
            OnCollapse();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnExpand();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnCollapse();
    }

    public void OnExpand()
    {
        if (isExpanded) return;
        isExpanded = true;

        if (animationRoutine != null) StopCoroutine(animationRoutine);
        animationRoutine = StartCoroutine(AnimateExpandCollapse(expandedHeight, true));
    }

    public void OnCollapse()
    {
        if (!isExpanded) return;
        isExpanded = false;

        if (animationRoutine != null) StopCoroutine(animationRoutine);
        animationRoutine = StartCoroutine(AnimateExpandCollapse(collapsedHeight, false));
    }

    private IEnumerator AnimateExpandCollapse(float targetHeight, bool showDetail)
    {
        float fromHeight = cardRect.sizeDelta.y;
        float duration = Mathf.Abs(targetHeight - fromHeight) / expandSpeed;
        float elapsed = 0f;

        if (!showDetail && detailText != null)
        {
            detailText.gameObject.SetActive(false);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newHeight = Mathf.Lerp(fromHeight, targetHeight, elapsed / duration);
            cardRect.sizeDelta = new Vector2(width, newHeight);
            yield return null;
        }

        cardRect.sizeDelta = new Vector2(width, targetHeight);

        if (showDetail && detailText != null)
        {
            detailText.gameObject.SetActive(true);
        }
    }
}
