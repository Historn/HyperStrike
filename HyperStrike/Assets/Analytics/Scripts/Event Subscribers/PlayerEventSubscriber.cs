/*using UnityEngine;

namespace Gamekit3D
{
    public class PlayerEventSubscriber : MonoBehaviour
    {
        [SerializeField] private Damageable playerDamageable;

        private void OnEnable()
        {
            if (playerDamageable != null)
            {
                // Subscribe to the UnityEvents
                playerDamageable.OnDeath.AddListener(SendOnDeathData);
                playerDamageable.OnReceiveDamage.AddListener(SendOnReceiveDamageData);
            }
        }

        private void OnDisable()
        {
            if (playerDamageable != null)
            {
                // Unsubscribe from the UnityEvents
                playerDamageable.OnDeath.RemoveListener(SendOnDeathData);
                playerDamageable.OnReceiveDamage.RemoveListener(SendOnReceiveDamageData);
            }
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
}*/
