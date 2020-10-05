using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(PlayerMovement))]
public class ProjectileManager : MonoBehaviour
{
    [Header("Settings")]
    public float size = 0.3f;
    public float speed = 20f;
    public float lifeTime = 5f;
    public float fireWaitTime = 0.5f;

    [Header("Audio")]
    public AudioSource projectileFireAudio; 
    public AudioSource shadowHitAudio;

    // private
    PlayerMovement playerMovement;
    Transform playerCam;
    bool activatedShadowPlayer;
    bool canFireProjectile;

    void Start()
    {
        playerMovement = transform.GetComponent<PlayerMovement>();
        playerCam = Camera.main.transform;
        playerMovement.OnMouseButtonPressed += CheckMouseButtonPress;
        activatedShadowPlayer = false;
        canFireProjectile = true;
    }

    void OnDisable()
    {
        //playerMovement.OnMouseButtonPressed -= CheckMouseButtonPress;
    }

    public void CheckMouseButtonPress(int mouseButton)
    {
        // Check for primary click
        if (mouseButton == 0 && canFireProjectile)
        {
            FireProjectile();
            canFireProjectile = false;
            StartCoroutine(FireProjectileCooldown());
        }
    }

    IEnumerator FireProjectileCooldown()
    {
        yield return new WaitForSeconds(fireWaitTime);
        canFireProjectile = true;
        yield return null;
    }

    void FireProjectile()
    {
        // Play fire projectile audio
        projectileFireAudio.PlayOneShot(projectileFireAudio.clip, 0.7f);

        // Spawn a projectile from PoolManager
        Vector3 spawnPos = transform.position + (transform.forward * 1.2f) + (transform.up * playerCam.transform.localPosition.y);
        GameObject spawn = PoolManager.instance.SpawnFromPool("Projectile", spawnPos, playerCam.rotation);

        // Set projectile properties
        spawn.transform.localScale = new Vector3(size, size, size);
        spawn.GetComponent<Projectile>().lifeTime = lifeTime;

        Vector3 spawnVelocity = spawn.transform.forward * speed;
        spawn.GetComponent<Rigidbody>().velocity = spawnVelocity;
        spawn.GetComponent<Projectile>().storedVelocity = spawnVelocity;

        spawn.GetComponent<Projectile>().OnSpawn();

        // Subcribe to projectile events
        spawn.GetComponent<Projectile>().OnProjectileCollision += CheckProjectileCollision;
        spawn.GetComponent<Projectile>().OnProjectileExpired += UnsubscribeFromProjectile;
    }

    void CheckProjectileCollision(Collision col, GameObject projectile, Vector3 projVel)
    {
        // Check if projectile has hit player
        if (col.collider.gameObject.CompareTag("Player") && !activatedShadowPlayer)
        {
            // Play shadow hit audio
            shadowHitAudio.PlayOneShot(shadowHitAudio.clip, 0.7f);

            activatedShadowPlayer = true;

            // Freeze player movement and actions
            transform.GetComponent<PlayerMovement>().FreezePlayer();

            // Spawn a shadow player in same position/rotation as player
            var spawn = PoolManager.instance.SpawnFromPool("ShadowPlayer", transform.position, transform.rotation);

            // Activate shadow player
            spawn.GetComponent<ShadowPlayer>().StartShadowMovement(this.gameObject, projVel);

            // Broadcast that shadow player starts
            BroadcastStartShadowPlayer();
        }

        UnsubscribeFromProjectile(projectile);
    }

    public delegate void StartShadowPlayer();
    public event StartShadowPlayer OnStartShadowPlayer;

    public void BroadcastStartShadowPlayer()
    {
        if (OnStartShadowPlayer != null)
            OnStartShadowPlayer();
    }

    void UnsubscribeFromProjectile(GameObject projectile)
    {
        // Unsubscribe from projectile
        projectile.GetComponent<Projectile>().OnProjectileCollision -= CheckProjectileCollision;
    }
}
