using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;
    [Header("Settings")]
    public float mouseSensitivity = 100f;

    float xRotation = 0f;

    void Start()
    {
        // Hide and lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotate player up-down with vertical mouse movement
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate player left-right with horizontal mouse movement
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
