using UnityEngine;
using UnityEngine.Events;

public class InteractableObjectController : InteractableObject
{
    [Header("Component References")]
    [SerializeField] private EPOOutline.Outlinable outlinable;

    [Header("General Events")]
    public UnityEvent OnLookEnterEvent;
    public UnityEvent OnLookExitEvent;
    public UnityEvent OnClickEvent;
    public UnityEvent OnInteractEvent;
    public UnityEvent OnStopInteractEvent;

    private void Start()
    {
        if (outlinable == null)
            outlinable = GetComponent<EPOOutline.Outlinable>();

        if (outlinable != null)
            outlinable.enabled = false;
    }

    public override void OnLookEnter()
    {
        if (outlinable != null)
            outlinable.enabled = true;

        InvokeEvent(OnLookEnterEvent);
    }

    public override void OnLookExit()
    {
        if (outlinable != null)
            outlinable.enabled = false;

        InvokeEvent(OnLookExitEvent);
    }

    public override void OnClick()
    {
        InvokeEvent(OnClickEvent);
    }

    public override bool Interact()
    {
        InvokeEvent(OnInteractEvent);
        return true;
    }

    public override void StopInteract()
    {
        InvokeEvent(OnStopInteractEvent);
    }

    private void InvokeEvent(UnityEvent unityEvent)
    {
        unityEvent?.Invoke();
    }
}
