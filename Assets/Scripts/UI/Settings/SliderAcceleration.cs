using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderAcceleration : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference navigateAction;
    
    [Header("Tuning")]
    private static float baseUnitsPerSecond = 10.0f;
    private static float accelerationDelay = 2.0f;
    private static float timeToFullAcceleration = 2.0f;
    private static float accelerationMultiplier = 15.0f;
    
    private Slider slider;
    private float holdTimer = 0.0f;
    private float accelerationTimer = 0.0f;
    private Vector2 navValue = Vector2.zero;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        if (!slider)
        {
            Debug.LogError($"SliderAcceleration script was attached to {gameObject.name}, which does not have a Slider component.");
        }
    }

    private void OnEnable()
    {
        if (navigateAction != null)
        {
            navigateAction.action.Enable();
        }
        else
        {
            Debug.LogError($"SliderAcceleration on {gameObject.name} does not have a Navigate Action Reference assigned.");
        }

        holdTimer = 0.0f;
        accelerationTimer = 0.0f;
    }

    private void OnDisable()
    {
        holdTimer = 0.0f;
        accelerationTimer = 0.0f;
    }

    private void Update()
    {
        if (!slider.interactable 
            || !isActiveAndEnabled 
            || !EventSystem.current 
            || EventSystem.current.currentSelectedGameObject != gameObject)
        {
            return;
        }

        if (navigateAction)
        {
            navValue = navigateAction.action.ReadValue<Vector2>();
        }
        else
        {
            navValue = Vector2.zero;
            Debug.LogError($"SliderAcceleration on {gameObject.name} does not have a Navigate Action Reference assigned.");
        }
    }

    private void FixedUpdate()
    {
        if (!slider.interactable 
            || !isActiveAndEnabled 
            || !EventSystem.current 
            || EventSystem.current.currentSelectedGameObject != gameObject)
        {
            if (holdTimer > 0)
            {
                holdTimer = 0.0f;
            }

            if (accelerationTimer > 0)
            {
                accelerationTimer = 0.0f;
            }
            return;
        }

        float x = navValue.x;
        
        if (Mathf.Approximately(x, 0f))
        {
            holdTimer = 0.0f;
            accelerationTimer = 0.0f;
            return;
        }
        
        holdTimer += Time.unscaledDeltaTime;
        
        float stepSize = Mathf.Pow(slider.maxValue - slider.minValue, 0.5f) / 10f;

        float rate = baseUnitsPerSecond * stepSize;
        
        if (holdTimer > accelerationDelay)
        {
            accelerationTimer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(accelerationTimer / timeToFullAcceleration);
            
            rate *= Mathf.Lerp(1.0f, accelerationMultiplier, t);
        }

        float delta = Mathf.Sign(x) * Time.unscaledDeltaTime * rate;
        
        float next = Mathf.Clamp(slider.value + delta, slider.minValue, slider.maxValue);

        if (slider.wholeNumbers)
        {
            next = Mathf.Round(next);
        }
        
        slider.value = next;
        Debug.Log($"SliderAcceleration on {gameObject.name} set to {rate}, next value = {next}");
    }
}
