using System.Collections.Generic;
using System;
using UnityEngine;

namespace JustPtrck.Shaders.Water{
    /// <summary>
    /// A single instance Class (per Scene) which manages waves across the Scene. <para/>
    /// LIST WaveManager <br/>
    /// [x] Make sure CPU and GPU get the exact same displacement
    /// [ ] Make sure Wavelenght cannot be 0
    /// [ ] Make sure um of Steepness cannot be higher than 1.0
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        public enum WaveType{Sine, Gerstner}
        public static WaveManager instance;

        [SerializeField, Tooltip("Sets water height in meters (Units)")] 
        private float YOffset = 0f;    
        [SerializeField] private Material waveMaterial;
        [SerializeField] private List<Wave> waves = new List<Wave>();
        [SerializeField] private List<ImpactWave> impactWaves = new List<ImpactWave>();

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
            UpdateGPUValues(waveMaterial);
        }

        /// <summary>
        /// This Method calculates the position displacement of the input Position and updates the Normal at this position.
        /// This Method makes a sum of the Waves in the Waves List and ImpactWaves List at the input Position. <br />
        /// Including a max of 10 SineWaves, 10 GerstnerWaves and 10 ImpactWaves. 
        /// </summary>
        /// <param name="position">Position on which the wave displacement should be calculated.</param>
        /// <param name="normal">Normal Vector that should be updated. To the displacement normal at "position"</param>
        /// <returns>A Vector3 (Position) containing a sum of all wave displacements and the input position. Which indicates the height of the wave at the Input position.</returns>
        public Vector3 GetWaveDisplacement(Vector3 position, ref Vector3 normal)
        {
            Vector3 tangent = new Vector3(1,0,0);
            Vector3 binormal = new Vector3(0,0,1);
            Vector3 p = new Vector3(position.x, YOffset, position.z);
            foreach(Wave wave in waves)
                switch(wave.waveType){
                    case WaveType.Gerstner:
                        p += GerstnerWave(wave, p, ref tangent, ref binormal);
                    break;

                    case WaveType.Sine:
                        p += SineWave(wave, p, ref tangent, ref binormal);
                    break;
                }
            foreach (ImpactWave wave in impactWaves)
            {
                p += ImpactRippleWave(wave, p, ref tangent, ref binormal);
            }
            normal = Vector3.Cross(binormal, tangent).normalized;
            return p;
        }

        public Vector3 GetWaveDisplacement(Vector3 position)
        {
            Vector3 normal = new Vector3();
            return GetWaveDisplacement(position, ref normal);
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

        private Vector3 SineWave(Wave wave, Vector3 position, ref Vector3 tangent, ref Vector3 normal){
            // Calculates the displacement of a point based on a Sinewave with the input values of Wave
            // Returns Displacement Vector3:
            // X: 0 
            // Y: amplitude * sin(2 * pi / wavelenght * dot(direction, position.XZ) - speed * t)
            // Z: 0
            Vector2 direction = new Vector2(Mathf.Cos(wave.direction * Mathf.PI / 180), Mathf.Sin(wave.direction * Mathf.PI / 180));
            float steepness = wave.steepness;
            float wavelength = wave.wavelength;
            if (wave.wavelength == 0) wavelength = 1f;
            float speed = wave.speed;

            float k = 2 * Mathf.PI / wavelength;
            Vector2 d = direction.normalized;
            float c = Mathf.Sqrt(9.8f / k) * speed;
            float f = k * (Vector2.Dot(d, new Vector2(position.x, position.z)) - c * Time.time);
            float a = steepness / k;

            tangent += new Vector3(0, d.x * (steepness * Mathf.Cos(f)),0);
            normal += new Vector3(0, d.y * (steepness * Mathf.Cos(f)), 0);
            
            return new Vector3(0, a * Mathf.Sin(f), 0);
        }

        private Vector3 GerstnerWave(Wave wave, Vector3 position, ref Vector3 tangent, ref Vector3 binormal){
            // Calculates the displacement of a point based on a Gerstnerwave with the input values of Wave
            // Returns Displacement Vector3:
            // X: direction.x * amplitude * cos(2 * pi / wavelenght * dot(direction, position.XZ) - sqrt(9.8 / (2 * pi / wavelenght)) * speed * t)
            // Y: amplitude * sin(2 * pi / wavelenght * dot(direction, position.XZ) - sqrt(9.8 / (2 * pi / wavelenght)) * speed * t)
            // Z: direction.z * amplitude * cos(2 * pi / wavelenght * dot(direction, position.XZ) - sqrt(9.8 / (2 * pi / wavelenght)) * speed * t)

            Vector2 direction = new Vector2(Mathf.Cos(wave.direction * Mathf.PI / 180), Mathf.Sin(wave.direction * Mathf.PI / 180));
            float steepness = wave.steepness;
            float wavelength = wave.wavelength;
            if (wave.wavelength == 0) wavelength = 1f;
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


        private Vector3 ImpactRippleWave(ImpactWave wave, Vector3 position, ref Vector3 tangent, ref Vector3 binormal) {
            float elapsed_time = Time.time - wave.timestamp;
            float steepness = wave.steepness * (wave.duration - (Time.time - wave.timestamp))/wave.duration;
            float wavelength = wave.wavelength;
            float delay = 3/wavelength;
            float k = 2 * Mathf.PI / (wavelength);
            float a = steepness / k ;
            Vector2 d = (new Vector2(position.x, position.y) - wave.origin).normalized;
            float c = Mathf.Sqrt(9.8f / k);
            
            float dist = Vector2.Distance(wave.origin, new Vector2(position.x, position.y));
            float maxDist = a * wavelength;
            float decay = (maxDist - dist) / maxDist;

            float f = k * (dist - elapsed_time * c) ;

            if (dist < maxDist * ( elapsed_time / (delay + elapsed_time) ) ){
                tangent += new Vector3(
                    -d.x * d.x * (steepness * decay * Mathf.Sin(f)),
                    d.x * (steepness * decay * Mathf.Cos(f)),
                    -d.x * d.y * (steepness * decay * Mathf.Sin(f))
                );
                binormal += new Vector3(
                    -d.x * d.y * (steepness * decay * Mathf.Sin(f)),
                    d.y * (steepness * decay * Mathf.Cos(f)),
                    -d.y * d.y * (steepness * decay * Mathf.Sin(f))
                ); 
                return new Vector3(
                    d.x * a * decay * Mathf.Cos(f), 
                    a * decay * Mathf.Sin(f), 
                    d.y * a * decay * Mathf.Cos(f)
                );
            }
            return Vector3.zero;
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

            foreach (Wave wave in waves){
                if (wave.waveType == WaveType.Gerstner) {
                    gerstner[gerstner_idx] = new Vector4(wave.direction, wave.steepness, wave.wavelength, wave.speed);
                    gerstner_idx++;
                }
                else if (wave.waveType == WaveType.Sine) {
                    sine[sine_idx] = new Vector4(wave.direction, wave.steepness, wave.wavelength, wave.speed);
                    sine_idx++;
                }
            }

            foreach (ImpactWave wave in impactWaves){
                if (Time.time - wave.timestamp > wave.duration) impactWaves.Remove(wave);
                else{
                    impact[impact_idx] = new Vector4(wave.origin.x, wave.origin.y, wave.steepness * (wave.duration - (Time.time - wave.timestamp))/wave.duration, wave.wavelength);
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

            foreach (Wave wave in waves){
                if (wave.waveType == WaveType.Gerstner) {
                    gerstner[gerstner_idx] = new Vector4(wave.direction, wave.steepness, wave.wavelength, wave.speed);
                    gerstner_idx++;
                }
                else if (wave.waveType == WaveType.Sine) {
                    sine[sine_idx] = new Vector4(wave.direction, wave.steepness, wave.wavelength, wave.speed);
                    sine_idx++;
                }
            }

            foreach (ImpactWave wave in impactWaves){
                if (Time.time - wave.timestamp > wave.duration) impactWaves.Remove(wave);
                else{
                    impact[impact_idx] = new Vector4(wave.origin.x, wave.origin.y, wave.steepness * (wave.duration - (Time.time - wave.timestamp))/wave.duration, wave.wavelength);
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