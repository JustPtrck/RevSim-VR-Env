using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Player Info", menuName = "RevSim/PlayerInfo")]
public class PlayerObject : ScriptableObject 
{
    public string playerName;
    [Range(1f, 2.5f)] public float height = 1.85f;

    // Sizes used source: https://www.pellein.com/en_nl/men-size-guide/
    [Range(0.1f, 1.5f)] public float torsoHeight = .6f, shoulderWidth = .46f, armLength = .6f, neckLength = .15f;
    

}