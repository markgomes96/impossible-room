using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : PortalTraveler
{
    [Header("Particle Systems")]
    public ParticleSystem trailSystem;
    public ParticleSystem burstSystem;

    [Header("Audio")]
    public AudioSource projectileHitAudio;
    public AudioSource projectileBurstAudio;

    [HideInInspector]
    public float lifeTime = 20;
    [HideInInspector]
    public Vector3 storedVelocity;

    private void Awake()
    {
        projectileHitAudio = transform.GetComponentsInChildren<AudioSource>()[0];
        projectileBurstAudio = transform.GetComponentsInChildren<AudioSource>()[1];
    }

    public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        base.Teleport(fromPortal, toPortal, pos, rot);

        Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
        rigidbody.velocity = toPortal.TransformVector(fromPortal.InverseTransformVector(rigidbody.velocity));
        rigidbody.angularVelocity = toPortal.TransformVector(fromPortal.InverseTransformVector(rigidbody.angularVelocity));
        Physics.SyncTransforms();

        storedVelocity = rigidbody.velocity;
    }

    public void OnSpawn()
    {
        ActivateBody();
        trailSystem.Stop(true);
        trailSystem.Play(false);

        StartCoroutine(HandleLifeTime());
    }

    IEnumerator HandleLifeTime()
    {
        yield return new WaitForSeconds(lifeTime);
        DeactivateBody();
        StartCoroutine(PlayBurstSystem());
    }

    void OnCollisionEnter(Collision collision)
    {
        // Play projectile audio
        projectileHitAudio.PlayOneShot(projectileHitAudio.clip, 1.0f);
        projectileBurstAudio.PlayOneShot(projectileBurstAudio.clip, 0.5f);

        // Handle events
        BroadcastProjectileCollision(collision, storedVelocity);
        DeactivateBody();
        StartCoroutine(PlayBurstSystem());
    }

    IEnumerator PlayBurstSystem()
    {
        trailSystem.Stop(true);
        burstSystem.Play(false);
        while (burstSystem.isEmitting)
        {
            yield return new WaitForEndOfFrame();
        }
        DespawnProjectile();
    }

    public delegate void ProjectileCollision(Collision col, GameObject projectile, Vector3 projVel);
    public event ProjectileCollision OnProjectileCollision;

    public void BroadcastProjectileCollision(Collision col, Vector3 projVel)
    {
        if (OnProjectileCollision != null)
            OnProjectileCollision(col, this.gameObject, projVel);
    }

    public delegate void ProjectileExpired(GameObject projectile);
    public event ProjectileExpired OnProjectileExpired;

    public void BroadcastProjectileExpired()
    {
        if (OnProjectileExpired != null)
            OnProjectileExpired(this.gameObject);
    }

    void ActivateBody()
    {
        transform.GetComponent<Rigidbody>().detectCollisions = true;
        transform.GetComponent<SphereCollider>().enabled = true;
        transform.GetComponentInChildren<MeshRenderer>().enabled = true;
    }

    void DeactivateBody()
    {
        // Stop movement, collision detection, make body not visiable
        Rigidbody rigBod = transform.GetComponent<Rigidbody>();
        rigBod.velocity = Vector3.zero;
        rigBod.detectCollisions = false;
        transform.GetComponent<SphereCollider>().enabled = false;
        transform.GetComponentInChildren<MeshRenderer>().enabled = false;
    }

    void DespawnProjectile()
    {
        this.gameObject.SetActive(false);
    }
}
