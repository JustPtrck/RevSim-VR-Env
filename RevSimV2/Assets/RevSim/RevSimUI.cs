using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Audio;

public class RevSimUI : MonoBehaviour
{
    [Header("Yaw Rotation")]
    [SerializeField] private Transform yawTrackerTransform;
    [SerializeField] private Text yaw, pitch, roll;
    [Header("VR Preferences")]
    [SerializeField] private Camera camera; 
    [SerializeField] private Light lightSource; 
    [SerializeField] private Volume volume;
    private Vignette vg;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider FOVSlider, vignetteSlider, brightnessSlider, masterSlider;
    [SerializeField] private InputField FOVField, vignetteField, brightnessField, masterField; 
    private float FOV, vignette, brightness;
    [Header("Level Selection")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private Slider timeSlider, levelSlider, modSlider;
    [SerializeField] private InputField timeField, levelField, modField;

    [Header("Spawn Settings")]
    [SerializeField] private CollectableSpawn collectableSpawn;
    [SerializeField] private Slider radiusSlider, horizontalSlider, verticalSlider, offsetSlider;
    [SerializeField] private InputField radiusField, horizontalField, verticalField, offsetField;
    
    private void Start()
    {
        volume.profile.TryGet(out vg);
        UpdateVignette();
        UpdateBrightness();
        UpdateFOV();
        UpdateLevelManagerValues();
        UpdateMasterVol();
        UpdateRadius();
        UpdateHorizontal();
        UpdateVertical();
        UpdateOffset();
    }

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

    public void UpdateLevelManagerValues()
    {
        timeSlider.value = levelManager.values.Item1;
        timeField.text = levelManager.values.Item1.ToString("0");
        levelSlider.value = levelManager.values.Item2;
        levelField.text = levelManager.values.Item2.ToString();
        modSlider.value = levelManager.values.Item3;
        modField.text = levelManager.values.Item3.ToString();
    }
    public void SetTransitionTime(float value)
    {
        levelManager.SetTransitionTime(value);
        UpdateLevelManagerValues();
    }
    public void SetTransitionTime(string value) => SetTransitionTime(float.Parse(value));

    public void SetLevel(float value)
    {
        levelManager.LevelSelect(value);
        UpdateLevelManagerValues();
    }
    public void SetLevel(string value) => SetLevel(float.Parse(value));
    public void SetMod(float value)
    {
        levelManager.SetSteepnessMod(value);
        UpdateLevelManagerValues();
    }
    public void SetMod(string value) => SetMod(float.Parse(value));

    public void UpdateMasterVol()
    {
        float value = 0f;
        audioMixer.GetFloat("masterVol", out value);
        masterField.text = value.ToString("0.0")+ "dB";
        masterSlider.value = value;
    }
    public void SetMasterVol(float value)
    {
        if (value < -80f) value = -80f;
        else if (value > 20f) value = 20f;
        audioMixer.SetFloat("masterVol", value);
        UpdateMasterVol();
    }
    public void SetMasterVol(string value) => SetMasterVol(float.Parse(value));


    public void UpdateFOV()
    {
        FOVField.text = camera.fieldOfView.ToString("0");
        FOVSlider.value = camera.fieldOfView;
    }
    public void SetFOV(float value)
    {
        camera.fieldOfView = value;
        UpdateFOV();
    }
    public void SetFOV(string value) => SetFOV(float.Parse(value));

    public void UpdateVignette()
    {
        vignetteField.text = vg.intensity.value.ToString("0.000");
        vignetteSlider.value = vg.intensity.value;
    }
    public void SetVignette(float value)
    {
        vg.intensity.value = value;
        UpdateVignette();
    }
    public void SetVignette(string value) => SetVignette(float.Parse(value));

    public void UpdateBrightness()
    {
        brightnessField.text =  lightSource.intensity.ToString("0.000");
        brightnessSlider.value = lightSource.intensity;
    }
    public void SetBrightness(float value)
    {
        lightSource.intensity = value;
        UpdateBrightness();
    }
    public void SetBrightness(string value) => SetBrightness(float.Parse(value));


    public void UpdateRadius()
    {
        radiusField.text =  collectableSpawn.radius.ToString("0.000");
        radiusSlider.value = collectableSpawn.radius;
    }
    public void SetRadius(float value)
    {
        collectableSpawn.radius = value;
        UpdateRadius();
    }
    public void SetRadius(string value) => SetRadius(float.Parse(value));

    public void UpdateHorizontal()
    {
        horizontalField.text =  collectableSpawn.horizontal.ToString("0.000");
        horizontalSlider.value = collectableSpawn.horizontal;
    }
    public void SetHorizontal(float value)
    {
        collectableSpawn.horizontal = value;
        UpdateHorizontal();
    }
    public void SetHorizontal(string value) => SetHorizontal(float.Parse(value));

    public void UpdateVertical()
    {
        verticalField.text =  collectableSpawn.vertical.ToString("0.000");
        verticalSlider.value = collectableSpawn.vertical;
    }
    public void SetVertical(float value)
    {
        collectableSpawn.vertical = value;
        UpdateVertical();
    }
    public void SetVertical(string value) => SetVertical(float.Parse(value));

    public void UpdateOffset()
    {
        offsetField.text =  collectableSpawn.offset.ToString("0.000");
        offsetSlider.value = collectableSpawn.offset;
    }
    public void SetOffset(float value)
    {
        collectableSpawn.offset = value;
        UpdateOffset();
    }
    public void SetOffset(string value) => SetOffset(float.Parse(value));
}
