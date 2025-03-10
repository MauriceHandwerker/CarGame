using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEditor;

public class CarController : MonoBehaviour
{
    public float MoveSpeed = 50;
    public float MaxSped = 15;
    public bool CanMove = false;
    public float Drag = 0.98f;
    public float SteerAngle = 20;
    public float Traction = 0.5f;
    
    // Added wheel variables
    public Transform leftFrontWheel;  // Reference to left front wheel transform
    public Transform rightFrontWheel; // Reference to right front wheel transform
    public float maxWheelTurnAngle = 30f; // Maximum angle the wheels can turn

    private Vector3 MoveForce;
    private float currentSteerAngle = 0f; // To store current steering angle

    public void FixedUpdate()
    {
        if (!CanMove)
        {
            return;
        }
        
        //move
        MoveForce += transform.forward * MoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
        transform.position += MoveForce * Time.deltaTime;

        //stearing
        float steerInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * steerInput * MoveForce.magnitude * SteerAngle * Time.deltaTime);

        // Update wheel rotation
        if (leftFrontWheel != null && rightFrontWheel != null)
        {
            // Calculate the target steering angle
            currentSteerAngle = steerInput * maxWheelTurnAngle;

            // Reset wheels rotation to match car's rotation first
            leftFrontWheel.localRotation = Quaternion.identity;
            rightFrontWheel.localRotation = Quaternion.identity;

            // Apply steering rotation to wheels
            leftFrontWheel.Rotate(Vector3.up, currentSteerAngle);
            rightFrontWheel.Rotate(Vector3.up, currentSteerAngle);
        }

        //Drag
        MoveForce *= Drag;
        MoveForce = Vector3.ClampMagnitude(MoveForce, MaxSped);

        //Traction
        Debug.DrawRay(transform.position, MoveForce.normalized * 3);
        Debug.DrawRay(transform.position, transform.forward * 3, Color.blue);
        MoveForce = Vector3.Lerp(MoveForce.normalized, transform.forward, Traction * Time.deltaTime) * MoveForce.magnitude;
    }
}