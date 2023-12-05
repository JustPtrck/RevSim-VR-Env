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
        [SerializeField] private float rudderMaxAngle = 40f;
        [SerializeField] private float rudderArea = 2f;
        [SerializeField] private float rudderDragC = 1.05f; 
        private Floater floater;


        private float rudderForce;
        private float rudderAngle;
        private float motorForce;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }
        
        private void Update()
        {
            rb.AddForce(transform.forward * motorForce, ForceMode.Force);
            Debug.Log($"Velocity: {rb.velocity.x} {rb.velocity.y} {rb.velocity.z}");
            float velo = rb.velocity.magnitude * Mathf.Cos(Vector3.Angle(rb.velocity, rudder.position) * Mathf.PI / 180);
            
            rudderForce = GetDragForceNormal(rudderArea, 1f,  velo, rudderDragC, rudderAngle)/8;
            Debug.Log($"Force: {rudderForce}");
            
            rb.AddForceAtPosition(rudder.transform.right * rudderForce, rudder.transform.position, ForceMode.Force);
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
            return;
        }

        private float GetDragForceNormal(float area, float density, float velocity, float coefficent, float angle)
        {
            float dragForce = .5f * density * Mathf.Pow(velocity, 2f) * coefficent * area;
            return  dragForce * Mathf.Sin(angle);
        }
    }   
}
