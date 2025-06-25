using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LobbyState : byte
{
    NONE = 0,
    CONNECTING,
    WAIT,
    COMPLETED
}

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public NetworkVariable<LobbyState> State { get; private set; } = new NetworkVariable<LobbyState>(LobbyState.NONE);

    public NetworkVariable<float> currentWaitTime = new NetworkVariable<float>(5.0f);

    float waitTime = 10f;

    private IEnumerator completedTimerCoroutine;

    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private List<GameObject> charactersPrefabs;

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApproval;

        GameObject ball = Instantiate(ballPrefab, new Vector3(0, 1, 0), Quaternion.identity);
        ball.GetComponent<NetworkObject>().Spawn(true);

        SetLobbyState(LobbyState.CONNECTING);
    }

    private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= 6)
        {
            response.Approved = false;
        }
        else
        {
            response.Approved = true;
        }
    }

    void OnClientConnected(ulong clientId)
    {
        Characters[] enumValues = (Characters[])System.Enum.GetValues(typeof(Characters));
        var character = (byte)UnityEngine.Random.Range(0, (enumValues.Length - 1));

        if (character != (byte)Characters.NONE)
        {
            GameObject player = Instantiate(charactersPrefabs[character], Vector3.zero, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= 6)
        {
            SetLobbyState(LobbyState.WAIT);
        }
    }

    void OnClientDisconnected(ulong clientId)
    {
        if (IsClient && !IsServer)
        {
            StartCoroutine(DisconnectAndLoadMenu());
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count < 6)
        {
            SetLobbyState(LobbyState.CONNECTING);
        }
    }

    private IEnumerator DisconnectAndLoadMenu()
    {
        yield return new WaitForSeconds(0.5f); // Let the server clean up
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    void Awake()
    {
        // Destroy, we dont want it to be in the other scenes, not needed
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
#if DEV_CLIENT
        if (Input.GetKeyDown(KeyCode.P) && State.Value != LobbyState.WAIT)
        {
            SetLobbyStateRpc(LobbyState.WAIT);
        }
#endif
    }

    void LobbyStateBehaviour()
    {
        switch (State.Value)
        {
            case LobbyState.NONE:
            case LobbyState.CONNECTING:
                {
                    if (completedTimerCoroutine != null)
                    {
                        StopCoroutine(completedTimerCoroutine);
                        completedTimerCoroutine = null;
                    }

                    currentWaitTime.Value = waitTime;

                    completedTimerCoroutine = LobbyCompletedTimer();
                }
                break;
            case LobbyState.WAIT:
                {
                    // Once all 6 players are in the lobby, init Coroutine with timer, then change scene in server.
                    StartCoroutine(completedTimerCoroutine);
                }
                break;
            case LobbyState.COMPLETED:
                {
                    var status = NetworkManager.Singleton.SceneManager.LoadScene("ArenaRoom", LoadSceneMode.Single);
                    if (status != SceneEventProgressStatus.Started)
                    {
                        Debug.LogWarning($"Failed to load Arena Room Scene" +
                              $"with a {nameof(SceneEventProgressStatus)}: {status}");
                    }
                }
                break;
            default:
                break;
        }
    }

    public void SetLobbyState(LobbyState state)
    {
        if (IsServer && State.Value != state)
        {
            State.Value = state;
            LobbyStateBehaviour();
        }
    }

    [Rpc(SendTo.Server)]
    public void SetLobbyStateRpc(LobbyState state)
    {
        SetLobbyState(state);
    }

    private IEnumerator LobbyCompletedTimer()
    {
        while (currentWaitTime.Value >= 0.0f)
        {
            yield return new WaitForSeconds(1f);
            currentWaitTime.Value--;
        }

        SetLobbyState(LobbyState.COMPLETED);
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApproval;
            }
        }
    }
}
