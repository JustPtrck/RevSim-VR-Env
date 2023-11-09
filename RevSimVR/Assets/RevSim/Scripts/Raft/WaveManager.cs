using System.Collections.Generic;
using System;
using UnityEngine;


public class WaveManager : MonoBehaviour
{
    private enum WaveType{Sine, Gerstner}
    public static WaveManager instance;

    [SerializeField, Tooltip("Sets water height in meters (Units)")] 
    private float YOffset = 0f;    
    [SerializeField, Tooltip("Specifies wave type\nSine: Clean stackable Sinewaves\nGerstner: Sinewave that offsets XZ with Cosinewave\nPerlin: Sinewave with Perlin noise applied")] 
    private WaveType waveType = WaveType.Sine;
    [SerializeField] private Material waveMaterial;
    [SerializeField] private List<Wave> waves = new List<Wave>();
    private void Awake(){
        if (instance == null) instance = this;
        else if (instance != this)
        {
            Debug.Log("Insance already exists, destroying object");
            Destroy(this);
        }
    }

    private void Update(){
        UpdateWaterShader();
    }


    public Vector3 GetWaveDisplacement(Vector3 position, ref Vector3 normal)
    {
        // NORMALS RETURNED ARE INCORRECT
        Vector3 tangent = new Vector3();
        Vector3 binormal = new Vector3();
        Vector3 p = new Vector3(position.x, YOffset, position.z);
        switch(waveType){
            case WaveType.Gerstner:
            foreach (Wave wave in waves)
                p += GerstnerWave(wave, p, ref tangent, ref binormal);
            normal = Vector3.Cross(binormal, tangent);
            break;

            case WaveType.Sine:
            foreach (Wave wave in waves)
                p += SineWave(wave, p, ref tangent, ref binormal);
            normal = binormal;
            break;
        }
        return p;
    }

    public Vector3 GetWaveDisplacement(Vector3 position)
    {
        Vector3 normal = new Vector3();
        return GetWaveDisplacement(position, ref normal);
    }

    private Vector3 SineWave(Wave wave, Vector3 position, ref Vector3 tangent, ref Vector3 normal){
        // Calculates the displacement of a point based on a Sinewave with the input values of Wave
        // Returns Displacement Vector3:
        // X: 0 
        // Y: amplitude * sin(2 * pi / wavelenght * dot(direction, position.XZ) - speed * t)
        // Z: 0
        Vector2 direction = new Vector2(Mathf.Cos(wave.direction * Mathf.PI / 180), Mathf.Sin(wave.direction * Mathf.PI / 180));
        float steepness = wave.steepness;
        float wavelength = wave.wavelength;
        if (wave.wavelength == 0) wavelength = 1;
        float speed = wave.speed;

        float k = 2 * Mathf.PI / wavelength;
        Vector2 d = direction.normalized;
        float c = Mathf.Sqrt(9.8f / k) * speed;
        float f = k * (Vector2.Dot(d, new Vector2(position.x, position.z)) - c * Time.time);
        float a = steepness / k;

        tangent = new Vector3(1, d.x * steepness * Mathf.Cos(f), 0).normalized;
        normal += new Vector3(-tangent.y, tangent.x, 0);
        
        return new Vector3(0, a * Mathf.Sin(f), 0);
    }

    private Vector3 GerstnerWave(Wave wave, Vector3 position, ref Vector3 tangent, ref Vector3 binormal){
        // Calculates the displacement of a point based on a Gerstnerwave with the input values of Wave
        // Returns Displacement Vector3:
        // X: direction.x * amplitude * cos(2 * pi / wavelenght * dot(direction, position.XZ) - sqrt(9.8 / (2 * pi / wavelenght)) * speed * t)
        // Y: amplitude * sin(2 * pi / wavelenght * dot(direction, position.XZ) - sqrt(9.8 / (2 * pi / wavelenght)) * speed * t)
        // Z: direction.z * amplitude * cos(2 * pi / wavelenght * dot(direction, position.XZ) - sqrt(9.8 / (2 * pi / wavelenght)) * speed * t)

        // THIS FUNCTION IS BROKEN --> XZ is not moving in a waveform 
        Vector2 direction = new Vector2(Mathf.Cos(wave.direction * Mathf.PI / 180), Mathf.Sin(wave.direction * Mathf.PI / 180));
        float steepness = wave.steepness;
        float wavelength = wave.wavelength;
        if (wave.wavelength == 0) wavelength = 1;
        float speed = wave.speed;

        float k = 2 * Mathf.PI / wavelength;
        float c = Mathf.Sqrt(9.8f / k) * speed;
        Vector2 d = direction.normalized;
        float f = k * (Vector2.Dot(d, new Vector2(position.x, position.z)) - c * Time.time);
        float a = steepness / k;
        
        tangent += new Vector3(
            -d.x * d.x * (steepness * Mathf.Sin(f)),
            d.x * (steepness * Mathf.Cos(f)),
            -d.x * d.y * (steepness * Mathf.Sin(f))
        );
        binormal += new Vector3(
            -d.x * d.y * (steepness *  Mathf.Sin(f)),
            d.y * (steepness *  Mathf.Cos(f)),
            -d.y * d.y * (steepness *  Mathf.Sin(f))
        );
        return new Vector3(
            d.x * (a * Mathf.Cos(f)),
            a * Mathf.Sin(f),
            d.y * (a * Mathf.Cos(f))
        );
    }


    private void UpdateWaterShader(){
        List<Vector4> temp = new List<Vector4>();
        foreach (Wave wave in waves){
            temp.Add(new Vector4(wave.direction, wave.steepness, wave.wavelength, wave.speed));
        }
        
        waveMaterial.SetVectorArray("_Waves", temp);
        waveMaterial.SetInt("_WaveAmount", temp.Count);
        waveMaterial.SetInt("_WaveType", (int)waveType);
        waveMaterial.SetFloat("_YOffset", YOffset);
        waveMaterial.SetFloat("_SyncedTime", Time.time);
    }

    [Serializable]
    public struct Wave{
        [Range(0, 360), Tooltip("Sets the direction of the wave with XZ vector in degrees")]
        public float direction;
        [Range(0,1), Tooltip("Determines the steepness and height of the waves\nSum of wave.steepness should be lower than 1.0")] 
        public float steepness;
        [Range(1,100), Tooltip("Sets the wavelenght of each wave")] 
        public float wavelength;
        [Range(0,10), Tooltip("Speed modifier of the wave offset")] 
        public float speed;
    };
}