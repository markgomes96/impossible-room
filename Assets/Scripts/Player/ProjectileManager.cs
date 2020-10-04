using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class ProjectileManager : MonoBehaviour
{
    [Header("References")]
    public GameObject projectile;

    // private
    PlayerMovement playerMovement;
    Transform playerCam;

    void Start()
    {
        playerMovement = transform.GetComponent<PlayerMovement>();
        playerCam = Camera.main.transform;

        playerMovement.OnMouseButtonPressed += CheckMouseButtonPress;
    }

    void OnDisable()
    {
        playerMovement.OnMouseButtonPressed -= CheckMouseButtonPress;
    }

    public void CheckMouseButtonPress(int mouseButton)
    {
        // Check for primary click
        if (mouseButton == 0)
        {
            FireProjectile();
        }
    }

    void FireProjectile()
    {
        Vector3 spawnPos = transform.position + (transform.forward * 2f) + (transform.up * 1f);
        GameObject spawn = Instantiate(projectile, spawnPos, playerCam.rotation);
        spawn.GetComponent<Rigidbody>().velocity = spawn.transform.forward * 20f;
    }
}
