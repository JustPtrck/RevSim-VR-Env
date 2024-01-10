using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YawVR;
/// <summary>
/// Sets the YawTracker's orientation based on the GameObject's orientation
/// </summary>
public class SimpleOrientation : MonoBehaviour
{
    /*
       This script simply copies this gameObject's rotation, and sends it to the YawTracker
    */
    YawController yawController; // reference to YawController
    MotionCompensation motionCompensation;

    private void Start() {
        yawController = YawController.Instance();
        motionCompensation = yawController.gameObject.GetComponent<MotionCompensation>();
    }
    private void FixedUpdate() 
    {
        if (motionCompensation?.GetDevice() == MotionCompensation.enumYawPitchRollDevice.YawVRController)
        {
            yawController.TrackerObject.SetRotation(transform.localEulerAngles);
        }
        else if (motionCompensation?.GetDevice() == MotionCompensation.enumYawPitchRollDevice.LeftController
              || motionCompensation?.GetDevice() == MotionCompensation.enumYawPitchRollDevice.RightController) 
        {
            Vector3 eulerAngles = new Vector3();

            try
            {
                eulerAngles = motionCompensation.GetOpenXRControllerTransform().localEulerAngles;
            }
            catch (Exception ex) 
            {
                ex.ToString();
            }

            if (null != eulerAngles) 
            {
                yawController.TrackerObject.SetRotation(eulerAngles);
            }
        }
    }
}
