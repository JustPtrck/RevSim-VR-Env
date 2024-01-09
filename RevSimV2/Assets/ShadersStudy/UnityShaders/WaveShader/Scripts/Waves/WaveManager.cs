using System.Collections.Generic;
using System;
using UnityEngine;

namespace JustPtrck.Shaders.Water{
        
    /// <summary>
    /// A single instance Class (per Scene) which manages waves across the Scene. <para/>
    /// LIST WaveManager <br/>
    /// [x] Move all calculations to GPU
    /// [ ] Make sure Wavelenght cannot be 0
    /// [ ] Make sure um of Steepness cannot be higher than 1.0
    /// [ ] Use buffers to communicate waves
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager instance;

        [SerializeField, Tooltip("Sets water height in meters (Units)")] 
        private float YOffset = 0f;    
        [SerializeField, Range(0f, 1f)] private float steepnessMod = 1f;
        [SerializeField] private WaveObject safe; 
        [SerializeField] private WaveObject waveObject = null;
        [SerializeField] private List<ImpactWave> impactWaves = new List<ImpactWave>();
        [SerializeField] private Material waveMaterial;
        [SerializeField] private ComputeShader computeShader;

        
        private void Awake(){
            if (instance == null) instance = this;
            else if (instance != this)
            {
                Debug.Log("Insance already exists, destroying object");
                Destroy(this);
            }
        }

        private void FixedUpdate(){
            if (waveObject == null)
                waveObject = safe;
            UpdateGPUValues(waveMaterial);
        }

        public Vector3 GetDisplacementFromGPU(Vector3 position)
        {
            Vector3 normal = Vector3.zero;
            return GetDisplacementFromGPU(position, ref normal);
        }
        private DN[] data;
        /// <summary>
        /// IDEA Look into how Buffers work!
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector3 GetDisplacementFromGPU(Vector3 position, ref Vector3 normal)
        {
            UpdateGPUValues(computeShader);
            DN dn = new DN();
            dn.vertex_in = position;
            DN[] temp = {dn};
            data = temp;
            int dnSize = sizeof(float) * 9;
            ComputeBuffer dnBuffer = new ComputeBuffer(data.Length, dnSize);

            dnBuffer.SetData(data);
            computeShader.SetFloat("time_in", Shader.GetGlobalVector("_Time").y);
            computeShader.SetBuffer(0, "dnBuffer", dnBuffer);
            computeShader.Dispatch(0, 1, 1, 1);

            dnBuffer.GetData(data);
            //Debug.Log($"{data[0].vertex_in}\n{data[0].displacement}\n{data[0].normal}");
            normal = data[0].normal;
            Vector3 displacement = data[0].displacement;

            dnBuffer.Dispose();
            return displacement;
        }

