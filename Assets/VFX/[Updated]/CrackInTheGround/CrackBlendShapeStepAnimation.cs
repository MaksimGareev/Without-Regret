using UnityEngine;

public class CrackBlendShapeStepSnap : MonoBehaviour
{

    SkinnedMeshRenderer mesh;

    //delay between snaps
    public float minDelay = 0.2f;
    public float maxDelay = 1.0f;


    int current = 0;
    float timer = 0f;
    float nextDelay;
    bool running = false;

    // Force all blend shapes to start at 100, pick the first random
    void Start()
    {
        mesh = GetComponent<SkinnedMeshRenderer>();

        int count = Mathf.Min(13, mesh.sharedMesh.blendShapeCount);

        for (int i = 0; i < count; i++)
            mesh.SetBlendShapeWeight(i, 100f);

        nextDelay = Random.Range(minDelay, maxDelay);
    }

    // main update loop
    void Update()
    {
        if (!running || current >= 13)
            return;

        timer += Time.deltaTime;

        if (timer < nextDelay)
            return;

        timer = 0f;
        nextDelay = Random.Range(minDelay, maxDelay);

        mesh.SetBlendShapeWeight(current, 0f);
        current++;
    }

    // trigger entry point
    public void StartCrack()
    {
        running = true;
    }
}
