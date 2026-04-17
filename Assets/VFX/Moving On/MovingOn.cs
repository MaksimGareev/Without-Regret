using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MovingOn : MonoBehaviour
{
    public float startValue = 50f;
    public float endValue = -100f;
    public float duration = 2f;

    public ParticleSystem targetParticle;
    public float particleEndY = -23f;

    float timeElapsed = 0f;
    public List<Renderer> material;
    bool isMoving = false;
    public Penelope penelope;
    public Darry darry;

    float particleStartY;

    void Start()
    {
        if (targetParticle != null)
        {
            targetParticle.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!isMoving) return;

        if (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;

            // Update shader float
            float value = Mathf.Lerp(startValue, endValue, t);
            for (int i = 0; i < material.Count; i++)
            {
                material[i].material.SetFloat("_MovingOn", value);
            }

            // Move particle system
            if (targetParticle != null)
            {
                Vector3 pos = targetParticle.transform.localPosition;
                pos.y = Mathf.Lerp(particleStartY, particleEndY, t);
                targetParticle.transform.localPosition = pos;

                // Deactivate when done
                if (t >= 1f)
                {
                    targetParticle.Stop();
                    targetParticle.gameObject.SetActive(false);
                    isMoving = false;
                    //gameObject.SetActive(false);
                }
            }
        }
    }

    public void StartMoving()
    {
        timeElapsed = 0f;
        isMoving = true;

        for (int i = 0; i < material.Count; i++)
        {
            material[i].material.SetFloat("_MovingOn", startValue);
        }

        if (targetParticle != null)
        {
            Vector3 pos = targetParticle.transform.localPosition;
            particleStartY = pos.y;

            targetParticle.gameObject.SetActive(true);
            targetParticle.Play(); // <-- Make sure it actually starts
        }

        if (penelope != null)
        {
            penelope.StartDissolve(1.5f);
        }

        if (darry != null)
        {
            darry.StartDissolve(1.5f);
        }
    }
}