using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roll : MonoBehaviour
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
        float roll = YawTracker.transform.eulerAngles.z;
        Vector3 forward = (this.transform.localRotation * new Vector3(0, -1, 0)).normalized;
        this.transform.localRotation = Quaternion.AngleAxis(roll, forward).normalized;
    }
}
