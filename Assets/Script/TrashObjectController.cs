using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashObjectController : InteractableObject
{
    [SerializeField] private EPOOutline.Outlinable outlinable;
    [SerializeField] private GameObject disapperFX;
    // Start is called before the first frame update
    void Start()
    {
        outlinable = GetComponent<EPOOutline.Outlinable>();
        outlinable.enabled = false;
    }

    public override void OnLookEnter()
    {
        outlinable.enabled = true;
    }

    public override void OnLookExit()
    {
        outlinable.enabled = false;
    }

    public override void OnClick()
    {
        GameObject fx = Instantiate(disapperFX, transform.position + transform.up * 0.2f, Quaternion.identity);
        Destroy(fx, 0.8f);
        Destroy(gameObject, 0.3f);
    }
}
