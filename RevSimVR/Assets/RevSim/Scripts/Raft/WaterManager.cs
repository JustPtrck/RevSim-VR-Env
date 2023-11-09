using System.Linq;
using UnityEngine;

public class WaterManager : MonoBehaviour
{

    [SerializeField] private Material material;
    [SerializeField] private int planeSize = 10;
    [SerializeField] private float UVScale = 2f;
    [SerializeField, Range(1, 10), Tooltip("Keep as low as possible!\nWill reduce performance GREATLY")] 
    private int meshResolution = 1;
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Vector3[] baseVertices;
    private Vector3[] baseNormals;
    
    private void Awake() {
        meshFilter = gameObject.GetComponent<MeshFilter>() ? gameObject.GetComponent<MeshFilter>() : gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>() ? gameObject.GetComponent<MeshRenderer>() : gameObject.AddComponent<MeshRenderer>();
        
        mesh = PlaneMeshGenerator.CreateMesh(planeSize, meshResolution, UVScale);
        meshFilter.mesh = mesh;
        meshRenderer.material = material;

        baseVertices = meshFilter.mesh.vertices.ToArray();
        baseNormals = meshFilter.mesh.normals.ToArray();
    }


    private void UpdateWaterMesh()
    {
        // REPLACED BY SHADER
        
        Vector3[] vertices = baseVertices;
        Vector3[] normals = baseNormals;
        for(int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = WaveManager.instance.GetWaveDisplacement(transform.position + vertices[i], ref normals[i]) - transform.position;
        }

        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.normals = normals;
    }
    
    private void DebugMethod()
    {
        Vector3 val = new Vector3(2,0,1);
        Debug.Log($"Input vector: {val}\nOutput vector: {WaveManager.instance.GetWaveDisplacement(transform.position + val, ref val) - transform.position}");
    }
}

