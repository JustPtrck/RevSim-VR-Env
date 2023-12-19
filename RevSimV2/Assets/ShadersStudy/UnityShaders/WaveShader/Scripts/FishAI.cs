using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JustPtrck.Shaders.Water;

public class FishAI : MonoBehaviour
{
    public float swimSpeed = 3f;          // Swimming speed of the fish
    public float rotationSpeed = 100f;    // Rotation speed of the fish
    public float jumpForce = 5f;          // Force applied when jumping
    public float jumpInterval = 2f;       // Time interval between jumps
    public Transform target;              // Target to follow (e.g., the player's transform)

    private Rigidbody fishRigidbody;


    void Start()
    {
        fishRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (target != null)
        {
            if (transform.position.y > WaveManager.instance.GetDisplacementFromGPU(transform.position).y)
                fishRigidbody.AddForce(Vector3.up * Physics.gravity.y, ForceMode.Acceleration);

            if (Vector3.Distance(target.position, transform.position) > 5)
                SwimTowardsTarget();

            else CircleTarget();
        }
    }

    void SwimTowardsTarget()
    {
        // Calculate the direction to the target
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        // Rotate towards the target
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        fishRigidbody.MoveRotation(Quaternion.RotateTowards(fishRigidbody.rotation, targetRotation, rotationSpeed * Time.deltaTime));

        // Swim forward
        fishRigidbody.AddForce(transform.forward * swimSpeed * Time.deltaTime, ForceMode.VelocityChange);
    }

    void Jump()
    {
        // Apply an upward force to make the fish jump
        fishRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }





    private void CircleTarget()
    {
        Vector3 targetDirection = (target.position - transform.position).normalized;
        Vector3 moveDirection = Quaternion.Euler(0, -90, 0) * targetDirection;
        
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        fishRigidbody.MoveRotation(Quaternion.RotateTowards(fishRigidbody.rotation, targetRotation, rotationSpeed * Time.deltaTime));

        fishRigidbody.AddForce(transform.forward * swimSpeed * Time.deltaTime, ForceMode.VelocityChange);

    }
}
