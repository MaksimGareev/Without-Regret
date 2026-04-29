using UnityEngine;
using System.Collections;

public class Ending : MonoBehaviour
{
    public static Ending Instance;

    public float scrollSpeed = 50f;
    public float endYPosition = 1200f;
    public TransitionToNewLevel transition;

    private RectTransform rectTransform;
    public bool finished = false;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError("No RectTransform");
            return;
        }

        // start below the screen
        rectTransform.anchoredPosition = new Vector2(0, -Screen.height *0.5f);
    }

    private void Update()
    {
        if (finished || !rectTransform)
        {
            DisableOtherCanvases();
            return;
        }

        rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        if (rectTransform.anchoredPosition.y >= endYPosition)
        {
            finished = true;
            StartCoroutine(LoadMainMenu());
            EnableOtherCanvases();
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

    private void EnableOtherCanvases()
    {
        if (!GameManager.Instance) return;

        if (GameManager.Instance.mainCanvas && !GameManager.Instance.mainCanvas.activeSelf)
        {
            GameManager.Instance.mainCanvas.SetActive(true);
        }

        if (GameManager.Instance.interactionIconsCanvas && !GameManager.Instance.interactionIconsCanvas.activeSelf)
        {
            GameManager.Instance.interactionIconsCanvas.SetActive(true);
        }

        if (GameManager.Instance.playerUICanvas && !GameManager.Instance.playerUICanvas.activeSelf)
        {
            GameManager.Instance.playerUICanvas.SetActive(true);
        }

        if (GameManager.Instance.gameOverCanvas && !GameManager.Instance.gameOverCanvas.activeSelf)
        {
            GameManager.Instance.gameOverCanvas.SetActive(GameOverManager.Instance.IsGameOver);
        }

        if (GameManager.Instance.objectivePanel && !GameManager.Instance.objectivePanel.activeSelf)
        {
            GameManager.Instance.objectivePanel.SetActive(GameManager.Instance.objectiveCanvas.IsVisible());
        }

        if (GameManager.Instance.qteCanvas && !GameManager.Instance.qteCanvas.activeSelf)
        {
            GameManager.Instance.qteCanvas.SetActive(true);
        }

        BossEnemyController boss = FindFirstObjectByType<BossEnemyController>();
        if (boss && boss.slidersContainer)
        {
            boss.slidersContainer.gameObject.SetActive(true);
        }
    }

    private void DisableOtherCanvases()
    {
        if (!GameManager.Instance) return;

        if (GameManager.Instance.mainCanvas && GameManager.Instance.mainCanvas.activeSelf)
        {
            GameManager.Instance.mainCanvas.SetActive(false);
        }

        if (GameManager.Instance.interactionIconsCanvas && GameManager.Instance.interactionIconsCanvas.activeSelf)
        {
            GameManager.Instance.interactionIconsCanvas.SetActive(false);
        }

        if (GameManager.Instance.journalUI && GameManager.Instance.journalUI.activeSelf)
        {
            GameManager.Instance.journalUI.SetActive(false);
        }

        if (GameManager.Instance.playerUICanvas && GameManager.Instance.playerUICanvas.activeSelf)
        {
            GameManager.Instance.playerUICanvas.SetActive(false);
        }

        if (GameManager.Instance.gameOverCanvas && GameManager.Instance.gameOverCanvas.activeSelf)
        {
            GameManager.Instance.gameOverCanvas.SetActive(false);
        }

        if (GameManager.Instance.dialoguePanel && GameManager.Instance.dialoguePanel.activeSelf)
        {
            GameManager.Instance.dialoguePanel.SetActive(false);
        }

        if (GameManager.Instance.objectivePanel && GameManager.Instance.objectivePanel.activeSelf)
        {
            GameManager.Instance.objectivePanel.SetActive(false);
        }

        if (GameManager.Instance.qteCanvas && GameManager.Instance.qteCanvas.activeSelf)
        {
            GameManager.Instance.qteCanvas.SetActive(false);
        }

        BossEnemyController boss = FindFirstObjectByType<BossEnemyController>();
        if (boss && boss.slidersContainer)
        {
            boss.slidersContainer.gameObject.SetActive(false);
        }
    }

}
