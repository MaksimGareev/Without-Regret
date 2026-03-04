using UnityEngine;

public class SpawnChasingEnemy : MonoBehaviour
{
    public GameObject Enemy;
    public string hintMessage = "Hurry Echo that thing is still chasing after Darry we need to help him!";
    public bool hasPassed = false;

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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Enemy.SetActive(true);
            Debug.Log("The chasing enemy has spawned.");

            if (hasPassed == false)
            {
                ChimeHintUI hintUI = FindObjectOfType<ChimeHintUI>();
                if (hintUI != null)
                {
                    hintUI.ShowHintMessage(hintMessage);
                }
            }

            hasPassed = true;
        }
    }
}
