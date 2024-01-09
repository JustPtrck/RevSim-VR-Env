using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JustPtrck.Shaders.Water;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private List<WaveObject> levels = new List<WaveObject>();
    [SerializeField] private int currentLevel = -1;
    [SerializeField] private int newLevel = 0;
    [SerializeField] private float lastMod = 1f;
    [SerializeField, Range(.1f, 1f)] private float steepnessMod = 1f;
    [SerializeField, Range(1f, 20f)] private float transitionTime = 2f; 
    [SerializeField] private Slider UITimeSlider, UILevelSlider, UIModSlider;
    [SerializeField] private Text UITimeText, UILevelText, UIModText;

    // Update is called once per frame
    private void Update()
    {
        ChangeWave();
        UpdateUI(); 
    }

    private void UpdateUI()
    {
        UITimeText.text = transitionTime.ToString();
        UITimeSlider.value = transitionTime;
        UILevelText.text = newLevel.ToString();
        UILevelSlider.value = newLevel;
        UILevelSlider.maxValue = levels.Count - 1;
        UIModText.text = steepnessMod.ToString();
        UIModSlider.value = steepnessMod;
    }

    private void ChangeWave()
    {
        if (newLevel != currentLevel || lastMod != steepnessMod)
        {
            try
            {
                if (WaveManager.instance.ChangeWaveObject(levels[newLevel], transitionTime, steepnessMod))
                {
                    currentLevel = newLevel;
                    lastMod = steepnessMod;
                }
            }
            catch (System.Exception)
            {
                newLevel = -1;
                if (WaveManager.instance.ChangeWaveObject(null, transitionTime, 0))
                {
                    currentLevel = newLevel;
                    lastMod = steepnessMod;
                }
            }
        }  
    }

    public void EmergancyStop()
    {
        newLevel = -1;
        transitionTime = 1;
    }

    public void LevelUp()
    {
        newLevel ++;
    }

    public void LevelDown()
    {
        newLevel --;
    }

    public void LevelSelect(float _level)
    {
        newLevel = (int)_level;
    }

    public void SetTransitionTime(float time)
    {
        transitionTime = time;
    }

    public void SetSteepnessMod(float mod)
    {
        steepnessMod = mod;
    }
}
