using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JustPtrck.Shaders.Water;

public class Hover : MonoBehaviour
{
    public float height = 1f;

    // Update is called once per frame
    void Update()
    {
       float disp = WaveManager.instance.GetDisplacementFromGPU(transform.position).y + height;
       transform.position += Vector3.up * (disp - transform.position.y);
    }
}
