using UnityEngine;

public class FollowChimeObjective : MonoBehaviour
{
    [SerializeField] private ObjectiveData linkedObjective;

    [Header("Chime Waypoint Settings")]
    [SerializeField] private Transform[] waypoints;
    [Tooltip("How close the player must get to Chime to trigger Chime moving to the next waypoint")]
    [SerializeField] private float playerReachDistance = 2f;
    [SerializeField] private float chimeMoveSpeed = 5f;
    [SerializeField] private Chime chime;
    [SerializeField] private Transform player;

    private int currentWaypointIndex = 0;
    private bool isObjectiveActive = false;
    private Transform chimeTransform;
    private bool completed = false;

    private void OnEnable()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveActivated.AddListener(OnObjectiveActivated);
            ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(OnObjectiveCompleted);
        }
    }

    private void OnDisable()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveActivated.RemoveListener(OnObjectiveActivated);
            ObjectiveManager.Instance.OnObjectiveCompleted.RemoveListener(OnObjectiveCompleted);
        }
    }

    private void Start()
    {
        // Ensure references exist, attempt to find if missing
        if (player == null)
        {
            Debug.LogWarning("FollowChimeObjective: Player transform reference missing. Attempting to find by tag.", this);
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
            else return;
        }

        if (chimeTransform == null || chime == null)
        {
            Debug.LogWarning("FollowChimeObjective: Chime reference missing. Attempting to find by tag.", this);
            var chimeObject = GameObject.FindWithTag("Chime");
            if (chimeObject != null)
            {
                chimeTransform = chimeObject.transform;
                chime = chimeObject.GetComponent<Chime>();
            }
            else
            {
                Debug.LogWarning("FollowChimeObjective: Chime not found in scene. Objective cannot guide without Chime.", this);
                return;
            }
        }
    }

    private void Update()
    {
        if (!isObjectiveActive || completed) return;

        // If no waypoints, nothing to do
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("FollowChimeObjective: No waypoints assigned for objective '" + (linkedObjective != null ? linkedObjective.title : name) + "'.");
            return;
        }

        // If Chime is guiding, check player distance to Chime
        if (chime.IsGuiding)
        {
            float dist = Vector3.Distance(player.position, chimeTransform.position);
            if (dist <= playerReachDistance)
            {
                // Player reached Chime -> advance to next waypoint (or complete)
                AdvanceWaypoint();
            }
        }
        else
        {
            // If Chime not yet guiding (but objective active) ensure it starts at current waypoint
            // This covers cases where chime wasn't present at Start.
            StartGuidingIfNeeded();
        }
    }

    private void OnObjectiveActivated(ObjectiveInstance instance)
    {
        if (instance == null || instance.data == null) return;
        if (instance.data != linkedObjective) return;

        // Begin objective
        isObjectiveActive = true;
        currentWaypointIndex = 0;

        // Try to find chime and player now and start guiding
        if (player == null)
        {
            Debug.LogWarning("FollowChimeObjective activated but Player reference not set. Attempting to find by tag.", this);  
            var playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null) player = playerGo.transform;
        }

        if (chime == null)
        {
            Debug.LogWarning("FollowChimeObjective activated but Chime reference not set. Attempting to find by tag.", this);
            var chimeGo = GameObject.FindWithTag("Chime");
            if (chimeGo != null)
            {
                chimeTransform = chimeGo.transform;
                chime = chimeGo.GetComponent<Chime>();
                StartGuidingIfNeeded();
            }
            else
            {
                Debug.LogWarning("FollowChimeObjective activated but Chime not found.", this);
            }
        }
    }

    private void OnObjectiveCompleted(ObjectiveInstance instance)
    {
        if (instance == null || instance.data == null) return;
        if (instance.data != linkedObjective) return;

        completed = true;
        // Stop Guiding and restore Chime
        EndGuiding();
        isObjectiveActive = false;
    }

    private void StartGuidingIfNeeded()
    {
        if (!isObjectiveActive || completed) return;
        if (waypoints == null || waypoints.Length == 0) return;
        if (chime == null) return;

        // If chime already guiding, don't restart.
        if (chime.IsGuiding) return;

        // Send chime to the current waypoint
        chime.GoToMarker(waypoints[currentWaypointIndex].position, chimeMoveSpeed);
    }

    private void AdvanceWaypoint()
    {
        ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);

        if (completed) return;

        // Move to next index
        currentWaypointIndex++;

        if (currentWaypointIndex < waypoints.Length)
        {
            // Instruct Chime to move to the next waypoint
            if (chime != null)
            {
                chime.GoToMarker(waypoints[currentWaypointIndex].position, chimeMoveSpeed);
            }
            else
            {
                Debug.LogWarning("FollowChimeObjective: chimeComp missing when advancing waypoint.");
            }
        }
    }

    private void EndGuiding()
    {
        if (chime != null)
        {
            chime.ReturnToPlayer();
        }
    }
}
