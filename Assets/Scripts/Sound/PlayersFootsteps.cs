using UnityEngine;

public class PlayersFootsteps : MonoBehaviour
{
    public AudioSource audioSource;
    public float StepInterval = 0.5f;

    [Header("Footstep Sounds")]
    [Tooltip("Default Steps can be set to whatever is most appropriate, can also be done per scene")]
    public AudioClip[] DefaultSteps;
    public AudioClip[] GrassSteps;
    public AudioClip[] HardwoodSteps;
    public AudioClip[] PavementSteps;
    public AudioClip[] DirtSteps;


    public CharacterController Controller;
    private float StepTimer;
    public bool footstepsActive = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
        }
    }

    // Update is called once per frame
    // void Update()
    // {
    //     if (Controller.isGrounded && Controller.velocity.magnitude > 0.2f)
    //     {
    //         StepTimer += Time.deltaTime;
    //         if (StepTimer >= StepInterval)
    //         {
    //             PlayFootStep();
    //             StepTimer = 0f;
    //         }
    //     }   
    // }

    public void PlayFootStep()
    {
        if (footstepsActive)
        {
            if (Controller != null)
            {
                if (!Controller.isGrounded || Controller.velocity.magnitude < 0.01f)
                    return;
            }


            SurfaceType surface = GetSurfaceType();
            AudioClip Clip = GetRandomClip(surface);
            if (Clip != null)
            {
                audioSource.PlayOneShot(Clip);
                //Debug.Log("Step");
            }
            if (Clip == null)
            {
                //Debug.Log("NO Step");
            }
        }
    }

    SurfaceType GetSurfaceType()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            Surface surface = hit.collider.GetComponent<Surface>();
            if (surface != null)
            {
                return surface.surfaceType;
            }
        }
        return SurfaceType.Default;
    }

    AudioClip GetRandomClip(SurfaceType surface)
    {
        AudioClip[] Clips = surface switch //List of each surface type and the footstep sounds they are linked to on the player
        {
            SurfaceType.Grass => GrassSteps,
            SurfaceType.Hardwood => HardwoodSteps,
            SurfaceType.Pavement => PavementSteps,
            SurfaceType.Dirt => DirtSteps,
            _ => DefaultSteps
        };

        if (Clips.Length == 0)
        {
            return null;
        }
        return Clips[Random.Range(0, Clips.Length)];
    }

    private void enableFootsteps()
    {
        if (!footstepsActive)
        {
            footstepsActive = true;
        }
    }

    private void disableFootsteps()
    {
        if (footstepsActive)
        {
            footstepsActive = false;
        }
    }

}
