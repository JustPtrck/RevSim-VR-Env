using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public Text text;
    private int m_frameCounter = 0;
    private float m_timeCounter = 0.0f;
    private float m_lastFramerate = 0.0f;
    [Range(.1f, 3f)]public float m_refreshTime = 0.5f;

    void Update()
    {
        if( m_timeCounter < m_refreshTime )
        {
            m_timeCounter += Time.deltaTime;
            m_frameCounter++;
        }
        else
        {
            //This code will break if you set your m_refreshTime to 0, which makes no sense.
            m_lastFramerate = (float)m_frameCounter/m_timeCounter;
            text.text = $"FPS: {(m_lastFramerate):0.0}"; 
            m_frameCounter = 0;
            m_timeCounter = 0.0f;
        }
    }
}

