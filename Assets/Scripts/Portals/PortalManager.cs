using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Rendering;

public class PortalManager : MonoBehaviour
{
    Portal[] portals;

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += RenderPortalCameras;
        RenderPipelineManager.beginFrameRendering += CheckToEnableCameras;
        //RenderPipelineManager.beginFrameRendering += PrePortalRender;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= RenderPortalCameras;
        RenderPipelineManager.beginFrameRendering -= CheckToEnableCameras;
        //RenderPipelineManager.beginFrameRendering -= PrePortalRender;
    }

    void Awake()
    {
        // Find all portals in the scene
        portals = FindObjectsOfType<Portal>();
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].SetupReferences();
            portals[i].CreateViewTexture();
        }
    }

    void RenderPortalCameras(ScriptableRenderContext context, Camera camera)
    {
        if (camera.transform.CompareTag("Portal Camera"))
        {
            camera.transform.parent.GetComponent<Portal>().Render();
        }

        // Post Portal Render
        if (camera.transform.CompareTag("MainCamera"))
        {
            for (int i = 0; i < portals.Length; i++)
            {
                portals[i].PostPortalRender();
            }
        }
    }

    /*
    void PrePortalRender(ScriptableRenderContext context, Camera[] cameras)
    {
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].PrePortalRendering();
        }
    }
    */

    void CheckToEnableCameras(ScriptableRenderContext context, Camera[] cameras)
    {
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].EnableVisiableCamera();
        }
    }
}
