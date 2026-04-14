using UnityEngine;


public class FaceHandler : MonoBehaviour
{
    [Tooltip("NPC's Neutral Facecard")]
    public Material NeutralFace;
    [Tooltip("NPC's Happy Facecard")]
    public Material HappyFace;
    [Tooltip("NPC's Sad Facecard")]
    public Material SadFace;
    [Tooltip("NPC's Angry Facecard")]
    public Material AngryFace;

    [Tooltip("Render attached to the Models Facecard Mesh")]
    public Renderer faceRender;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ResetExpression();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            HappyExpression();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SadExpression();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            AngryExpression();
        }

    }

    public void SetExpression(LineTone tone) //This function links to the Linetone in the dialogue JSON, grabbing the 2D Facecard sprite and matching the appropriate 
    {
        switch (tone)
        {
            case LineTone.Happy:
                faceRender.material = HappyFace;
                Debug.Log("Happy Face!");
                break;
            case LineTone.Neutral:
                faceRender.material = NeutralFace;
                Debug.Log("Neutral Face");
                break;
            case LineTone.Upset:
                faceRender.material = AngryFace;
                Debug.Log("Angry Face!!!");
                break;
        }

    }

    public void ResetExpression()
    {
        //call Neutral Face Material
        if (NeutralFace != null)
        {
            faceRender.material = NeutralFace;
        }
    }

    public void HappyExpression()
    {
        //call Happy Face Material
        if (HappyFace != null)
        {
            faceRender.material = HappyFace;
        }
    }
    public void SadExpression()
    {
        //call Sad Face Material
        if (SadFace != null)
        {
            faceRender.material = SadFace;
        }

    }
    public void AngryExpression()
    {
        //call Angry Face Material
        if (AngryFace != null)
        {
            faceRender.material = AngryFace;
        }
    }

}
