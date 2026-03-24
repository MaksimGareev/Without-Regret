using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayersFootsteps : MonoBehaviour
{
    public AudioSource audioSource;
    public float StepInterval = 0.5f;

    [Header("Footstep Sounds")]
    public AudioClip[] DefaultSteps;
    public AudioClip[] GrassSteps;
    public AudioClip[] HardwoodSteps;

    private CharacterController Controller;
    private float StepTimer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Controller = GetComponent<CharacterController>();
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
        SurfaceType surface = GetSurfaceType();
        AudioClip Clip = GetRandomClip(surface);
        if (Clip != null)
        {
            audioSource.PlayOneShot(Clip);
            Debug.Log("Step");
        }
        if (Clip == null)
        {
            Debug.Log("Step");
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
        AudioClip[] Clips = surface switch
        {
            SurfaceType.Grass => GrassSteps,
            SurfaceType.Hardwood => HardwoodSteps,
            _ => DefaultSteps
        };

        if (Clips.Length == 0)
        {
            return null;
        }
        return Clips[Random.Range(0, Clips.Length)];
    }
}
