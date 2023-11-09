
using System.Collections.Generic;
using UnityEngine;


public class Floater : MonoBehaviour
{
    // Private variables
    private Rigidbody rb;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    [Header("Setup Floaters")]

    [SerializeField, Tooltip("Tips for setting up Floaters:\n1. Floaters NEED to be arranged clockwise\n2. Floaters need to be child objects to this object\n3. Place Floaters all on the same Y-level (Optional)\n4. The only data used from Floaters is their XZ position")] 
    private List<Transform> floaters = new List<Transform>();
    [SerializeField, Tooltip("Toggles between:\n1. True: Transform based on Physics\n2. False: Transform based on Geometry")] 
    private bool simulatePhysics = true;

    [Header("Physics")]
    [SerializeField] private float depthBeforeSubmerged = 1f;
    [SerializeField] private float displacementAmount = 3f;
    [SerializeField] private float waterDrag = 0.99f;
    [SerializeField] private float waterAngularDrag = 0.5f;

    [Header("Visualization")]
    [SerializeField, Tooltip("Toggles the MeshRenderer for the Mesh Object")] private bool showMesh;
    [SerializeField, Tooltip("Empty object which gets used to draw Mesh (Debugging)")] 
    private GameObject meshObject;
    [SerializeField, Tooltip("Material used for the Mesh")] private Material meshMat;
    [SerializeField, Tooltip("Toggles Draw Gizmos for the floaters")] private bool showFloaters;
    [SerializeField, Tooltip("Toggles Draw Gizmos for the meanVector")] private bool showMeanVector;


    private Vector3 meanVector;

    private void Start() {
        if (floaters.Count <= 0) floaters.Add(transform);
        mesh = new Mesh();
        mesh.name = meshObject.name;
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = meshMat ?? new Material(Shader.Find("ProceduralGrid"));
        meshFilter = meshObject.AddComponent<MeshFilter>();
        rb = gameObject.GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
    }

    
    private void FixedUpdate() {
        if (simulatePhysics) SimulatePhysics();
        else CalculatePosition();

        if (showMesh && meshRenderer.enabled == false) meshRenderer.enabled = true;
        if (!showMesh && meshRenderer.enabled == true) meshRenderer.enabled = false;
    }


    private void CalculatePosition()
    {
        // THIS FUNCTION SETS THE POSITION AND THE ROTATION OF THE PARENT OBJECT BASED ON THE WAVEMANAGER
        // Calculates the position each floater should be at according to the waves
        // Sets the position of the object in the mean center of the floaters
        // Generates a mesh from the location of the floaters
        // Calculates a mean normal vector from the mesh and sets this as the vector.up rotation of the object

        Vector3 sumVector = Vector3.zero;
        List<Vector3> points = new List<Vector3>(); 
        foreach (Transform floater in floaters)
        {
            Vector3 point = WaveManager.instance.GetWaveDisplacement(floater.position) - new Vector3(transform.position.x, 0, transform.position.z);
            points.Add(point);
            sumVector += point;
        }
        meanVector = sumVector / floaters.Count;
        transform.position = meanVector;
        mesh = CreateMeshFromPoints(meanVector, points);
        meshFilter.mesh = mesh;

        Vector3 normalVector = Vector3.zero;
        foreach(Vector3 normal in mesh.normals)
        {
            normalVector += normal;
        }
        normalVector = normalVector/mesh.normals.Length;
        transform.up = normalVector;
    }

    private void SimulatePhysics()
    {
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

    private Mesh CreateMeshFromPoints(Vector3 _meanVector3, List<Vector3> _floaters){
        // Creates a mesh from a list of vector3s with the meanVector3 as centre
        // triangles are always connected to the meanVector  
        // THIS FUNCTION BREAKS WHEN FLOATERS ARE NOT INSERTED CLOCKWISE (!!!)
        
        Mesh tempMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>();
        
        // Set vertices 
        vertices.Add(_meanVector3);
        for (int i = 0; i < _floaters.Count; i++)
            vertices.Add(_floaters[i]);


        // Set Triangles
        for (int i = 0; i < vertices.Count; i++)
        {
            if (i > 0)
            {
                tris.Add(i);
                if (i < _floaters.Count) tris.Add(i + 1);
                else tris.Add(1);
                tris.Add(0);
            }
        }

        tempMesh.vertices = vertices.ToArray();
        tempMesh.triangles = tris.ToArray();
        tempMesh.RecalculateNormals();
        return tempMesh;
    }


    private void OnDrawGizmos() {
        
        Gizmos.color = Color.red;
        if (showMeanVector)
        {   
            Gizmos.DrawSphere(meanVector, .2f);
        }
        Gizmos.color = Color.yellow;
        if (showFloaters)
        {
            foreach (Transform floater in floaters)
            {
                Gizmos.DrawSphere(floater.position, .2f);
            }
        }

    }
}
