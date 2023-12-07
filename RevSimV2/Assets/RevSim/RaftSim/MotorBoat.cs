using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using JustPtrck.Shaders.Water;

namespace JustPtrck.Water.Interactables{
    public class MotorBoat : MonoBehaviour
    {
        [SerializeField] private float acceleration = 1f;   // m/s^2
        [SerializeField] private float maxVelocity = 5f;    // m/s 
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform rudder;
        [SerializeField] private Transform motor;
        [SerializeField] private float rudderMaxAngle = 40f;
        [SerializeField] private float rudderArea = 2f;
        [SerializeField] private float rudderDragC = 1.05f; 
        private Floater floater;
        private float rudderAngle;
        private Rigidbody rudderRb;

        private float motorForce;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }
        
        private void Update()
        {
            rb.AddForceAtPosition(motor.forward * motorForce, motor.position, ForceMode.Force);
            Debug.Log($"Velocity: {rb.velocity.x} {rb.velocity.y} {rb.velocity.z}");

            rb.AddForceAtPosition(rudder.right * rb.velocity.magnitude / 2 *  Mathf.Clamp(rudderAngle, -1f, 1f), rudder.position, ForceMode.Force);

        }
        public void OnAccelerate(InputValue value)
        {
            motorForce = acceleration * value.Get<float>();
            Debug.Log($"Accelerate Input: {value.Get<float>()}");
            return;
        }
        public void OnSteering(InputValue value)
        {
            rudderAngle = (rudderMaxAngle * value.Get<float>() * 180) / Mathf.PI;
            Debug.Log($"Steering Input: {value.Get<float>()}");
            rudder.transform.localRotation = Quaternion.Euler(0f, rudderAngle, 0f);
            return;
        }
    }   
}
