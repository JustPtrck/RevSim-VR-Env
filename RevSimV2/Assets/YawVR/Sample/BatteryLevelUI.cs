using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YawVR;

/// <summary>
/// Shows the IMU's battery level
/// </summary>
public class BatteryLevelUI : MonoBehaviour
{

    private Slider slider;

    YawController yawController;
    private void Awake()
    {
        slider = GetComponent<Slider>();

       
    }
    private void Start()
    {
        yawController = YawController.Instance();

        StartCoroutine(UpdateUI());
    }
    IEnumerator UpdateUI()
    {

        WaitForSeconds wait = new WaitForSeconds(2f);
        while(true)
        {
            slider.value = yawController.Device.batteryVoltage;
            yield return wait;
        }
    }
}
