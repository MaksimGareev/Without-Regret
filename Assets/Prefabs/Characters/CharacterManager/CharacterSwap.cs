using UnityEngine;

[System.Serializable]
public enum PlayerModel
{
    Echo,
    Chime
}

public class CharacterSwap : MonoBehaviour
{
    [Tooltip("The character that the model should be when starting the game.")]
    public PlayerModel startingPlayerModel = PlayerModel.Echo;
    public System.Action<Animator> onAnimatorChanged;
    public GameObject Echo;
    public GameObject Chime;
    [HideInInspector] public bool isEcho;
    [HideInInspector] public bool isChime;

    public Animator currentAnimator;

    void Awake()
    {
        if (Echo == null)
        {
            Debug.LogError("Reference to Echo gameobject is null in CharacterSwap script.");
        }

        if (Chime == null)
        {
            Debug.LogError("Reference to Chime gameobject is null in CharacterSwap script.");
        }
        
        if (startingPlayerModel == PlayerModel.Echo)
        {
            SwitchToEcho();
        }
        else
        {
            SwitchToChime();
        }
    }
    
    void Start()
    {
        if (startingPlayerModel == PlayerModel.Echo)
        {
            SwitchToEcho();
        }
        else
        {
            SwitchToChime();
        }
    }

    // Update is called once per frame
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.C))
    //     {
    //         SwapCharacters();
    //     }
    // }

    public void SwapCharacters()
    {
        if (isEcho)
        {
            SwitchToChime();
        }
        else if (isChime)
        {
            SwitchToEcho();
        }
    }

    public void SwitchToEcho()
    {
        if (isEcho) return;
        
        //Debug.Log("Swapped to Echo");
        isEcho = true;
        isChime = false;
        Echo.SetActive(true);
        Chime.SetActive(false);

        SetCurrentAnimator(Echo);
    }

    public void SwitchToChime()
    {
        if (isChime) return;
        
        Debug.Log("Swapped to Chime");
        isChime = true;
        isEcho = false;
        Chime.SetActive(true);
        Echo.SetActive(false);

        SetCurrentAnimator(Chime);
    }

    public void SetCurrentAnimator(GameObject model)
    {
        currentAnimator = model.GetComponentInChildren<Animator>(true);

        onAnimatorChanged?.Invoke(currentAnimator);
    }

    public Animator GetAnimator()
    {
        return currentAnimator;
    }    
}
