using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Class is used to move the parent GameObject with the Transform of Origin
/// </summary>
public class MoveWithOrigin : MonoBehaviour
{
    [SerializeField] private Transform origin;
    [SerializeField] private bool x, y, z;
    [SerializeField] private bool yaw, pitchRoll;


    // Update is called once per frame
    void Update()
    {
        if (x && y && z)
            transform.position = origin.position;
        else if (x && !y && z)
            transform.position = new Vector3(origin.position.x, transform.position.y, origin.position.z);
        else if (x && y && !z)
            transform.position = new Vector3(origin.position.x, origin.position.y, transform.position.z);
        else if (!x && y && z)
            transform.position = new Vector3(transform.position.x, origin.position.y, origin.position.z);
        else if (x && !y && !z)
            transform.position = new Vector3(origin.position.x, transform.position.y, transform.position.z);
        else if (!x && y && !z)
            transform.position = new Vector3(transform.position.x, origin.position.y, transform.position.z);
        else if (!x && !y && z)
            transform.position = new Vector3(transform.position.x, transform.position.y, origin.position.z);

        if (yaw && pitchRoll)
            transform.rotation = origin.rotation;
        else if (yaw && !pitchRoll)
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, origin.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

    }
}
