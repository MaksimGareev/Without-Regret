using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Ending : MonoBehaviour
{
    public float scrollSpeed = 50f;
    public float endYPosition = 1200f;
    public TransitionToNewLevel transition;

    private RectTransform rectTransform;
    private bool finished = false;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError("No RectTransform");
            return;
        }

        // start below the screen
        rectTransform.anchoredPosition = new Vector2(0, -Screen.height);
    }

    private void Update()
    {
        if (finished) return;

        rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        if (rectTransform.anchoredPosition.y >= endYPosition)
        {
            finished = true;
            StartCoroutine(LoadMainMenu());
        }
    }

    IEnumerator LoadMainMenu()
    {
        yield return new WaitForSeconds(2f);
        if (transition != null)
        {
            transition.TriggerSceneLoad();
        }
        else
        {
            Debug.LogError("Transition reference missing");
        }
    }

}
