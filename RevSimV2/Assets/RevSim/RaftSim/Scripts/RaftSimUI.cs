using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Audio;
using YawVR;
using System.Net;
using UnityEngine.XR;
using UnityEditor.XR.LegacyInputHelpers;
public class RaftSimUI : MonoBehaviour 
{
    [Header("Level Selection")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private Slider timeSlider, levelSlider, modSlider;
    [SerializeField] private InputField timeField, levelField, modField;

    [Header("Spawn Settings")]
    [SerializeField] private ReachingExerciseSpawner reachingSpawner;
    [SerializeField] private Slider radiusSlider, horizontalSlider, verticalSlider, offsetSlider;
    [SerializeField] private InputField radiusField, horizontalField, verticalField, offsetField;
    
    private void Start()
    {
        UpdateLevelManagerValues();
        UpdateRadius();
        UpdateHorizontal();
        UpdateVertical();
        UpdateOffset();
    }

    // Update is called once per frame


#region LevelManager
    public void UpdateLevelManagerValues()
    {
        timeSlider.value = levelManager.values.Item1;
        timeField.text = levelManager.values.Item1.ToString("0");
        levelSlider.value = levelManager.values.Item2;
        levelField.text = levelManager.values.Item2.ToString();
        modSlider.value = levelManager.values.Item3;
        modField.text = levelManager.values.Item3.ToString();
    }

    public void ESTOP() => levelManager.EmergancyStop();

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

#endregion

#region Collectible
    public void UpdateRadius()
    {
        radiusField.text =  reachingSpawner.radius.ToString("0.00");
        radiusSlider.value = reachingSpawner.radius;
    }
    public void SetRadius(float value)
    {
        reachingSpawner.radius = value;
        UpdateRadius();
    }
    public void SetRadius(string value) => SetRadius(float.Parse(value));

    public void UpdateHorizontal()
    {
        horizontalField.text =  reachingSpawner.horizontal.ToString("0.00");
        horizontalSlider.value = reachingSpawner.horizontal;
    }
    public void SetHorizontal(float value)
    {
        reachingSpawner.horizontal = value;
        UpdateHorizontal();
    }
    public void SetHorizontal(string value) => SetHorizontal(float.Parse(value));

    public void UpdateVertical()
    {
        verticalField.text =  reachingSpawner.vertical.ToString("0.00");
        verticalSlider.value = reachingSpawner.vertical;
    }
    public void SetVertical(float value)
    {
        reachingSpawner.vertical = value;
        UpdateVertical();
    }
    public void SetVertical(string value) => SetVertical(float.Parse(value));

    public void UpdateOffset()
    {
        offsetField.text =  reachingSpawner.offset.ToString("0.00");
        offsetSlider.value = reachingSpawner.offset;
    }
    public void SetOffset(float value)
    {
        reachingSpawner.offset = value;
        UpdateOffset();
    }
    public void SetOffset(string value) => SetOffset(float.Parse(value));
#endregion

    public void ExitApp()
    {
        SetTransitionTime(5);
        SetLevel(-1);
    }

}
