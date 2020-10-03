using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // private reference
    CharacterController characterController;

    [Header("References")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Settings")]
    public float speed = 12f;
    public float accelScalar = 3.0f;
    public float gravity = -9.8f;
    public float jumpHeight = 3.0f;

    Vector3 velocity;
    float currentSpeed;
    bool isGrounded;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // check if character is grounded at start of frame
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Zero out vertical velocity when grounded
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        // Get horizontal movement input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Zero out current speed when no input
        if (x == 0f && z == 0f)
        {
            currentSpeed = 0f;
        }

        // Add x & z component of move vector
        Vector3 moveVect = transform.right * x + transform.forward * z;

        // Normalize horizontal move vector components
        moveVect = moveVect.normalized;

        // Lerp current speed to max speed
        currentSpeed = Mathf.Lerp(currentSpeed, speed, Time.deltaTime * accelScalar);
        moveVect *= currentSpeed;

        // Apply jump velocity if player grounded
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity to velocity
        velocity.y += gravity * Time.deltaTime;

        // Apply velocity to move vector
        moveVect.y = velocity.y;

        // Move player by move vector
        characterController.Move(moveVect * Time.deltaTime);
    }
}
