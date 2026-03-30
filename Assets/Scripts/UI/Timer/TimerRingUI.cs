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

    public UIFadeConrtoller uiFade;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        characterSwap = FindObjectOfType<CharacterSwap>();
        uiFade = FindFirstObjectByType<UIFadeConrtoller>();
        if (characterSwap != null)
        {
            animator = characterSwap.GetAnimator();

            characterSwap.onAnimatorChanged += UpdateAnimator;
        }

        SetRingState(RingState.Full);

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SubtractRingSection(1);
        }
    }

    public void SubtractRingSection(int sections)
    {
        for (int i = 0; i < sections; i++)
        {
            switch (currentRingState)
            {
                case RingState.Full:
                    SetRingState(RingState.TwoThirds);
                    if (uiFade != null) uiFade.ShowUI();
                    break;
                case RingState.TwoThirds:
                    SetRingState(RingState.OneThird);
                    if (uiFade != null) uiFade.ShowUI();
                    break;
                case RingState.OneThird:
                    SetRingState(RingState.Empty);
                    if (uiFade != null) uiFade.ShowUI();
                    EndGame();
                    break;
                case RingState.Empty:
                    // Already empty, do nothing
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
                    SetRingState(RingState.OneThird);
                    break;
                case RingState.OneThird:
                    SetRingState(RingState.TwoThirds);
                    break;
                case RingState.TwoThirds:
                    SetRingState(RingState.Full);
                    break;
                case RingState.Full:
                    break;
            }
        }
    }


    private void EndGame()
    {   
        if (GameOverManager.Instance != null)
        {
            Debug.Log("Timer has run out! Triggering end game sequence.");
            GameOverManager.Instance.TriggerGameOver();
            animator.SetBool("GameOver", true);
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
                //animator.SetBool("GameOver", false);
                //animator.SetBool("GameOverLoop", false);
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

    IEnumerator wait()
    {
        yield return new WaitForSeconds(1);
        animator.SetBool("GameOverLoop", true);
    }

}
