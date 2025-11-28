using UnityEngine;
using UnityEngine.UI;

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

    private RingState currentRingState;
    public static TimerRingUI Instance { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

    public void SubtractRingSection(int sections)
    {
        for (int i = 0; i < sections; i++)
        {
            switch (currentRingState)
            {
                case RingState.Full:
                    SetRingState(RingState.TwoThirds);
                    break;
                case RingState.TwoThirds:
                    SetRingState(RingState.OneThird);
                    break;
                case RingState.OneThird:
                    SetRingState(RingState.Empty);
                    break;
                case RingState.Empty:
                    EndGame();
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
}
