using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyRotationCenter : MonoBehaviour
{
    [SerializeField] private GameObject offset = null;
    [SerializeField] private FindRotationCenter findRotationCenter = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 v = findRotationCenter.Offset;

        if (float.IsNaN(v.x)) { v.x = 0; }
        if (float.IsNaN(v.y)) { v.y = 0; }
        
        offset.transform.localPosition = new Vector3(0, Mathf.Abs(v.x), -Mathf.Abs(v.y));
    }
}
