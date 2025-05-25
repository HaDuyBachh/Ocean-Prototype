using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InnerWaterControl : MonoBehaviour
{
    private EnvairomentController envairoment;
    private ThirdPersonController character;

    private void Start()
    {
        envairoment = FindAnyObjectByType<EnvairomentController>();
        character = FindAnyObjectByType<ThirdPersonController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            character.ChangeToWaterEnvairoment();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            character.ChangeToGroundEnvairoment();
        }
    }
}
