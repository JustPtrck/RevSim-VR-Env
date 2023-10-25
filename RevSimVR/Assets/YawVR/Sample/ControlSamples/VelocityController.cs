using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YawVR;

[RequireComponent(typeof(Rigidbody))]

/// <summary>
/// Sets the YawTracker's orientation based on the GameObject's speed
/// </summary>
public class VelocityController : MonoBehaviour
{
    /*
     This script uses the gameObjects's rigidbody's velocity to control the YawTracker
  */
    YawController yawController;

    private Rigidbody rigid;


    [SerializeField]
    private Vector3 multiplier = new Vector3(3f, 1f, -2f);
    private void Awake() {
        rigid = GetComponent<Rigidbody>();
    }
    private void Start() {
        yawController = YawController.Instance();
    }



    private void FixedUpdate() {
      
        //float x, y, z;
        Vector3 vel = transform.InverseTransformVector(rigid.velocity);

        vel.x *= multiplier.x;
        vel.y *= multiplier.y;
        vel.z *= multiplier.z;

        Vector3 v = new Vector3(vel.z, 0f, vel.x);
   
        yawController.TrackerObject.SetRotation(v);
    }


}
