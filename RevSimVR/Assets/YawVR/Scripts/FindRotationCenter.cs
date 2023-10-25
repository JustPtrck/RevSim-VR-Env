using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using UnityEngine;
using UnityEditor;
using TMPro;
using System;
using YawVR;

public class FindRotationCenter : MonoBehaviour
{
    [SerializeField] public Vector2 Offset;
    [SerializeField] private TextMeshProUGUI Text;
    [Space]
    [SerializeField]  private YawTracker tracker;
    [SerializeField] private YawController yawController;

    [SerializeField] private Transform Camera;

    [SerializeField] private float waitingTime = 1;
    [SerializeField] private float currentWaitingTime = 0;

    [SerializeField] private Vector3 targetPosition;


    
    [Space] 

    [SerializeField] private float[] targetPositionsForPitch = new float[3];

    

    [SerializeField] private int indexer = 0;
    [SerializeField] private bool coordinateGettingStarted;
    [Header("Constant Movement")]
    [SerializeField] private bool constantMovement;
    [SerializeField] private float startPos;
    [SerializeField] private float endPos;
    [SerializeField] private float travelTime;
    [SerializeField] private float traveledTime = 0;
    [SerializeField] private float starterTime = 0;
    [Space]
    [SerializeField] private int sampleCount = 6;
    [SerializeField] private float interpolateTime=0;
    [SerializeField] private List<Vector2> calculateedCenters = new List<Vector2>();

    [Header("Results")]
    [SerializeField] private List<Vector2> constantMovementPositions = new List<Vector2>();
    [SerializeField] private Vector2[] positions = new Vector2[3];

    



    private void Start()
    {
    }

    public void GetCoordinates()
    {
        indexer = 0;
        Offset = new Vector2(0, 0);

        Debug.Log("GetCoordinates Started!");
        if (!constantMovement)
        {
            
            SetTarget();
            coordinateGettingStarted = true;
        }
        else
        {
            constantMovementPositions.Clear();
            calculateedCenters.Clear();
            targetPosition.x = startPos;
            tracker.transform.rotation = Quaternion.Euler(targetPosition);
            StartCoroutine(WaitforTime(3));
        }
    }

    private IEnumerator WaitforTime(float second)
    {
        
        yield return new WaitForSeconds(second);
        interpolateTime = 0;
        starterTime = Time.realtimeSinceStartup;
        coordinateGettingStarted = true;
    }

    private void SetTarget()
    {
        currentWaitingTime = 0;
        targetPosition.x = targetPositionsForPitch[indexer];
        tracker.transform.rotation = Quaternion.Euler(targetPosition);
        
    }


    private void Update()
    {
        if (coordinateGettingStarted)
        {
            if (constantMovement)
            {
                interpolateTime += (Time.deltaTime / travelTime);
                traveledTime = Time.realtimeSinceStartup;
                targetPosition.x = Mathf.Lerp(startPos, endPos, interpolateTime);
                tracker.transform.rotation = Quaternion.Euler(targetPosition);


                if (Time.realtimeSinceStartup-starterTime>= travelTime/sampleCount)
                {
                    constantMovementPositions.Add(getHeadPosition());
                    starterTime = Time.realtimeSinceStartup;
                }
                if (interpolateTime >= 1)
                {
                    FinishedConstantMovement();
                }
            }
            else
            {
                if ((indexer != positions.Length))
                {
                    Text.text = yawController.Device.ActualPosition.pitch.ToString("0.00") + " - " + targetPositionsForPitch[indexer].ToString("0.00") + " t: " + currentWaitingTime.ToString("0.00");
                }
                if ((indexer < positions.Length) && Math.Abs(Math.Abs(yawController.Device.ActualPosition.pitch) - Math.Abs(targetPositionsForPitch[indexer])) < 3)
                {
                    if (currentWaitingTime < waitingTime)
                    {
                        currentWaitingTime += Time.deltaTime;
                    }
                    else
                    {
                        GetcurrentData();
                        indexer++;
                        SetTarget();
                    }
                }
                if (indexer == positions.Length)
                {
                    Offset = GetCircumcenter(positions[0], positions[1], positions[2]);
                    Text.text = Offset.x + " - " + Offset.y;
                    //SettingsValues.horizontalDistance = Offset.y;
                    //SettingsValues.verticalDistance = Offset.x;

                    tracker.transform.rotation = Quaternion.Euler(Vector3.zero);
                }
                coordinateGettingStarted = false;
            }
        }
    }

