using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public virtual void OnLookEnter() { }
    public virtual void OnLookExit() { }
    public virtual void OnClick() { }
}
