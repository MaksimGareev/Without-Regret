using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimerRingUI : MonoBehaviour
{
    public enum RingState
    {
        Full,
        TwoThirds,
        OneThird,
        Empty
    }

    public enum PlayerPortrait
    {
        Echo,
        Chime
    }

    [Header("References")]
    [SerializeField] private Image ringImage;
    [SerializeField] private Image portraitImage;

    [Header("Ring Textures")]
    [SerializeField] private Sprite ringFull;
    [SerializeField] private Sprite ringTwoThirds;
    [SerializeField] private Sprite ringOneThird;
    [SerializeField] private Sprite ringEmpty;

    [Header("Portrait Textures")]
    [SerializeField] private Sprite EchoPortraitFull;
    [SerializeField] private Sprite EchoPortraitTwoThirds;
    [SerializeField] private Sprite EchoPortraitOneThird;
    [SerializeField] private Sprite EchoPortraitEmpty;
    
    [SerializeField] private Sprite ChimePortraitFull;
    [SerializeField] private Sprite ChimePortraitTwoThirds;
    [SerializeField] private Sprite ChimePortraitOneThird;
    [SerializeField] private Sprite ChimePortraitEmpty;
    
    public PlayerPortrait currentPortrait =  PlayerPortrait.Echo;

    [Header("Animation")]
    public Animator animator;
    private CharacterSwap characterSwap;
    
    public RingState currentRingState;
    public static TimerRingUI Instance { get; private set; }

    public UIFadeController uiFade;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        uiFade = FindFirstObjectByType<UIFadeController>();
        
        SetRingState(RingState.Full);
    }

    private void OnEnable()
    {
        characterSwap = FindFirstObjectByType<CharacterSwap>();
        
        if (characterSwap != null)
        {
            animator = characterSwap.GetAnimator();

            characterSwap.onAnimatorChanged += UpdateAnimator;
        }
        else
        {
            Debug.LogError("No CharacterSwap found!");
        }
    }

    public void Update()
    {
        if (Time.timeSinceLevelLoad < 0.1 && currentRingState ==  RingState.Empty)
        {
            SetRingState(RingState.Full);
        }
    }

    public void SubtractRingSection(int sections)
    {
        for (int i = 0; i < sections; i++)
        {
            switch (currentRingState)
            {
                case RingState.Full:
                    if (uiFade && !uiFade.inExcludedScene) uiFade.ShowUI();
                    SetRingState(RingState.TwoThirds);
                    break;
                
                case RingState.TwoThirds:
                    if (uiFade && !uiFade.inExcludedScene) uiFade.ShowUI();
                    SetRingState(RingState.OneThird);
                    break;
                
                case RingState.OneThird:
                    if (uiFade && !uiFade.inExcludedScene) uiFade.ShowUI();
                    SetRingState(RingState.Empty);
                    EndGame();
                    break;
                
                case RingState.Empty:
                    // Already empty, try ending game again if not already
                    if (GameOverManager.Instance && !GameOverManager.Instance.IsGameOver)
                    {
                        EndGame();
                    }
                    break;
            }
        }
    }

    public void AddRingSection(int sections)
    {
        for (int i = 0; i < sections; i++)
        {
            switch (currentRingState)
            {
                case RingState.Empty:
                    if (uiFade && !uiFade.inExcludedScene) uiFade.ShowUI();
                    SetRingState(RingState.OneThird);
                    break;
                
                case RingState.OneThird:
                    if (uiFade && !uiFade.inExcludedScene) uiFade.ShowUI();
                    SetRingState(RingState.TwoThirds);
                    break;
                
                case RingState.TwoThirds:
                    if (uiFade && !uiFade.inExcludedScene) uiFade.ShowUI();
                    SetRingState(RingState.Full);
                    break;
                
                case RingState.Full:
                    if (uiFade) uiFade.ShowUI();
                    break;
            }
        }
    }
    
    private void EndGame()
    {   
        if (GameOverManager.Instance)
        {
            Debug.Log("Timer has run out! Triggering end game sequence.");
            
            if (animator)
            {
                StartCoroutine(GameOverAnimation());
            }
            else if (characterSwap)
            {
                animator = characterSwap.GetAnimator();
                StartCoroutine(GameOverAnimation());
            }
            
            GameOverManager.Instance.TriggerGameOver();
        }
        else
        {
            Debug.LogError("GameOverManager instance not found! Cannot trigger game over.");
        }
    }

    public void SetRingState(RingState state)
    {
        switch (state)
        {
            case RingState.Full:
                ringImage.sprite = ringFull;
                portraitImage.sprite = currentPortrait == PlayerPortrait.Echo ? EchoPortraitFull : ChimePortraitFull;
                currentRingState = RingState.Full;
                break;
            
            case RingState.TwoThirds:
                ringImage.sprite = ringTwoThirds;
                portraitImage.sprite = currentPortrait == PlayerPortrait.Echo ? EchoPortraitTwoThirds : ChimePortraitTwoThirds;
                currentRingState = RingState.TwoThirds;
                break;
            
            case RingState.OneThird:
                ringImage.sprite = ringOneThird;
                portraitImage.sprite = currentPortrait == PlayerPortrait.Echo ? EchoPortraitOneThird : ChimePortraitOneThird;
                currentRingState = RingState.OneThird;
                break;
            
            case RingState.Empty:
                ringImage.sprite = ringEmpty;
                portraitImage.sprite = currentPortrait == PlayerPortrait.Echo ? EchoPortraitEmpty : ChimePortraitEmpty;
                currentRingState = RingState.Empty;
                break;
        }
    }
    
    void UpdateAnimator(Animator newAnimator)
    {
        animator = newAnimator;

        if (characterSwap.isEcho && currentPortrait != PlayerPortrait.Echo)
        {
            currentPortrait = PlayerPortrait.Echo;
            SetRingState(currentRingState);
            Debug.Log("Updated portrait to  " + currentPortrait);
        }
        else if (characterSwap.isChime && currentPortrait != PlayerPortrait.Chime)
        {
            currentPortrait = PlayerPortrait.Chime;
            SetRingState(currentRingState);
            Debug.Log("Updated portrait to  " + currentPortrait);
        }
    }

    IEnumerator GameOverAnimation()
    {
        Debug.Log("Started Game Over Animation");
        animator?.SetBool("GameOver", true);
        yield return new WaitForSecondsRealtime(0.5f);
        animator?.SetBool("GameOverLoop", true);
    }

}
