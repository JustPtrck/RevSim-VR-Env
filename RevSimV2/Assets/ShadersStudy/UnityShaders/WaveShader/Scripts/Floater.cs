
using System.Collections.Generic;
using UnityEngine;

namespace JustPtrck.Shaders.Water{
    public class Floater : MonoBehaviour
    {
        public enum FloaterType{Physics, Ideal};

        // Private variables
        private Rigidbody rb;

        [Header("Setup Floaters")]

        [SerializeField, Tooltip("Tips for setting up Floaters:\n1. Floaters need to be child objects to this object\n2. Place Floaters all on the same Y-level (Optional)\n3. The only data used from Floaters is their XZ position")] 
        private List<Transform> floaters = new List<Transform>();
        [SerializeField, Tooltip("Choose floating type\n1. Physics: Transform based on simulated Physics\n2. Ideal: Transform based on Geometry")] 
        private FloaterType floaterType = FloaterType.Ideal;

        public FloaterType floatType{get{return floaterType;}}
        [System.Serializable]
        public struct Anchor {
            public Vector3 position;
            public float rotationAngle;
        };

        [Header("Physics Settings")]
        [SerializeField] private float depthBeforeSubmerged = 1f;
        [SerializeField] private float displacementAmount = 3f;
        [SerializeField] private float boyancyMod = 2f;
        [SerializeField] private float waterDrag = 0.99f;
        [SerializeField] private float waterAngularDrag = 0.5f;
        [SerializeField] private Transform modelTransform;
        
        [Header("Ideal Settings")]
        [SerializeField] private Anchor anchor = new Anchor();
        public Anchor anchorPoint {get{ return anchor;} set{anchor = value;}}
        [Header("Debugging")]
        [SerializeField, Tooltip("Toggles Draw Gizmos for the floaters")] private bool showFloaters;
        [SerializeField, Tooltip("Toggles Draw Gizmos for the meanVector")] private bool showMeanVector;
        [SerializeField, Tooltip("Toggles Debug for the normals")] private bool showNormals;
        [SerializeField, Tooltip("Toggles Debug for the normals")] private bool showFloaterDisplacement;

        private Vector3 meanVector;
        private Vector3 meanNormal;
        private List<Vector3> points;

        private void Start() {
            if (floaters.Count <= 0) floaters.Add(transform);        
            rb = gameObject.GetComponent<Rigidbody>() ? gameObject.GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
        }

        private void FixedUpdate() {
            switch(floaterType){
                case FloaterType.Physics:
                rb.useGravity = true;
                SimulatePhysics();
                break;

                case FloaterType.Ideal:
                rb.useGravity = false;
                CalculatePosition();
                break;
            }
        }

        /// <summary>
        /// This Method calculates the Transform for the "floating" GameObject based on the Floaters Transforms.
        /// The Method calls on the WaveManager instance for the displacement and normal on each floater position.<para/>
        /// BUG The results are off when using Impact waves,
        /// Also teleporting happens
        /// </summary>
        private void CalculatePosition(){
            Vector3 sumVector = Vector3.zero;
            Vector3 sumNormals = Vector3.zero;
            points = new List<Vector3>(); 

            foreach (Transform floater in floaters)
            {
                Vector3 normal = new Vector3();
                Vector3 floaterPos = anchor.position + Vector3.Scale(transform.localScale, floater.localPosition);
                Vector3 point = WaveManager.instance.GetDisplacementFromGPU(floaterPos, ref normal) ;
                points.Add(point);
                sumVector += point;
                sumNormals += normal;
                if (showNormals) Debug.DrawRay(point, normal, Color.yellow);
            }
            meanVector = sumVector / floaters.Count;
            meanNormal = sumNormals / floaters.Count;
            transform.position = meanVector;
            transform.up = meanNormal;
            Vector3 temp = transform.position + new Vector3(Mathf.Sin(anchor.rotationAngle), 0f, Mathf.Cos(anchor.rotationAngle)).normalized;
            transform.Rotate(transform.up, anchor.rotationAngle, Space.World);
            if (showNormals) Debug.DrawRay(transform.position, meanNormal, Color.red);
        }

        /// <summary>
        /// This method uses a Rigidbody for the floating Physics. 
        /// It calls on the WaveManager instance to get the wave height and apply boyancy force on those positions, opposing the gravitational forces.<para/>
        /// FIX Update Method so the GameObject has proper gravity physics <br/>
        /// FIX Only uses height which is ONLY SINE WAVES
        /// </summary>
        private void SimulatePhysics(){
            foreach (Transform floater in floaters)
            {
                Vector3 floaterPos = floater.position;
                Vector3 displacement = WaveManager.instance.GetDisplacementFromGPU(floaterPos) - new Vector3(floater.position.x, 0, floater.position.z);

                float temp = Mathf.Clamp(displacement.y - floater.position.y, 0f, 5f) * boyancyMod;
                // ERROR FIX THIS SHIT AAAAHHHH
                if (floater.position.y < displacement.y)
                {
                    // NEW
                    float buoyancyForce = Mathf.Abs(Physics.gravity.y) * ((displacement.y - floater.position.y) * boyancyMod + 1);
                    Vector3 buoyancyVector = new Vector3(0, buoyancyForce, 0) / floaters.Count;
                    rb.AddForceAtPosition(buoyancyVector, floater.position, ForceMode.Acceleration);


                    float displacementMultiplier = (Mathf.Clamp01(displacement.y - floater.position.y) / depthBeforeSubmerged) * displacementAmount;
                    // Vector3 buoyancyVector = new Vector3(displacement.x / 2, Mathf.Abs(Physics.gravity.y) * temp, displacement.z / 2) / floaters.Count * displacementMultiplier;
                    // rb.AddForceAtPosition(buoyancyVector, floater.position, ForceMode.Force);
                    Debug.DrawLine(floater.position + buoyancyVector, floater.position, Color.red);
                    // rb.AddForceAtPosition(new Vector3(-displacement.x, Mathf.Abs(Physics.gravity.y), -displacement.z) * displacementMultiplier, floaterPos, ForceMode.Acceleration);
                    rb.AddForce(displacementMultiplier * -rb.velocity * waterDrag * Time.fixedDeltaTime / floaters.Count, ForceMode.VelocityChange);
                    rb.AddTorque(displacementMultiplier * -rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime / floaters.Count, ForceMode.VelocityChange);
                }
            }
        }

        // private void OnDrawGizmos() {
            
        //     Gizmos.color = Color.red;
        //     if (showMeanVector)
        //     {   
        //         Gizmos.DrawSphere(meanVector, .05f);
        //     }
                
        //     int i = 0;
        //     foreach (Vector3 point in points)
        //     {
        //         Gizmos.color = Color.blue;
        //         if (showFloaters) Gizmos.DrawSphere(floaters[i].position, .05f);
        //         Gizmos.color = Color.yellow;
        //         if (showFloaterDisplacement)
        //         {
        //             Gizmos.DrawLine(point, floaters[i].position);
        //             Gizmos.DrawSphere(point, .05f);
        //         }
        //         i++;
        //     }
            

        // }
    }
}
