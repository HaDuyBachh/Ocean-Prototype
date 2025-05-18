using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvairomentController : MonoBehaviour
{
    public GameObject WaterSurface;
    public GameObject Water;

    void Start()
    {
        
    }

    public void InnerWater()
    {
        WaterSurface.SetActive(false);
        Water.SetActive(false);
    }

    public void OutterWater()
    {
        WaterSurface.SetActive(true);
        Water.SetActive(true);
    }
}
