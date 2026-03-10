using UnityEngine;

public class ForPlayerSeeTroughScript : MonoBehaviour
{
    public static int PlayerPosID = Shader.PropertyToID("_PlayerPosition");
    public static int SizeID = Shader.PropertyToID("_Size");

    public Material WallMaterial;
    public Camera Cam;
    public LayerMask Mask;

    void Update()
    {
        // Direction from player to camera
        var dir = Cam.transform.position - transform.position;

        // Ray from player toward camera
        var ray = new Ray(transform.position, dir.normalized);

        // Check if something blocks the player
        if (Physics.Raycast(ray, 3000, Mask))
            WallMaterial.SetFloat(SizeID, 1f);
        else
            WallMaterial.SetFloat(SizeID, 0f);

        // Send player position to shader in viewport space
        var view = Cam.WorldToViewportPoint(transform.position);
        WallMaterial.SetVector(PlayerPosID, view);
    }
}