        private void UpdateGPUValues(ComputeShader GPULocation){
            // This Method updates the variables in the GetWaveDisplacement.hlsl Sub-Shader
            // Communicates the variables from CPU to GPU

            Vector4[] gerstner = new Vector4[10];
            Vector4[] sine = new Vector4[10];
            Vector4[] impact = new Vector4[10];
            float[] impactTimes = new float[10];

            int gerstner_idx = 0;
            int sine_idx = 0;
            int impact_idx = 0;

            foreach (Wave wave in waveObject.Waves){
                if (wave.waveType == WaveType.Gerstner) {
                    gerstner[gerstner_idx] = new Vector4(wave.direction, wave.steepness * steepnessMod, wave.wavelength, wave.speed);
                    gerstner_idx++;
                }
                else if (wave.waveType == WaveType.Sine) {
                    sine[sine_idx] = new Vector4(wave.direction, wave.steepness * steepnessMod, wave.wavelength, wave.speed);
                    sine_idx++;
                }
            }

            foreach (ImpactWave wave in impactWaves){
                if (Time.time - wave.timestamp > wave.duration) impactWaves.Remove(wave);
                else{
                    impact[impact_idx] = new Vector4(wave.origin.x, wave.origin.y, wave.steepness * steepnessMod * (wave.duration - (Time.time - wave.timestamp))/wave.duration, wave.wavelength);
                    impactTimes[impact_idx] = wave.timestamp;   
                    impact_idx++;
                }
            }
            
            for(int i = gerstner_idx; i < 10; i++) gerstner[i] = new Vector4(0, 0, 10, 1);
            for(int i = sine_idx; i < 10; i++) sine[i] = new Vector4(0, 0, 10, 1);
            for(int i = impact_idx; i < 10; i++) {
                impact[i] = new Vector4(0, 0, 10, 1);
                impactTimes[i] = 0;
            }
            
            GPULocation.SetInt("_GerstnerAmount", gerstner_idx);
            GPULocation.SetInt("_SineAmount", sine_idx);
            GPULocation.SetInt("_ImpactAmount", impact_idx);

            GPULocation.SetVectorArray("_GerstnerWaves", gerstner);
            GPULocation.SetVectorArray("_SineWaves", sine);
            GPULocation.SetVectorArray("_ImpactWaves", impact);
            GPULocation.SetFloats("_ImpactTimes", impactTimes);

            GPULocation.SetFloat("_YOffset", YOffset);
            GPULocation.SetFloat("_SyncedTime", Time.time);
        }
        private void UpdateGPUValues(Material GPULocation){
            // This Method updates the variables in the GetWaveDisplacement.hlsl Sub-Shader
            // Communicates the variables from CPU to GPU

            Vector4[] gerstner = new Vector4[10];
            Vector4[] sine = new Vector4[10];
            Vector4[] impact = new Vector4[10];
            float[] impactTimes = new float[10];

            int gerstner_idx = 0;
            int sine_idx = 0;
            int impact_idx = 0;

            foreach (Wave wave in waveObject.Waves){
                if (wave.waveType == WaveType.Gerstner) {
                    gerstner[gerstner_idx] = new Vector4(wave.direction, wave.steepness * steepnessMod, wave.wavelength, wave.speed);
                    gerstner_idx++;
                }
                else if (wave.waveType == WaveType.Sine) {
                    sine[sine_idx] = new Vector4(wave.direction, wave.steepness * steepnessMod, wave.wavelength, wave.speed);
                    sine_idx++;
                }
            }

            foreach (ImpactWave wave in impactWaves){
                if (Time.time - wave.timestamp > wave.duration) impactWaves.Remove(wave);
                else{
                    impact[impact_idx] = new Vector4(wave.origin.x, wave.origin.y, wave.steepness * steepnessMod * (wave.duration - (Time.time - wave.timestamp))/wave.duration, wave.wavelength);
                    impactTimes[impact_idx] = wave.timestamp;   
                    impact_idx++;
                }
            }
            
            for(int i = gerstner_idx; i < 10; i++) gerstner[i] = new Vector4(0, 0, 10, 1);
            for(int i = sine_idx; i < 10; i++) sine[i] = new Vector4(0, 0, 10, 1);
            for(int i = impact_idx; i < 10; i++) {
                impact[i] = new Vector4(0, 0, 10, 1);
                impactTimes[i] = 0;
            }
            
            GPULocation.SetInt("_GerstnerAmount", gerstner_idx);
            GPULocation.SetInt("_SineAmount", sine_idx);
            GPULocation.SetInt("_ImpactAmount", impact_idx);

            GPULocation.SetVectorArray("_GerstnerWaves", gerstner);
            GPULocation.SetVectorArray("_SineWaves", sine);
            GPULocation.SetVectorArray("_ImpactWaves", impact);
            GPULocation.SetFloatArray("_ImpactTimes", impactTimes);

            GPULocation.SetFloat("_YOffset", YOffset);
            GPULocation.SetFloat("_SyncedTime", Time.time);
        }

        public bool ChangeWaveObject(WaveObject _wave, float transitionTime, float maxSteepness)
        {
            float steepnessSpeed = 2/transitionTime;
            if (_wave == null)
                _wave = safe;

            if (steepnessMod < 0 && waveObject != _wave)
                steepnessMod = 0;
            else if (steepnessMod > 0 && waveObject != _wave)
                steepnessMod -= steepnessSpeed * Time.deltaTime;
            else if (steepnessMod == 0 && waveObject != _wave)
                waveObject = _wave;
            else if (steepnessMod > maxSteepness + .01f && waveObject == _wave)
                steepnessMod -= steepnessSpeed * Time.deltaTime;
            else if (steepnessMod < maxSteepness - .01f && waveObject == _wave)
                steepnessMod += steepnessSpeed * Time.deltaTime;
            else if (steepnessMod > maxSteepness && waveObject == _wave)
                steepnessMod = maxSteepness;
            else if (steepnessMod >= 0 && waveObject == _wave)
                steepnessMod += steepnessSpeed * Time.deltaTime;

            if (steepnessMod == maxSteepness && waveObject == _wave)
                return true;
            else 
                return false;
        }



        /// <summary>
        /// Create an Impact Wave in the WaveManager. 
        /// This is a Ripple Gerstnerwave going outwards in the XZ directions from the Origin. This wave will be deleted from the list after the Duration is surpassed.
        /// </summary>
        /// <param name="origin">The Origin position of the Wave.</param>
        /// <param name="steepness">The Steepness of the wave. This value should be in the range of 0.0 to 1.0!</param>
        /// <param name="wavelength">The Wavelength of the wave. The bigger the Wavelength the bigger the wave.</param>
        /// <param name="duration">The Duration of the wave in seconds. The Wave's Steepness will be dimmed when the elapsed time approaches the Duration. When the elapsed time surpasses this the wave will be deleted.</param>
        public void CreateImpactWave(Vector3 origin, float steepness, float wavelength, float duration){
            ImpactWave newWave = new ImpactWave();
            newWave.origin = new Vector2(origin.x, origin.z);
            newWave.steepness = steepness;
            newWave.wavelength = wavelength;
            newWave.duration = duration;
            newWave.timestamp = Time.time;
            impactWaves.Add(newWave);
        }

        public struct DN{
            public Vector3 vertex_in;
            public Vector3 displacement;
            public Vector3 normal;
        }

        public struct ImpactWave{
            public Vector2 origin;
            public float steepness;
            public float wavelength;
            public float timestamp;
            public float duration;
        };
    }
}