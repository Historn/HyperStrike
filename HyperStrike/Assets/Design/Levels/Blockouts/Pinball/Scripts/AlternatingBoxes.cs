using UnityEngine;

public class AlternatingBoxesAnimated : MonoBehaviour
{
    public Transform boxA;
    public Transform boxB;

    public float moveDistance = 3f;           // Distance boxes move up/down
    public float switchInterval = 30f;        // Time between swaps
    public float animationDuration = 2f;      // Time to animate each movement

    private Vector3 boxAUpPos, boxADownPos;
    private Vector3 boxBUpPos, boxBDownPos;

    private float switchTimer;
    private float animationTimer;
    private bool isAnimating = false;
    private bool isBoxAUp = true;

    private Transform movingUp;
    private Transform movingDown;

    void Start()
    {
        // Initialize up/down positions relative to starting local positions
        Vector3 offset = new Vector3(0f, -moveDistance, 0f);
        boxAUpPos = boxA.localPosition;
        boxADownPos = boxA.localPosition + offset;
        boxBDownPos = boxB.localPosition;
        boxBUpPos = boxB.localPosition - offset;

        // Set initial state
        boxA.localPosition = boxAUpPos;
        boxB.localPosition = boxBDownPos;

        switchTimer = switchInterval;
    }

    void Update()
    {
        if (!isAnimating)
        {
            switchTimer -= Time.deltaTime;
            if (switchTimer <= 0f)
            {
                switchTimer = switchInterval;
                StartAnimation();
            }
        }
        else
        {
            Animate();
        }
    }

    void StartAnimation()
    {
        isAnimating = true;
        animationTimer = 0f;

        if (isBoxAUp)
        {
            movingUp = boxB;
            movingDown = boxA;
        }
        else
        {
            movingUp = boxA;
            movingDown = boxB;
        }

        isBoxAUp = !isBoxAUp;
    }

    void Animate()
    {
        animationTimer += Time.deltaTime;
        float t = Mathf.Clamp01(animationTimer / animationDuration);

        if (isBoxAUp)
        {
            boxA.localPosition = Vector3.Lerp(boxADownPos, boxAUpPos, t);
            boxB.localPosition = Vector3.Lerp(boxBUpPos, boxBDownPos, t);
        }
        else
        {
            boxA.localPosition = Vector3.Lerp(boxAUpPos, boxADownPos, t);
            boxB.localPosition = Vector3.Lerp(boxBDownPos, boxBUpPos, t);
        }

        if (t >= 1f)
        {
            isAnimating = false;
        }
    }
}
