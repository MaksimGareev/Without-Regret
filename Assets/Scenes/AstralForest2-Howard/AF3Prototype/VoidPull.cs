using UnityEngine;

public class VoidPull : MonoBehaviour
{
    public float pullSpeed = 6f; // Speed the player gets pulled down

    void Update()
    {
        transform.position += Vector3.down * pullSpeed * Time.deltaTime; // Frame by frame player gets pulled down
    }
}