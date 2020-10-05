using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShadowPlayer : PortalTraveler
{
    [Header("Particle Systems")]
    public ParticleSystem explosionSystem;

    [Header("Audio")]
    AudioSource shadowExplosionAudio;

    Transform playerT;

    void Awake()
    {
        shadowExplosionAudio = transform.GetComponent<AudioSource>();
    }

    public void StartShadowMovement(GameObject player, Vector3 projVel)
    {
        playerT = player.transform;

        // Add player to portal camera mask
        player.layer = 11;
        for (int i = 0; i < player.transform.childCount; i++)
        {
            player.transform.GetChild(i).gameObject.layer = 11;
            for (int j = 0; j < player.transform.GetChild(i).childCount; j++)
            {
                player.transform.GetChild(i).GetChild(j).gameObject.layer = 11;
            }
        }

        // Add shadow player to main camera mask
        this.gameObject.layer = 10;
        for (int i = 0; i < this.transform.childCount; i++)
        {
            this.transform.GetChild(i).gameObject.layer = 10;
            for (int j = 0; j < this.transform.GetChild(i).childCount; j++)
            {
                this.transform.GetChild(i).GetChild(j).gameObject.layer = 10;
            }
        }

        // Rotate player to face in direction of projectile

        // Rotate the forward vector towards the target direction
        float velScal = -1f;
        if (SceneManager.GetActiveScene().name.Equals("Level_1"))
            velScal = -1f;
        else if (SceneManager.GetActiveScene().name.Equals("Level_2"))
            velScal = 1f;
        else if (SceneManager.GetActiveScene().name.Equals("Level_3"))
            velScal = -11f;

        Vector3 newDirection = Vector3.RotateTowards(transform.forward, projVel * velScal, 4.0f, 0.0f);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        Quaternion newRotation = Quaternion.LookRotation(newDirection);

        StartCoroutine(RotateToRotation(newRotation, 2.5f));
    }

    IEnumerator RotateToRotation(Quaternion newRotation, float speed)
    {
        
        while (Mathf.Abs(Quaternion.Angle(newRotation, transform.rotation)) > 0.1f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * speed);
            yield return new WaitForEndOfFrame();
        }
        // Go to next step
        MoveShadowToPlayer();
        yield return null;
    }

    void MoveShadowToPlayer()
    {
        transform.GetComponent<Rigidbody>().velocity = transform.forward * 5f;
    }

    public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        base.Teleport(fromPortal, toPortal, pos, rot);

        Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
        //rigidbody.velocity = toPortal.TransformVector(fromPortal.InverseTransformVector(rigidbody.velocity));
        //rigidbody.angularVelocity = toPortal.TransformVector(fromPortal.InverseTransformVector(rigidbody.angularVelocity));
        // Turn to face player
        transform.LookAt(playerT);
        rigidbody.velocity = transform.forward * 5f;

        // Remove duplicate from Main Camera Mask and add to Portal Camera Mask
        this.gameObject.layer = 11;
        for (int i = 0; i < this.transform.childCount; i++)
        {
            this.transform.GetChild(i).gameObject.layer = 11;
            for (int j = 0; j < this.transform.GetChild(i).childCount; j++)
            {
                this.transform.GetChild(i).GetChild(j).gameObject.layer = 11;
            }
        }

        StartCoroutine(ExplodeInFrontOfPlayer());
    }

    IEnumerator ExplodeInFrontOfPlayer()
    {
        // Wait for shadow to get in front of player
        while (Vector3.Distance(transform.position, playerT.position) > 5f)
        {
            yield return new WaitForEndOfFrame();
        }

        // Stop the shadow
        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;

        // Pause for some time
        yield return new WaitForSeconds(2f);

        // Play shadow explosion audio
        shadowExplosionAudio.PlayOneShot(shadowExplosionAudio.clip, 0.4f);

        // Deactivate collisions
        transform.GetComponent<Rigidbody>().detectCollisions = false;
        transform.GetComponent<CapsuleCollider>().enabled = false;

        // Hide shadow body
        MeshRenderer[] meshs = transform.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.enabled = false;
        }
        //transform.GetComponentInChildren<MeshRenderer>().enabled = false;

        // Activate explosion partical effect
        explosionSystem.Stop(true);
        explosionSystem.Play(false);
        while (explosionSystem.isEmitting)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        // Tell game manager to go to next scene
        playerT.GetComponent<PlayerMovement>().BroadcastNextStage();

        yield return null;
    }
}
