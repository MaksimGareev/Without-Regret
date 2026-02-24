using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class QTETriggerVolume : MonoBehaviour, IInteractable
{
    [SerializeField] private BossEnemyController bossEnemy;
    [SerializeField] private PlayerController playerController;
    [SerializeField, Tooltip("The QTE Canvas object")] private GameObject qteUI;
    [SerializeField, Tooltip("The sprite assets for each arrow. Should have 4 (in order of up->left->down->right)")] private Sprite[] arrowImages;
    [SerializeField, Tooltip("The image components of the arrows in the ArrowHolder. Should have 4")] private RawImage[] arrows;

    [SerializeField] bool showDebugLogs = false;

    private readonly int[] directionAssignments = new int[] { 0, 0, 0, 0 }; // 0 = up, 1 = left, 2 = down, 3 = right in terms of layout on the d-pad and arrow keys. Randomly generated on QTE start
    private int arrowIndex = 0;
    private int arrowsLength;
    private PlayerControls controls;
    private bool controlsLocked = false;
    private bool initiated = false;

    public float interactionPriority => 10;

    public InteractType interactType => InteractType.Lockpick;

    private void Awake()
    {
        controls = new PlayerControls();
        arrowsLength = arrows.Length;
        qteUI.SetActive(false);
    }

    void OnEnable()
    {
        controls.Enable();
    }

    private void Update()
    {
        if (!initiated) return;

        if (!controlsLocked && arrowIndex < directionAssignments.Length)
        {
            // Check the player's input
            if (controls.LockPicking.ArrowUp.triggered)
            {
                CheckDirection(0);
            }
            else if (controls.LockPicking.ArrowRight.triggered)
            {
                CheckDirection(1);
            }
            else if (controls.LockPicking.ArrowDown.triggered)
            {
                CheckDirection(2);
            }
            else if (controls.LockPicking.ArrowLeft.triggered)
            {
                CheckDirection(3);
            }
        }
        else if (arrowIndex >= directionAssignments.Length)
        {
            // Succeeded
            qteUI.SetActive(false);

            // Unlock player movement
            Rigidbody rb = playerController.GetComponent<Rigidbody>();
            playerController.MovementLocked = false;
            playerController.enabled = true;
            rb.constraints = RigidbodyConstraints.None;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            initiated = false;

            if (showDebugLogs) Debug.Log("QTE succeeded");

            if (bossEnemy != null)
            {
                bossEnemy.TakeDamage();
            }
            else
            {
                Debug.LogError("Boss Enemy reference for QTE trigger volume is null!", this);
            }

            Destroy(gameObject);
        }
    }

    public bool CanInteract(GameObject player)
    {
        return !initiated && arrowIndex < directionAssignments.Length;
    }

    public void OnPlayerInteraction(GameObject player)
    {
        // Generate sequence for the QTE, freeze player movement, and initialize UI
        GenerateSolutions();
        playerController.MovementLocked = true;
        playerController.enabled = false;
        player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        for (int i = 0; i < arrowsLength; i++)
        {
            arrows[i].color = Color.white;
        }

        qteUI.SetActive(true);
        initiated = true; // start the qte
        if (showDebugLogs) Debug.Log("Starting QTE");
    }

    private void GenerateSolutions()
    {
        for (int i = 0; i < arrowsLength; i++)
        {
            int direction = Random.Range(0, 4);
            directionAssignments[i] = direction;

            //assigns sprites to the ui images based off directional inputs given
            arrows[i].texture = arrowImages[direction].texture;
        }
    }

    private void CheckDirection(int input)
    {
        if (input == directionAssignments[arrowIndex])
        {
            // Correct direction, move on
            Color tempColor = Color.green;
            tempColor.a = 0.75f;
            arrows[arrowIndex].color = tempColor;
            arrowIndex++;
        }
        else
        {
            arrowIndex = 0;
            controlsLocked = true;
            // Wrong Direction, start over
            StartCoroutine(WrongDirection());
        }
    }
    IEnumerator WrongDirection()
    {
        if (showDebugLogs) Debug.Log("Wrong Direction input, resetting");
        for (int i = 0; i < arrowsLength; i++)
        {
            arrows[i].color = Color.red;
        }

        yield return new WaitForSeconds(.25f);

        for (int i = 0; i < arrowsLength; i++)
        {
            arrows[i].color = Color.white;
        }
        controlsLocked = false;
    }


    public void OnDrawGizmos()
    {
        // Visualize the trigger volume
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider>().bounds.size);
    }
}
