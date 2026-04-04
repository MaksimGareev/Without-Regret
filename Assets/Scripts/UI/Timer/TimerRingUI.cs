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

    private enum PlayerPortrait
    {
        Echo,
        Chime
    }
    
    public static TimerRingUI Instance { get; private set; }

    [Header("References")]
    [Tooltip("The image component that represents the health ring behind the portrait. This will be updated to show the current state of the health.")]
    [SerializeField] private Image ringImage;
    
    [Tooltip("The image component that shows the portrait of the current character.")]
    [SerializeField] private Image portraitImage;

    [Header("Ring Textures")]
    [Tooltip("The sprite to be used for the full ring state. This should be a complete ring with no sections missing.")]
    [SerializeField] private Sprite ringFull;
    
    [Tooltip("The sprite to be used for the two thirds ring state. This should be a ring with one section missing, leaving two thirds of the ring remaining.")]
    [SerializeField] private Sprite ringTwoThirds;
    
    [Tooltip("The sprite to be used for the one third ring state. This should be a ring with two sections missing, leaving one third of the ring remaining.")]
    [SerializeField] private Sprite ringOneThird;
    
    [Tooltip("The sprite to be used for the empty ring state. This should be a ring with all sections missing, leaving an empty circle.")]
    [SerializeField] private Sprite ringEmpty;
    
    [Header("Echo Portrait Textures")]
    [Tooltip("The sprite to be used when the player is playing as Echo and the health is full. Should be the happiest of the portraits.")]
    [SerializeField] private Sprite EchoPortraitFull;
    
    [Tooltip("The sprite to be used when the player is playing as Echo and the health is at two thirds. Should be progressively less happy than the previous portrait.")]
    [SerializeField] private Sprite EchoPortraitTwoThirds;
    
    [Tooltip("The sprite to be used when the player is playing as Echo and the health is at one thirds. Should be progressively less happy than the previous portrait.")]
    [SerializeField] private Sprite EchoPortraitOneThird;
    
    [Tooltip("The sprite to be used when the player is playing as Echo and the health is empty. Should be the saddest looking of the portraits.")]
    [SerializeField] private Sprite EchoPortraitEmpty;
    
    [Header("Chime Portrait Textures")]
    [Tooltip("The sprite to be used when the player is playing as Chime and the health is full. Should be the happiest of the portraits.")]
    [SerializeField] private Sprite ChimePortraitFull;
    
    [Tooltip("The sprite to be used when the player is playing as Chime and the health is at two thirds. Should be progressively less happy than the previous portrait.")]
    [SerializeField] private Sprite ChimePortraitTwoThirds;
    
    [Tooltip("The sprite to be used when the player is playing as Chime and the health is at one thirds. Should be progressively less happy than the previous portrait.")]
    [SerializeField] private Sprite ChimePortraitOneThird;
    
    [Tooltip("The sprite to be used when the player is playing as Chime and the health is empty. Should be the saddest looking of the portraits.")]
    [SerializeField] private Sprite ChimePortraitEmpty;
    
    private PlayerPortrait currentPortrait =  PlayerPortrait.Echo;
    
    [Header("Damage Cooldown Settings")]
    [Tooltip("The time in seconds that the player will be invincible for before being able to take damage again")]
    [SerializeField, Range(0.0f, 5.0f)] private float damageCooldown = 1.0f;
    public bool canTakeDamage = true;
    private float damageAvailableTime = 0f;
    
    private Animator animator;
    private CharacterSwap characterSwap;
    
    public RingState currentRingState;
    private UIFadeController uiFade;
    

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

            if (characterSwap.isEcho)
            {
                currentPortrait = PlayerPortrait.Echo;
                SetRingState(currentRingState);
            }
            else
            {
                currentPortrait = PlayerPortrait.Chime;
                SetRingState(currentRingState);
            }
        }
        else
        {
            Debug.LogError("No CharacterSwap found!");
        }
    }

    public void Update()
    {
        if (Time.timeSinceLevelLoad < 0.1 && currentRingState == RingState.Empty)
        {
            if (currentRingState == RingState.Empty)
            {
                SetRingState(RingState.Full);
            }

            if (!canTakeDamage)
            {
                canTakeDamage = true;
                damageAvailableTime = Time.realtimeSinceStartup;
            }
        }
        
        // Check if damage cooldown has expired using real time
        if (!canTakeDamage && Time.realtimeSinceStartup >= damageAvailableTime)
        {
            canTakeDamage = true;
            Debug.Log("Player damage cooldown finished, setting canTakeDamage to true.");
        }
    }

    public bool SubtractRingSection(int sections)
    {
        if (!canTakeDamage) return false;
        
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
        
        StartDamageCooldown();
        return true;
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
    
    private void StartDamageCooldown()
    {
        canTakeDamage = false;
        damageAvailableTime = Time.realtimeSinceStartup + damageCooldown;
        Debug.Log($"Player has taken damage. Setting canTakeDamage to false. Will be available at real time: {damageAvailableTime}");
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
            // Debug.Log("Updated portrait to  " + currentPortrait);
        }
        else if (characterSwap.isChime && currentPortrait != PlayerPortrait.Chime)
        {
            currentPortrait = PlayerPortrait.Chime;
            SetRingState(currentRingState);
            // Debug.Log("Updated portrait to  " + currentPortrait);
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
