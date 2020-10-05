using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : PortalTraveler
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
    public float maxFallSpeed = 9.0f;

    [Header("Audio")]
    public AudioSource walkAudio;
    public AudioSource landAudio;

    Vector3 velocity;
    float currentSpeed;
    bool isGrounded;
    bool freezePlayer;
    bool wasGrounded;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        freezePlayer = false;
        wasGrounded = true;
    }

    void Update()
    {
        if (!freezePlayer)
        {
            CheckInputs();
            HandleMovement();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BroadcastButtonPress((KeyCode.Escape));
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            BroadcastButtonPress((KeyCode.Tab));
        }
    }

    public void EnablePlayerControl()
    {
        freezePlayer = false;
        transform.GetComponentInChildren<MouseLook>().enabled = true;
        // Hide and lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void DisablePlayerControl()
    {
        freezePlayer = true;
        transform.GetComponentInChildren<MouseLook>().enabled = false;
    }

    public void FreezePlayer()
    {
        freezePlayer = true;
    }

    void CheckInputs()
    {
        // Keyboard inputs
        if (Input.GetKeyDown(KeyCode.E))
        {
            BroadcastButtonPress((KeyCode.E));
        }

        // Mouse inputs
        if (Input.GetMouseButtonDown(0))
        {
            BroadcastMouseButtonPress(0);
        }
    }

    public delegate void ButtonPressed(KeyCode keyCode);
    public event ButtonPressed OnButtonPress;

    void BroadcastButtonPress(KeyCode keyCode)
    {
        if (OnButtonPress != null)
            OnButtonPress(keyCode);
    }

    public delegate void MouseButtonPressed(int mouseButton);
    public event MouseButtonPressed OnMouseButtonPressed;

    void BroadcastMouseButtonPress(int mouseButton)
    {
        if (OnMouseButtonPressed != null)
            OnMouseButtonPressed(mouseButton);
    }

    public delegate void NextStage();
    public event NextStage OnNextStage;

    public void BroadcastNextStage()
    {
        if (OnNextStage != null)
            OnNextStage();
    }

    void HandleMovement()
    {
        // check if character is grounded at start of frame
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (!wasGrounded && isGrounded)
        {
            landAudio.PlayOneShot(landAudio.clip, 0.7f);
        }

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

        // Clamp vertical speed to max speed
        if (velocity.y < -maxFallSpeed)
        {
            velocity.y = -maxFallSpeed;
        }

        // Apply velocity to move vector
        moveVect.y = velocity.y;

        // Check if grounded and moving horizontally
        Vector2 horizVect = new Vector2(moveVect.x, moveVect.z);
        if (isGrounded && horizVect.sqrMagnitude > 0.001)
        {
            if (!walkAudio.isPlaying)
                walkAudio.Play();
        }
        else
        {
            walkAudio.Stop();
        }    

        // Move player by move vector
        characterController.Move(moveVect * Time.deltaTime);

        wasGrounded = isGrounded;
    }

    public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        //Vector3 eulerRot = rot.eulerAngles;
        transform.rotation = rot;
        velocity = toPortal.TransformVector(fromPortal.InverseTransformVector(velocity));
        Physics.SyncTransforms();
    }
}
