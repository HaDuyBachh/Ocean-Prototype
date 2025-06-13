using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctopusAnimationController : MonoBehaviour
{
    private Animator animator;
    public void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void PlayAnimation(string name)
    {
        animator.PlayInFixedTime(Animator.StringToHash(name), 0, 0.1f);
    }    
}
