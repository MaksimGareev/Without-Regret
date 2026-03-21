using UnityEngine;

public class TreeRotater : MonoBehaviour
{
    
    private float rotationValue;

    void Start()
    {
        rotationValue = Random.Range(30f, 360f);

        gameObject.transform.Rotate(transform.rotation.x, transform.rotation.y + rotationValue, transform.rotation.z);
    }

    
}
