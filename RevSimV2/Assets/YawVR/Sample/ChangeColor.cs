using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YawVR;


/* Change color based on device state */
public class ChangeColor : MonoBehaviour
{
    [SerializeField]
    private Color stoppedColor;
    [SerializeField]
    private Color startedColor;


    public void StateChanged(DeviceState state) {
        switch (state) {
            case DeviceState.STOPPED:
                this.gameObject.GetComponent<Renderer>().material.color = stoppedColor;
                break;
            case DeviceState.STARTED:
                this.gameObject.GetComponent<Renderer>().material.color = startedColor;
                break;
        }
    }
}
