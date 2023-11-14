
using System.Collections.Generic;
using UnityEngine;


public class Floater : MonoBehaviour
{
    private enum FloaterType{Physics, Ideal};

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
    
    [Header("Ideal Settings")]
    [SerializeField] private Vector2 anchorPoint = new Vector2(0,0);
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
    }

    
    private void FixedUpdate() {
        switch(floaterType){
            case FloaterType.Physics:
            SimulatePhysics();
            break;

            case FloaterType.Ideal:
            CalculatePosition();
            break;
        }
    }


    private void CalculatePosition(){
        // THIS FUNCTION SETS THE POSITION AND THE ROTATION OF THE PARENT OBJECT BASED ON THE WaveManager.cs
        // Calculates the position and normal each floater should be at according to the waves
        // Sets the position of the object in the mean center of the floaters and sets the normal 

        Vector3 sumVector = Vector3.zero;
        Vector3 sumNormals = Vector3.zero;
        points = new List<Vector3>(); 

        foreach (Transform floater in floaters)
        {
            Vector3 normal = new Vector3();
            Vector3 XZ_placement = new Vector3(transform.position.x - anchorPoint.x, 0, transform.position.z - anchorPoint.y);
            Vector3 point = WaveManager.instance.GetWaveDisplacement(floater.position, ref normal) - XZ_placement;
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

    private void SimulatePhysics(){
        // Gets the height of the waves at each floater position
        // Uses this to apply upward force to the object in the floater positions opposing the gravitational forces

        foreach (Transform floater in floaters)
        {
            Vector3 floaterPos = floater.position;
            rb.AddForceAtPosition(Physics.gravity/floaters.Count, floaterPos);
            float waveHeight = WaveManager.instance.GetWaveDisplacement(floaterPos).y;
            if (floaterPos.y < waveHeight)
            {
                float displacementMultiplier = Mathf.Clamp01((waveHeight - floaterPos.y) / depthBeforeSubmerged) * displacementAmount;
                rb.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), floaterPos,ForceMode.Acceleration);
                rb.AddForce(displacementMultiplier * -rb.velocity * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
                rb.AddTorque(displacementMultiplier * -rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
            }
        }
    }

    private void OnDrawGizmos() {
        
        Gizmos.color = Color.red;
        if (showMeanVector)
        {   
            Gizmos.DrawSphere(meanVector, .05f);
        }
            
        int i = 0;
        foreach (Vector3 point in points)
        {
            Gizmos.color = Color.blue;
            if (showFloaters) Gizmos.DrawSphere(floaters[i].position, .05f);
            Gizmos.color = Color.yellow;
            if (showFloaterDisplacement)
            {
                Gizmos.DrawLine(point, floaters[i].position);
                Gizmos.DrawSphere(point, .05f);
            }
            i++;
        }
        

    }
}
