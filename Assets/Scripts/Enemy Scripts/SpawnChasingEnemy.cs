using UnityEngine;

public class SpawnChasingEnemy : MonoBehaviour
{
    public GameObject Enemy;

    private void Start()
    {
        Enemy.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Enemy.SetActive(true);
            Debug.Log("The chasing enemy has spawned.");
        }
    }
}
