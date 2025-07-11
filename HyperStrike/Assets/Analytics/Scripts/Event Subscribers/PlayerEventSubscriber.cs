using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerEventSubscriber : MonoBehaviour
{
    public UnityEvent OnDeath;
    public UnityEvent OnReceiveDamage;
    public UnityEvent OnHeal;

    private void OnEnable()
    {
        OnDeath.AddListener(SendOnDeathData);
        OnReceiveDamage.AddListener(SendOnReceiveDamageData);
        OnHeal.AddListener(SendOnHealData);
    }

    private void OnDisable()
    {
        OnDeath.RemoveListener(SendOnDeathData);
        OnReceiveDamage.RemoveListener(SendOnReceiveDamageData);
        OnHeal.RemoveListener(SendOnHealData);
    }

    private void SendOnDeathData()
    {
        // Collect and send death event data
        string positionString = transform.position.ToString();

        if (string.IsNullOrEmpty(positionString))
        {
            Debug.LogError("Position is invalid or null!");
            return;
        }

        EventData data = new EventData
        {
            EventType = "Death",
            Timestamp = System.DateTime.UtcNow.ToString("o"),
            Position = positionString
        };

        Debug.Log("Sending OnDeath() Position: " + transform.position.ToString());
        SendDataToServer(data);
    }


    private void SendOnReceiveDamageData()
    {
        // Collect and send damage event data
        EventData data = new EventData
        {
            EventType = "ReceiveDamage",
            Timestamp = System.DateTime.UtcNow.ToString("o"),
            Position = transform.position.ToString()
        };

        Debug.Log("Sending OnReceiveDamage() Position: " + transform.position.ToString());
        SendDataToServer(data);
    }
    
    private void SendOnHealData()
    {
        // Collect and send damage event data
        EventData data = new EventData
        {
            EventType = "Heal",
            Timestamp = System.DateTime.UtcNow.ToString("o"),
            Position = transform.position.ToString()
        };

        Debug.Log("Sending OnHeal() Position: " + transform.position.ToString());
        SendDataToServer(data);
    }

    private void SendDataToServer(EventData data)
    {
        string jsonData = JsonUtility.ToJson(data);
        StartCoroutine(ServerSender.SendData(jsonData));
    }
}

[System.Serializable]
public class EventData
{
    public string EventType;
    public string Timestamp;
    public string Position;
}
