using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithOrigin : MonoBehaviour
{
    [SerializeField] private Transform origin;
    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(origin.position.x, origin.position.y, origin.position.z);
    }
}
