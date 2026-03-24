using UnityEngine;

public class MovingOn : MonoBehaviour
{
    public float startValue = 16.5f;
    public float endValue = -2f;
    public float duration = 2f;

    public ParticleSystem targetParticle;
    public float particleStartY = 23f;
    public float particleEndY = -23f;

    float timeElapsed = 0f;
    Material material;
    bool isMoving = false;

    void Start()
    {
        material = GetComponent<Renderer>().material;

        if (targetParticle != null)
        {
            Vector3 pos = targetParticle.transform.position;
            pos.y = particleStartY;
            targetParticle.transform.position = pos;

            targetParticle.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Trigger animation on pressing '9'
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            StartMoving();
        }

        if (!isMoving) return;

        if (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;

            // Update shader float
            float value = Mathf.Lerp(startValue, endValue, t);
            material.SetFloat("_MovingOn", value);

            // Move particle system
            if (targetParticle != null)
            {
                Vector3 pos = targetParticle.transform.position;
                pos.y = Mathf.Lerp(particleStartY, particleEndY, t);
                targetParticle.transform.position = pos;

                // Deactivate when done
                if (t >= 1f)
                {
                    targetParticle.Stop();
                    targetParticle.gameObject.SetActive(false);
                    isMoving = false;
                }
            }
        }
    }

    public void StartMoving()
    {
        if (targetParticle != null)
        {
            targetParticle.gameObject.SetActive(true);
            targetParticle.Play(); // <-- Make sure it actually starts
            Vector3 pos = targetParticle.transform.position;
            pos.y = particleStartY;
            targetParticle.transform.position = pos;
        }

        timeElapsed = 0f;
        isMoving = true;
    }
}