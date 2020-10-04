using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

public class Portal : MonoBehaviour
{
    [Header("References")]
    public Portal linkedPortal;
    public MeshRenderer screen;

    [Header("Settings")]
    public int recursionLimit = 2;
    public float nearClipOffset = 0.05f;
    public float nearClipLimit = 0.2f;

    // Private variables
    Camera playerCam;
    Camera portalCam;
    RenderTexture viewTexture;
    List<PortalTraveler> trackedTravelers;

    //MeshFilter screenMeshFilter;
    //bool isRecursiveRendering;

    public void SetupReferences()
    {
        playerCam = Camera.main;
        portalCam = GetComponentInChildren<Camera>();
        trackedTravelers = new List<PortalTraveler>();

        //screenMeshFilter = screen.GetComponent<MeshFilter>();
        //isRecursiveRendering = false;
    }

    void LateUpdate()
    {
        HandleTravellers();
    }

    void HandleTravellers()
    {
        for (int i = 0; i < trackedTravelers.Count; i++)
        {
            PortalTraveler traveler = trackedTravelers[i];
            Transform travelerT = traveler.transform;

            // if traveler is viewer use camera transform
            Transform offsetTransform = (traveler.isViewer) ? playerCam.transform : traveler.transform;

            var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travelerT.localToWorldMatrix;

            Vector3 offsetFromPortal = offsetTransform.position - transform.position;
            int portalSide = System.Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
            int portalSideOld = System.Math.Sign(Vector3.Dot(traveler.previousOffsetFromPortal, transform.forward));

            // Teleport traveler if crossed from one side of portal to other
            if (portalSide != portalSideOld)
            {
                var positionOld = travelerT.position;
                var rotOld = travelerT.rotation;
                

                traveler.Teleport(transform, linkedPortal.transform, m.GetColumn(3), m.rotation);
                traveler.graphicsClone.transform.SetPositionAndRotation(positionOld, rotOld);
                
                linkedPortal.OnTravelerEnterPortal(traveler);
                trackedTravelers.RemoveAt(i);
                i--;
            }
            else
            {
                traveler.graphicsClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
                traveler.previousOffsetFromPortal = offsetFromPortal;
            }
        }
    }

    /*
    public void PrePortalRendering()
    {
        foreach (var traveler in trackedTravelers)
        {
            UpdateSliceParams(traveler);
        }
    }
    */

