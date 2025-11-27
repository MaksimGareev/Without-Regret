using UnityEngine;

public class TimerUITester : MonoBehaviour
{
    private TimerRingUI timerRingUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (timerRingUI == null)
        {
            timerRingUI = FindObjectsByType<TimerRingUI>(FindObjectsSortMode.None)[0];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (timerRingUI != null)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                timerRingUI.SetRingState(TimerRingUI.RingState.Full);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                timerRingUI.SetRingState(TimerRingUI.RingState.TwoThirds);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                timerRingUI.SetRingState(TimerRingUI.RingState.OneThird);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                timerRingUI.SetRingState(TimerRingUI.RingState.Empty);
            }
        }
    }
}
