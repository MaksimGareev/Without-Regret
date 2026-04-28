using UnityEngine;
using System.Collections.Generic;

public class SpawnChasingEnemy : MonoBehaviour
{
    public GameObject Enemy;
    public string hintMessage = "Hurry Echo that thing is still chasing after Darry we need to help him!";
    public bool hasPassed = false;

    public bool enableChimeHint;
    public List<string> blockInteractionIfComplete = new List<string>();

    private void Start()
    {
        Enemy.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (hasPassed == false)
        {
            Enemy.SetActive(false);
        }

        foreach (string id in blockInteractionIfComplete)
        {
            if (ObjectiveManager.Instance.IsObjectiveCompleted(id))
            {
                this.gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Enemy.SetActive(true);
            Debug.Log("The chasing enemy has spawned.");

            if (hasPassed == false)
            {
                ChimeHintUI hintUI = FindFirstObjectByType<ChimeHintUI>();
                if (hintUI != null && enableChimeHint)
                {
                    hintUI.ShowHintMessage(hintMessage);
                }
            }

            hasPassed = true;
        }
    }
}
