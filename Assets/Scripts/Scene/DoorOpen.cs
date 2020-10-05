using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public enum RotationDirection { Negative, Positve};
    public enum RotationAxis { XAxis, YAxis, ZAxis};

    [Header("References")]
    public DoorOpen linkedDoor;

    [Header("Settings")]
    public RotationDirection rotationDirection;
    public RotationAxis rotationAxis;
    public float maxRotAngle = 90f;
    public float rotationSpeed = 9f;

    [Header("Audio")]
    public AudioSource doorOpenAudio;

    Transform doorT;
    float rotScal;
    Vector3 rotAxis;
    float angleCount;
    bool canOpen;

    void Start()
    {
        doorT = transform.parent.transform;
        angleCount = 0;
        canOpen = true;
    }

    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerMovement>();
        if (player)
        {
            player.OnButtonPress += CheckButtonInput;
        }
    }

    void OnTriggerExit(Collider other)
    {
        var player = other.GetComponent<PlayerMovement>();
        if (player)
        {
            player.OnButtonPress -= CheckButtonInput;
        }
    }

    void CheckButtonInput(KeyCode keyCode)
    {
        if (keyCode == KeyCode.E && canOpen)
        {
            StartOpeningDoor();
            if (linkedDoor)
            {
                linkedDoor.StartOpeningDoor();
            }
        }
    }

    public void StartOpeningDoor()
    {
        if (rotationDirection == RotationDirection.Positve)
        {
            rotScal = 1.0f;
        }
        else if (rotationDirection == RotationDirection.Negative)
        {
            rotScal = -1.0f;
        }

        if (rotationAxis == RotationAxis.XAxis)
        {
            rotAxis = Vector3.right;
        }
        if (rotationAxis == RotationAxis.YAxis)
        {
            rotAxis = Vector3.up;
        }
        if (rotationAxis == RotationAxis.ZAxis)
        {
            rotAxis = Vector3.forward;
        }

        StartCoroutine(OpenDoor());
        canOpen = false;
    }

    IEnumerator OpenDoor()
    {
        doorOpenAudio.PlayOneShot(doorOpenAudio.clip, 0.7f);

        while (angleCount < maxRotAngle)
        {
            float rotDelta = rotationSpeed * rotScal * Time.deltaTime;
            doorT.Rotate(rotAxis, rotDelta);
            angleCount += Mathf.Abs(rotDelta);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
}
