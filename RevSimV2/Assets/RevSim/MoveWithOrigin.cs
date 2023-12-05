using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithOrigin : MonoBehaviour
{
    [SerializeField] private Transform origin;
    [SerializeField] private bool x, y, z;


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
    }
}
