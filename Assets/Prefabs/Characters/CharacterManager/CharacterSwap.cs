using UnityEngine;

public class CharacterSwap : MonoBehaviour
{
    public System.Action<Animator> onAnimatorChanged;
    public GameObject Echo;
    public GameObject Chime;
    public bool isPlayingEcho;
    public bool isPlayingChime;

    public Animator currentAnimator;

    void Awake()
    {
        isPlayingEcho = true;
        isPlayingChime = false;

        Echo.SetActive(true);
        Chime.SetActive(false);

        SetCurrentAnimator(Echo);
    }    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetCurrentAnimator(Echo);
        Chime.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwapCharacters();
        }
    }

    public void SwapCharacters()
    {
        if (isPlayingEcho)
        {
            Debug.Log("Swapped to Chime");
            isPlayingChime = true;
            isPlayingEcho = false;
            Chime.SetActive(true);
            Echo.SetActive(false);

            SetCurrentAnimator(Chime);
        }
        else if (isPlayingChime)
        {
            Debug.Log("Swapped to Echo");
            isPlayingEcho = true;
            isPlayingChime = false;
            Echo.SetActive(true);
            Chime.SetActive(false);

            SetCurrentAnimator(Echo);
        }
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
