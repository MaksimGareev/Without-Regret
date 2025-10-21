using UnityEngine;

public class LockPicking : MonoBehaviour
{
    public Camera cam;
    public RectTransform InnerLock;
    public RectTransform PickPosition;
    public GameObject LockPickUi;
    public float MaxAngle = 90;
    public float LockSpeed = 10;

    [Min(1)]
    [Range(1, 25)]
    public float LockRange = 10;

    private float EulerAngle;
    private float UnlockAngle;
    private Vector2 UnlockRange;

    private float KeyPressTime = 0;

    private bool MovePick = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NewLock();
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = PickPosition.position;

        if (MovePick)
        {
            Vector3 Direction = Input.mousePosition - cam.WorldToScreenPoint(transform.position);

            EulerAngle = Vector3.Angle(Direction, Vector3.up);

            Vector3 Cross = Vector3.Cross(Vector3.up, Direction);
            if (Cross.z < 0)
            {
                EulerAngle = -EulerAngle;
            }

            EulerAngle = Mathf.Clamp(EulerAngle, -MaxAngle, MaxAngle);

            Quaternion RotateTo = Quaternion.AngleAxis(EulerAngle, Vector3.forward);
            transform.rotation = RotateTo;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            MovePick = false;
            KeyPressTime = 1;
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            MovePick = true;
            KeyPressTime = 0;
        }

        KeyPressTime = Mathf.Clamp(KeyPressTime, 0, 1);

        float Percentage = Mathf.Round(100 - Mathf.Abs(((EulerAngle - UnlockAngle) / 100) * 100));
        float LockRotation = ((Percentage / 100) * MaxAngle) * KeyPressTime;
        float MaxRotation = (Percentage / 100) * MaxAngle;

        float LockLerp = Mathf.Lerp(InnerLock.eulerAngles.z, LockRotation, Time.deltaTime * LockSpeed);
        InnerLock.eulerAngles = new Vector3(0, 0, LockLerp);

        if (LockLerp >= MaxRotation - 1)
        {
            if(EulerAngle < UnlockRange.y && EulerAngle > UnlockRange.x)
            {
                Debug.Log("Unlocked");
                // NewLock();

                MovePick = true;
                KeyPressTime = 0;
            }
            else
            {
                float RandomRotation = Random.insideUnitCircle.x;
                transform.eulerAngles += new Vector3(0, 0, Random.Range(-RandomRotation, RandomRotation));
            }

        }
    }

    void NewLock()
    {
        LockPickUi.SetActive(true);
        UnlockAngle = Random.Range(-MaxAngle + LockRange, MaxAngle - LockRange);
        UnlockRange = new Vector2(UnlockAngle - LockRange, UnlockAngle + LockRange);
    }
}
