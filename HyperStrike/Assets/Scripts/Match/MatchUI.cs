using TMPro;
using UnityEngine;

public class MatchUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI initTimerText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (initTimerText != null && MatchManager.Instance.State == MatchState.INIT)
        {
            float time = MatchManager.Instance.GetCurrentWaitTime();
            initTimerText.text = time.ToString();
            if (time <= 0f) initTimerText.enabled = false;
        }
    }
}
