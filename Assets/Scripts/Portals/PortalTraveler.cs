using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTraveler : MonoBehaviour
{
    [Header("References")]
    public GameObject graphicsObject;

    [Header("Settings")]
    public bool isViewer;

    public GameObject graphicsClone { get; set; }
    [HideInInspector]
    public Vector3 previousOffsetFromPortal { get; set; }

    [HideInInspector]
    public Material[] originalMaterials { get; set; }
    [HideInInspector]
    public Material[] cloneMaterials { get; set; }

    public virtual void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }

    // Called when first touches portal
    public virtual void EnterPortalThreshold()
    {
        if (graphicsClone == null)
        {
            graphicsClone = Instantiate(graphicsObject);
            graphicsClone.transform.parent = graphicsObject.transform.parent;
            graphicsClone.transform.localScale = graphicsObject.transform.localScale;
            
            //originalMaterials = GetMaterials(graphicsObject);
            //cloneMaterials = GetMaterials(graphicsClone);
        }
        else
        {
            graphicsClone.SetActive(true);
        }
    }

    // Called once no longer touching portal (excluding when teleporting)
    public virtual void ExitPortalThreshold()
    {
        graphicsClone.SetActive(false);
        /*
        // Disable slicing
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            originalMaterials[i].SetVector("sliceNormal", Vector3.zero);
        }
        */
    }

    /*
    public void SetSliceOffsetDist(float dist, bool clone)
    {
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            if (clone)
            {
                cloneMaterials[i].SetFloat("sliceOffsetDist", dist);
            }
            else
            {
                originalMaterials[i].SetFloat("sliceOffsetDist", dist);
            }
        }
    }

    Material[] GetMaterials(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<MeshRenderer>();
        var matList = new List<Material>();
        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.sharedMaterials)
            {
                matList.Add(mat);
            }
        }
        return matList.ToArray();
    }
    */
}
