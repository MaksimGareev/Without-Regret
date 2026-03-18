using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public GameObject currentPlayer;
    public GameObject chimePrefab;
    public GameObject EchoPrefab;
    public PlayerController playerController;

    public bool chimePlaying = false;
    public bool echoPlaying = true;

    public void Start()
    {
        currentPlayer = GameObject.FindGameObjectWithTag("Player");
        if (currentPlayer != null)
        {
            Debug.Log("Found Player");
        }
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (echoPlaying)
            {
                SwapToChime();
            }
            else if (chimePlaying)
            {
                SwapToEcho();
            }
        }
    }

    public void SwapToChime()
    {
        echoPlaying = false;
        chimePlaying = true;
        Debug.Log("Swapped to Chime");
        Vector3 pos = currentPlayer.transform.position;
        Quaternion rot = currentPlayer.transform.rotation;

        GameObject oldPlayer = currentPlayer;

        GameObject newPlayer = Instantiate(chimePrefab, pos, rot);

        currentPlayer = newPlayer;
        CameraMovement cam = Camera.main.GetComponent<CameraMovement>();
        if (cam != null)
        {
            cam.SetTarget(newPlayer.transform);
        }
        Destroy(oldPlayer);

    }

    public void SwapToEcho()
    {
        chimePlaying = false;
        echoPlaying = true;
        Debug.Log("Swapped to Echo");
        Vector3 pos = currentPlayer.transform.position;
        Quaternion rot = currentPlayer.transform.rotation;

        GameObject oldPlayer = currentPlayer;

        GameObject newPlayer = Instantiate(EchoPrefab, pos, rot);

        currentPlayer = newPlayer;
        CameraMovement cam = Camera.main.GetComponent<CameraMovement>();
        if (cam != null)
        {
            cam.SetTarget(newPlayer.transform);
        }
        Destroy(oldPlayer);

    }

}