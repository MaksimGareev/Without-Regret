using UnityEngine;

public class TreeRotater : MonoBehaviour
{
    
    private float rotationValue;

    void Start()
    {
        rotationValue = Random.Range(30f, 360f);

        this.gameObject.transform.Rotate(0, rotationValue, 0);
    }

    
}
