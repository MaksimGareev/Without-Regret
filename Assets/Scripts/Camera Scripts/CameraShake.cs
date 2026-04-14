using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public Transform cameraToShake; // camera transform to move
    public float duration = 1.3f;
    public float magnitude = 0.3f;

    [Header("Audio")]
    public AudioSource audioSource; // optional audio source
    public AudioClip shakeSound;    // sound to play

    private Vector3 originalPos;

    void Update()
    {
        // press 0 to trigger shake
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            StartShake();
        }
    }

    // call this function to start shake
    public void StartShake()
    {
        if (cameraToShake == null) return;

        StopAllCoroutines();
        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        originalPos = cameraToShake.localPosition;

        // play sound if assigned
        if (audioSource != null && shakeSound != null)
        {
            audioSource.PlayOneShot(shakeSound);
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            cameraToShake.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraToShake.localPosition = originalPos;
    }
}