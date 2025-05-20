using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if NEW_INPUT_SYSTEM_INSTALLED
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// A basic example of a UI to start a host or client.
/// If you want to modify this Script please copy it into your own project and add it to your copied UI Prefab.
/// </summary>
public class TestNetButtons : MonoBehaviour
{
    [SerializeField]
    Button m_StartServerButton;
    [SerializeField]
    Button m_StartClientButton;

    void Awake()
    {
        if (!FindAnyObjectByType<EventSystem>())
        {
            var inputType = typeof(StandaloneInputModule);
#if ENABLE_INPUT_SYSTEM && NEW_INPUT_SYSTEM_INSTALLED
                inputType = typeof(InputSystemUIInputModule);                
#endif
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), inputType);
            eventSystem.transform.SetParent(transform);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_StartServerButton.onClick.AddListener(StartServer);
        m_StartClientButton.onClick.AddListener(StartClient);
    }

    void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        DeactivateButtons();
    }

    void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        DeactivateButtons();
    }

    void DeactivateButtons()
    {
        m_StartServerButton.interactable = false;
        m_StartClientButton.interactable = false;
    }
}