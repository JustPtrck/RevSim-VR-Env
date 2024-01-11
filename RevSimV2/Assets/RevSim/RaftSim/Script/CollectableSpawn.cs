using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableSpawn : MonoBehaviour
{
    [SerializeField, Range(0.5f, 5f)] private float spawnRadius = 1.2f;
    [SerializeField, Range(0f, 360f)] private float horizontalRange = 180f;
    [SerializeField, Range(0f, 180f)] private float verticalRange = 90f;
    [SerializeField, Range(0f, 180f)] private float verticalOffset = 30f;
    [SerializeField] private GameObject prefab;

    private GameObject currentTarget = null;

    void Update()
    {
        if (!currentTarget) SpawnCoin();
    }


    private void SpawnCoin(){
        // Gets a random X Y Z for the given range 
        float z = Mathf.Cos(Mathf.Deg2Rad * (Random.Range(- horizontalRange, horizontalRange) / 2));
        float x = Mathf.Sin(Mathf.Deg2Rad * (Random.Range(- horizontalRange, horizontalRange) / 2));
        float y = Mathf.Sin(Mathf.Deg2Rad * (Random.Range(- verticalRange, verticalRange) / 2 + verticalOffset));
        
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
        
        Gizmos.DrawRay(transform.position, new Vector3(0, Mathf.Sin(Mathf.Deg2Rad * (verticalRange / 2 + verticalOffset)), Mathf.Cos(Mathf.Deg2Rad * (verticalRange / 2 + verticalOffset)) ) * spawnRadius);
        Gizmos.DrawRay(transform.position, new Vector3(0, Mathf.Sin(Mathf.Deg2Rad * (- verticalRange / 2 + verticalOffset)), Mathf.Cos(Mathf.Deg2Rad *  (- verticalRange / 2 + verticalOffset)) ) * spawnRadius);
        
    }
}
