using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns a object for reaching exercises.
/// Object is spawned randomly in the preset range and at the exact radius. 
/// </summary>
public class ReachingExerciseSpawner : MonoBehaviour
{
    [SerializeField, Range(0.5f, 4f)] private float spawnRadius = 1.2f;
    [SerializeField, Range(0f, 360f)] private float horizontalRange = 180f;
    [SerializeField, Range(0f, 180f)] private float verticalRange = 90f;
    [SerializeField, Range(-180f, 180f)] private float verticalOffset = 30f;
    [SerializeField] private GameObject prefab;

    private GameObject currentTarget = null;

    public float radius{get{return spawnRadius;} set{if (value >= 0.5 && value <= 4) spawnRadius = value;}}
    public float horizontal{get{return horizontalRange;} set{if (value >= 0 && value <= 360) horizontalRange = value;}}
    public float vertical{get{return verticalRange;} set{if (value >= 0 && value <= 180) verticalRange = value;}}
    public float offset{get{return verticalOffset;} set{if (value >= -180 && value <= 180) verticalOffset = value;}}

    void Update()
    {
        // TODO Make this more adjustable
        if (!currentTarget) SpawnObjectiveObject();
    }

    /// <summary>
    /// Spawns a clone of the Prefab GameObject at a random position in the set range
    /// </summary>
    private void SpawnObjectiveObject(){
        // Gets a random X Y Z for the given range 
        float z = Mathf.Cos(Mathf.Deg2Rad * (Random.Range(- horizontalRange, horizontalRange) / 2));
        float x = Mathf.Sin(Mathf.Deg2Rad * (Random.Range(- horizontalRange, horizontalRange) / 2));
        float y = Mathf.Sin(Mathf.Deg2Rad * (Random.Range(0, verticalRange) + verticalOffset));
        
        // Normalizes a vector based on the XYZ so the length is always 1 and multiplies it by the radius
        Vector3 spawnPosition = new Vector3(x, y, z).normalized * spawnRadius;

        // Spawns a clone of prefab at spawnposition
        currentTarget = Instantiate(prefab, transform.position + spawnPosition, Quaternion.identity, this.transform);
        
        // Log spawning
        Debug.Log($"NEW {prefab.name} INSTANTIATED {spawnPosition}");
    }

    
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, new Vector3(Mathf.Sin(Mathf.Deg2Rad * horizontalRange / 2), 0, Mathf.Cos(Mathf.Deg2Rad * horizontalRange / 2) ) * spawnRadius);
        Gizmos.DrawRay(transform.position, new Vector3(Mathf.Sin(Mathf.Deg2Rad * - horizontalRange / 2), 0, Mathf.Cos(Mathf.Deg2Rad * - horizontalRange / 2) ) * spawnRadius);
        
        Gizmos.DrawRay(transform.position, new Vector3(0, Mathf.Sin(Mathf.Deg2Rad * (verticalRange + verticalOffset)), Mathf.Cos(Mathf.Deg2Rad * (verticalRange  + verticalOffset)) ) * spawnRadius);
        Gizmos.DrawRay(transform.position, new Vector3(0, Mathf.Sin(Mathf.Deg2Rad * verticalOffset), Mathf.Cos(Mathf.Deg2Rad * verticalOffset) ) * spawnRadius);
        
    }
}
