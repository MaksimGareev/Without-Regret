using UnityEngine;

public class Gate : MonoBehaviour
{
    [SerializeField] private GameObject RotateRightDoor;
    [SerializeField] private GameObject RotateLeftDoor;

    private Animator rightDoorAnimator;
    private Animator leftDoorAnimator;

    void Awake()
    {
        rightDoorAnimator = RotateRightDoor.GetComponent<Animator>();
        leftDoorAnimator = RotateLeftDoor.GetComponent<Animator>();
    }
    
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Found Player");

            rightDoorAnimator.SetBool("NearPlayer", true);
            leftDoorAnimator.SetBool("NearPlayer", true);
        }
    }
}
