using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pitch : MonoBehaviour
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
        float pitch = YawTracker.transform.eulerAngles.x;
        Vector3 right = (this.transform.localRotation * new Vector3(0, 0, 1)).normalized;
        this.transform.localRotation = Quaternion.AngleAxis(pitch, right).normalized;
    }
}
