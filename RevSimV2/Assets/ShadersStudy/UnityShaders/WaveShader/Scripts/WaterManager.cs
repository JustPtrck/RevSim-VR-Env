using System.Linq;
using UnityEngine;

using JustPtrck.Shaders.Water;

/// <summary>
/// The WaterManager Class is a class to test the WaveManager
/// </summary>
public class WaterManager : MonoBehaviour
{
    [Header("Plane Mesh Setup (Before Play)")]
    [SerializeField] private Material material;
    [SerializeField] private int planeSize = 10;
    [SerializeField] private int xTilesAmount = 1;
    private float UVScale;
    [SerializeField, Range(1, 10), Tooltip("Keep as low as possible!\nWill reduce performance GREATLY")] 
    private int meshResolution = 1;
    
    [Header("Impact Wave Test (Runtime)")]
    [SerializeField] private bool impactOnMouseClick;
    [SerializeField, Range (0f, 1f)] private float steepness;
    [SerializeField] private float wavelength, duration;
    [SerializeField] private Camera cam;
    
    
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    
    private void Awake() {
        meshFilter = gameObject.GetComponent<MeshFilter>() ? gameObject.GetComponent<MeshFilter>() : gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>() ? gameObject.GetComponent<MeshRenderer>() : gameObject.AddComponent<MeshRenderer>();
        meshCollider = gameObject.GetComponent<MeshCollider>() ? gameObject.GetComponent<MeshCollider>() : gameObject.AddComponent<MeshCollider>();
        if (xTilesAmount < 1) xTilesAmount = 1;
        UVScale = planeSize / xTilesAmount;
        mesh = PlaneMeshGenerator.CreateMesh(planeSize, meshResolution, UVScale);
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshCollider.excludeLayers = ~0;
        meshRenderer.material = material;
        if(cam == null) cam = Camera.main;
    }

    private void OnMouseDown() {
        if (impactOnMouseClick)
        {
            RaycastHit RayHit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RayHit ))
            {
                WaveManager.instance.CreateImpactWave(RayHit.point, steepness, wavelength, duration);
            }
        }
    }

}

