
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

        [Header("Physics Settings")]
        [SerializeField] private float depthBeforeSubmerged = 1f;
        [SerializeField] private float displacementAmount = 3f;
        [SerializeField] private float waterDrag = 0.99f;
        [SerializeField] private float waterAngularDrag = 0.5f;
        [SerializeField] private Transform modelTransform;
        
        [Header("Ideal Settings")]
        [SerializeField] private Vector3 anchorPoint = Vector3.zero;
        [Header("Debugging")]
        [SerializeField, Tooltip("Toggles Draw Gizmos for the floaters")] private bool showFloaters;
        [SerializeField, Tooltip("Toggles Draw Gizmos for the meanVector")] private bool showMeanVector;
        [SerializeField, Tooltip("Toggles Debug for the normals")] private bool showNormals;
        [SerializeField, Tooltip("Toggles Debug for the normals")] private bool showFloaterDisplacement;

        private Vector3 meanVector;
        private Vector3 meanNormal;
        private List<Vector3> points;

        public FloaterType state{get{return floaterType;}}

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
                Vector3 floaterPos = anchorPoint + Vector3.Scale(transform.localScale, floater.localPosition);
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
            if (showNormals) Debug.DrawRay(transform.position, meanNormal, Color.red);
        }

        /// <summary>
        /// This method uses a Rigidbody for the floating Physics. 
        /// It calls on the WaveManager instance to get the wave height and apply boyancy force on those positions, opposing the gravitational forces.<para/>
        /// FIX Update Method so the GameObject has proper gravity physics <br/>
        /// IDEA Comlete revision
        /// </summary>
        private void SimulatePhysics(){
            foreach (Transform floater in floaters)
            {
                Vector3 floaterPos = modelTransform.position + Vector3.Scale(modelTransform.localScale, floater.localPosition);
                Vector3 displacement = WaveManager.instance.GetDisplacementFromGPU(floaterPos) - new Vector3(modelTransform.position.x, 0, modelTransform.position.z);
                if (floater.position.y < displacement.y)
                {
                    float displacementMultiplier = Mathf.Clamp01((displacement.y - floater.position.y) / depthBeforeSubmerged) * displacementAmount;
                    Vector3 buoyancyForce = new Vector3(0, Mathf.Abs(Physics.gravity.y) / floaters.Count, 0) * displacementMultiplier;
                    rb.AddForceAtPosition(buoyancyForce, floater.position, ForceMode.Acceleration);
                    Debug.DrawLine(floater.position + buoyancyForce, floater.position, Color.red);
                    // rb.AddForceAtPosition(new Vector3(-displacement.x, Mathf.Abs(Physics.gravity.y), -displacement.z) * displacementMultiplier, floaterPos, ForceMode.Acceleration);
                    rb.AddForce(displacementMultiplier * -rb.velocity * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
                    rb.AddTorque(displacementMultiplier * -rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
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
