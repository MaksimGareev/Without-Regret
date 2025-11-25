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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetRingState(RingState.Full);
    }

    public void SetRingState(RingState state)
    {
        switch (state)
        {
            case RingState.Full:
                ringImage.sprite = ringFull;
                portraitImage.sprite = portraitFull;
                break;
            case RingState.TwoThirds:
                ringImage.sprite = ringTwoThirds;
                portraitImage.sprite = portraitTwoThirds;
                break;
            case RingState.OneThird:
                ringImage.sprite = ringOneThird;
                portraitImage.sprite = portraitOneThird;
                break;
            case RingState.Empty:
                ringImage.sprite = ringEmpty;
                portraitImage.sprite = portraitEmpty;
                break;
        }
    }
}
