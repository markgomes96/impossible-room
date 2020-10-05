using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float xRotation = 5f;
    public float yRotation = 5f;
    public float zRotation = 5f;

    void Update()
    {
        transform.Rotate(xRotation * Time.deltaTime, 
            yRotation * Time.deltaTime, 
            zRotation * Time.deltaTime);
    }
}
