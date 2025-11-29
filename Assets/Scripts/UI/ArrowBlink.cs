using UnityEngine;
using UnityEngine.UI;

public class ArrowBlink : MonoBehaviour
{
    public float blinkSpeed = 2f;

    private Image img;
    private float timer;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    private void OnEnable()
    {
        timer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime * blinkSpeed;

        float alpha = Mathf.PingPong(timer, 1f);

        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