    public void FinishedConstantMovement()
    {
        List<Vector2> triangle = new List<Vector2>();

        foreach (var item in constantMovementPositions)
        {
            if (triangle.Count < 3)
            {
                triangle.Add(item);
            }
            else
            {
                calculateedCenters.Add(GetCircumcenter(triangle));
                triangle.Clear();
            }
        }

        foreach (var item in calculateedCenters)
        {
            Offset += item;

        }

        Offset = Offset / calculateedCenters.Count;


        coordinateGettingStarted = false;
        tracker.transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    private void GetcurrentData()
    {
        Debug.Log("Index: " + indexer);

        positions[indexer] = getHeadPosition();
    }

    private Vector2 getHeadPosition()
    {
        Vector2 result = new Vector2();

        result.x = Camera.localPosition.y;
        result.y = Camera.localPosition.z;

        return result;
    }


    #region GetDataForCircle
    public static Vector2 GetCircumcenter(Vector2 pointA, Vector2 pointB, Vector2 pointC)
    {
        LinearEquation lineAB = new LinearEquation(pointA, pointB);
        LinearEquation lineBC = new LinearEquation(pointB, pointC);

        Vector2 midPointAB = Vector2.Lerp(pointA, pointB, .5f);
        Vector2 midPointBC = Vector2.Lerp(pointB, pointC, .5f);

        LinearEquation perpendicularAB = lineAB.PerpendicularLineAt(midPointAB);
        LinearEquation perpendicularBC = lineBC.PerpendicularLineAt(midPointBC);

        Vector2 circumcircle = GetCrossingPoint(perpendicularAB, perpendicularBC);

        float circumRadius = Vector2.Distance(circumcircle, pointA);

        return circumcircle;
    }

    public static Vector2 GetCircumcenter(List<Vector2> points)
    {
        LinearEquation lineAB = new LinearEquation(points[0], points[1]);
        LinearEquation lineBC = new LinearEquation(points[1], points[2]);

        Vector2 midPointAB = Vector2.Lerp(points[0], points[1], .5f);
        Vector2 midPointBC = Vector2.Lerp(points[1], points[2], .5f);

        LinearEquation perpendicularAB = lineAB.PerpendicularLineAt(midPointAB);
        LinearEquation perpendicularBC = lineBC.PerpendicularLineAt(midPointBC);

        Vector2 circumcircle = GetCrossingPoint(perpendicularAB, perpendicularBC);

        float circumRadius = Vector2.Distance(circumcircle, points[0]);

        return circumcircle;
    }

    static Vector2 GetCrossingPoint(LinearEquation line1, LinearEquation line2)
    {
        float A1 = line1._A;
        float A2 = line2._A;
        float B1 = line1._B;
        float B2 = line2._B;
        float C1 = line1._C;
        float C2 = line2._C;

        //Cramer's rule
        float Determinant = A1 * B2 - A2 * B1;
        float DeterminantX = C1 * B2 - C2 * B1;
        float DeterminantY = A1 * C2 - A2 * C1;

        float x = DeterminantX / Determinant;
        float y = DeterminantY / Determinant;

        return new Vector2(x, y);
    }
    #endregion
}

[System.Serializable]
public class LinearEquation
{
    public float _A;
    public float _B;
    public float _C;

    public LinearEquation() { }

    //Ax+By=C
    public LinearEquation(Vector2 pointA, Vector2 pointB)
    {
        float deltaX = pointB.x - pointA.x;
        float deltaY = pointB.y - pointA.y;
        _A = deltaY; //y2-y1
        _B = -deltaX; //x1-x2
        _C = _A * pointA.x + _B * pointA.y;
    }

    public LinearEquation PerpendicularLineAt(Vector3 point)
    {
        LinearEquation newLine = new LinearEquation();

        newLine._A = -_B;
        newLine._B = _A;
        newLine._C = newLine._A * point.x + newLine._B * point.y;

        return newLine;
    }
}

