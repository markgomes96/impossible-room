using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideWalls : MonoBehaviour
{
    public GameObject[] wallList;

    ProjectileManager projectileManager;

    void Start()
    {
        // Subscribe to player's input broadcast
        projectileManager = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<ProjectileManager>();
        projectileManager.OnStartShadowPlayer += ActivateHideWalls;
    }

    void ActivateHideWalls()
    {
        for (int i = 0; i < wallList.Length; i++)
        {
            wallList[i].SetActive(false);
        }
    }
}
