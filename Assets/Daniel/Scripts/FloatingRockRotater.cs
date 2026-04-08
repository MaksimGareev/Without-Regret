using Unity.Collections;
using UnityEngine;

public class FloatingRockRotater : MonoBehaviour
{
    
    [SerializeField] private float rotateSpeed;

    private float x,y,z;
     private enum rotation_Axis
    {
        z_axis,
        y_axis,
        x_axis
    };

    [SerializeField] private rotation_Axis currentAxis;

    void Start()
    {
        if(currentAxis is rotation_Axis.z_axis)
        {
            z = 1f;
        }

        if(currentAxis is rotation_Axis.x_axis)
        {
            x = 1f;
        }

        if(currentAxis is rotation_Axis.y_axis)
        {
            y = 1f;
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(x*Time.deltaTime*rotateSpeed,y*Time.deltaTime*rotateSpeed,z*Time.deltaTime*rotateSpeed);
    }


   

    
}
