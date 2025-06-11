using UnityEngine;
using UnityEngine.Events;

public class GoalEventSubscriber : MonoBehaviour
{
    public UnityEvent<Vector3> OnPlayerHitBallScored;

    private void OnEnable()
    {
        OnPlayerHitBallScored.AddListener(SendOnPlayerHitBallData);
    }

    private void OnDisable()
    {
        OnPlayerHitBallScored.RemoveListener(SendOnPlayerHitBallData);
    }

    private void SendOnPlayerHitBallData(Vector3 pos)
    {
        // Collect and send damage event data
        EventData data = new EventData
        {
            EventType = "PlayerHitBallScored",
            Timestamp = System.DateTime.UtcNow.ToString("o"),
            Position = pos.ToString()
        };

        Debug.Log("Sending OnPlayerHitBallScored() Position: " + pos.ToString());
        SendDataToServer(data);
    }

    private void SendDataToServer(EventData data)
    {
        string jsonData = JsonUtility.ToJson(data);
        StartCoroutine(ServerSender.SendData(jsonData));
    }
}
