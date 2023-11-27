using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class yaw : MonoBehaviour
{
    [SerializeField]
    GameObject YawTracker;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float yaw = YawTracker.transform.eulerAngles.y;
        Vector3 up = (this.transform.localRotation * new Vector3(0, 1, 0)).normalized;
        this.transform.localRotation = Quaternion.AngleAxis(yaw, up).normalized;
    }
}
