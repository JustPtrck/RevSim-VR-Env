using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YawVR;

public class RotationMultipliers : MonoBehaviour
{
    [SerializeField] private Slider yawSlider;
    [SerializeField] private Slider pitchSlider;
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
            controller.SetRotationMultiplier(yawSlider.value, pitchSlider.value, rollSlider.value);
        }
        catch
        {
            Debug.LogError("rotationmultiplier error!");
        }
    }
}