    public void CreateViewTexture()
    {
        // Check if view texture is initialized
        if (viewTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height)
        {
            // Release old view texture
            if (viewTexture != null)
            {
                viewTexture.Release();
            }

            viewTexture = new RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.DefaultHDR);
            // Render the view from the portal camera to the view texture
            portalCam.targetTexture = viewTexture;
            // Display the view texture on the screen of the linked portal
            linkedPortal.screen.material.SetTexture("_BaseMap", viewTexture);
            linkedPortal.screen.material.SetVector("_BaseColor", new Vector4(1f, 1f, 1f, 1f));
        }
    }

    public void Render()
    {
        //screen.enabled = false;
        CreateViewTexture();

        // Match matrices
        portalCam.projectionMatrix = playerCam.projectionMatrix;

        // Move portal camera to correct opposing position and rotation
        var m = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * playerCam.transform.localToWorldMatrix;
        portalCam.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

        // Handle clipping
        SetNearClipPlane();
        //HandleClipping
    }

    /* Attemp at recursive rendering with coroutines
    // Called just before player camera is rendered
    public void Render()
    {        
        //screen.enabled = false;
        CreateViewTexture();

        // Match matrices
        portalCam.projectionMatrix = playerCam.projectionMatrix;

        // Check if recursively rendering to break out
        if (isRecursiveRendering) return;

        var localToWorldMatrix = playerCam.transform.localToWorldMatrix;
        var renderPositions = new Vector3[recursionLimit];
        var renderRotations = new Quaternion[recursionLimit];

        // Render a recursive series of portal camera locations and rotations
        int startIndex = 0;
        for (int i = 0; i < recursionLimit; i++)
        {
            if (i > 0)
            {
                // Skip if linked portals are not visiable through each other
                if (!CameraUtility.BoundsOverlap(screenMeshFilter, linkedPortal.screenMeshFilter, portalCam))
                {
                    break;
                }
            }

            localToWorldMatrix = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * localToWorldMatrix;
            int renderOrderIndex = recursionLimit - i - 1;
            renderPositions[renderOrderIndex] = localToWorldMatrix.GetColumn(3);
            renderRotations[renderOrderIndex] = localToWorldMatrix.rotation;

            portalCam.transform.SetPositionAndRotation(renderPositions[renderOrderIndex],
                renderRotations[renderOrderIndex]);
            startIndex = renderOrderIndex;
        }

        // Hide screen so camera can see through portal
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        linkedPortal.screen.sharedMaterial.SetInt("displayMask", 0);

        // Recursive rendering
        StartCoroutine(RecursivePortalRender(startIndex, renderPositions, renderRotations));

        // Unhide objects hidden at start of render
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    IEnumerator RecursivePortalRender(int startIndex, Vector3[] renderPositions, Quaternion[] renderRotations)
    {
        isRecursiveRendering = true;
        for (int i = startIndex; i < recursionLimit; i++)
        {
            portalCam.transform.SetPositionAndRotation(renderPositions[i], renderRotations[i]);
            SetNearClipPlane();
            //HandleClipping();
            //portalCam.Render();

            if (i == startIndex)
            {
                linkedPortal.screen.material.SetInt("displayMask", 1);
            }
            yield return new WaitForEndOfFrame();
        }
        isRecursiveRendering = false;
        yield return null;
    }
    */

    static bool VisibleFromCamera(Renderer renderer, Camera camera)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
    }

    public void EnableVisiableCamera()
    {
        if (VisibleFromCamera(linkedPortal.screen, playerCam))
        {
            portalCam.enabled = true;
        }
        else
        {
            portalCam.enabled = false;
        }
    }

    public void PostPortalRender()
    {
        /*
        foreach (var traveler in trackedTravelers)
        {
            UpdateSliceParams(traveler);
        }
        */
        ProtectScreenFromClipping(playerCam.transform.position);
    }

    // Sets the thickness of the portal screen so as not to clip with camera near plane when player goes through
    void ProtectScreenFromClipping(Vector3 viewPoint)
    {
        float halfHeight = playerCam.nearClipPlane * Mathf.Tan(playerCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * playerCam.aspect;
        float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, playerCam.nearClipPlane).magnitude;
        float screenThickness = dstToNearClipPlaneCorner;

        Transform screenT = screen.transform;
        bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
        screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, screenThickness);
        screenT.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
    }

    /*
    void UpdateSliceParams(PortalTraveler traveler)
    { }
    */

    // Use custom projection matrix to align portal camera's near clip plane with portal's surface
    void SetNearClipPlane()
    {
        Transform clipPlane = transform;
        int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - portalCam.transform.position));

        Vector3 camSpacePos = portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        float camSpaceDist = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

        // Don't use oblique clip plane very close to portal to avoid visual artifacts
        if (Mathf.Abs(camSpaceDist) > nearClipLimit)
        {
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDist);

            // Update projection based on new clip plane
            // Calculate matrix with player cam so that player camera settings (fov, etc) are used
            portalCam.projectionMatrix = playerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }
        else
        {
            portalCam.projectionMatrix = playerCam.projectionMatrix;
        }
    }

    void OnTravelerEnterPortal(PortalTraveler traveler)
    {
        // Check if traveler is already being tracked
        if (!trackedTravelers.Contains(traveler))
        {
            traveler.EnterPortalThreshold();
            // if traveler is viewer use camera transform
            if (traveler.isViewer)
            {
                traveler.previousOffsetFromPortal = playerCam.transform.position - transform.position;
            }
            else
            {
                traveler.previousOffsetFromPortal = traveler.transform.position - transform.position;
            }
            trackedTravelers.Add(traveler);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var traveler = other.GetComponent<PortalTraveler>();
        if (traveler)
        {
            OnTravelerEnterPortal(traveler);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var traveler = other.GetComponent<PortalTraveler>();
        if (traveler && trackedTravelers.Contains(traveler))
        {
            traveler.ExitPortalThreshold();
            trackedTravelers.Remove(traveler);
        }
    }

    void OnValidate()
    {
        // Link linked portal back to original if not done already
        if (linkedPortal != null) {
            linkedPortal.linkedPortal = this;
        }
    }
}
