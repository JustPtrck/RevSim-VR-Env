using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RevSimUI : MonoBehaviour
{
    [SerializeField] private Transform yawTrackerTransform;
    
    [SerializeField] private Text yaw, pitch, roll;


    // Update is called once per frame
    void Update()
    {
        Vector3 angles = yawTrackerTransform.rotation.eulerAngles;
        yaw.text = FormatAngles(angles.y);
        pitch.text = FormatAngles(angles.x);
        roll.text = FormatAngles(angles.z);
    }

    private string FormatAngles(float angle)
    {
        if (angle > 180)
            return (angle - 360).ToString("0.000");
        else
            return angle.ToString("0.000");

    }
}
