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

    [Header("Invincibility Frame Length")]
    [SerializeField] private float invincibilityFrameDuration = 1.5f;

    [Header("References")]
    [SerializeField] private Image ringImage;
    [SerializeField] private Image portraitImage;

    [Header("Ring Textures")]
    [SerializeField] private Sprite ringFull;
    [SerializeField] private Sprite ringTwoThirds;
    [SerializeField] private Sprite ringOneThird;
    [SerializeField] private Sprite ringEmpty;

    [Header("Portrait Textures")]
    [SerializeField] private Sprite portraitFull;
    [SerializeField] private Sprite portraitTwoThirds;
    [SerializeField] private Sprite portraitOneThird;
    [SerializeField] private Sprite portraitEmpty;

    [Header("Animation")]
    public Animator animator;
    private CharacterSwap characterSwap;

    public RingState currentRingState;
    public static TimerRingUI Instance { get; private set; }

    public UIFadeController uiFade;

    private bool isInvincible = false;

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
        
        characterSwap = FindFirstObjectByType<CharacterSwap>();
        uiFade = FindFirstObjectByType<UIFadeController>();
        if (characterSwap != null)
        {
            animator = characterSwap.GetAnimator();

            characterSwap.onAnimatorChanged += UpdateAnimator;
        }

        SetRingState(RingState.Full);
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
        if (isInvincible) return;

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

        isInvincible = true;
        StartCoroutine(InvincibilityCoroutine());
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
                portraitImage.sprite = portraitFull;
                currentRingState = RingState.Full;
                break;
            
            case RingState.TwoThirds:
                ringImage.sprite = ringTwoThirds;
                portraitImage.sprite = portraitTwoThirds;
                currentRingState = RingState.TwoThirds;
                break;
            
            case RingState.OneThird:
                ringImage.sprite = ringOneThird;
                portraitImage.sprite = portraitOneThird;
                currentRingState = RingState.OneThird;
                break;
            
            case RingState.Empty:
                ringImage.sprite = ringEmpty;
                portraitImage.sprite = portraitEmpty;
                currentRingState = RingState.Empty;
                break;
        }
    }
    
    void UpdateAnimator(Animator newAnimator)
    {
        animator = newAnimator;
    }

    IEnumerator GameOverAnimation()
    {
        Debug.Log("Started Game Over Animation");
        animator?.SetBool("GameOver", true);
        yield return new WaitForSecondsRealtime(0.5f);
        animator?.SetBool("GameOverLoop", true);
    }

    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityFrameDuration);
        isInvincible = false;
    }

}
