using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] Vector3 axis = new Vector3(0, 1, 0);
    [SerializeField] float speed = 360;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion orientation = Quaternion.Euler(axis * speed * Time.deltaTime);
        this.transform.rotation *= orientation;
    }
}
