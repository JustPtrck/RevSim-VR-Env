using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;

public class DebuggingUI : MonoBehaviour
{
    // Reference Transform used to calculate the user's posture
    [SerializeField] private Transform referenceTransform;
    // Headset's Transform
    [SerializeField] private Transform headsetTransform;

    void Update()
    {
        // TODO Fix temporary shit
        MeasurePosture();
    }


    private void MeasurePosture()
    {
        
        Vector3 positionDifference = headsetTransform.position - referenceTransform.position;
        Quaternion rotationDifference = headsetTransform.rotation * Quaternion.Inverse(referenceTransform.rotation);

        // TEMPORARY
        Debug.Log($"---| NEW DATA SET |-----------------------------------\n" +
                  $"Position from Reference: {positionDifference}\n" +
                  $"Rotation from Reference: {rotationDifference}\n" +
                  $"Position from World Origin: {headsetTransform.position}\n" +
                  $"Rotation from World Origin: {headsetTransform.rotation}");
    }
}
