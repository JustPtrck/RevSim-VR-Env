using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JustPtrck.Shaders.Water;
using UnityEngine.UI;
using System;

/// <summary>
/// A Class that updates the WaveManager.
/// TODO This could be more robust
/// </summary>
public class LevelManager : MonoBehaviour
{
    [SerializeField] private List<WaveObject> levels = new List<WaveObject>();
    [SerializeField] private int currentLevel = -1;
    [SerializeField] private int newLevel = 0;
    [SerializeField] private float lastMod = 1f;
    [SerializeField, Range(.1f, 1f)] private float steepnessMod = 1f;
    [SerializeField, Range(1f, 20f)] private float transitionTime = 2f; 
    public Tuple<float, int, float> values {get{return new Tuple<float, int, float> (transitionTime, newLevel, steepnessMod);}}
    
    
    private void Update()
    {
        ChangeWave();
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
        steepnessMod = 0;
    }

    public void LevelUp()
    {
        newLevel ++;
        try
        {
            WaveObject temp = levels[newLevel];
        }
        catch (System.Exception)
        {
            newLevel = -1;
        }
    }

    public void LevelDown()
    {
        newLevel --;
        try
        {
            WaveObject temp = levels[newLevel];
        }
        catch (System.Exception)
        {
            newLevel = -1;
        }
    }

    public void LevelSelect(float _level)
    {
        newLevel = (int)_level;
        try
        {
            WaveObject temp = levels[newLevel];
        }
        catch (System.Exception)
        {
            newLevel = -1;
        }
    }

    public void SetTransitionTime(float time)
    {
        if (time >= 1)
            transitionTime = time;
    }

    public void SetSteepnessMod(float mod)
    {
        if (mod >= 0 && mod <= 1)
            steepnessMod = mod;
    }
}
