using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;



namespace JustPtrck.Shaders.Water
{
    [
        CreateAssetMenu(fileName = "New WaveObject", 
                        menuName = "JustPtrck/Water/WaveObject"),
        Serializable
    ]
    public class WaveObject : ScriptableObject 
    {
        // TODO Check if works
        [SerializeField] private List<Wave> waves = new List<Wave>();
        public List<Wave> Waves{ get{return waves;} }
    }


    public enum WaveType{Sine, Gerstner}

    [Serializable]
    public struct Wave{
        public WaveType waveType;
        [Range(0, 360), Tooltip("Sets the direction of the wave with XZ vector in degrees\n 0 degrees: x=1, z=0")]
        public float direction;
        [Range(0,1), Tooltip("Determines the steepness and height of the waves\nSum of wave.steepness should be lower than 1.0")] 
        public float steepness;
        [Range(1,100), Tooltip("Sets the wavelenght of each wave")] 
        public float wavelength;
        [Range(0,10), Tooltip("Speed modifier of the wave offset")] 
        // IDEA: Change speed to offset
        public float speed;
    };
}