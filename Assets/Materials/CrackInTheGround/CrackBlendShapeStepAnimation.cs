using UnityEngine;

public class CrackBlendShapeStepSnap : MonoBehaviour
{
    SkinnedMeshRenderer mesh;

    public float minDelay = 0.2f;
    public float maxDelay = 1.0f;

    int current = 0;
    float timer = 0f;
    float nextDelay;

    void Start()
    {
        mesh = GetComponent<SkinnedMeshRenderer>();

        int count = Mathf.Min(13, mesh.sharedMesh.blendShapeCount);

        // Start all at 100
        for (int i = 0; i < count; i++)
            mesh.SetBlendShapeWeight(i, 100f);

        // Random delay for first snap
        nextDelay = Random.Range(minDelay, maxDelay);
    }

    void Update()
    {
        if (current >= 13) return;

        timer += Time.deltaTime;
        if (timer < nextDelay) return;

        timer = 0f;
        nextDelay = Random.Range(minDelay, maxDelay);

        // Snap current to 0
        mesh.SetBlendShapeWeight(current, 0f);

        current++;
    }
}
