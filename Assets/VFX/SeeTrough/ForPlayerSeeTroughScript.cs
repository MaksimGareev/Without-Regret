using UnityEngine;

public class ForPlayerSeeTroughScript : MonoBehaviour
{
    public static int PlayerPosID = Shader.PropertyToID("_PlayerPosition");
    public static int SizeID = Shader.PropertyToID("_Size");

     Material WallMaterial;
    [SerializeField] Camera Cam;
    [SerializeField] LayerMask Mask;

    private RaycastHit hit; //the raycast hit that returns information about the material

    void Update()
    {
        // Direction from player to camera
        var dir = Cam.transform.position - transform.position;

        // Ray from player toward camera
        //var ray = new Ray(transform.position, dir.normalized);

        // Check if something blocks the player
        if (Physics.Raycast(transform.position, dir, out hit, Mask)){
            WallMaterial =  hit.collider.gameObject.GetComponent<Material>(); //gets the material of whatever the raycast hit and assigns it to this variable
            WallMaterial.SetFloat(SizeID, 1f);
        }
        else
        {
            WallMaterial.SetFloat(SizeID, 0f);
        }
        

        // Send player position to shader in viewport space
        var view = Cam.WorldToViewportPoint(transform.position);
        WallMaterial.SetVector(PlayerPosID, view);
    }
}