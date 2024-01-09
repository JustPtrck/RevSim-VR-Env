using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using JustPtrck.Shaders.Water;

namespace JustPtrck.Water.Interactables{
    public class MotorBoat : MonoBehaviour
    {
        [Header("Boat Parameters")]
        [SerializeField] private float acceleration = 10f;   // m/s^2
        //[SerializeField] private float maxVelocity = 5f;    // m/s 
        [SerializeField, Range(0, 90)] private float rudderMaxAngle = 40f;
        [SerializeField] private float rotationSpeed = 2f;
        //[SerializeField] private float rudderDragC = 1.05f; 

        [Header("Transform References")]
        [SerializeField] private Transform rudder;
        [SerializeField] private Transform motor;
        [SerializeField] private Floater floater;
        
        
        private Rigidbody rb;
        private float rudderAngle;
        private float motorForce;

        private void OnEnable()
        {
            rb = gameObject.GetComponent<Rigidbody>() ? gameObject.GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>();
            floater = gameObject.GetComponent<Floater>() ? gameObject.GetComponent<Floater>() : gameObject.AddComponent<Floater>();
        }
        
        private void Update()
        {
            // if (motor.position.y < WaveManager.instance.GetDisplacementFromGPU(motor.position).y)
            //     rb.AddForceAtPosition(motor.forward * motorForce, motor.position, ForceMode.Acceleration);
            
            float curVelo = new Vector2(rb.velocity.x, rb.velocity.z).magnitude;
            // if (rudder.position.y < WaveManager.instance.GetDisplacementFromGPU(rudder.position).y)
            //     rb.AddForceAtPosition(rudder.right * curVelo * Mathf.Sin(Mathf.Deg2Rad * rudderAngle), rudder.position, ForceMode.Force);
            // Debug.Log($"Velocity: {curVelo}");


            // TEMP Formula for force
            if (floater.floatType == Floater.FloaterType.Ideal)
                IdealMove(motorForce);
            else if (motor.position.y < WaveManager.instance.GetDisplacementFromGPU(motor.position).y)
                rb.AddForceAtPosition(rudder.forward * motorForce, motor.position, ForceMode.Acceleration);

        }
        public void OnAccelerate(InputValue value)
        {
            float mod = value.Get<float>();
            motorForce = acceleration * mod * Mathf.Cos(Mathf.Deg2Rad * rudderAngle);
            // Debug.Log($"Accelerate Input: {mod}");
            return;
        }
        public void OnSteering(InputValue value)
        {
            rudderAngle = rudderMaxAngle * value.Get<float>();
            // Debug.Log($"Steering Input: {value.Get<float>()}");
            rudder.transform.localRotation = Quaternion.Euler(0f, rudderAngle, 0f);
            return;
        }


        private void IdealMove(float speed)
        {
            Floater.Anchor anchor = new Floater.Anchor();
            anchor.position = floater.anchorPoint.position + (transform.forward * speed * Time.deltaTime);
            anchor.rotationAngle = floater.anchorPoint.rotationAngle - rudderAngle * rotationSpeed * Time.deltaTime;
            floater.anchorPoint = anchor;
        }
    }   
}
