using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YawVR;

public class RotationLimits : MonoBehaviour
{
    [SerializeField] private Slider yawSlider;
    [SerializeField] private Slider pitchBackwardSlider;
    [SerializeField] private Slider pitchForwardSlider;
    [SerializeField] private Slider rollSlider;

    private YawController controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = YawController.Instance();
    }

    public void OnChange()
    {
        try
        {
            controller.SetTiltLimits(yawSlider.value, pitchBackwardSlider.value, pitchForwardSlider.value, rollSlider.value);
        }
        catch
        {
            Debug.LogError("rotation limits error!");
        }
    }
}
