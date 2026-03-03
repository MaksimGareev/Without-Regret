using UnityEngine;

public class Tendril : MonoBehaviour
{
    private float originalY;

    private void Awake()
    {
        originalY = transform.position.y;
    }

    private void OnEnable()
    {
        // Move the tendril up to its original Y position to make it visible
        transform.position = new Vector3(transform.position.x, originalY, transform.position.z);
    }

    private void OnDisable()
    {
        // The tendril is deep underground and should not be visible at the start of the game, so we move it down to a very low position.
        // It will be moved up into view when the player enters the ObjectSpawnTriggerVolume.
        transform.position = new Vector3(transform.position.x, -100f, transform.position.z);
    }
}
