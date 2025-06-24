using System.Collections;
using TMPro;
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
    
    [SerializeField]
    TextMeshProUGUI m_FindStatusText;

    [SerializeField]
    TMP_InputField m_InputField;

    string m_ServerIP;
    ushort m_ServerPort;

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

    public void SetOnlineServerParams(string ip, ushort port)
    {
        m_ServerIP = ip;
        m_ServerPort = port;
    }

    void FindMatch()
    {
#if UNITY_EDITOR
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 8100);
#elif ONLINE_SERVER

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(m_ServerIP, m_ServerPort); //Set Online Server IP
#else
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("90.170.224.218", 8100); //Set Port Forwarded Server IP
        if (m_InputField.text != "") { NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(m_InputField.text, 8100); }
#endif

        m_FindStatusText.color = Color.white;
        m_FindStatusText.text = "Joining Server";

        StartCoroutine(WaitFullSync());
        if (m_FindMatchButton) m_FindMatchButton.interactable = false;
    }
    private void SceneManager_OnSynchronize(ulong clientId)
    {
        Debug.Log($"Client-Id ({clientId}) is synchronizing!");
    }

    void StartServer()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("0.0.0.0", 8100);
        NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApproval;
        NetworkManager.Singleton.StartServer();
        var status = NetworkManager.Singleton.SceneManager.LoadScene("LobbyRoom", LoadSceneMode.Single);
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Failed to load Lobby" +
                  $"with a {nameof(SceneEventProgressStatus)}: {status}");
        }
    }

    private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (SceneManager.GetActiveScene().name == "ArenaRoom")
        {
            response.Approved = false;
        }
        else
        {
            response.Approved = true;
        }
    }

    void DeactivateButtons()
    {
        if (m_StartServerButton) m_StartServerButton.interactable = false;
        if (m_FindMatchButton) m_FindMatchButton.interactable = false;
        gameObject.SetActive(false);
    }

    private IEnumerator WaitFullSync()
    {
        yield return new WaitForSeconds(2);

        var success = NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnect;
        if (success && NetworkManager.Singleton.IsApproved)
        {
            NetworkManager.Singleton.SceneManager.OnSynchronize += SceneManager_OnSynchronize; // Must be here, before loading the NetworkObjects from next scene
            DeactivateButtons();
        }
    }

    void OnDisconnect(ulong obj)
    {
        if (m_FindMatchButton) m_FindMatchButton.interactable = true;
        m_FindStatusText.color = Color.red;
        m_FindStatusText.text = "Unable to join the server, please try again or check server status.";
    }
}