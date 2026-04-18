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
    [SerializeField] private float baseUnitsPerSecond = 1.0f;
    [SerializeField] private float accelerationDelay = 3.0f;
    [SerializeField] private float accelerationMultiplier = 4.0f;
    
    private Slider slider;
    private float holdTimer;
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
    }

    private void OnDisable()
    {
        holdTimer = 0.0f;
    }

    private void Update()
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
        
        float x = navValue.x;
        
        if (Mathf.Approximately(x, 0f))
        {
            holdTimer = 0.0f;
            return;
        }
        
        holdTimer += Time.unscaledDeltaTime;

        float rate = baseUnitsPerSecond;
        
        if (holdTimer > accelerationDelay)
        {
            rate = baseUnitsPerSecond *  accelerationMultiplier;
        }

        float delta = x * Time.unscaledDeltaTime * rate;
        
        float next = Mathf.Clamp(slider.value + delta, slider.minValue, slider.maxValue);

        if (slider.wholeNumbers)
        {
            next = Mathf.Round(next);
        }
        
        slider.value = next;
        Debug.Log($"SliderAcceleration on {gameObject.name} set to {rate}, next value = {next}");
    }
}
