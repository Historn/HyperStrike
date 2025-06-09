using Unity.Multiplayer;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if NEW_INPUT_SYSTEM_INSTALLED
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// A basic example of a UI to start a host or client.
/// If you want to modify this Script please copy it into your own project and add it to your copied UI Prefab.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [SerializeField]
    Button m_StartServerButton;
    [SerializeField]
    Button m_FindMatchButton;

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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (m_StartServerButton) m_StartServerButton.onClick.AddListener(StartServer);
        if (m_FindMatchButton) m_FindMatchButton.onClick.AddListener(FindMatch);

        if (MultiplayerRolesManager.ActiveMultiplayerRoleMask == MultiplayerRoleFlags.Server) StartServer(); 
    }

    void FindMatch()
    {
#if UNITY_EDITOR
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 7777);
#else
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("192.168.1.22", 7777);
#endif
        var success = NetworkManager.Singleton.StartClient();
        if (success)
        {
            NetworkManager.Singleton.SceneManager.OnSynchronize += SceneManager_OnSynchronize; // Must be here, before loading the NetworkObjects from next scene
        }
        DeactivateButtons();
    }
    private void SceneManager_OnSynchronize(ulong clientId)
    {
        Debug.Log($"Client-Id ({clientId}) is synchronizing!");
    }

    void StartServer()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("0.0.0.0", 7777);
        NetworkManager.Singleton.StartServer();
        var status = NetworkManager.Singleton.SceneManager.LoadScene("LobbyRoom", LoadSceneMode.Single);
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Failed to load Lobby" +
                  $"with a {nameof(SceneEventProgressStatus)}: {status}");
        }
        DeactivateButtons();
    }

    void DeactivateButtons()
    {
        if (m_StartServerButton) m_StartServerButton.interactable = false;
        if (m_FindMatchButton) m_FindMatchButton.interactable = false;
        gameObject.SetActive(false);
    }
}