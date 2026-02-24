using UnityEngine;

public class CrackTest2 : MonoBehaviour
{
    // SkinnedMeshRenderer that contains the blend shapes
    SkinnedMeshRenderer mesh;

    // Random delay range between each snap
    public float minDelay = 0.2f;
    public float maxDelay = 1.0f;

    // Internal state
    int current = 0;
    int count = 0;
    float timer = 0f;
    float nextDelay;
    bool running = false;

    void Start()
    {
        // Get the SkinnedMeshRenderer on this object
        mesh = GetComponent<SkinnedMeshRenderer>();

        if (mesh == null || mesh.sharedMesh == null)
        {
            Debug.LogWarning("No SkinnedMeshRenderer or mesh found!");
            return;
        }

        // Read ALL blend shapes on the mesh
        count = mesh.sharedMesh.blendShapeCount;

        // Start everything at 100
        for (int i = 0; i < count; i++)
            mesh.SetBlendShapeWeight(i, 100f);

        // Choose initial random delay
        nextDelay = Random.Range(minDelay, maxDelay);
    }

    void Update()
    {
        // Do nothing until StartCrack() is called
        if (!running || current >= count)
            return;

        // Wait for the random delay
        timer += Time.deltaTime;
        if (timer < nextDelay)
            return;

        // Reset timer and pick a new random delay
        timer = 0f;
        nextDelay = Random.Range(minDelay, maxDelay);

        // Snap from 100 -> 0
        mesh.SetBlendShapeWeight(current, 0f);
        current++;
    }

    // Called by trigger object
    public void StartCrack()
    {
        running = true;
    }
}